using TheSingularityWorkshop.FSM_API;
#if WORKSHOP_USE_REPOSITORY_SOURCES
using FSMApi = TheSingularityWorkshop.FSM_API.FsmApi;
#else
using FSMApi = TheSingularityWorkshop.FSM_API.FSM_API;
#endif

namespace TheSingularityWorkshop.Renderer;

/// <summary>Identifies the terminal or active state of a prepared bucket pipeline execution.</summary>
public enum PreparedBucketPipelineExecutionState
{
    /// <summary>Prepared stages are being executed in order.</summary>
    Executing,
    /// <summary>All stages succeeded and the finalization action is pending or running.</summary>
    Finalization,
    /// <summary>All stages and finalization completed successfully.</summary>
    Completed,
    /// <summary>A stage or finalization action failed; publication was not completed.</summary>
    Failed
}

/// <summary>
/// Executes one prepared bucket pipeline through a bucket-local FSM_API instance.
/// </summary>
/// <remarks>
/// Each FSM update performs at most one meaningful pipeline stage. Stage and finalization
/// callbacks are supplied by the runtime integration; this type has no graphics/backend
/// dependency. A failed callback moves the FSM to its terminal Failed state and prevents
/// the finalization callback from running.
/// </remarks>
public sealed class PreparedBucketPipelineExecution : IDisposable
{
    private const string ProcessingGroup = "Renderer.PreparedPipeline";
    private const string DefinitionName = "Renderer.PreparedBucketPipeline.Execution.v1";
    private static readonly object DefinitionLock = new();

    private readonly ExecutionContext _context;
    private readonly FSMHandle _handle;
    private bool _disposed;

    /// <summary>Creates an execution instance for a prepared pipeline.</summary>
    /// <param name="name">Unique identity for this execution context.</param>
    /// <param name="pipeline">The immutable pipeline prepared by the composition host.</param>
    /// <param name="stageActions">One runtime action for each prepared stage identity.</param>
    /// <param name="finalize">Action that publishes the successfully completed result.</param>
    public PreparedBucketPipelineExecution(
        string name,
        PreparedBucketPipeline pipeline,
        IReadOnlyDictionary<string, Action> stageActions,
        Action finalize)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(stageActions);
        ArgumentNullException.ThrowIfNull(finalize);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("An execution name is required.", nameof(name));

