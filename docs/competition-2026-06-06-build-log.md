# Build Log — 2026-06-06

Running log of build observations, prints, assembly steps, tuning sessions, and anything else worth remembering.
Fill in chronologically. Build strategy is in [competition-2026-06-06.md](./competition-2026-06-06.md);
parts list is in [competition-2026-06-06-bom.md](./competition-2026-06-06-bom.md).

## 2026-05-24 (evening)

- Started PLA fit-check print on Bambu X1C: `shell_no_weapon.stl` + `lid.stl`. Print intended to validate
  dimensional fit before committing to PPA-CF — _not_ the final structural shell.

## 2026-05-25

- Placed orders: Adafruit, Digi-Key, Amazon, FingerTech. See [BOM](./competition-2026-06-06-bom.md).
- PLA fit-check print: _(observations to fill in)_
- PPA-CF filament drying started: _(start time + temp + duration)_

## 2026-05-31 (firmware + RC bring-up, USB bench)

Tethered bench session over USB (battery + motors not yet wired). All checks via the
`JUST_DO_DIAGNOSTIC_LOOP` build read over serial @115200.

- **MCU bring-up:** flashed a fresh ItsyBitsy 32u4 (freshly soldered headers) — uploads clean via
  `arduino:avr:leonardo`, boots, streams diagnostics. Board survived soldering. Now a programmed comp-ready spare.
  - Toolchain: not in `dialout` group — used `sg dialout -c '...'` wrapper. Pinned libs via `--library` (no global install).
- **IMU:** H3LIS331 confirmed **live** — `Raw Accel G` jitters (real noise), not the frozen value an absent I2C
  device returns. At-rest reads ~−4.9 g raw (uncalibrated; `Zero G Offset` still 0.00 — calibrate via config mode after assembly).
  - ⚠️ Firmware silently reports a plausible G value when the accel is **absent** (no presence check). Filed `hamachi-fut` (post-comp).
- **RC channel mapping — verified by isolating one control at a time:**

  | Control | MEUS ch | MCU pin | Firmware field |
  |---|---|---|---|
  | Steering wheel | CH1 | D7 | L/R (±~500 µs, proportional) |
  | Trigger | CH2 | D0 | Throttle |
  | 3-position switch | CH4 | D1 | F/B (−1 / 0 / +1, neutral confirmed) |

  - Receiver was rewired twice to land here: MEUS convention (CH1=steer, CH2=throttle) crossed the trigger onto
    F/B initially; **CH3 is only 2-state** (no neutral) so F/B moved to **CH4**, which has a true 3-position switch.
