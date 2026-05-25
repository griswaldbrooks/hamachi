# ADR-0002 — Keep Arduino Micro for the 2026-06-06 competition

- **Status:** Accepted
- **Date:** 2026-05-25

## Context

The 2026-06-06 competition is 12 days out. Several MCUs were considered as alternatives to the upstream-specified
Arduino Micro (Atmega32u4):

- **Arduino Nano (Atmega328):** explicitly unsupported upstream — only 2 hardware interrupts, need 3 RC channels.
  Pin-change interrupts could work but require firmware changes.
- **Arduino Nano 33 BLE (nRF52840):** all GPIOs interrupt-capable, but: 3.3V logic (level shifting needed for MOSFET
  gate), `EEPROM.h` doesn't apply (need flash/preferences port), AVR-timer-specific 490Hz PWM in `motor_driver.cpp`
  needs rewrite, SleepyDog support is library-version-dependent. Multi-day port.
- **Pi Pico (RP2040):** all GPIOs interrupt-capable, but: same set of porting issues as Nano 33 BLE (PWM, EEPROM,
  watchdog), plus less mature Arduino-core ecosystem for some of the SparkFun/Adafruit libs in use.
- **Pi Zero / Zero 2:** full Linux. Real-time RC pulse decode on a non-RT kernel is unreliable; ~30s boot from a
  brownout is a fight-killer. Wrong tool for combat.

## Decision

Use the Arduino Micro (Atmega32u4) as-specified upstream. No firmware changes for 6/6.

## Consequences

- Minimum-risk path. The firmware is proven on this hardware.
- Deferred work: ESP32 port is the long-term target (Phase 2 in [roadmap](../roadmap.md)). Pico and Nano 33 BLE
  remain on the bench for future experimentation but are not on the critical path.
- The on-hand Nano 33 BLE and Pi Pico don't help us today, but the ESP32 port plan can borrow from porting work that
  would also apply to them.

## Implementation notes (2026-05-25)

Specifically ordered the **Adafruit ItsyBitsy 32u4 5V 16MHz** ([PID 3677](https://www.adafruit.com/product/3677)),
×2 (primary + spare), instead of the upstream-specified Arduino Micro. Same Atmega32u4 MCU, smaller form factor,
identical firmware compatibility — no code changes required. Bundled in the same Adafruit order as the H3LIS331
([PID 4627](https://www.adafruit.com/product/4627)) breakout to consolidate shipping.
