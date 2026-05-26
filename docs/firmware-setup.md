---
title: Firmware setup & flashing
sidebar_position: 1
---

# Firmware setup & flashing

End-to-end walkthrough for building, flashing, and bench-testing the openmelt firmware on the 2026-06-06 hamachi build. The firmware itself is upstream `nothinglabs/openmelt2` C++ — minor case-corrected includes for Linux file systems (commit `b48bcfe`), nothing else changed for the competition. The long-term ESP32 port is in [Roadmap](./roadmap.md).

## Target hardware

| Role | Component | Notes |
|---|---|---|
| MCU | Arduino Micro **OR** Adafruit ItsyBitsy 32u4 5V 16MHz | Both use Atmega32u4 with the same Caterina bootloader. See [ADR-0002](./decisions/0002-keep-arduino-micro-for-comp.md). |
| Accelerometer | H3LIS331DH (400g) on Adafruit breakout #4627 | Has built-in 3V↔5V level shifter. SparkFun breakout works with an external level shifter. |
| Radio | MEUS Racing ME-10B TX + M-10A RX (10ch 2.4GHz) | Must be set to **Simulation servo / 50Hz** mode for `rc_handler.cpp` compatibility. See [ADR-0005](./decisions/0005-select-meus-me-8b-radio.md). |
| Battery | 2S LiPo 7.4V | Voltage divider in `melty_config.h` is set for ~10:1 (10kΩ to GND, 100kΩ to Bat+). Warns below 7.0V. |
| Heading LED | Cree C503B-BCN-CV0Z0461 (5mm) | Wired through current-limiting resistor between `HEADING_LED_PIN` and GND. |
| Motor driver | IRLB3813 N-channel MOSFET on copper-clad FR4 heatsink | One per motor. Gate driven by `MOTOR_PIN1` / `MOTOR_PIN2`. |

**Atmega328-based Arduinos are not supported** — openmelt needs 3 hardware interrupts for the RC inputs, and the 328 only has 2.

## Pin mapping (Arduino Micro / ItsyBitsy 32u4)

