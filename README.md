# Gamma Tool

A Windows utility for adjusting monitor brightness, contrast, and colour tone
via the Win32 [`SetDeviceGammaRamp`](https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-setdevicegammaramp) API.

## Features

| Feature | Details |
|---|---|
| **Multi-monitor** | Pick any connected display from a dropdown; each monitor keeps its own settings. |
| **Brightness and Contrast sliders** | Real-time adjustment (-100 to +100) applied on top of the current LUT. |
| **Target App picker** | Lists visible windows so you can auto-select the monitor a specific app is on. |
| **14 built-in presets** | One-click colour/tone shifts -- see table below. |
| **Restore on exit** | Checkbox (on by default) restores every monitor's original gamma ramp when the app closes. |

## Presets

All presets are designed to stay within the Windows display-driver constraints
(each ramp entry within +/-32,000 of the identity value, monotonically non-decreasing).
Brightness and contrast sliders compose on top of any active preset.

| Preset | Description | Channel Adjustments |
|---|---|---|
| Night Mode | Reduces blue light and adds slight warmth to ease eye strain during evening use. | R x1.04, G unchanged, B x0.58 |
| High Contrast | Stretches the tone curve so dark areas go darker and bright areas go brighter. | 1.5x linear stretch around midpoint, clamped to [0, 65535] |
| Warm Tone | Adds a warm amber cast, pleasant for reading or media consumption. | R unchanged, G x0.90, B x0.72 |
| Night Vision (Green) | Green-dominant tint that simulates night-vision goggles. | R x0.52, G unchanged, B x0.52 |
| Reading Mode | Dimmed warm light that reduces overall brightness for extended reading sessions. | R x0.78, G x0.70, B x0.55 |
| Sepia | Warm vintage tint reminiscent of aged photographs. | R unchanged, G x0.82, B x0.58 |
| Cool Tone | Adds a blue-shifted cast resembling cool fluorescent lighting. | R x0.85, G x0.95, B unchanged |
| Dark Room | Red-dominant output suited for astronomy observation or photographic darkrooms. | R x0.85, G x0.52, B x0.52 |
| Cinema | Lifts blacks and compresses highlights to produce a filmic log look. | Output range remapped to [8 %, 94 %] across all channels |
| Brighten | Applies a gamma power curve to lift mid-tones without clipping whites; useful for dim panels. | Gamma 0.75 applied to all channels |
| Pastel | Raises shadow levels and compresses highlights for a soft, low-contrast appearance. | Output range remapped to [14 %, 86 %] across all channels |
| Vivid | Applies a cubic S-curve to increase contrast in shadows and highlights while leaving the midpoint unchanged. | S-curve with strength 2.0 applied to all channels |
| Gaming | Combines a contrast S-curve with per-channel scaling for a vibrant, cool-toned look suited to gaming. | S-curve (s=1.8), R x0.96, G x1.00, B x1.06 |
| Focus Mode | Applies a gentle gamma lift for crisp text rendering and a moderate blue reduction for comfortable extended use. | Gamma 0.88, R x1.02, G x0.97, B x0.75 |

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
   **original hardware LUT**. All adjustments are computed relative to that baseline,
   so existing display calibration is preserved.
2. A **preset** (optional) transforms the original LUT into a new base ramp
   (e.g. scaling individual RGB channels).
3. **Brightness** shifts the base ramp up or down; **Contrast** scales it around the midpoint.
4. The combined result is clamped to stay within driver-safe bounds
   (`ClampToDriverLimits`) and pushed to the GPU via `SetDeviceGammaRamp`.

### Why some effects are not possible

`SetDeviceGammaRamp` is a per-channel lookup table -- it cannot mix channels
(e.g. true greyscale requires knowing R+G+B together). Windows also enforces that
each ramp value stays within roughly +/-50 % of the identity curve and is monotonically
non-decreasing. Effects like full colour inversion or solarization violate these
constraints and will be silently rejected by the driver.

## Project structure

```
GammaTool.slnx               Solution file
GammaTool.csproj              .NET 10 / Windows Forms project file
Program.cs                    Entry point
Form1.cs / Form1.Designer.cs  Main window -- UI + application logic
GammaInterop.cs               P/Invoke: gdi32 (gamma ramp, CreateDC) + user32 (EnumWindows)
MonitorGamma.cs               Per-monitor gamma state, brightness/contrast math, driver clamping
GammaPreset.cs                Built-in preset definitions
WindowInfo.cs                 Simple model for the Target App window list
README.md                     This file
```

## Limitations

- **Per-monitor, not per-application.** The gamma ramp affects the entire display output.
  The Target App dropdown is a convenience that auto-selects the correct monitor.
- **Driver limits.** Extreme adjustments may be silently rejected. The status bar shows
  whether `SetDeviceGammaRamp` succeeded or failed.
- **No ICC profile awareness.** The tool captures whatever ramp is active at launch.
  If another colour-management app changes the ramp externally, those changes will not be
  reflected until Gamma Tool is restarted.

## License

This project is provided as-is for personal use.