- **Throttle neutral fix → [ADR-0006](./decisions/0006-throttle-neutral-at-trigger-center.md):** the pistol trigger
  spring-centers to ~1507 µs → read 43% at the upstream 1250 µs idle point (won't arm; idle-spins). Radio SUB TR / TH
  Trim capped at ~1380 µs (~22%), couldn't reach 0%. Fixed in firmware: `IDLE_THROTTLE_PULSE_LENGTH` **1250 → 1550**.
  Verified: released trigger → 0% (16/16), full squeeze → 100% (16/16). **Breaks the AGENTS.md firmware freeze, intentionally.**
- **Drivetrain — single-motor confirmed** (builder, 2026-05-31): reference design uses **one** drive motor on
  **D9**; the firmware's D10 phase-2 signal is generated but left unconnected. BOM's Qty-2 motors = primary + spare,
  not a two-motor drivetrain. → Power electronics need only **1 FET + 1 Schottky in-circuit** (BOM's Qty-4 are spares).
- **Power-electronics sourcing (DigiKey delayed):** DigiKey order (MOSFETs / Schottky / bus cap / resistors / LED)
  showed "EOD 5/31" but FedEx had not scanned it out of NJ — treating it as **won't make 6/6** (not cancelled; bonus
  if it lands). Ordered **IRLB8721PBF** (logic-level N-FET) via Amazon Prime as the primary motor switch. Micro Center
  run for passives + a backup logic-level FET. **Watch out:** need a true *logic-level* FET (Rds rated at Vgs ≤5 V) —
  IRLZ44N OK, **IRFZ44N / NTE2389 are standard-gate, do NOT use** at the 5 V ItsyBitsy gate.
- **VNH5019 carrier (on hand):** with single-motor confirmed, this is now a **complete** drive backup (not half). Can
  bench-test motor drive today in binary mode (INA=H, INB=L, EN=H, **D9 → PWM**) with **zero firmware change**. Not the
  long-term pick (heavier/bulkier than a TO-220); IRLB8721 preferred. Its variable/EN-PWM mode is unneeded —
  `BINARY_THROTTLE` already shapes throttle at the spin level.
- **Still open:** failsafe end-to-end test (see Radio configuration below); reflash **competition** firmware (currently the diagnostic build is on the board) before the event.

## 2026-05-31 (evening) — Micro Center sourcing run

Motor-switch path now fully sourced (DigiKey treated as won't-arrive). Single-motor build → 1 FET + 1 clamp in-circuit.

**Acquired:**
- **Backup MOSFET — RFP30N06LE** (TO-220, N-ch, 60 V / 30 A). Genuine logic-level ("LE" = logic-enhanced),
  Rds(on) ~0.047 Ω at Vgs = 5 V — fully on off the 32u4's 5 V gate. Pinout (label facing you, L→R):
  **Gate – Drain – Source** (same as IRLZ44N; drops into the low-side wiring). Notably this is the
  *upstream-original* openmelt2 FET the BOM had marked discontinued. Backup to the IRLB8721 (Amazon, primary).
- **Back-EMF clamp — KBPC2502 bridge rectifier** (25 A / 200 V). Using **one** of its four diodes as the low-side
  freewheel clamp: **AC (~) → FET drain / switched node**, **"+" → battery positive / motor high side** (anode at
  switched node, cathode at +V — verified correct low-side orientation). Other AC + "−" left unconnected. 25 A ≫ a
  540's freewheel current; slow recovery is irrelevant at melty switching rates (tens of Hz). ⚠️ Heavy chassis-mount
  block — fine for bench/backup, but prefer a lighter dedicated Schottky (DigiKey 30SQ045 or a Prime part) for the
  final antweight build.
- **Passives:** 4700 µF / ≥16 V electrolytic ×2 (bus cap), 100 Ω (LED), 10 kΩ + 100 kΩ (battery divider), 5 mm LED
  (heading), 10 kΩ (FET gate→source pulldown).

**Rejected:**
- 5.1 V / 5 W Zener — wrong device; clamps reverse at 5.1 V (below the 7.4 V pack) so it'd conduct backward across the
  motor. Not a flyback diode.
- 6 A / 600 V axial rectifier — would work but 6 A is under the ≥10 A target (leans on surge rating), and $34.99 was a
  ripoff. Chose the KBPC2502 instead.

**Motor-switch path complete:** IRLB8721 (primary) / RFP30N06LE (backup) low-side FET + KBPC2502 single-diode
freewheel + 10 kΩ gate pulldown. VNH5019 remains on hand as a full integrated-flyback alternate.

## Prints

### PLA fit-check

- File(s):
- Printer settings:
- Print time:
- Result / fit observations:

### PPA-CF (final structural)

- Filament drying: temp / hours
- Slicer profile / settings:
- Print order:
  - [ ] `shell_no_weapon.stl`
  - [ ] `lid.stl`
  - [ ] `weapon.stl` (after platform tuning)
  - [ ] `weapon_pin.stl` (after platform tuning)
- Dimensional adjustments vs PLA:
- Thread-forming screw test (on scrap):

## Foam wheel fabrication

- Technique (from prior build):
- Material / dimensions:
- Balance check:

## Electronics assembly

### MOSFET heatsink

- Copper-clad FR4 piece size:
- Solder process notes (high-temp tip / flux used):

### Wiring

- Power path: battery → switch → MOSFET drain → motor → … → ground
- Schottky placement across motor leads:
- Signal: M-10A → ItsyBitsy pins per `melty_config.h`
  - LEFTRIGHT_RC_CHANNEL_PIN 7
  - FORBACK_RC_CHANNEL_PIN 1 (TX)
  - THROTTLE_RC_CHANNEL_PIN 0 (RX)
- LED + 100 Ω current-limit resistor:
- Battery monitor divider (10kΩ / 100kΩ) → A0:

### Drive train

- Wheel hub drilled out 3 mm → 3.175 mm: yes / no
- Hub-to-shaft fit:

## First power-up (USB, battery disconnected)

- Serial Monitor (115200 baud) output captured: ✅ 2026-05-31 — `JUST_DO_DIAGNOSTIC_LOOP` streams at ~4 Hz.
- RC channel values via diagnostic loop: ✅ all three channels mapped + verified (see 2026-05-31 session entry).
- Accel reading at rest: ✅ live (jittering), uncalibrated ~−4.9 g raw; `Zero G Offset` 0.00 (calibrate post-assembly).
- Battery voltage reading: A0 floating (no divider wired) → reads garbage (~12–15 V). Re-check after the 10k/100k
  divider is in place — expect ~0 with battery disconnected.

## Radio configuration (MEUS ME-10B)

**Note:** TX in hand is the **MEUS ME-8B** (8-channel), not the ME-10B that ADR-0005's implementation note
anticipated. Channel map verified 2026-05-31 (see session entry above).

- Receiver mode: Simulation servo / 50 Hz — _(confirm)_
- Pistol-grip mapping (verified 2026-05-31 by isolating each control over serial):
  - Trigger → CH2 → **D0 / Throttle** ✅ (neutral handled in firmware per ADR-0006, not radio trim)
  - Wheel → CH1 → **D7 / L/R** ✅ (proportional, ±~500 µs)
  - 3-position switch → CH4 → **D1 / F/B** ✅ (−1 / 0 / +1; CH3 was 2-state, no neutral — avoid)
- Failsafe: CH2 (throttle) failsafe value set to low on the radio (2026-05-31). **End-to-end test still pending:**
  power on, wait ≥60 s (TX pushes failsafe config once/min per manual), kill TX, confirm over serial that
  `Health → 0` AND `Throttle → 0` within ~1 s. Re-verify before every test session (`hamachi-0xx`).

## Tuning sessions

### Session 1

- Date:
- Surface / environment:
- Battery state:
- Config-mode adjustments:
  - DEFAULT_ACCEL_MOUNT_RADIUS_CM final value:
  - DEFAULT_LED_OFFSET_PERCENT final value:
  - DEFAULT_ACCEL_ZERO_G_OFFSET final value:
- Max RPM observed:
- Issues:

## Risks that materialized

- _(fill in if/as they happen)_

## Competition day (2026-06-06)

- Results:
- Fights:
- What broke:
- What held up well:
- Post-event repair list:
