# Onshape migration plan — antweight reference platform

Migrate `antweight_reference_platform/spinbot_ant.scad` from OpenSCAD into Onshape as a true parametric model.

> **Status (2026-05-26):** v1 functionally complete for the detachable-weapon build.
> - Variable Studio populated (33 base + 5 derived).
> - `lid` Part Studio **BUILT** (11 features, volume within 0.4% of SCAD prediction).
> - `shell_no_weapon` Part Studio **BUILT** (23 features incl. battery wall; volume within 0.03% of summed SCAD predictions).
> - `weapon` Part Studio **BUILT** (6 features). Caveat: 2 disconnected bodies in v1, top connecting bridge missing — see bead `hamachi-6q9`.
> - `weapon_pin` Part Studio **BUILT** (2 features).
> - `shell_with_weapon` — deferred (`hamachi-v5c`); detachable-weapon build via weapon+weapon_pin is the v1 path.
> - Arduino shelf in shell — deferred (`hamachi-xy5`); SCAD's compound rotation needs FeatureScript or 3-point plane workaround.
>
> The "Lessons learned (v1)" section near the bottom captures what didn't survive contact with the CAD kernel. Open handoff items in `bd list` filtered by "hamachi-".

## Why migrate

OpenSCAD has carried the antweight reference platform fine, but three near-term needs justify the move:

1. **Fastener change** — current `lidScrewHoleDiameter = 2.2 mm` is sized for #2-28 thread-forming
   self-tap screws (the imperial spec called out in the upstream readme). We want to swap to **M3**, which
   needs ~2.5 mm pilot for self-tap into PPA-CF or PLA. Re-running OpenSCAD for that is trivial; the
   problem is the OpenSCAD module is **misnamed** `makeM3TapHole` already, despite the 2.2 mm value never
   having been M3-sized. The name is load-bearing in ways grep doesn't show — clearing this up in a fresh
   parametric model is cleaner than carrying the bad name forward.
2. **Wall-thickness adjustment** — the upstream `lidThickness = 1` is sparse. Want to thicken the lid
   (and possibly localized regions of the shell) without rebuilding the lid module from scratch.
3. **Future warp mitigation** — fillets, ribs, and selective thickening are easier to add in a feature-tree
   CAD system than in CSG. PPA-CF wants generous radii and we'll iterate on those after the 6/6 build.

The migration also unlocks Onshape's downstream conveniences: in-browser sharing, version history with diffs,
STEP export for FEA, drawings with dimensions if we ever need them.

## What stays in OpenSCAD

`antweight_reference_platform/spinbot_ant.scad` is **not deleted**. It stays as the historical reference
and as a sanity check — once the Onshape model is built, exported STLs from both sources should agree on
external dimensions to within Onshape's tolerance (~1e-5 mm). After that the OpenSCAD file is frozen.

## Source-of-truth strategy

- **Onshape model** — sole source of truth for printable geometry once migration completes.
- **OpenSCAD file** — frozen reference; archived but not deleted.
- **STL files in `antweight_reference_platform/`** — current 6/6 build prints from the OpenSCAD model.
  After Onshape parity is verified, regenerate these from Onshape and commit alongside a short note in
  the build log.

## Document & element layout

Single Onshape document named `hamachi` (created 2026-05-25; `documentId = 3662b787ba73917e77c5a543`, `workspaceId = 64e642b76ea639b57af58924`). Inside:

