# Architecture and coding rules

This project uses a Passive View variant of Model-View-Presenter (MVP).
The purpose of the architecture is to keep game rules testable while Unity
components remain focused on scene references, input, and presentation.

## Dependency direction

```text
Unity View --input--> Presenter --commands--> Model
Unity View <--render-- Presenter <--state/events-- Model
```

- Models contain runtime state and game rules. They must not depend on a View
  or Presenter.
- Views are `MonoBehaviour` components. They expose player input as events and
  render values supplied by a Presenter. They do not decide game rules.
- Presenters are ordinary C# classes wherever practical. They coordinate a
  Model and one or more narrow View interfaces.
- ScriptableObjects contain immutable authoring definitions. Mutable runtime
  state belongs to Models or serializable save-data classes.
- Scene-level composition roots create Presenters and connect dependencies.
- Cross-feature access is expressed through small interfaces or domain events,
  not by looking up arbitrary scene objects.

Existing scene-facing component names may temporarily remain as compatibility
facades while their rules move into Models and Presenters. This keeps prefab
and scene references valid during migration.

## Feature boundaries

- `Battle`: battle flow, turn order, action selection, and battle results.
- `Grid`: board state, pathfinding, ranges, grid input, and highlighting.
- `Unit`: character definitions, runtime combatants, status, and presentation.
- `Formation`: selected party state and preparation-screen presentation.
- `Map`: dungeon-run state, map generation, and map presentation.
- `Recruitment`: candidate generation and roster changes. Existing `Gacha`
  names are migration-only terminology.
- `Inventory`: currency, equipment, rewards, and recovered items.
- `Progression`: experience, training, mentorship, unlocks, and records.

The vertical-slice requirements in `GAME_DESIGN_DOCUMENT.md` determine which
extension points are introduced. Deferred ideas do not justify abstractions
until a current requirement needs them.

## Member order

Members inside every type use the following order:

1. `const`
2. `static readonly`
3. public fields (legacy/serialization only; avoid in new code)
4. public properties
5. public events
6. constructors
7. public methods
8. `[SerializeField] private` fields
9. other private fields
10. Unity message methods (`Awake`, `Start`, `Update`, and similar)
11. private methods

Within a section, keep closely related members together. Attributes and XML
documentation stay attached to the member they describe.

Apply and verify the order after adding or moving members:

```powershell
dotnet run --project Tools/MemberOrder/MemberOrder.csproj -- Assets
```

The tool uses Roslyn syntax nodes, so attributes, documentation, comments, and
method bodies move with their declarations.

## Current MVP migration map

| Feature | Model | Presenter | View / composition root |
|---|---|---|---|
| Unit | `UnitModel` | battle presenters | `Unit` |
| Grid | `GridPathfinder`, `GridRangeCalculator` | `BattleManager` during migration | `GridManager`, `GridCell` |
| Battle | `BattleTurnOrderModel`, action models | `BattleHudPresenter`, `BattleManager` during migration | `BattleHUD` and focused panels |
| Formation | `FormationModel` | `FormationScreenPresenter` | `FormationScreenUI`, `FormationManager` facade |
| Dungeon map | `DungeonRunModel` | `MapPresenter` | `MapUI`, `MapNodeUI`, `MapManager` facade |
| Recruitment | `CharacterRosterModel`, `CharacterGenerator` | `RecruitmentPresenter` | `GachaManager` compatibility view |

`BattleManager`, `GridManager`, `FormationManager`, `MapManager`, and
`GachaManager` retain their names where Unity scenes already serialize them.
New rules must go into the Model or Presenter listed above rather than growing
these compatibility facades again.

## General rules

- Prefer `[SerializeField] private` plus a read-only property over a mutable
  public field.
- Use names that describe domain intent; avoid generic `Manager` names for new
  classes.
- A class should have one reason to change.
- Calculation services must not update visuals.
- Do not store mutable per-character state in a ScriptableObject definition.
- Subscribe and unsubscribe events symmetrically.
- Prefer explicit dependencies over global singletons. Existing persistent
  singletons are migration boundaries and must not spread into new code.
- Add Edit Mode tests for extracted Models, Presenters, and calculation
  services before extending their rules.

