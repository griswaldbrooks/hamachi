---
title: Firmware setup & flashing
sidebar_position: 1
---

# Firmware setup & flashing

> **Status:** Stub. To be expanded with full Arduino IDE walkthrough + pinned-library setup.

## Target hardware

- **Competition MCU (2026-06-06):** Arduino Micro (Atmega32u4). See [ADR-0002](./decisions/0002-keep-arduino-micro-for-comp.md) — no firmware changes through the comp date.
- **Long-term fork target:** ESP32 (post-competition). See [Roadmap](./roadmap.md).

## Library versions

Pinned library snapshots live in `arduino_library_archives/`. **Do not assume "latest" works** — the upstream README calls out an I2C hang in newer SparkFun_LIS331. Use the archived versions.

## Building

```bash
# Use the Arduino IDE or arduino-cli to build openmelt/openmelt.ino
arduino-cli compile -b arduino:avr:leonardo openmelt/
```

## Flashing

The user that runs Claude Code is in the `dialout` group, but the parent process predates the group add, so direct flashing needs `sg dialout`:

```bash
sg dialout -c 'arduino-cli upload -b arduino:avr:leonardo -p /dev/ttyACM0 openmelt/'
```

If you're flashing manually in a fresh shell, the group is already active and the `sg` wrapper is unnecessary.

## TODO

- Arduino IDE installation steps (per OS).
- Library archive load procedure (where to drop each `.zip` from `arduino_library_archives/`).
- Pin assignments for the Hamachi build (radio channel → MCU pin map).
- Bench-test checklist (IMU calibration, radio binding, motor direction verification).
- Competition firmware checklist (failsafe verified, mixing configured, etc. — see `bd show hamachi-0xx` and `hamachi-1og`).
- Notes on the case-sensitive `Arduino.h` include fix (commit `b48bcfe`).
