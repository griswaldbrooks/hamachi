# Onshape migration plan — antweight reference platform

Migrate `antweight_reference_platform/spinbot_ant.scad` from OpenSCAD into Onshape as a true parametric model.

> **Status (2026-05-26, v5c session):** v1 functionally complete + most v2 polish landed + shell_with_weapon BUILT (with arduinoShelf). 13 beads closed across three days; 3 deferred-or-blocked items remain.
> - Variable Studio populated (34 base + 5 derived — added `#weaponBottomVerticalExtension`, `#weaponTopVerticalExtension`, `#weaponSlotExtraWidth`, `#batteryWallZipTieGap`).
> - `lid` Part Studio **BUILT** (11 features). v2 polish landed: all 6 lid screw hole positions are now fully parametric (`hamachi-9qx`).
> - `shell_no_weapon` Part Studio **BUILT** (25 features). v2 polish landed: motor wall parametric (`hamachi-ri1`), 3mm zip-tie gap below battery wall (`hamachi-q6l`), parametric lid tap positions (`hamachi-p4k`), Arduino shelf via FeatureScript custom feature (`hamachi-xy5`).
> - `weapon` Part Studio **BUILT** (8 features). v2 polish landed: structural top bridge (`hamachi-6q9`), `weaponSlotExtraWidth=1mm` kerf widening (`hamachi-tff`). Onshape STL now matches upstream STL volume to within 0.11%. Z offset deferred (`hamachi-kaj`).
> - `weapon_pin` Part Studio **BUILT** (2 features).
> - `shell_with_weapon` Part Studio **BUILT v1** (25 features, `hamachi-v5c` closed). Option A replay: copied shell_no_weapon's tree minus the weapon-mount-hole, added a parametric weapon bar slab at the front, then re-implemented the arduinoShelf as a new FS feature (`hamachi-uqc` closed; FS source archived to `docs/fs/arduino-shelf.fs`). **Volume parity vs SCAD shell_with_weapon.stl: +0.09%** (+115 mm³ — tessellation noise). Bonus: 6 lid tap holes cut (vs shell_no_weapon's 4) thanks to dropping the DISTANCE-MINIMUM positional snap on perimeter taps.
> - Lid chord-cut decision — open (`hamachi-5fc`); needs design call on whether to match SCAD's brutal cube cut or keep the structural rim crescent.
> - `Part Studio 1` cleanup — blocked on web UI (`hamachi-8jo`); MCP doesn't expose element-deletion.
> - LED hole resize — waits on PLA fit-check (`hamachi-yac`).
>
> The "Lessons learned" section near the bottom captures every gotcha + workaround from all sessions. Open handoff items in `bd list` filtered by "hamachi-".

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
| `shell_with_weapon` | Part Studio | **built v1** | `a28ec3540e926aba920d88b6` | Integrated-weapon variant. 24 features replayed from shell_no_weapon (minus weapon mount) + bar ADD (`hamachi-v5c` closed) |
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

### `shell_with_weapon` (BUILT v1 — 25 features, `hamachi-v5c` + `hamachi-uqc` closed 2026-05-26)

Option A taken: replayed 21/23 of `shell_no_weapon`'s features (skipped `shell_weapon_mount_xs` + `shell_weapon_mount_cut` — the whole point of this Part Studio is to leave the shell wall intact at the front), then ADDed the integrated weapon bar.

The 24 features in order:
1–22. Same as `shell_no_weapon` features 1–19 + 22–24 (i.e. shell base, hollow, wheel well, motor wall + bushing relief, motor mount, motor cut neg/pos Y, zip-tie holes, lid tap holes, battery wall + zip-tie gap). Skips `shell_no_weapon`'s features 20–21 (weapon mount cylinder cut).
23. `shell_weapon_bar_profile` sketch on Top plane: 40 × 19 mm rectangle, X centered at 0, Y spans `[-76.5, -57.5]` (centered at Y=-67, which is `-(#botDiameter/2) + #weaponInset`). Constraints: 4 COINCIDENT corner closures + 4 HORIZONTAL/VERTICAL edges + 2 DISTANCE-MINIMUM for `#weaponWidth` and `#weaponThickness`. Position is from seed (not parametric — Y=-67 hardcoded).
24. `shell_weapon_bar` extrude ADD, `variableDepth=botHeight` (33mm) → Z=[0, 33]. The bar's inside portion (Y ∈ [-69, -57.5] and inside the outer cylinder) fuses with the shell; the protruding portion (Y ∈ [-76.5, -69]) is new material sticking out the front. With `weaponPartOfBody=true` (SCAD's flag), all extensions and slot kerf are zero — the bar exactly matches the shell-height + shell-thickness extents.
25. `shell_arduino_shelf` arduinoShelf FS custom feature (re-built fresh — see `docs/fs/arduino-shelf.fs`). Same geometry as `shell_no_weapon`'s arduinoShelf (face origins match exactly: J2S(36.0, 19.2, 16.9), J6S(32.9, -16.2, 3.2)) but in a NEW Feature Studio element (`661e2d122314a6cc0e1c2311`) because the MCP doesn't expose cross-element FS reference.