        _context = new ExecutionContext(name, pipeline, stageActions, finalize);
        EnsureDefinition();
        _handle = FSMApi.Create.CreateInstance(DefinitionName, _context, ProcessingGroup);
    }

    /// <summary>Gets the immutable prepared pipeline being executed.</summary>
    public PreparedBucketPipeline Pipeline => _context.Pipeline;

    /// <summary>Gets the current FSM execution state.</summary>
    public PreparedBucketPipelineExecutionState State
    {
        get
        {
            if (!Enum.TryParse<PreparedBucketPipelineExecutionState>(_handle.CurrentState, out var state))
                throw new InvalidOperationException($"Unknown pipeline execution state '{_handle.CurrentState}'.");
            return state;
        }
    }

    /// <summary>Gets whether every stage and the finalization action completed successfully.</summary>
    public bool IsFinalized => _context.IsFinalized;

    /// <summary>Gets the exception that caused execution to fail, if any.</summary>
    public Exception? Failure => _context.Failure;

    /// <summary>Executes at most one pipeline stage or advances finalization by one FSM update.</summary>
    /// <returns>The FSM state after the update.</returns>
    public PreparedBucketPipelineExecutionState Advance()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _handle.Update(ProcessingGroup);
        return State;
    }

    /// <summary>Destroys the owned FSM instance without changing caller-owned pipeline data.</summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _context.IsValid = false;
        FSMApi.Interaction.DestroyInstance(_handle);
        _disposed = true;
    }

    private static void EnsureDefinition()
    {
        lock (DefinitionLock)
        {
            if (FSMApi.Interaction.Exists(DefinitionName, ProcessingGroup))
                return;

            FSMApi.Create.CreateFiniteStateMachine(
                    DefinitionName,
                    processRate: 0,
                    processingGroup: ProcessingGroup)
                .State(
                    nameof(PreparedBucketPipelineExecutionState.Executing),
                    onEnter: null,
                    onUpdate: context => ((ExecutionContext)context).ExecuteNextStage(),
                    onExit: null)
                .State(
                    nameof(PreparedBucketPipelineExecutionState.Finalization),
                    onEnter: null,
                    onUpdate: context => ((ExecutionContext)context).TryFinalize(),
                    onExit: null)
                .State(
                    nameof(PreparedBucketPipelineExecutionState.Completed),
                    onEnter: null,
                    onUpdate: null,
                    onExit: null)
                .State(
                    nameof(PreparedBucketPipelineExecutionState.Failed),
                    onEnter: null,
                    onUpdate: null,
                    onExit: null)
                .WithInitialState(nameof(PreparedBucketPipelineExecutionState.Executing))
                .Transition(
                    nameof(PreparedBucketPipelineExecutionState.Executing),
                    nameof(PreparedBucketPipelineExecutionState.Failed),
                    context => ((ExecutionContext)context).HasFailed)
                .Transition(
                    nameof(PreparedBucketPipelineExecutionState.Executing),
                    nameof(PreparedBucketPipelineExecutionState.Finalization),
                    context => ((ExecutionContext)context).AllStagesSucceeded)
                .Transition(
                    nameof(PreparedBucketPipelineExecutionState.Finalization),
                    nameof(PreparedBucketPipelineExecutionState.Failed),
                    context => ((ExecutionContext)context).HasFailed)
                .Transition(
                    nameof(PreparedBucketPipelineExecutionState.Finalization),
                    nameof(PreparedBucketPipelineExecutionState.Completed),
                    context => ((ExecutionContext)context).IsFinalized)
                .BuildDefinition();
        }
    }

    private sealed class ExecutionContext : IStateContext
    {
        private readonly IReadOnlyDictionary<string, Action> _stageActions;
        private readonly Action _finalize;
        private int _nextStageIndex;

        public ExecutionContext(
            string name,
            PreparedBucketPipeline pipeline,
            IReadOnlyDictionary<string, Action> stageActions,
            Action finalize)
        {
            Name = name;
            Pipeline = pipeline;
            _finalize = finalize;

            var actions = new Dictionary<string, Action>(StringComparer.Ordinal);
            foreach (var pair in stageActions)
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value is null)
                    throw new ArgumentException("Stage action identities and callbacks are required.", nameof(stageActions));
                actions.Add(pair.Key, pair.Value);
            }

            foreach (var stage in pipeline.Stages)
            {
                if (!actions.ContainsKey(stage.Id))
                    throw new ArgumentException($"No runtime action was supplied for stage '{stage.Id}'.", nameof(stageActions));
            }

            if (actions.Keys.Any(id => !pipeline.Stages.Any(stage => stage.Id == id)))
                throw new ArgumentException("A runtime action was supplied for a stage not present in the prepared pipeline.", nameof(stageActions));

            _stageActions = actions;
        }

        public string Name { get; set; }

        public bool IsValid { get; set; } = true;

        public PreparedBucketPipeline Pipeline { get; }

        public Exception? Failure { get; private set; }

        public bool HasFailed => Failure is not null;

        public bool IsFinalized { get; private set; }

        public bool AllStagesSucceeded => !HasFailed && _nextStageIndex == Pipeline.Stages.Count;

        public void ExecuteNextStage()
        {
            if (HasFailed || IsFinalized || _nextStageIndex >= Pipeline.Stages.Count)
                return;

            var stage = Pipeline.Stages[_nextStageIndex];
            try
            {
                _stageActions[stage.Id]();
                _nextStageIndex++;
            }
            catch (Exception exception)
            {
                Failure = exception;
            }
        }

        public void TryFinalize()
        {
            if (HasFailed || _nextStageIndex != Pipeline.Stages.Count)
                return;

            try
            {
                _finalize();
                IsFinalized = true;
            }
            catch (Exception exception)
            {
                Failure = exception;
            }
        }
    }
}
