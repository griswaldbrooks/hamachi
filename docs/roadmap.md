# Roadmap

This is a multi-year personal project. Short-term (6/6/2026 competition) priorities are tracked separately in
[competition-2026-06-06.md](./competition-2026-06-06.md). Everything below is post-competition.

## Phase 1 — Fork hygiene

- Rename `openmelt/` → `hamachi/` (sketch folder, `.ino`, KiCad project, readme branding).
- Refresh top-level `readme.md` to reflect fork status and link to `docs/`.
- Cleanup `.DS_Store` debris inherited from upstream; tighten `.gitignore`.
- Set up CI: at minimum Arduino-CLI compile-check on every push, plus markdown lint.

## Phase 2 — MCU port (ESP32)

The reference design targets Atmega32u4 (Arduino Micro). The fork's long-term target is ESP32:

- Dual core: dedicate one core to the rotation timing loop.
- All GPIOs interrupt-capable → no pin constraints for RC channels.
- `ledc` PWM is fully configurable (replaces AVR-timer 490Hz hack in `motor_driver.cpp`).
- NVS replaces AVR `EEPROM.h` in `config_storage.cpp`.
- Hardware watchdog replaces Adafruit SleepyDog.
- 3.3V logic — driver-side level shifting needed for the MOSFET gate; H3LIS331 already 3V-native (drop the Adafruit breakout's level converter).

Stretch: also evaluate Pi Pico (RP2040) and Nano 33 BLE (nRF52840), both of which are already on hand. ESP32 is the
default for telemetry reasons (Wi-Fi + BLE + ESP-NOW).

## Phase 3 — Beetleweight mechanical redesign

The antweight reference (`antweight_reference_platform/`) is a teaching design, not a competitive beetleweight.
Real 1kg-class build needs:

- Brushless inrunner + ESC running SimonK firmware (upstream readme mentions this works but no build plans exist —
  recreate them).
- 3S or 4S LiPo with appropriate voltage regulation for the MCU rail.
- Durable shell material (PPA-CF / nylon / TPU-jacketed) instead of PLA.
- Beefier wheel + hub system (FingerTech foam is fine for ants, not beetles).
- Possibly larger-radius accel placement + drop to 200g range for tracking accuracy.
- OpenSCAD shell needs reparametrization for the new internal volume.

## Phase 4 — Receiver protocol independence

Decouple from per-channel PWM by adding receiver protocol support:

- SBUS (Futaba, FrSky)
- PPM (legacy combined-channel)
- iBUS (FlySky)

Keep the existing PWM path as a fallback for hobby radios like the MEUS ME-8B.

## Phase 5 — Telemetry

ESP32 enables live telemetry without a separate radio:

- BLE GATT for ground-station readout (RPM, battery voltage, accel offset, fault state).
- ESP-NOW for low-latency point-to-point if BLE proves laggy.
- Optional Wi-Fi for tethered tuning sessions.

## Phase 6 — Documentation site

- [Docusaurus](https://docusaurus.io/) site sourced from `docs/`.
- Deploy via GitHub Pages.
- Auto-generated changelog from conventional commits.

## Exploratory — Rust rewrite

Open question, post Phase 2. Candidates:

- **embassy-rs on ESP32** (via `esp-hal` + `embassy-executor`) — async runtime gives clean separation of the rotation
  loop, RC decode, accel read, and telemetry. Mature crates exist (`esp-hal`, `embassy-net`, `esp-now`).
- **embassy-rs on RP2040** — even more mature ecosystem, but no built-in radio.
- **embassy-rs on nRF52840** (Nano 33 BLE) — best BLE story.

Decision deferred until ESP32 C++ port stabilizes. Goal would be: type-safe state machine for spin/idle/config-mode
transitions, compile-time-checked pin assignments, smaller failure surface.

## Issue tracking convention

- **GitHub Issues** — strategic items, anything worth surfacing to collaborators or future-me.
- **beads / `bd`** — agent working memory, fine-grained subtasks, exploration notes that don't need to be public.
