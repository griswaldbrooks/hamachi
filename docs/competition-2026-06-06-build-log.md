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
- **Still open:** failsafe end-to-end test (see Radio configuration below); reflash **competition** firmware (currently the diagnostic build is on the board) before the event.

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
