# Handoff: Geometry Grammar Validator — TDD Session

> **Session type:** `/tdd` — write EditMode tests that enforce the geometry construction grammar, then write the validator that makes them pass.
> **Branch:** create `feat/geometry-grammar-validator` from `main`
> **Priority:** Tests are the deliverable. The validator is the implementation that makes them pass.

## Context

DESYNC uses modular graybox geometry (Unity cube primitives) for its house scenes. We've codified construction rules in `docs/handoff-prompts/current/GEOMETRY_GRAMMAR.md` to prevent coplanar-face artifacts (light leaks, shadow banding from IEEE 754 float imprecision).

The existing scene `House_Graybox.unity` has **known violations** of these rules — this is intentional. The tests should FAIL against the current scene until a separate fix session corrects the geometry. Write the tests to be correct per the grammar; don't weaken them to match current scene state.

### Existing test infrastructure
- Assembly: `Desync.Tests.EditMode` at `unity-DESYNC/Assets/_Project/Tests/EditMode/`
- Existing tests: `NetworkBootstrapConsistencyTests.cs` (regression for NGO connection approval)
- Existing geometry tests: `HouseGrayboxGeometryTests.cs` (basic bounds and inset checks — see `docs/TODO.md` "Nascent — Geometry Test Maturity" for known limitations)
- asmdef: `Desync.Tests.EditMode.asmdef`

### Key docs to read first
1. `docs/handoff-prompts/current/GEOMETRY_GRAMMAR.md` — the grammar rules (source of truth for what to test)
2. `docs/ARCH.md` — "URP lighting: modular graybox floor/ceiling construction" section for historical fix context
3. `docs/TODO.md` — "Nascent — Geometry Test Maturity" section for known test fragility issues

## What to Build

### Test file: `GeometryGrammarValidatorTests.cs`

EditMode tests in `Desync.Tests.EditMode` that validate the geometry grammar rules against a loaded scene. Each rule from the grammar maps to one or more test methods.

### Tests to write (map to grammar rules):

**R1 — Horizontal Separators:**
- `R1_1_AllHorizontalSeparators_HaveMinimumThickness` — every floor/ceiling/roof piece has Y-scale >= 0.1m
- `R1_2_HorizontalSeparators_ExtendAboveWallTops` — separator top face is >= 0.05m above the top of any wall it contacts
- `R1_3_HorizontalSeparators_AreInsetFromExteriorWalls` — separator XZ edges are >= 0.05m inside exterior wall inner faces
- `R1_4_InterFloorSeparators_Overlap` — where a floor sits above a ceiling, floor bottom <= ceiling top (overlap, not gap)
- `R1_6_HorizontalSeparators_HaveTwoSidedShadows` — all separator MeshRenderers have shadowCastingMode == TwoSided

**R3 — Internal Wall T-Junctions:**
- `R3_1_InternalWalls_TrimmedInsideExteriorWalls` — internal wall exterior-facing endpoints are >= 0.05m inside the exterior wall inner face (never at the outer face)

**R4 — Railings:**
- `R4_1_Railings_ExtendIntoFloorSlabs` — railing bottoms are >= 0.05m below floor slab top face
- `R4_2_Railings_TrimmedAtExteriorWalls` — railing ends touching exterior walls are >= 0.05m inside the inner face (per R3.1); ends touching internal walls extend >= 0.05m into wall volume

**R5 — General:**
- `R5_1_NoGeometry_ExtendsBeyondBuildingEnvelope` — all internal geometry within exterior wall outer faces

### Implementation: `GeometryGrammarValidator.cs`

A runtime-usable utility class (NOT test-only) at `unity-DESYNC/Assets/_Project/Scripts/Debug/GeometryGrammarValidator.cs` in namespace `Desync.Debug`. This is important — the house graph runtime will generate rooms dynamically, and we'll need to validate generated geometry at runtime too.

**Public API sketch:**
```csharp
namespace Desync.Debug
{
    public static class GeometryGrammarValidator
    {
        public struct Violation
        {
            public string Rule;        // e.g. "R1.2"
            public string Description; // human-readable
            public GameObject Offender;
            public GameObject Host;    // the piece it should overlap with (nullable)
        }

        // Validate all geometry under a root transform
        public static List<Violation> ValidateHierarchy(Transform root);
        
        // Validate a single piece against its neighbors
        public static List<Violation> ValidatePiece(Transform piece, Transform root);
    }
}
```

The tests call `ValidateHierarchy` on the loaded scene root and assert zero violations per rule category.

### Piece classification

The validator needs to classify pieces into: horizontal separator, exterior wall, internal wall, railing. Options for classification (in order of preference):
1. **Tags** — `HorizontalSeparator`, `ExteriorWall`, `InternalWall`, `Railing` (cleanest, requires tagging existing objects)
2. **Naming convention** — parse prefixes like `GF_Floor`, `SF_Ceiling`, `Railing_` (fragile but works for current scene)
3. **Heuristic** — infer from dimensions/position (the existing `HouseGrayboxGeometryTests` does this; see TODO.md caveats)

Recommend **option 1 (tags)** for the validator, with a helper that auto-tags based on naming convention for the existing scene. The tests should tag objects before validating.

### Constants

```csharp
public const float MinSeparatorThickness = 0.1f;
public const float SafetyOverlap = 0.05f;
public const float FloatTolerance = 0.001f; // for float comparison
```

## Architectural Constraints

- **Namespace:** `Desync.Debug` for validator, `Desync.Tests.EditMode` for tests
- **No runtime dependencies** from the validator on test assemblies
- **~50 LoC per function** — decompose the validation into small, focused methods
- **Deep module pattern** — small public API (`ValidateHierarchy`, `ValidatePiece`), complex internals
- **Do NOT modify the scene** — tests are read-only validation
- **Do NOT weaken tests to match current scene state** — the scene has known violations; tests should flag them

## Known Issues to Navigate

From `docs/TODO.md` "Nascent — Geometry Test Maturity":
- `GameObject.Find` name collisions risk — use hierarchy traversal, not `Find`
- Square wall panel misclassification — handle in classification logic
- Single-sided wall unbounded interior — not your problem (existing test limitation)
- Hardcoded minInset — your `SafetyOverlap` constant replaces this

## Done Definition

- [ ] All test methods listed above exist and compile
- [ ] Tests correctly FAIL against current `House_Graybox.unity` (known violations exist)
- [ ] `GeometryGrammarValidator` class exists at the specified path with the public API
- [ ] Validator correctly identifies the known violations (run tests, check output)
- [ ] No new warnings or errors in Unity console after compilation
- [ ] All code follows project conventions (CLAUDE.md architecture rules)
- [ ] Committed on `feat/geometry-grammar-validator` branch
