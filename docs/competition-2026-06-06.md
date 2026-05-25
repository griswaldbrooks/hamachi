# Competition: 2026-06-06

1kg / beetleweight class. **Strategy: compete underweight using the upstream antweight reference design** rather
than attempt a real beetleweight build in 12 days. See [ADR-0003](./decisions/0003-compete-underweight-in-1kg-class.md).

## Build summary

- **Mechanical:** upstream `antweight_reference_platform/` STLs.
  - **First print: PLA fit-check** (in progress 5/24 evening → done morning of 5/25).
  - **Final print: PPA-CF** ([ADR-0004](./decisions/0004-use-ppa-cf-print-material.md)).
  - **Foam wheels fabricated in-house** (prior-build technique) rather than purchased.
- **MCU:** Adafruit ItsyBitsy 32u4 5V (same Atmega32u4 family as upstream Arduino Micro). No firmware changes. See
  [ADR-0002](./decisions/0002-keep-arduino-micro-for-comp.md).
- **Radio:** MEUS RACING **ME-10B** with **M-10A** receiver (10-channel sibling of the originally-considered ME-8B
  / M-08A). Set to Simulation servo / 50Hz mode. See
  [ADR-0005](./decisions/0005-select-meus-me-8b-radio.md).
- **Electronics:** upstream antweight BOM with the **IRLB3813PBF** MOSFET (RFP30N06LE is discontinued). No SimonK /
  brushless detour.
- **Full reproducible BOM with vendor part numbers:** see [competition-2026-06-06-bom.md](./competition-2026-06-06-bom.md).

## 13-day schedule

| Day | Date | Task |
|---|---|---|
| -1 | 5/24 evening | PLA fit-check shell + lid prints started on X1C. |
| 0 | 5/25 | Orders placed (Adafruit, Digi-Key, Amazon, FingerTech). PLA prints come off; dry PPA-CF spool 8+ hrs @ 80°C. |
| 1–5 | 5/26–30 | Parts arrive (Amazon today/Thu; Adafruit + Digi-Key mid-week; FingerTech latest). Print PPA-CF shell + lid + weapon + weapon_pin. Fabricate foam wheels. Drill wheel hub 3mm → 3.175mm. Solder MOSFET to copper-clad heatsink. Pre-tin wires. Assemble. |
| 6–7 | 5/31–6/1 | Smoke test on USB (battery disconnected). Serial diagnostics. Pair MEUS ME-10B; configure pistol-grip → 3-axis melty mapping via mixing; verify failsafe → throttle 0%. |
| 8–9 | 6/2–3 | Live spin-ups. Run interactive config: accel radius → LED offset. Tune trims. |
| 10 | 6/4 | Drive practice in an enclosed space. Verify low-battery alert, watchdog, throttle-zero arming. |
| 11 | 6/5 | Charge spare batteries. Pack: spare LED, MOSFET, Schottky, screws, glue, spare M-10A receiver. |
| 12 | **6/6** | Compete. |

## PPA-CF print notes (Bambu X1C)

PPA-CF behaves very differently from PLA. Validate with a small test print before committing the shell.

- **Dry the filament** at ~80°C for 8+ hours before printing. Wet PPA prints poorly and loses strength.
- Hardened nozzle required (X1C ships with one — confirm before starting).
- Annealing post-print improves strength but introduces ~1–2% shrinkage; consider whether mounting holes need to be
  printed slightly oversize.
- Self-tap screws (#2-28 from the upstream BOM) behave differently in PPA-CF than PLA. Test thread engagement on a
  scrap. Heat-set inserts are an alternative if threads strip.
- Shell + lid should fit-check before assembly; CF-filled prints have less compliance than PLA.

## Parts

Full reproducible BOM with vendor part numbers, quantities, and substitution rationale lives in
[competition-2026-06-06-bom.md](./competition-2026-06-06-bom.md). Ordered 2026-05-25.

## Software setup

- Arduino IDE
- Pinned library archives are in `arduino_library_archives/` — use those, not the latest upstream versions
  (upstream readme calls out an I2C hang in newer SparkFun_LIS331). Specifically:
  - `Adafruit_SleepyDog_Library/`
  - `SparkFun_LIS331_Arduino_Library-master/`

## Risks / fallbacks

| Risk | Mitigation |
|---|---|
| PPA-CF shell cracks or won't fit | PLA fit-check happens first (5/24-25); only commit to PPA-CF after dimensional validation |
| MOSFET fails mid-tuning | 4× IRLB3813 ordered; pre-tinned wires speed swap; copper-clad heatsink mounted |
| MEUS receiver lost / failsafe misconfigured | Re-pair and re-verify before every spin-up day; standalone M-10A spare ordered for battle damage |
| H3LIS331 I2C hang | Watchdog @ 2s catches; pinned lib version (`arduino_library_archives/SparkFun_LIS331_Arduino_Library-master/`) mitigates |
| LED visibility poor (5mm Cree CV bin is narrow-beam) | Mount as far outboard on shell perimeter and proud of shell edge to maximize off-angle visibility |
| Plastic thread-forming screws strip in PPA-CF | Test thread engagement on a scrap before committing; heat-set brass inserts as fallback |
| Foam wheels unbalanced (causing tracking errors) | Test-spin before installing; rebuild if vibration evident |
