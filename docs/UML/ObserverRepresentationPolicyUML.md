# ObserverRepresentationPolicy — class diagram

```mermaid
classDiagram
    class RepresentationHorizonRule {
      +EventHorizon Horizon
      +RepresentationTarget Representation
    }
    class ObserverRepresentationDecision {
      <<readonly record struct>>
      +double Distance
      +EventHorizon Horizon
      +RepresentationTarget Representation
      +string HorizonName
      +double UpdatesPerSecond
    }
    class ObserverRepresentationPolicy {
      -EventHorizon[] _horizons
      -Dictionary~EventHorizon, RepresentationHorizonRule~ _ruleByHorizon
      +Select(double distance) ObserverRepresentationDecision
    }
    ObserverRepresentationPolicy o-- RepresentationHorizonRule : validates and maps
    RepresentationHorizonRule --> EventHorizon : selects horizon
    RepresentationHorizonRule --> RepresentationTarget : desired representation
    ObserverRepresentationPolicy ..> ObserverDistance : selects horizon by distance
    ObserverRepresentationPolicy ..> ObserverRepresentationDecision : returns
    ObserverRepresentationDecision --> EventHorizon : retains identity and cadence
```

## Use case

The policy turns an observer distance into an explicit horizon and representation decision. A host can then map that decision to a computation bucket or rendering stage without parsing a display name.

## Boundary

The diagram describes policy selection only. It does not imply automatic FSM scheduling, occlusion handling, frame creation, or GPU execution.