All pins are configured in [`openmelt/melty_config.h`](https://github.com/griswaldbrooks/hamachi/blob/main/openmelt/melty_config.h):

| Signal | Pin | Source | Notes |
|---|---|---|---|
| RC ch1 (Left/Right) | D7 | M-10A CH1 | Must be an interrupt pin |
| RC ch2 (Forward/Back) | D1 (labeled "TX") | M-10A CH2 | [Atmega32u4 pin map](https://docs.arduino.cc/hacking/hardware/PinMapping32u4) |
| RC ch3 (Throttle) | D0 (labeled "RX") | M-10A CH3 | |
| Heading LED | D8 | LED anode (with resistor to GND) | Not D13 (on-board LED) |
| Motor 1 driver | D9 | MOSFET 1 gate | 490 Hz PWM-capable on 32u4 |
| Motor 2 driver | D10 | MOSFET 2 gate | Required even if running a single-motor build (signal is generated regardless) |
| Battery monitor | A0 | Voltage divider mid-point | |
| Accelerometer | SDA / SCL | I2C address 0x18 (Adafruit) or 0x19 (SparkFun) | Default Arduino I2C pins |

## Libraries

**Use the pinned snapshots in [`arduino_library_archives/`](https://github.com/griswaldbrooks/hamachi/tree/main/arduino_library_archives)**, not the latest versions from the IDE Library Manager. The upstream README documents an I2C hang in newer SparkFun_LIS331 builds.

| Library | Pinned version | Source |
|---|---|---|
| SparkFun_LIS331 | 1.0.1 | `arduino_library_archives/SparkFun_LIS331_Arduino_Library-master/` |
| Adafruit_SleepyDog | 1.6.4 | `arduino_library_archives/Adafruit_SleepyDog_Library/` |

## Install — Arduino IDE

1. Install Arduino IDE 2.x (or arduino-cli if you prefer headless).
2. Add board support if needed:
   - For an Arduino Micro: the bundled "Arduino AVR Boards" package includes "Arduino Leonardo" which works (same Atmega32u4).
   - For an Adafruit ItsyBitsy 32u4: add Adafruit's board manager URL or just use `arduino:avr:leonardo`.
3. Drop the pinned libraries into your sketchbook's `libraries/` folder:
   ```bash
   cp -r arduino_library_archives/Adafruit_SleepyDog_Library ~/Arduino/libraries/
   cp -r arduino_library_archives/SparkFun_LIS331_Arduino_Library-master ~/Arduino/libraries/SparkFun_LIS331
   ```
4. Open `openmelt/openmelt.ino`. Verify the sketch compiles for **Arduino Leonardo** (board target) at 16 MHz.

## Install — arduino-cli (headless)

```bash
arduino-cli core update-index
arduino-cli core install arduino:avr
arduino-cli lib install --zip-path arduino_library_archives/Adafruit_SleepyDog_Library
arduino-cli lib install --zip-path arduino_library_archives/SparkFun_LIS331_Arduino_Library-master
arduino-cli compile -b arduino:avr:leonardo openmelt/
```

## Flashing

Find the USB device (it appears as `/dev/ttyACM0` on Linux when in bootloader mode):

```bash
arduino-cli board list
```

Then upload:

```bash
arduino-cli upload -b arduino:avr:leonardo -p /dev/ttyACM0 openmelt/
```

### dialout group on Linux

Serial access on Linux needs the user to be in the `dialout` group. If you've just been added but the current shell predates the change, use `sg`:

```bash
sg dialout -c 'arduino-cli upload -b arduino:avr:leonardo -p /dev/ttyACM0 openmelt/'
```

A fresh terminal after the group add (or a re-login) makes the `sg` wrapper unnecessary.

### Caterina bootloader quirk

The 32u4 uses the Caterina bootloader, which presents two USB devices: one for normal operation (`/dev/ttyACM0` for serial) and a separate VID:PID during the brief upload window. If `arduino-cli` reports "device not found," it usually means the bootloader window closed before the tool reconnected. Just retry — `arduino-cli` will trigger a soft-reset and catch the bootloader on the second try. Holding the on-board reset button right when you run `upload` reliably forces the device into the bootloader window.

## Compile-time configuration (`melty_config.h`)

These are the knobs you'll most often want to flip. All live in [`openmelt/melty_config.h`](https://github.com/griswaldbrooks/hamachi/blob/main/openmelt/melty_config.h):

### Diagnostics

```c
//#define JUST_DO_DIAGNOSTIC_LOOP
```

Uncomment to disable all motor control and just print accel + RC + battery info over USB serial every 250 ms. Useful for verifying the RC link, accel I2C, and battery monitor without risking a spin-up.

### Throttle behavior

```c
#define THROTTLE_TYPE BINARY_THROTTLE
```

- `BINARY_THROTTLE` — motors hard on/off; throttle is the portion of each rotation they're on. Simplest, safe default.
- `FIXED_PWM_THROTTLE` — motors PWM at fixed duty (`PWM_MOTOR_ON`); throttle is still per-rotation portion. For brushless ESCs that want a 490 Hz PWM input.
- `DYNAMIC_PWM_THROTTLE` — PWM duty scales with RC throttle. Reduces current during spin-up at part-throttle.

The IRLB3813 MOSFET drives in the BOM use `BINARY_THROTTLE`. If you swap in a brushless ESC, switch to `FIXED_PWM_THROTTLE` or `DYNAMIC_PWM_THROTTLE`.

### Translational drift

```c
#define DEFAULT_ACCEL_MOUNT_RADIUS_CM 3.9
#define DEFAULT_LED_OFFSET_PERCENT 7
#define DEFAULT_ACCEL_ZERO_G_OFFSET 0.0f
#define LEFT_RIGHT_HEADING_CONTROL_DIVISOR 1.5f
#define MIN_TRANSLATION_RPM 400
```

Accel mount radius **must** match the physical distance from the bot's center of rotation to the accel chip. For hamachi, that's the distance from the center of the shell to the accel breakout on the floor — measure this before tuning. Wrong radius = wrong RPM math = wrong heading.

`DEFAULT_ACCEL_ZERO_G_OFFSET` is auto-calibrated on first entry/exit of config mode (saved to EEPROM). The first power-up will be off until you enter and exit config mode once.

### Safety

```c
#define ENABLE_WATCHDOG
#define WATCH_DOG_TIMEOUT_MS 2000
#define VERIFY_RC_THROTTLE_ZERO_AT_BOOT
```

Leave both enabled. The watchdog protects against I2C hangs (a known SparkFun_LIS331 failure mode) and runaway loops. The throttle-zero check prevents the bot from spinning up at power-on if the RC was left on with throttle.

### Accelerometer range

In [`openmelt/accel_handler.h`](https://github.com/griswaldbrooks/hamachi/blob/main/openmelt/accel_handler.h):

```c
#define ACCEL_RANGE LIS331::HIGH_RANGE   // 400g
#define ACCEL_MAX_SCALE 400
```

For small-radius bots that don't reach top RPM, drop to `MED_RANGE` (200g) for improved resolution. Hamachi uses HIGH_RANGE because the antweight reference reaches ~2300 RPM at the default 3.9cm radius.

## Runtime UX

While powered, the heading LED reports status:

| LED pattern | Meaning |
|---|---|
| Slow flash (250 on / 250 off) at boot | Waiting for valid RC signal at zero throttle |
| Fast flash (30 on / 120 off) | RC signal good, bot idle |
| Fast flash + occasional double-blip | RC signal good, bot in **config mode** |
| Slow flash (30 on / 600 off) | RC signal lost — motors off, won't spin |
| Flicker | Battery voltage below `BATTERY_ADC_WARN_VOLTAGE_THRESHOLD` (7.0V) |
| 1 quick blink per rotation while spinning | Heading beacon |

### Stick gestures

- **Throttle > 0%:** spin one rotation. Throttle stays > 0% → keep spinning.
- **Forward stick + zero throttle, held 750 ms:** flash out max RPM (LED blinks once per 100 RPM, punctuated by a 1.5 s pause at end).
- **Backward stick, held 750 ms:** toggle config mode. Settings are saved to EEPROM on exit.
- **In config mode, L/R stick:** adjust heading LED offset (controls perceived "front" of the bot).
- **In config mode, L/R lower-diagonal:** adjust accel mount radius (controls RPM math).

## Bench-test checklist

Before every bench session:

1. **Power on bench supply or fresh battery.** Confirm battery is above 7.4V.
2. **Verify RC link.** Power the radio first, then the bot. LED should slow-flash at boot then transition to fast-flash within ~1 second once throttle is at 0%. If it stays slow-flashing, the radio or wiring is wrong — see [Troubleshooting](#troubleshooting).
3. **Verify accel via diagnostic loop.** Build with `JUST_DO_DIAGNOSTIC_LOOP` uncommented, flash, open serial monitor @ 115200, confirm "Raw Accel G" is near 0.0 at rest and goes positive when the bot is spun by hand. Recomment + reflash before competition.
4. **Verify failsafe.** Power on, get good RC link, then power off the radio. Bot should drop to slow-flash within 900 ms and motors should be off. **Verify before every test session** (`hamachi-0xx`).
5. **Verify motor wiring.** With wheels off the ground and zero throttle, momentarily raise throttle. Confirm both motors spin in the directions you expect; reverse motor polarity if not. (Sequence: kill power → swap leads → repower.)
6. **Calibrate accel zero.** Enter config mode (back-stick 750ms), exit config mode. The current at-rest accel reading is saved as the zero-G offset.

## Competition firmware checklist

Pre-comp checks (`bd show hamachi-0xx` / `hamachi-1og` / `hamachi-8ue` track these):

- [ ] ME-10B in Simulation servo / 50Hz mode.
- [ ] Failsafe configured: throttle channel returns to ~1000 µs (0%) on signal loss. Tested.
- [ ] Mixing configured: pistol-grip trigger → throttle (CH3); wheel → L/R (CH1); aux thumb stick → F/B (CH2).
- [ ] M-10A per-channel PWM output confirmed before wiring to MCU (jumper + scope, or LED on a channel pin).
- [ ] `DEFAULT_ACCEL_MOUNT_RADIUS_CM` set to the measured value on the physical bot.
- [ ] At-rest accel zero-G calibration done in config mode after final assembly.
- [ ] Heading LED offset (`DEFAULT_LED_OFFSET_PERCENT`) tuned so the LED appears where the bot translates.
- [ ] Spare M-10A receiver tested and bound (battle-damage redundancy).
- [ ] Spare programmed MCU ready (with the same `EEPROM_WRITTEN_SENTINEL_VALUE` so on-disk defaults take effect).

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| LED slow-flashes forever at boot | No RC signal, or throttle isn't at 0% | Power radio first; verify M-10A channel mapping matches `melty_config.h` pins; check `MAX_RC_PULSE_LENGTH` / `MIN_RC_PULSE_LENGTH` thresholds. |
| LED never enters fast-flash, no serial output | I2C hang on accel | Confirm SDA/SCL wiring + pull-ups; the watchdog should reset on the next 2000ms tick — listen for the boot pattern repeating. |
| Bot spins but LED beacon is in the wrong spot | LED offset is wrong | Enter config mode, adjust L/R stick, exit to save. |
| RPM math off / wrong-direction translation | Accel mount radius doesn't match physical mount | Re-measure, edit `DEFAULT_ACCEL_MOUNT_RADIUS_CM`, increment `EEPROM_WRITTEN_SENTINEL_VALUE` to force-load the new default. |
| Battery alert triggers in healthy battery | Voltage divider ratio off | Adjust `VOLTAGE_DIVIDER` to match your actual resistor pair; resistor tolerances stack. |
| `arduino-cli upload` says device not found | Bootloader window missed | Retry. Or hold the on-board reset button right as you run `upload`. |
| `Adafruit_SleepyDog.h: case mismatch` on Linux | Case-sensitive FS + uppercase include | Already fixed for hamachi (commit `b48bcfe`). If you sync from upstream, watch for this. |

## See also

- Upstream documentation: [`readme.md`](https://github.com/griswaldbrooks/hamachi/blob/main/readme.md) at the repo root.
- Build context for the 2026-06-06 competition: [competition-2026-06-06.md](./competition-2026-06-06.md).
- Decision rationale: [ADR-0002 — Keep Arduino Micro](./decisions/0002-keep-arduino-micro-for-comp.md), [ADR-0005 — Select MEUS ME-8B radio](./decisions/0005-select-meus-me-8b-radio.md).
- [Build & assembly guide](./build-assembly-guide.md) — physical assembly + wiring (stub).
