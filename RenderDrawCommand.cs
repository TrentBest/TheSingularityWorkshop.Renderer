using System.Numerics;

namespace TheSingularityWorkshop.Renderer;

/// <summary>Describes one platform-neutral draw operation produced by the Renderer.</summary>
public readonly record struct RenderDrawCommand
{
    /// <summary>Initializes a draw command.</summary>
    public RenderDrawCommand(
        RenderResourceId geometry,
        RenderResourceId material,
        Matrix4x4 worldTransform,
        int indexCount,
        int firstIndex,
        int baseVertex,
        RendererRepresentation representation,
        RenderResourceId instances = default,
        int instanceCount = 1,
        int firstInstance = 0)
    {
        if (!geometry.IsValid)
            throw new ArgumentException("A valid geometry resource is required.", nameof(geometry));
        if (!material.IsValid)
            throw new ArgumentException("A valid material resource is required.", nameof(material));
        if (indexCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(indexCount));
        if (firstIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(firstIndex));

        if (instanceCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(instanceCount));
        if (firstInstance < 0)
            throw new ArgumentOutOfRangeException(nameof(firstInstance));
        if (!instances.IsValid && (instanceCount != 1 || firstInstance != 0))
            throw new ArgumentException("An instance-data resource is required for an instanced range.", nameof(instances));

        Geometry = geometry;
        Material = material;
        WorldTransform = worldTransform;
        IndexCount = indexCount;
        FirstIndex = firstIndex;
        BaseVertex = baseVertex;
        Representation = representation;
        Instances = instances;
        InstanceCount = instanceCount;
        FirstInstance = firstInstance;
    }

    /// <summary>Gets the geometry resource identifier.</summary>
    public RenderResourceId Geometry { get; }
    /// <summary>Gets the material resource identifier.</summary>
    public RenderResourceId Material { get; }
    /// <summary>Gets the world transform.</summary>
    public Matrix4x4 WorldTransform { get; }
    /// <summary>Gets the number of indices to draw.</summary>
    public int IndexCount { get; }
    /// <summary>Gets the first index in the geometry resource.</summary>
    public int FirstIndex { get; }
    /// <summary>Gets the base vertex offset.</summary>
    public int BaseVertex { get; }
    /// <summary>Gets the computational representation that produced this command.</summary>
    public RendererRepresentation Representation { get; }
    /// <summary>Gets the optional instance-data resource identifier; invalid means one non-instanced draw.</summary>
    public RenderResourceId Instances { get; }
    /// <summary>Gets the number of instances to draw.</summary>
    public int InstanceCount { get; }
    /// <summary>Gets the first instance in the instance-data resource.</summary>
    public int FirstInstance { get; }
}