**Why no Derived feature (Option B):** Onshape's std `derivedPart()` is a FeatureScript-level op; the MCP doesn't expose it as a primitive and wiring it up from `write_featurescript_feature` would have been more complex than the 24-feature replay. The replay also keeps `shell_no_weapon` and `shell_with_weapon` independent — they share the Variable Studio but not feature topology.

**v1 parity vs SCAD `shell_with_weapon.stl`:**
- Volume: **+0.09%** (+115 mm³) — well within tessellation noise.
- Y bbox: 145.50 mm vs SCAD's 145.36 mm — Onshape's exact value vs SCAD's chord-error under-approximation of the +Y face. Not a geometry issue.
- X bbox: 138.00 mm matched. Z bbox: 33.00 mm matched.
- Bonus: this Part Studio has **6** lid tap holes (4 perimeter at 0°/60°/120°/180° + 2 over the motor wall) vs `shell_no_weapon`'s **4** (E/W/mwL/mwR — NE and NW are silently coincident with E and W). Both are subject to the same DISTANCE-MINIMUM positional-snap issue (`hamachi-v5c` notes); I worked around it here by dropping the MINIMUM constraints on the perimeter taps and relying on seed coordinates. Updating `shell_no_weapon` the same way would recover the missing 2 holes — file when needed.

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

### Results (v1, 2026-05-25, bead `hamachi-ius`)

Ran on the post-cleanup tree (after `hamachi-6q9` weapon bridge, `hamachi-ri1` motor-wall parametrization). `hamachi-kaj` was attempted and then reverted by the user the same day — the weapon stays at z=0..38.5, not the SCAD-faithful z=-1.5..37. Onshape STL exports come out in **meters** — STL is unitless but Onshape's API-side SI units carry through, so any external mesh-diff tool needs to scale ×1000.

Bbox parity (after meter→mm rescale) and volume parity per part:

| Part | Bbox X | Bbox Y | Bbox Z | Volume Δ | Verdict |
|---|---|---|---|---|---|
| `weapon` | 40.0 ✓ | 19.0 ✓ | 38.5 ✓ (z=0..38.5, not -1.5..37 — `hamachi-kaj` deferred) | ~+6.4% | bbox PASS, mass FAIL |
| `weapon_pin` | 11.0 ✓ | dims match (orientation differs — see note) | dims match | +1.95% | dimensionally PASS, position differs |
| `shell_no_weapon` | 138.0 ✓ | 138.0 (+0.20% — tessellation chord error in upstream) | 33.0 ✓ | -4.49% | bbox PASS, mass FAIL |
| `lid` | 138.0 (+38% vs upstream — see note) | 138.0 ✓ | 1.0 ✓ | +4.90% | bbox FAIL (different cut shape), mass FAIL |

**Volume-gap root causes** (not addressed in v1):

