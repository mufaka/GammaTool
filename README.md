# Gamma Tool

A small Windows utility for adjusting monitor brightness, contrast, and colour tone
via the Win32 [`SetDeviceGammaRamp`](https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-setdevicegammaramp) API.

> **Note:** The majority of this code was generated with the help of AI (GitHub Copilot)
> and then reviewed/tweaked by hand. It is a quick utility, not a production-grade product.

## Features

| Feature | Details |
|---|---|
| **Multi-monitor** | Pick any connected display from a dropdown; each monitor keeps its own settings. |
| **Brightness & Contrast sliders** | Real-time adjustment (−100 to +100) applied on top of the current LUT. |
| **Target App picker** | Lists visible windows so you can auto-select the monitor a specific app is on. |
| **14 built-in presets** | One-click colour/tone shifts — see table below. |
| **Restore on exit** | Checkbox (on by default) restores every monitor's original gamma ramp when the app closes. |

## Presets

All presets are designed to stay within the Windows display-driver constraints
(each ramp entry within ±32 000 of the identity value, monotonically non-decreasing).
Brightness and contrast sliders compose on top of any active preset.

| Preset | Effect |
|---|---|
| Night Mode | Blue-light filter — reduces blue to ~58 %, slight red warmth |
| High Contrast | 1.5× linear stretch around midpoint — deeper blacks, brighter whites |
| Warm Tone | Amber cast — green ×0.90, blue ×0.72 |
| Night Vision (Green) | Green dominant — red and blue cut to ~52 % |
| Reading Mode | Dim warm light — R ×0.78, G ×0.70, B ×0.55 |
| Sepia | Vintage photograph — G ×0.82, B ×0.58 |
| Cool Tone | Blue-ish fluorescent feel — R ×0.85, G ×0.95 |
| Dark Room | Red-dominant for astronomy or photo darkrooms |
| Cinema | Lifted blacks (8 %) + compressed whites (94 %) — filmic log look |
| Brighten | Gamma 0.75 power curve — lifts mid-tones, anchors black/white |
| Pastel | Raised shadows + compressed highlights — soft, dreamy look |
| Vivid | Cubic S-curve — punchier shadows and highlights, midpoint unchanged |
| Gaming | S-curve (s=1.8) + slight blue lift (×1.06) and red trim (×0.96) — vibrant, cool-toned gaming look |
| Focus Mode | Gamma 0.88 lift + 25 % blue cut — warm-neutral tone, easy on the eyes for long coding sessions |

## Requirements

- Windows 10 or later
- .NET 10 (or the corresponding runtime)

## Building

```
dotnet build GammaTool.csproj
```

Or open `GammaTool.slnx` in Visual Studio 2022+ and build from the IDE.

## How it works

1. On startup the app calls `GetDeviceGammaRamp` for every monitor to capture the
   **original hardware LUT**.  All adjustments are computed relative to that baseline,
   so existing display calibration is preserved.
2. A **preset** (optional) transforms the original LUT into a new *base ramp*
   (e.g. scaling individual RGB channels).
3. **Brightness** shifts the base ramp up/down; **Contrast** scales it around the midpoint.
4. The combined result is clamped to stay within driver-safe bounds
   (`ClampToDriverLimits`) and pushed to the GPU via `SetDeviceGammaRamp`.

### Why some effects aren't possible

`SetDeviceGammaRamp` is a per-channel lookup table — it cannot mix channels
(e.g. true greyscale requires knowing R+G+B together).  Windows also enforces that
each ramp value stays within roughly ±50 % of the identity curve and is monotonically
non-decreasing.  Effects like full colour inversion or solarization violate these
constraints and will be silently rejected by the driver.

## Project structure

```
GammaTool.slnx               Solution file
GammaTool.csproj              .NET 10 / Windows Forms project file
Program.cs                    Entry point
Form1.cs / Form1.Designer.cs  Main window — UI + application logic
GammaInterop.cs               P/Invoke: gdi32 (gamma ramp, CreateDC) + user32 (EnumWindows)
MonitorGamma.cs               Per-monitor gamma state, brightness/contrast math, driver clamping
GammaPreset.cs                Built-in preset definitions
WindowInfo.cs                 Simple model for the Target App window list
README.md                     This file
```

## Limitations

- **Per-monitor, not per-application.** The gamma ramp affects the entire display output.
  The *Target App* dropdown is a convenience that auto-selects the correct monitor.
- **Driver limits.** Extreme adjustments may be silently rejected.  The status bar shows
  whether `SetDeviceGammaRamp` succeeded or failed.
- **No ICC profile awareness.** The tool captures whatever ramp is active at launch.
  If another colour-management app changes the ramp externally, those changes won't be
  reflected until Gamma Tool is restarted.

## License

This project is provided as-is for personal use.