| Element | Type | Status | elementId | Purpose |
|---|---|---|---|---|
| `variables` | Variable Studio | **populated** | `8ad42fc0569446006c975dba` | All shared parameters (see [variable mapping](#variable-mapping)) |
| `lid` | Part Studio | **built v1** | `4184aeece3d6787270371115` | Top lid with LED/power holes and motor clearance cut |
| `shell_no_weapon` | Part Studio | **built v1** | `9e9326c01379985fe1982ee6` | Main shell with weapon-mount hole. Includes battery wall; Arduino shelf deferred (`hamachi-xy5`) |
| `weapon` | Part Studio | **built v1** | `109b30fd7fdba9ecc8543a6e` | Detachable weapon. 2 disconnected bodies in v1; top bridge fix in `hamachi-6q9` |
| `weapon_pin` | Part Studio | **built v1** | `d8b341a8f7fcb9bbee7d33a3` | Press-fit pin for detachable weapon |
| `shell_with_weapon` | Part Studio | deferred | — | Integrated-weapon variant. Tracked in `hamachi-v5c` |
| `fit_check` | Assembly | planned | — | Holds shell + lid + placeholder electronics for visual verification (low priority) |
| `Part Studio 1` | Part Studio | empty default | `064c70496e243190d7179534` | To delete (bead `hamachi-8jo`) |

Rationale for separate Part Studios per printed part: clean STL export per part, isolates feature-tree
churn on one part from breaking another, and matches the Onshape convention of one printable per Part
Studio. The trade-off — having to re-reference shared geometry like the lid-screw bolt circle — is
managed by pinning those positions to variables in the Variable Studio, not by cross-element references.

The dummy electronics, motor, and wheel modules in the SCAD file are visualization aids only — they don't
contribute to printed geometry. In Onshape they live (eventually) in the `fit_check` assembly as
placeholder parts, but they are **out of scope for migration v1**.

## Variable mapping

OpenSCAD parameters map cleanly to Onshape Variable Studio variables. All units are millimeters
(SCAD's implicit mm matches Onshape's default), all angles are degrees.

| OpenSCAD | Onshape variable | Default | Notes |
|---|---|---|---|
| `botHeight` | `#botHeight` | 33 mm | |
| `botDiameter` | `#botDiameter` | 138 mm | |
| *(new)* | `#botRadius` | `=#botDiameter / 2` | Derived; added during lid v1 build for downstream readability. Cross-variable derivation works in Variable Studio at write-time; downstream resolution via constraint VALUE not yet confirmed (use base `#botDiameter / 2` inline if uncertain) |
| `botShellThickness` | `#botShellThickness` | 5 mm | |
| `floorThickness` | `#floorThickness` | 3 mm | |
| `lidThickness` | `#lidThickness` | 1 mm | Candidate for thickening — leave default for parity, override in v2 |
| `motorDepthBelowFloorSurface` | `#motorDepthBelowFloor` | 5 mm | |
| `wheelDiameter` | `#wheelDiameter` | 44 mm | Reference-only (no printed wheel) |
| `wheelWidth` | `#wheelWidth` | 12.5 mm | Reference-only |
| `wheelDistanceFromMotorWall` | `#wheelClearance` | 8 mm | |
| `motorWallThickness` | `#motorWallThickness` | 3 mm | |
| `motorWallOffset` | `#motorWallOffset` | 28 mm | |
| `motorDiameter` | `#motorDiameter` | 37 mm | Already includes 1 mm clearance per SCAD comment |
| `motorBushingDiameter` | `#motorBushingDiameter` | 13 mm | |
| `motorBushingHeight` | `#motorBushingHeight` | 2 mm | |
| `motorLength` | `#motorLength` | 51 mm | Already includes 1 mm clearance per SCAD comment |
| `motorMountHeight` | `#motorMountHeight` | 7 mm | |
| `motorMountLength` | `=#motorLength + 4 mm` | — | Derived |
| `motorMountWidth` | `=#motorDiameter + 4 mm` | — | Derived |
| `motorMountOffset` | `#motorMountOffset` | -2 mm | |
| `motorMountHoleOffset` | `#zipTieHoleOffset` | 6 mm | Renamed: these are zip-tie holes, not motor mount holes |
| `motorMountHoleDiameter` | `#zipTieHoleDiameter` | 5.25 mm | Renamed (same reason) |
| `weaponMountHoleDiameter` | `#weaponMountHoleDiameter` | 12 mm | |
| `weaponPinExtraLength` | `#weaponPinExtraLength` | 5 mm | |
| `weaponPinDiameterReduce` | `#weaponPinKerf` | 1 mm | |
| `weaponThickNess` | `#weaponThickness` | 19 mm | Typo fixed |
| `weaponWidth` | `#weaponWidth` | 40 mm | |
| `weaponInset` | `#weaponInset` | 2 mm | |
| `lidMotorCutWidthAdjustment` | `#lidMotorClearance` | 1.5 mm | |
| `lidScrewHoleDiameter` | `#lidScrewPilotDiameter` | 2.5 mm | **Changed default** — 2.5 mm for M3 self-tap (was 2.2 mm for #2-28) |
| `lidScrewHoleDepth` | `#lidScrewHoleDepth` | 10 mm | |
| `ledAndPowerHoleDiameter` | `#ledHoleDiameter` | 5 mm | **Changed default** — 5 mm to match the LED actually being used. **SCAD bug discovered during migration:** the variable is named `Diameter` but `makeLedAndPowerHoles()` passes it positionally to `cylinder(h, r1, r2)`, so SCAD actually cuts **8 mm** holes (the 4 mm value is used as radius). Onshape v1 uses 5 mm intentionally. |
| `powerHoleOffsetAngle` | `#powerHoleAngle` | 140° | |
| `ledHoleOffsetAngle` | `#ledHoleAngle` | -24° | |
| `motorWallWidth` | derived expression | — | `2 * sqrt((#botDiameter/2)^2 - (#motorWallOffset + #motorWallThickness)^2) - #botShellThickness` |

Booleans like `weaponPartOfBody`, `renderLid`, `makeTapHoles` don't translate — they were SCAD's way of
suppressing geometry per-render. In Onshape, suppression is per-feature (or per-Part-Studio); we'll just
have separate Part Studios for `shell_no_weapon` and `shell_with_weapon` and not worry about a flag.

## Coordinate system

Same as the SCAD model:

- Origin: center of bot, at the bottom outer face of the shell (i.e. the floor surface that touches the
  arena).
- `+Z` up (matches Onshape's Top plane normal).
- `+Y` away from the wheel side. SCAD positions the wheel at `-Y` (via `motorMountOffset`); keep that.
- `+X` is the long axis of the weapon slot (SCAD's `weapon()` is centered on the `-Y` face).

The main shell base circle is sketched on the **Top plane**. Extrude `+Z` for `#botHeight`.

## Feature-tree mapping (per Part Studio)

These are the planned feature lists. Each item maps to one Onshape feature; references like *sketch on
top of shell* assume `list_entities` has been used to get the relevant face deterministic ID.

### `shell_no_weapon`

1. Sketch on Top plane: circle ⌀ `#botDiameter` → **Extrude** `+Z` `#botHeight` (NEW body)
2. Sketch on Top plane offset `#floorThickness` (or face of top body): circle ⌀ `(#botDiameter - 2*#botShellThickness)` → **Extrude** `+Z` `(#botHeight - #floorThickness + 1mm)` (REMOVE) — hollows the shell, leaves the floor
3. **Wheel-well cut** — sketch a rectangle on Top plane spanning the `-Y` half: extrude through, REMOVE. SCAD does this with an intersection-then-subtract; the simpler Onshape primitive is a single REMOVE of a rectangular prism limited to the inner cavity.
4. **Motor wall** — sketch a rectangle on Front plane (or an offset plane perpendicular to `+Y`) centered at `y = -#motorWallOffset`, width `motorWallWidth` (derived), height `#botHeight`, then extrude `#motorWallThickness` ADD. Cut the bushing relief through-hole.
5. **Motor mount** — sketch on the floor (top face of the floor body), rectangle `#motorMountWidth × #motorMountLength` positioned at `y = #motorMountOffset + #motorMountLength/2`, extrude `#motorMountHeight` ADD. Then REMOVE the motor cylinder (two-step: bushing + body).
6. **Zip-tie holes** — six holes through the motor mount. Sketch six circles on the top face of the motor mount at the SCAD-defined offsets, extrude REMOVE downward through the floor.
7. **Lid tap holes** — eight tap holes on the top face. Six around the bolt circle (every 60° from 0° to 200°, per the SCAD loop), two over the motor wall. Sketch on the top annular face, extrude REMOVE downward `#lidScrewHoleDepth`.
8. **Weapon mount hole** — through-hole on the `-Y` face: sketch a circle on Right plane (or an offset plane) at the SCAD-defined Z, extrude REMOVE through-all.
9. **Arduino shelf** — at SCAD's hardcoded `[46,19,15]` with `[-21,0,175]` rotation. This is the one feature where the SCAD geometry is unapologetically hardcoded; the Onshape version should parameterize the shelf angle (`#mcuShelfAngle = 21°`), position, and dimensions explicitly in the Variable Studio. **Out of scope for v1** — replicate SCAD's hardcoded values literally first; parameterize after parity.
10. **Battery wall** — same story: SCAD hardcodes `translate([-33,58,floorThickness + 3]) rotate([90,0,0]) cube([66, botHeight - 8, 3])`. Replicate literally for v1, parameterize later if needed.

### `shell_with_weapon`

Derived from `shell_no_weapon` minus the weapon mount hole, plus the weapon body fused as a single solid.
Two options:

- **Option A:** Copy the `shell_no_weapon` feature tree, delete the weapon-mount-hole REMOVE, and add the weapon as an ADD extrude positioned and clipped per the SCAD `weapon()` module.
- **Option B:** Reference `shell_no_weapon` via a Derived feature, then suppress/unsuppress features. This is cleaner but couples the two Part Studios — changes to the base shell propagate.

**Recommend Option A** for v1 to keep the Part Studios independent. Revisit after parity if maintenance becomes painful.

### `lid` (BUILT v1 — 11 features)

As actually built, not the original plan. Differences from the plan called out inline.

1. `lid_outer_profile` sketch (Top plane): circle ⌀ `#botDiameter` (constraint-first; DIAMETER value="#botDiameter")
2. `lid_body` extrude `variableDepth=lidThickness` → NEW body, 138×138×1 mm
3. `lid_screw_holes` sketch (Top plane): six circles at SCAD-derived seed positions, all DIAMETER `#lidScrewPilotDiameter`. **Diverges from plan** — original plan was "clearance hole" (3.2 mm M3 clearance) for the lid; v1 uses pilot diameter for SCAD parity. Clearance-hole upgrade is deferred to v2 (no bead yet — file when needed).
4. `lid_screw_holes_cut` extrude REMOVE `variableDepth=lidThickness` → 6 through-holes
5. `lid_led_power_holes` sketch (Top plane): two circles at radius-60 positions matching SCAD's `((botDiameter / 2) - (botShellThickness + ledAndPowerHoleDiameter))` formula; DIAMETER `#ledHoleDiameter` = 5 mm. SCAD computes the same X/Y center positions but cuts 8 mm wide (see SCAD bug note in [variable mapping](#variable-mapping)).
6. `lid_led_power_holes_cut` extrude REMOVE through
7. `lid_wheel_well` sketch (Top plane): arc + closing line forming a D-shape at radius 64 (= `#botDiameter / 2 - #botShellThickness`), chord at y = -31. **Diverges from plan** — original plan said "rectangle on Top plane spanning the -Y half"; this misread SCAD. SCAD's `wheelWellCut()` is intersected with the *inner* cylinder (radius 64), which leaves a 5 mm rim of lid material at radius 64..69 on the -Y edge. The D-shape replicates that.
8. `lid_wheel_well_cut` extrude REMOVE through
9. `lid_motor_xs` sketch (Front plane): circle at sketch-(0, -15) ⌀ `#motorDiameter` = 37 mm. The Z = -15 is the motor axis Z in lid-local frame, derived from SCAD's chain of translates.
10. `lid_motor_cut_negY` extrude REMOVE 28 mm in default direction (-Y) — sweeps the motor cylinder from Y=0 to Y=-28
11. `lid_motor_cut_posY` extrude REMOVE 23 mm with `oppositeDirection=true` (+Y) — sweeps Y=0 to Y=+23

**Why two extrudes for the motor cut:** the motor cylinder spans Y=-28 to Y=+23 in the lid frame (asymmetric about Y=0 by 2.5 mm). The cleanest approach would be a SYMMETRIC extrude from a Y=-2.5 offset plane, but `create_offset_plane` returns a `cPlane` feature_id that fails as `faceId` on the subsequent sketch (`SKETCH_NO_PLANE`). Workaround: sketch on Front plane (Y=0) and do two BLIND REMOVE extrudes in opposite directions. Tracked in bead `hamachi-fbm`.

### `weapon`

1. Sketch a rectangle `#weaponWidth × #weaponThickness` on the appropriate plane, extrude `(#botHeight + weaponTopVerticalExtension + weaponBottomVerticalExtension)`.
2. Subtract two offset copies of the main shell cylinder to carve the inset (per SCAD's `weapon()` module).
3. Subtract weapon-mount-hole.

### `weapon_pin`

Single cylinder: ⌀ `(#weaponMountHoleDiameter - #weaponPinKerf)`, length `(#weaponThickness + #weaponPinExtraLength)`. Trivial.

## Naming fixes

These get cleaned up during migration; no need to track them as separate tasks:

- `makeM3TapHole` → drop the misnamed module; use the variable name `#lidScrewPilotDiameter` (which is honest about being a pilot hole, agnostic about screw thread).
- `motorMountHole*` → renamed to `zipTieHole*` (these are zip-tie holes, not motor mounting holes).
- `weaponThickNess` → `#weaponThickness` (typo).
- `weaponPinDiameterReduce` → `#weaponPinKerf` (clearer intent).

## Validation

After each Part Studio is built, run two checks:

1. **Geometry diff vs SCAD STL** — export STL from Onshape at matching tessellation tolerance, compare to the upstream STL in `antweight_reference_platform/`. Tools: Meshlab, `gmsh`, or `numpy-stl` script. Acceptance: external dimensions agree within 0.1 mm; topology counts (faces/edges/vertices) need not match (CSG vs feature-tree produce different meshes), but the **bounding box and mass** should be within 1%.
2. **Mass properties cross-check** — `mcp__plugin_jarvis-onshape-mcp_onshape__get_mass_properties` against SCAD volume computed from the STL. Expected agreement: within 1% (CSG tessellation introduces minor differences).

## Iteration sequencing

1. `variables` Variable Studio — empty doc with all variables defined.
2. `lid` Part Studio — simplest, validates the variable-reference pattern. Confirms parity on a small part before tackling the shell.
3. `shell_no_weapon` Part Studio — bulk of the geometry. Defer the Arduino shelf and battery wall (steps 9–10) until everything else passes parity.
4. `shell_with_weapon` Part Studio — Option A copy-and-extend.
5. `weapon` + `weapon_pin` Part Studios — trivial, do them together.
6. Arduino shelf + battery wall — parameterize and finish v1.

Each step ends with a `describe_part_studio` verification render and a geometry-diff check against the
corresponding STL.

## Out of scope for v1

- `fit_check` assembly with electronics/motor/wheel placeholders.
- Warp-mitigation features (fillets, ribs, selective thickening).
- M2 ↔ M3 ↔ #2-28 screw-family switching as a parameter; v1 ships with one default (M3) and the variable can be edited manually for builds that need a different screw.
- Replacing hardcoded Arduino shelf and battery wall positions with fully parameterized layouts.

Each of these becomes a follow-up task once v1 is done and parity is confirmed.

## Open questions

- ~~Does the Onshape Variable Studio support derived expressions that reference other variables in the same studio?~~ **Partial answer (lid v1):** write-time accepted (`motorMountLength = #motorLength + 4 mm` and `botRadius = #botDiameter / 2` both stored). Base variables resolve at use-time in constraint VALUE fields (DIAMETER, RADIUS, DISTANCE) including inline arithmetic over multiple base vars, and in `variableDepth` on extrudes. The `variableRadius` arg on sketch circles failed with both base and derived vars (`SKETCH_DIMENSION_MISSING_PARAMETER`) — unclear whether that's a `variableRadius` bug or a derived-var resolution bug; constraint-first sketches with explicit DIAMETER constraint values are the proven path. Tracked in bead `hamachi-2cf`.
- Is there a clean way to keep the bolt-circle position synced between `shell_no_weapon`, `shell_with_weapon`, and `lid` without resorting to cross-element Derived references? **Still open** — lid v1 used seed-only positions, so no cross-element coupling was needed yet. Will become a real question when v2 makes hole positions parametric (bead `hamachi-9qx`). Tracked in `hamachi-p4k`.
- For `weapon_pin`, do we want the press-fit kerf as a separate variable from the shell-screw kerf? Tracked in `hamachi-p4k`.

## Lessons learned (v1)

Surfaced during the lid build; documenting here so the shell build doesn't re-burn the same turns.

### Sketch patterns that work
- **Constraint-first sketches with `DIAMETER value="#var"` or `RADIUS value="..."`** — the proven path for parametric circles. Inline arithmetic over multiple base variables works in constraint values (e.g. `value="#botDiameter / 2 - #botShellThickness"`).
- **`variableDepth` on `create_extrude`** — works with base variable names. No `#` prefix in this arg.
- **Multiple BLIND REMOVE extrudes from one sketch in opposite directions** — pass `oppositeDirection=true` on the second one. Cleaner than fighting with offset planes.

### Sketch patterns that broke (or have gotchas)
- **`variableRadius` arg on sketch circles** — failed with `SKETCH_DIMENSION_MISSING_PARAMETER` for both base and derived variables. Use constraint-first DIAMETER instead.
- **`DISTANCE` constraint sign behavior** — A positive `value` does NOT preserve seed sign. A circle seeded at `(+66.5, 0)` with `DISTANCE direction=HORIZONTAL value=66.5` between its center and origin resolved to `(-66.5, 0)`. Workaround: drop the DISTANCE constraint and rely on seed-only position when sign matters. Investigate properly before any feature where exact position is critical (v2 will need to revisit).
- **`create_circular_pattern` on a REMOVE-only feature** — fails with `REGEN_ERROR (ERROR)`. The patterned feature seems to need to create new bodies, which a REMOVE extrude doesn't. Workaround: replicate all instance positions as separate entities in one sketch + one extrude.
- **`create_offset_plane` feature_id as `faceId` on a sketch** — fails with `SKETCH_NO_PLANE (ERROR)` both with and without the `_1` suffix. Workaround: sketch on standard planes and use opposite-direction extrudes for asymmetric ranges. Tracked in `hamachi-fbm`.

### SCAD bugs discovered during translation
- **`ledAndPowerHoleDiameter`** — named "Diameter" but used as RADIUS in `cylinder(h, r1, r2)` positional call. SCAD actually cuts 8 mm holes, not 4 mm. Onshape v1 uses 5 mm (the correct value for the LED in use).
- **`makeM3TapHole`** — misnamed module. The 2.2 mm value is sized for #2-28 imperial self-tap, not M3.
- **`motorMountHoleDiameter` / `motorMountHoleOffset`** — these are zip-tie hole params, not motor mount params. Renamed `zipTieHole*` in Onshape.

### Deliberate divergences from SCAD STL (v1)
These will show up in any STL diff vs `antweight_reference_platform/*.stl`:
- All screw pilots use 2.5 mm (M3) instead of SCAD's 2.2 mm (#2-28).
- LED/power holes use 5 mm instead of SCAD's accidental 8 mm.
- Hole positions are seed-only (not parametric); position values still match SCAD numerically.

Bounding box and mass should still agree within 1% (the holes are tiny relative to total volume).

## Onshape document IDs (for agent sessions)

```
documentId       = 3662b787ba73917e77c5a543
workspaceId      = 64e642b76ea639b57af58924

variables        (Variable Studio) = 8ad42fc0569446006c975dba
lid              (Part Studio)     = 4184aeece3d6787270371115
shell_no_weapon  (Part Studio)     = 9e9326c01379985fe1982ee6
weapon           (Part Studio)     = 109b30fd7fdba9ecc8543a6e
weapon_pin       (Part Studio)     = d8b341a8f7fcb9bbee7d33a3
Part Studio 1    (empty default)   = 064c70496e243190d7179534    # to delete (hamachi-8jo)
```

These won't change unless the doc is recreated. Drop them straight into tool args to skip a `get_document` / `get_elements` lookup at the start of each session.
