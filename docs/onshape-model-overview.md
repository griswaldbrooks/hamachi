---
title: Onshape model overview
sidebar_position: 1
---

# Onshape model overview

Reference for the parametric Onshape model that supersedes the OpenSCAD source in `antweight_reference_platform/spinbot_ant.scad`. All printable geometry comes from this document; the SCAD file is frozen as a historical reference.

For the long-form record of *why* the migration happened and how each Part Studio was built (every bead closed, every gotcha hit), see [Onshape migration log](./onshape-migration.md). This page is the "what" — read it when you need to find a variable, locate a feature, or figure out which Part Studio produces a given STL.

## Quick links

- **Document:** `hamachi` (Onshape) — id `3662b787ba73917e77c5a543` / workspace `64e642b76ea639b57af58924`
- **Source of truth:** Onshape for all printable geometry; SCAD reference is frozen
- **Migration log:** [onshape-migration.md](./onshape-migration.md) (rationale + per-feature build history)
- **Agent quick-start:** the `hamachi-onshape` skill at `.claude/skills/hamachi-onshape/SKILL.md`

## Coordinate system

- **Origin:** center of bot, at the bottom outer face of the shell (the arena-touching surface).
- **+Z** up (Onshape's Top plane normal).
- **+Y** away from the wheel side. SCAD places the wheel at `-Y`.
- **+X** is the long axis of the weapon slot.

The main shell base is sketched on the **Top plane** as a circle of diameter `#botDiameter`, extruded `+Z` for `#botHeight`. The motor axis runs along `+X` at `(0, -28, -15)` in shell-local frame (`Y = -#motorWallOffset`, `Z = -(#motorDepthBelowFloor + #motorDiameter/2 - #floorThickness)`).

## Part Studios at a glance

| Element | Type | Features | Status |
|---|---|---:|---|
| `variables` | Variable Studio | 35 base + 5 derived | [populated](#variable-studio-reference) |
| [`lid`](#lid) | Part Studio | 11 | built |
| [`shell_no_weapon`](#shell_no_weapon) | Part Studio | 25 | built |
| [`shell_with_weapon`](#shell_with_weapon) | Part Studio | 25 | built |
| [`weapon`](#weapon) | Part Studio | 8 | built |
| [`weapon_pin`](#weapon_pin) | Part Studio | 2 | built |

## Variable Studio reference

All sketches and extrudes pull values from a single Variable Studio (`elementId = 8ad42fc0569446006c975dba`). Base variables resolve everywhere — constraint values, `variableDepth` on extrudes, and inline arithmetic.

### Shell

| Variable | Default | Notes |
|---|---:|---|
| `#botHeight` | 33 mm | Floor to lid mating face |
| `#botDiameter` | 138 mm | Outer shell OD |
| `#botShellThickness` | 5 mm | Outer wall thickness |
| `#floorThickness` | 3 mm | Shell floor thickness |

### Lid

| Variable | Default | Notes |
|---|---:|---|
| `#lidThickness` | 1 mm | Candidate for thickening in v2 |
| `#lidScrewPilotDiameter` | 2.5 mm | M3 self-tap pilot (was 2.2 mm in SCAD for #2-28) |
| `#lidScrewHoleDepth` | 10 mm | |
| `#ledHoleDiameter` | 5 mm | LED + power switch (was 4 mm in SCAD, but SCAD bug cut 8 mm — see migration log) |
| `#lidMotorClearance` | 1.5 mm | Extra Z clearance in the motor cutout |
| `#powerHoleAngle` | 140° | Position of power switch hole |
| `#ledHoleAngle` | -24° | Position of LED hole |

### Motor

| Variable | Default | Notes |
|---|---:|---|
| `#motorDiameter` | 37 mm | Includes 1 mm assembly clearance |
| `#motorLength` | 51 mm | Includes 1 mm assembly clearance |
| `#motorBushingDiameter` | 13 mm | |
| `#motorBushingHeight` | 2 mm | |
| `#motorMountHeight` | 7 mm | Height of motor mount block above floor |
| `#motorMountOffset` | -2 mm | Y offset of mount center from bot center |
| `#motorWallOffset` | 28 mm | -Y distance from bot center to motor wall |
| `#motorWallThickness` | 3 mm | |
| `#motorDepthBelowFloor` | 5 mm | How far below the floor surface the motor axis sits |

### Weapon

| Variable | Default | Notes |
|---|---:|---|
| `#weaponWidth` | 40 mm | X extent of weapon bar |
| `#weaponThickness` | 19 mm | Y extent of weapon bar |
| `#weaponInset` | 2 mm | How far the bar protrudes past the shell |
| `#weaponMountHoleDiameter` | 12 mm | Through-hole for the pin |
| `#weaponPinKerf` | 1 mm | Pin OD reduced by this for press-fit clearance |
| `#weaponPinExtraLength` | 5 mm | Pin length beyond `weaponThickness` |
| `#weaponBottomVerticalExtension` | 1.5 mm | How far the detachable weapon extends below shell |
| `#weaponTopVerticalExtension` | 4 mm | Top bridge keeping the two slot halves connected |
| `#weaponSlotExtraWidth` | 1 mm | Kerf widening on the slot in -Y |

### Mounting & wiring

| Variable | Default | Notes |
|---|---:|---|
| `#zipTieHoleDiameter` | 5.25 mm | Renamed from SCAD's `motorMountHoleDiameter` |
| `#zipTieHoleOffset` | 6 mm | Renamed from SCAD's `motorMountHoleOffset` |
| `#batteryWallZipTieGap` | 3 mm | Gap below battery wall (z = 3..6) for zip-ties |

### Wheel (reference only — not printed)

| Variable | Default | Notes |
|---|---:|---|
| `#wheelDiameter` | 44 mm | Foam wheel OD |
| `#wheelWidth` | 12.5 mm | |
| `#wheelClearance` | 8 mm | Motor-wall to inboard-wheel-face distance |

### Derived variables

These resolve only as expression text in the Variable Studio itself. **Do not reference them from constraint VALUES, `variableDepth`, or `variableRadius`** — they fail silently as `SKETCH_UNSOLVABLE_CONSTRAINT`. Paste the inline expression verbatim where you need them (sqrt and `^` work fine).

| Derived | Expression | Used for |
|---|---|---|
| `#botRadius` | `#botDiameter / 2` | Readability shorthand |
| `#shellInteriorHeight` | `#botHeight - #floorThickness` | Hollow depth |
| `#motorMountWidth` | `#motorDiameter + 4 mm` | Motor mount X extent |
| `#motorMountLength` | `#motorLength + 4 mm` | Motor mount Y extent |
| `#motorWallWidth` | `2 * sqrt((#botDiameter/2)^2 - (#motorWallOffset + #motorWallThickness)^2) - #botShellThickness` | Chord length at the motor wall |

## Part Studios

### `lid`

11 features. Element `4184aeece3d6787270371115`. Produces `lid.stl` (138 mm ⌀ × 1 mm disk).

![lid iso](/img/onshape/img_54df2738adc12ea5.png)
![lid top](/img/onshape/img_5d32b858ad63cfe1.png)

(The lid is 1 mm thin; the front view is a single horizontal line and omitted.)

**Feature tour (build order):**

1. `lid_outer_profile` / `lid_body` — circle ⌀ `#botDiameter` on Top plane, extruded `#lidThickness` upward. Creates the disk.
2. `lid_screw_holes` / `lid_screw_holes_cut` — six ⌀ `#lidScrewPilotDiameter` pilot holes. Four around the bolt circle (E/NE/NW/W from SCAD's `for([0:60:200])` loop) plus two over the motor wall. All positions are parametric, driven by `#botDiameter` / `#botShellThickness` / `#motorWallOffset` / `#motorWallThickness`. Cuts through with `variableDepth=lidThickness`.
3. `lid_led_power_holes` / `lid_led_power_holes_cut` — two ⌀ `#ledHoleDiameter` holes at radius `#botDiameter/2 - #botShellThickness - #ledHoleDiameter` for the LED and power switch. Angular positions from `#ledHoleAngle` and `#powerHoleAngle`.
4. `lid_wheel_well` / `lid_wheel_well_cut` — D-shape (arc at radius `#botDiameter/2 - #botShellThickness`, chord at `Y=-31`), cut through. Diverges from SCAD's cube-chord cut: **the lid retains a 5 mm outer rim crescent** below the chord for structural stiffness. Decision recorded as `hamachi-5fc`; also noted in `spinbot_ant.scad` near line 215.
5. `lid_motor_xs` / `lid_motor_cut_negY` / `lid_motor_cut_posY` — single Ø `#motorDiameter` circle on the Front plane at `(0, Z=-15)`, then two opposite-direction BLIND REMOVE extrudes (28 mm in -Y, 23 mm in +Y). Two extrudes instead of one symmetric extrude because `create_offset_plane` faceId fails on the subsequent sketch (`SKETCH_NO_PLANE`); workaround tracked in `hamachi-fbm`.

### `shell_no_weapon`

25 features. Element `9e9326c01379985fe1982ee6`. Produces `shell_no_weapon.stl` (138 mm ⌀ × 33 mm; the variant with a Ø12 weapon mount hole through the -Y wall).

![shell_no_weapon iso](/img/onshape/img_37db3505bfcc016a.png)
![shell_no_weapon top](/img/onshape/img_6a934d67d48009f9.png)
![shell_no_weapon front](/img/onshape/img_3937e3fec5acf745.png)

The front view (looking at +Y, i.e. the back of the bot from SCAD's perspective) shows the battery wall edge and the lid taps; the weapon mount hole on the opposite -Y face is visible in the top view as a small disc near the bottom of the wheel-well chord.

**Feature tour (build order):**

1. `shell_outer_profile` / `shell_body` — ⌀ `#botDiameter` circle, extruded `#botHeight`. NEW body.
2. `shell_inner_profile` / `shell_hollow` — inner circle ⌀ `#botDiameter - 2*#botShellThickness` extruded down from the top face, REMOVE. Leaves the `#floorThickness` floor intact.
3. `shell_wheel_well` / `shell_wheel_well_cut` — D-shape on the -Y half, intersected with the inner cylinder, REMOVE through.
4. `shell_motor_wall_profile` / `shell_motor_wall` — rectangle on the floor at `Y=-#motorWallOffset`, width = derived `#motorWallWidth` expression, height = `#botHeight`. ADD.
5. `shell_motor_wall_bushing_relief` / `_cut` — Ø `#motorBushingDiameter` cut through the motor wall.
6. `shell_motor_mount_profile` / `shell_motor_mount` — `#motorMountWidth × #motorMountLength` box on the floor at `Y=#motorMountOffset`, extruded `#motorMountHeight`. ADD.
7. `shell_motor_xs` / `shell_motor_cut_negY` / `shell_motor_cut_posY` — Ø `#motorDiameter` cut on Front plane at `Z=-15`, same two-extrude pattern as the lid.
8. `shell_zip_tie_holes` / `_cut` — six ⌀ `#zipTieHoleDiameter` holes through the motor mount + floor.
9. `shell_lid_tap_holes` / `_cut` — Ø `#lidScrewPilotDiameter` × `#lidScrewHoleDepth` pilots in the shell rim. **Only 4 of 6 cut** in this Part Studio (E/W perimeter + 2 over the motor wall). NE/NW slipped to coincident positions due to a `DISTANCE direction=MINIMUM` solver collapse — fixed in `shell_with_weapon` by dropping the MINIMUM constraints. Tracked in `hamachi-v5c` notes; backport to `shell_no_weapon` is filed when needed.
10. `shell_weapon_mount_xs` / `shell_weapon_mount_cut` — **distinguishing feature** of this variant. Ø `#weaponMountHoleDiameter` through-hole on the -Y face at motor-axis Z.
11. `shell_battery_wall_profile` / `shell_battery_wall` — wall slab at `Y=+58` spanning `X = ±33`, `Z = 6..31`.
12. `shell_battery_wall_zip_tie_gap` — second REMOVE on the same sketch, 3 mm in -Z, opens a `Z = 3..6` gap under the wall. `forceOppositeDirection=false` to override the REMOVE-on-face auto-flip.
13. `shell_arduino_shelf` — FeatureScript custom feature. Tilted shelf + strut at roughly `(46, 19, 15)` with `Rx=-21°`. Source archived to `docs/fs/arduino-shelf.fs`. Built on a 3-point plane derived from the Rx·Rz rotation math, extruded `+2 mm` along the tilted normal, then `opBoolean UNION` with `qAllNonMeshSolidBodies()`.

### `shell_with_weapon`

25 features. Element `a28ec3540e926aba920d88b6`. Produces `shell_with_weapon.stl` (the variant with an integrated weapon bar at the front; no separate `weapon` part needed).

![shell_with_weapon iso](/img/onshape/img_d2e0d781eeda39d7.png)
![shell_with_weapon top](/img/onshape/img_987fd7bb24d9606d.png)

The protruding weapon bar at `Y = -76.5` is best seen from the iso and top views. The front view (looking +Y, identical to `shell_no_weapon`'s) is omitted because the bar isn't visible from that angle.

**Differences vs `shell_no_weapon`:**

- **Skips** `shell_weapon_mount_xs` / `shell_weapon_mount_cut` (features 10 above). The shell wall is left intact at the front — the integrated bar replaces the detachable weapon assembly.
- **Adds** `shell_weapon_bar_profile` / `shell_weapon_bar` — a `#weaponWidth × #weaponThickness` rectangle on the Top plane centered at `(0, -67)` (i.e. `-#botDiameter/2 + #weaponInset` along Y), extruded `variableDepth=botHeight`. The inside portion fuses with the shell; the protruding portion (`Y ∈ [-76.5, -69]`) adds material out the -Y front.
- **6 lid tap holes** (vs 4) — the `DISTANCE-MINIMUM` perimeter constraints were dropped in favor of seed coordinates, so all four `for([0:60:200])` positions plus the two motor-wall taps cut cleanly.
- **Arduino shelf** is a separate FeatureScript feature in a freshly-built Feature Studio (`661e2d122314a6cc0e1c2311`). Same geometry as `shell_no_weapon`'s shelf — face origins match `(36.0, 19.2, 16.9)` and `(32.9, -16.2, 3.2)` exactly. The MCP doesn't expose cross-element FS reference, so the source was re-uploaded from `docs/fs/arduino-shelf.fs` (`hamachi-uqc`).

Volume parity vs SCAD: **+0.09%** (within tessellation noise).

### `weapon`

8 features. Element `109b30fd7fdba9ecc8543a6e`. Produces `weapon.stl` (the detachable weapon bar that mates with the `shell_no_weapon` variant via a 12 mm pin).

![weapon iso](/img/onshape/img_7e10a734bab6cf6a.png)
![weapon front](/img/onshape/img_c9820c035587286d.png)

The top view is a featureless 40 × 19 rectangle (the slot at the -Y face and the pin hole on the side aren't visible looking straight down) and omitted.

**Feature tour (build order):**

1. `weapon_bar_profile` / `weapon_bar` — `#weaponWidth × #weaponThickness` rectangle centered at `(0, -67)` on the Top plane, extruded `#botHeight + #weaponTopVerticalExtension + #weaponBottomVerticalExtension` = 38.5 mm. NEW body spanning `Z = 0..38.5`.
2. `weapon_shell_annulus` / `weapon_shell_cut` — sketch the main shell outer cylinder (Ø `#botDiameter`) on the Top plane, REMOVE through. Carves the curved inset on the +Y face of the bar that mates with the shell wall.
3. `weapon_mount_hole_xs` / `weapon_mount_hole_cut` — Ø `#weaponMountHoleDiameter` through-hole at the motor-axis Z, perpendicular to the bar's long axis. Pin passes through here.
4. `weapon_shell_annulus_offset` / `weapon_shell_cut_offset` — second shell-cylinder REMOVE, shifted by `#weaponSlotExtraWidth = 1 mm` in -Y. Widens the slot for 3D-print kerf so the printed weapon slides freely over the printed shell wall.

Volume parity vs SCAD: **−0.11%** after the kerf-widening 2nd cut landed.

### `weapon_pin`

2 features. Element `d8b341a8f7fcb9bbee7d33a3`. Produces `weapon_pin.stl` (a press-fit cylinder).

![weapon_pin iso](/img/onshape/img_f9ec6db03431b6f6.png)

**Feature tour:**

1. `pin_profile` / `pin_body` — single circle of diameter `#weaponMountHoleDiameter - #weaponPinKerf = 11 mm`, extruded `#weaponThickness + #weaponPinExtraLength = 24 mm`. Drop-in cylinder; print-orientation-agnostic.

## STL cross-reference

Each printable STL in `antweight_reference_platform/` is generated by exactly one Part Studio. There are no SCAD-only legacy STLs; the OpenSCAD file (`spinbot_ant.scad`) is kept as a frozen reference but doesn't generate any committed STLs anymore — the post-migration practice is to re-export from Onshape when a print is made.

| STL file | Part Studio | Notes |
|---|---|---|
| `lid.stl` | [`lid`](#lid) | |
| `shell_no_weapon.stl` | [`shell_no_weapon`](#shell_no_weapon) | Use with `weapon.stl` + `weapon_pin.stl` |
| `shell_with_weapon.stl` | [`shell_with_weapon`](#shell_with_weapon) | Single-piece variant — no separate weapon |
| `weapon.stl` | [`weapon`](#weapon) | Only needed when printing `shell_no_weapon.stl` |
| `weapon_pin.stl` | [`weapon_pin`](#weapon_pin) | Only needed when printing `shell_no_weapon.stl` |
| `spinbot_ant.scad` | — | Frozen reference; not used for prints |
| `hamachi.3mf` | (slicer project) | Plate file packaging one of the two shell variants for the printer |

Onshape STL exports come out in **meters**, not millimeters — external mesh-diff tooling needs to scale ×1000 to compare with the SCAD-native STLs.
