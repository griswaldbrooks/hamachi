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

- Serial Monitor (115200 baud) output captured:
- RC channel pulse widths reported via diagnostic loop:
- Accel reading at rest:
- Battery voltage reading (expected 0 with battery disconnected):

## Radio configuration (MEUS ME-10B)

- Receiver mode: Simulation servo / 50 Hz — confirmed
- Pistol-grip mapping:
  - Trigger → throttle (zero at bottom, no reverse range): ___
  - Wheel → L/R: ___
  - Aux thumb stick → F/B: ___
- Failsafe verified — throttle → 0% on signal loss: ___ (date / re-verified ___)

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