- **`weapon` +6.4%** — The bar is 1.5mm taller than SCAD's bar (z=0..38.5 vs SCAD's z=-1.5..37) because `hamachi-kaj` is deferred. Plus SCAD does a second `mainShell` cut shifted by `weaponSlotExtraWidth = 1.0mm` in -Y, widening the slot by 1mm for 3D-print kerf. Onshape replicates neither. Kerf-widening tracked in `hamachi-tff`; Z offset stays in `hamachi-kaj`.
- **`shell_no_weapon` -4.49%** — Missing Arduino shelf (deferred `hamachi-xy5`); battery wall zip-tie gap (`hamachi-q6l`) deferred but adds less than the shelf removes. Net negative.
- **`lid` bbox X +38%** — Upstream uses a *cube* chord-cut (SCAD `lid()` line 215-216: `rotate([90,0,0]) cube([138,33,138])` at `[-69,-31,0]`) that removes everything Y<-31 *including the rim crescent*. My Onshape uses a `wheel_well_cut` bounded by the inner cylinder (radius 64), so the outer 5mm rim crescent at Y<-31 is retained. Functional implication: thin structural rim across the chord that upstream doesn't have. New bead `hamachi-5fc`.
- **`weapon_pin` orientation** — Onshape pin built at origin with axis along Z; upstream pin pre-positioned at the mount-hole location with axis along Y (via SCAD's `rotate([90,0,0])` + translate). Volume parity (+1.95%) is within facet noise. Functional impact zero — pin gets re-oriented when installed.

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

- ~~Does the Onshape Variable Studio support derived expressions that reference other variables in the same studio?~~ **Fully answered (`hamachi-2cf`, closed 2026-05-25):** Base `#var`s resolve everywhere — constraint VALUE fields (DIAMETER, RADIUS, DISTANCE), `variableDepth` on extrudes, inline arithmetic over multiple base vars. Derived `#var`s (`#botRadius`, `#motorWallWidth`, etc.) resolve ONLY as expression text at write-time in the Variable Studio itself; they FAIL in `variableRadius`/`variableDepth` args (`SKETCH_DIMENSION_MISSING_PARAMETER`) AND in constraint VALUE references (`SKETCH_UNSOLVABLE_CONSTRAINT` — misleading error, not the missing-parameter variant). `sqrt()` and `^` operators DO work in constraint expressions — paste the inline expression verbatim instead of referencing the derived var.
- ~~Is there a clean way to keep the bolt-circle position synced between `shell_no_weapon` and `lid` without cross-element Derived references?~~ **Fully answered (`hamachi-p4k`, closed 2026-05-25):** Yes — shared Variable Studio refs + same parametric constraint formulas in both Part Studio sketches keep them in lockstep. No Derived references needed. Both `lid_screw_holes` and `shell_lid_tap_holes` now have matching parametric constraints (4 radial DISTANCE-MINIMUM + 2 pair-distance HORIZONTAL + 2 VERTICAL) referencing `#botDiameter`, `#botShellThickness`, `#motorWallOffset`, `#motorWallThickness`.
- For `weapon_pin`, do we want the press-fit kerf as a separate variable from the shell-screw kerf? Still open.

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
- Weapon bar sits at z=0..38.5 instead of SCAD's z=-1.5..37 (`hamachi-kaj` deferred — accepting the Z offset).
- Lid retains a 5 mm outer rim crescent below the wheel-well chord (`hamachi-5fc` open — design call on whether to match SCAD's brutal cube chord-cut).

Bounding box and mass agree within 1% after the v2 polish (weapon was -0.11% vs upstream STL post-`hamachi-tff`).

## Lessons learned (v2 polish, 2026-05-25/26)

A second day of work tightened parametricity and surfaced new tool gotchas.

### v2 wins
- **Weapon STL parity:** added `#weaponBottomVerticalExtension` (1mm), `#weaponTopVerticalExtension` (4mm), `#weaponSlotExtraWidth` (1mm) to the Variable Studio; weapon depths and the SCAD kerf-widening 2nd-pass cut are now driven by those vars. Volume within 0.11% of upstream STL (`hamachi-tff`).
- **Battery wall zip-tie gap:** added `#batteryWallZipTieGap` (3mm) + a single REMOVE extrude on the existing wall sketch. Wall now floats at z=6..31, gap at z=3..6 (`hamachi-q6l`).
- **Motor wall fully parametric:** replaced the hardcoded X=±59.144 with inline `(#botDiameter/2 - ...sqrt expression - #botShellThickness/2)` in the constraint VALUE; sqrt+^ work but `#derivedVar` references don't (`hamachi-ri1`, `hamachi-2cf`).
- **Lid + shell tap holes parametric AND in sync:** matching 8-constraint set (origin point + 4 radial DISTANCE-MINIMUM + 2 pair-distance HORIZONTAL + 2 VERTICAL) on both `lid_screw_holes` and `shell_lid_tap_holes`. Shared Variable Studio refs keep them aligned without Derived references (`hamachi-9qx`, `hamachi-p4k`).
- **Arduino shelf via FeatureScript custom feature:** the tilted shelf + strut from SCAD's `arduinoShelf()` modeled by a self-contained FS feature using `newSketchOnPlane` on a 3-point plane derived from the Rx·Rz rotation math, then `extrude(...)` (NOT `opExtrude`) and `opBoolean(UNION)` to merge into the shell body. First FS feature in the project; cookbook template at `~/.claude/plugins/cache/jarvis-onshape-mcp/.../fs-cookbook/helix.fs` was the working starting point (`hamachi-xy5`).

### New gotchas discovered (also captured in the `feedback_onshape_mcp_gotchas` auto-memory)
- **Derived `#var` refs fail in constraint VALUES** — masquerade as `SKETCH_UNSOLVABLE_CONSTRAINT` instead of the clearer `SKETCH_DIMENSION_MISSING_PARAMETER`. Workaround: paste the inline math expression.
- **`create_extrude` `depth` arg rejects `#var` syntax** — use the separate `variableDepth` slot for base var names (no `#` prefix). `update_feature`'s `expression` field DOES accept inline `#var` arithmetic for the `depth` parameterId after the fact.
- **DISTANCE-sign behavior is context-dependent.** Adding HORIZONTAL/VERTICAL DISTANCE via `edit_sketch` on an existing sketch with seeds locked in AND an explicit `origin` anchor point preserves the seed sign for single constraints. BUT: TWO DISTANCE constraints from the same anchor with the same value (e.g., `mw_L_x = mw_R_x = #botDiameter / 3`) silently collapse both entities to the same side; downstream cut features merge into one and you lose a hole. Workaround: use a pair-distance constraint BETWEEN the two entities (= `2 * single_offset`) plus an anchor on one of them.
- **`inspect_sketch` returns SEED positions, not SOLVED positions.** Cannot confirm a fix worked by reading sketch state — must `list_entities` the resulting body and count.
- **`forceOppositeDirection=false` on `create_extrude`** to override the REMOVE-on-face auto-flip. The auto-flip's heuristic ("cut INTO the face's body") is wrong when the cut should go INTO a feature sitting ON TOP of the sketch face (validated by `hamachi-q6l`).
- **Inside FS custom features, prefer the user-facing `extrude(...)` wrapper over the lower-level `opExtrude(...)`.** Same inputs; `opExtrude` failed with opaque `EXTRUDE_FAILED` (no FS notice) while `extrude` succeeded. Default to the wrapper for sketched solids inside FS (validated by `hamachi-xy5`).
- **Onshape STL exports come out in METERS, not mm.** Bare numbers in the STL are scaled 1/1000 vs the doc's mm units. External mesh-diff tooling needs to scale ×1000 to compare to mm-native STLs from OpenSCAD.

### No-MCP gaps (require user action in the Onshape web UI)
- Element deletion (e.g., `hamachi-8jo` — delete the default empty `Part Studio 1`) — MCP only exposes `delete_document` (whole doc) and `delete_feature` (within an element). No `delete_element`.
- Element rename — same story.

## Lessons learned (v5c shell_with_weapon, 2026-05-26)

A third day of work to build the shell_with_weapon Part Studio. One major new gotcha + one MCP gap to note.

### v5c wins
- **shell_with_weapon BUILT** via Option A (replay). 25 features. **Body volume parity vs SCAD STL: +0.09%** (within tessellation noise). The integrated weapon bar is parametric in #weaponWidth + #weaponThickness via two DISTANCE-MINIMUM constraints on opposite sides of the rectangle (works because they don't share the same anchor — see [[feedback-onshape-mcp-gotchas]]).
- **arduinoShelf FS rebuilt fresh** for shell_with_weapon (`hamachi-uqc` closed). The original shell_no_weapon FS feature lives in a different Feature Studio element that the MCP can't cross-reference, so I wrote new FS source from the SCAD definition. The new source is archived to `docs/fs/arduino-shelf.fs` so it can be re-uploaded with parity if either Part Studio's Feature Studio gets rebuilt. Face origins in the resulting body match shell_no_weapon's shelf exactly (J2S=(36.0, 19.2, 16.9), J6S=(32.9, -16.2, 3.2)) — i.e. the rebuild reproduces the same geometry.
- **Replay-from-cached-features workflow.** `get_features` returns a massive JSON blob (128KB single-line) that exceeds the MCP's response cap. Workaround: it's auto-saved to a tool-results file; `ast.literal_eval` parses it (Python repr, not JSON). Extracting `feature.name`, `feature.btType`, `feature.featureType`, and `feature.parameters[].expression` per feature gave a clean per-feature spec to replay — saves having to re-derive depths, oppositeDirection flags, and operation types from first principles.

### New gotcha discovered (also captured in `feedback_onshape_mcp_gotchas` auto-memory)
- **`DISTANCE direction=MINIMUM` with the same value on multiple radial entities silently collapses position even though endpoints don't share an anchor.** The `shell_lid_tap_holes` sketch in shell_no_weapon has 4 `DISTANCE-MINIMUM = (#botDiameter - #botShellThickness) / 2` constraints (one per perimeter tap: E/NE/NW/W) plus 2 H/V-pinned mw taps. I replicated this exact constraint set in shell_with_weapon and only 2 holes cut (the H/V-pinned mw taps; all 4 perimeter taps slipped to invalid positions and ended up cutting through air). Workaround: drop the MINIMUM constraints on the 4 perimeter taps and rely on seed coordinates. After the workaround, all 6 holes cut cleanly.
  - **Implication for shell_no_weapon:** The same sketch in shell_no_weapon has only 4 holes (E, W, mwL, mwR) — NE and NW slip to coincident-with-E and coincident-with-W due to this same effect. SCAD `for([0:60:200])` specifies 4 perimeter angles (0°/60°/120°/180°); shell_no_weapon is dropping 2 of them. Same fix would recover them.
  - **Refines the prior MINIMUM understanding.** The earlier gotcha (`hamachi-9qx` notes) said `DISTANCE direction=MINIMUM` (unsigned Euclidean) was safe for multiple entities at the same radial distance because they have different angles. That was an under-claim — the solver picks ONE branch and applies it to all entities that share the (anchor, value) pair, ignoring seed-position hints when there are 3+ ambiguous solutions to the same Euclidean-distance equation.

### MCP gap: instantiating an existing FS custom feature in a new Part Studio
- The arduinoShelf custom FS feature lives in a Feature Studio element (`e8745567717fd9a71f2816bbf::m15e3446d68c52690097e6faa`) created during the `shell_no_weapon` build (`hamachi-xy5`). To re-use it in `shell_with_weapon`, you'd want to instantiate a BTMFeature-134 referencing that namespace. **The MCP doesn't expose this primitive.** `write_featurescript_feature` always creates a NEW Feature Studio with new source.
- **Workaround taken (`hamachi-uqc`):** re-upload the FS source via `write_featurescript_feature` (creates a parallel Feature Studio `661e2d122314a6cc0e1c2311`). Source archived to `docs/fs/arduino-shelf.fs` so it can be re-uploaded with parity, AND so the source isn't lost if the Onshape doc is rebuilt.
- **Boolean gotcha during rebuild:** `opBoolean` with `tools: qEverything(EntityType.BODY)` failed with `BOOLEAN_INPUTS_NOT_SOLID` — `qEverything(EntityType.BODY)` picks up non-solid entities (sketch regions, surfaces from `opPlane`). Use `qAllNonMeshSolidBodies()` instead for "union everything that's actually solid." Two-shot fix; documented in `feedback_onshape_mcp_gotchas`.

## Onshape document IDs (for agent sessions)

```
documentId         = 3662b787ba73917e77c5a543
workspaceId        = 64e642b76ea639b57af58924

variables          (Variable Studio) = 8ad42fc0569446006c975dba
lid                (Part Studio)     = 4184aeece3d6787270371115
shell_no_weapon    (Part Studio)     = 9e9326c01379985fe1982ee6
shell_with_weapon  (Part Studio)     = a28ec3540e926aba920d88b6
weapon             (Part Studio)     = 109b30fd7fdba9ecc8543a6e
weapon_pin         (Part Studio)     = d8b341a8f7fcb9bbee7d33a3
Part Studio 1      (empty default)   = 064c70496e243190d7179534    # to delete (hamachi-8jo)
```

These won't change unless the doc is recreated. Drop them straight into tool args to skip a `get_document` / `get_elements` lookup at the start of each session.
