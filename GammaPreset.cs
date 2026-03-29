namespace GammaTool;

/// <summary>
/// A named gamma ramp transformation that builds a new LUT from the original.
/// Brightness and contrast are applied on top of the preset ramp.
/// <para>
/// Windows enforces that each gamma ramp entry stays within ~±32 768 of the
/// identity value (i × 257) and is monotonically non-decreasing.  Presets must
/// be designed with this in mind; <see cref="MonitorGamma"/> also applies a
/// safety clamp after the transform.
/// </para>
/// </summary>
internal sealed class GammaPreset
{
    public required string Name { get; init; }
    public required string Description { get; init; }

    /// <summary>
    /// Given the original (hardware-captured) ramp, produces the preset base ramp.
    /// </summary>
    public required Func<ushort[,], ushort[,]> BuildRamp { get; init; }

    public override string ToString() => Name;

    // ------------------------------------------------------------------
    //  Built-in presets
    //
    //  Scaling factors are chosen so that the worst-case deviation at
    //  index 255 (identity = 65 535) stays under 32 000:
    //      factor >= 0.52  →  65 535 × 0.52 = 34 078  →  delta 31 457  ✓
    // ------------------------------------------------------------------

    public static readonly GammaPreset[] All =
    [
        // ── Blue-light filter ───────────────────────────────────────
        new()
        {
            Name = "Night Mode",
            Description = "Reduces blue light and adds slight warmth for less eye strain.",
            BuildRamp = original =>
            {
                var ramp = (ushort[,])original.Clone();
                for (int i = 0; i < 256; i++)
                {
                    ramp[0, i] = (ushort)Math.Min(original[0, i] * 1.04, 65535); // tiny red warmth
                    // green unchanged
                    ramp[2, i] = (ushort)(original[2, i] * 0.58);                // 42 % blue cut
                }
                return ramp;
            }
        },

        // ── Contrast enhancement ────────────────────────────────────
        new()
        {
            Name = "High Contrast",
            Description = "Stretches the curve so darks go darker and brights go brighter.",
            BuildRamp = original =>
            {
                // Linear stretch 1.5× around midpoint, clamped to [0, 65535].
                // The resulting ramp is still monotonically non-decreasing and
                // the deviation from identity at the extremes is ≈ 0.
                var ramp = new ushort[3, 256];
                for (int c = 0; c < 3; c++)
                    for (int i = 0; i < 256; i++)
                    {
                        double t = original[c, i] / 65535.0;
                        double stretched = 0.5 + (t - 0.5) * 1.5;
                        ramp[c, i] = (ushort)(Math.Clamp(stretched, 0.0, 1.0) * 65535.0);
                    }
                return ramp;
            }
        },

        // ── Warm amber cast ─────────────────────────────────────────
        new()
        {
            Name = "Warm Tone",
            Description = "Adds a warm amber cast \u2014 pleasant for reading or media.",
            BuildRamp = original =>
            {
                var ramp = (ushort[,])original.Clone();
                for (int i = 0; i < 256; i++)
                {
                    // R stays at 100 %, G drops a touch, B drops moderately
                    ramp[1, i] = (ushort)(original[1, i] * 0.90);
                    ramp[2, i] = (ushort)(original[2, i] * 0.72);
                }
                return ramp;
            }
        },

        // ── NVG-style green dominant ────────────────────────────────
        new()
        {
            Name = "Night Vision (Green)",
            Description = "Green-dominant tint simulating night-vision goggles.",
            BuildRamp = original =>
            {
                var ramp = new ushort[3, 256];
                for (int i = 0; i < 256; i++)
                {
                    // Cut R and B to the minimum the driver will accept (~52 %),
                    // keep green at full intensity.
                    ramp[0, i] = (ushort)(original[0, i] * 0.52);
                    ramp[1, i] = original[1, i];
                    ramp[2, i] = (ushort)(original[2, i] * 0.52);
                }
                return ramp;
            }
        },

        // ── Dim warm reading mode ───────────────────────────────────
        new()
        {
            Name = "Reading Mode",
            Description = "Dimmed warm light \u2014 easy on the eyes for long reading sessions.",
            BuildRamp = original =>
            {
                var ramp = new ushort[3, 256];
                for (int i = 0; i < 256; i++)
                {
                    ramp[0, i] = (ushort)(original[0, i] * 0.78);
                    ramp[1, i] = (ushort)(original[1, i] * 0.70);
                    ramp[2, i] = (ushort)(original[2, i] * 0.55);
                }
                return ramp;
            }
        },

        // ── Vintage amber tint ──────────────────────────────────────
        new()
        {
            Name = "Sepia",
            Description = "Warm vintage look \u2014 like an old photograph.",
            BuildRamp = original =>
            {
                var ramp = (ushort[,])original.Clone();
                for (int i = 0; i < 256; i++)
                {
                    // R full, G reduced, B heavily reduced
                    ramp[1, i] = (ushort)(original[1, i] * 0.82);
                    ramp[2, i] = (ushort)(original[2, i] * 0.58);
                }
                return ramp;
            }
        },

        // ── Blue-ish fluorescent feel ───────────────────────────────
        new()
        {
            Name = "Cool Tone",
            Description = "Blueish tint \u2014 opposite of warm, like cool fluorescent light.",
            BuildRamp = original =>
            {
                var ramp = (ushort[,])original.Clone();
                for (int i = 0; i < 256; i++)
                {
                    ramp[0, i] = (ushort)(original[0, i] * 0.85);
                    ramp[1, i] = (ushort)(original[1, i] * 0.95);
                    // B stays at 100 %
                }
                return ramp;
            }
        },

        // ── Red-dominant for astronomy / darkrooms ──────────────────
        new()
        {
            Name = "Dark Room",
            Description = "Red-dominant output for astronomy or photo-darkroom use.",
            BuildRamp = original =>
            {
                var ramp = new ushort[3, 256];
                for (int i = 0; i < 256; i++)
                {
                    ramp[0, i] = (ushort)(original[0, i] * 0.85);
                    ramp[1, i] = (ushort)(original[1, i] * 0.52);
                    ramp[2, i] = (ushort)(original[2, i] * 0.52);
                }
                return ramp;
            }
        },

        // ── Lifted blacks / compressed whites (film look) ───────────
        new()
        {
            Name = "Cinema",
            Description = "Lifted blacks and compressed highlights \u2014 a filmic log look.",
            BuildRamp = original =>
            {
                // Remap the output range from [0, 65535] to [floor, ceiling].
                const double floor   = 0.08 * 65535.0;   // blacks lifted to ~8 %
                const double ceiling = 0.94 * 65535.0;    // whites pulled down to ~94 %
                double span = ceiling - floor;

                var ramp = new ushort[3, 256];
                for (int c = 0; c < 3; c++)
                    for (int i = 0; i < 256; i++)
                    {
                        double t = original[c, i] / 65535.0;
                        ramp[c, i] = (ushort)(floor + t * span);
                    }
                return ramp;
            }
        },

        // ── Gamma-curve brightener ──────────────────────────────────
        new()
        {
            Name = "Brighten",
            Description = "Lifts mid-tones without blowing out whites \u2014 good for dim panels.",
            BuildRamp = original =>
            {
                // A gamma curve < 1.0 brightens the mid-range while
                // keeping black (0) and white (65535) anchored.
                var ramp = new ushort[3, 256];
                for (int c = 0; c < 3; c++)
                    for (int i = 0; i < 256; i++)
                    {
                        double t = original[c, i] / 65535.0;
                        ramp[c, i] = (ushort)(Math.Pow(t, 0.75) * 65535.0);
                    }
                return ramp;
            }
        },

        // ── Soft washed-out / pastel look ───────────────────────────
        new()
        {
            Name = "Pastel",
            Description = "Raised shadows and compressed highlights \u2014 soft, dreamy look.",
            BuildRamp = original =>
            {
                // Remap into a narrower band centred on mid-grey.
                const double floor   = 0.14 * 65535.0;   // lift blacks to ~14 %
                const double ceiling = 0.86 * 65535.0;    // pull whites to ~86 %
                double span = ceiling - floor;

                var ramp = new ushort[3, 256];
                for (int c = 0; c < 3; c++)
                    for (int i = 0; i < 256; i++)
                    {
                        double t = original[c, i] / 65535.0;
                        ramp[c, i] = (ushort)(floor + t * span);
                    }
                return ramp;
            }
        },

        // ── S-curve punch ───────────────────────────────────────────
        new()
        {
            Name = "Vivid",
            Description = "S-curve \u2014 deeper shadows and brighter highlights for punchier colour.",
            BuildRamp = original =>
            {
                // Cubic S-curve: t + s·t·(1−t)·(2t−1)  with s = 2.0
                // Derivative = 1 + s·(6t²−6t+1); min at t=0.5 → 1 − s/2 = 0.
                // Monotonically non-decreasing for s ≤ 2.0.
                const double s = 2.0;
                var ramp = new ushort[3, 256];
                for (int c = 0; c < 3; c++)
                    for (int i = 0; i < 256; i++)
                    {
                        double t = original[c, i] / 65535.0;
                        double curved = t + s * t * (1.0 - t) * (2.0 * t - 1.0);
                        ramp[c, i] = (ushort)(Math.Clamp(curved, 0.0, 1.0) * 65535.0);
                    }
                return ramp;
            }
        },

        // ── Gaming — punchy S-curve with a slight blue lift ─────────
        new()
        {
            Name = "Gaming",
            Description = "Punchy contrast with vibrant colours \u2014 designed for immersive gaming.",
            BuildRamp = original =>
            {
                // S-curve (s = 1.8) for contrast + per-channel scale:
                //   R ×0.96 (slight trim), G ×1.00, B ×1.06 (lift)
                // giving a vivid, cool-toned look popular in gaming.
                // Worst-case driver deviation stays well within ±32 000.
                const double s = 1.8;
                double[] channelScale = [0.96, 1.00, 1.06];
                var ramp = new ushort[3, 256];
                for (int c = 0; c < 3; c++)
                    for (int i = 0; i < 256; i++)
                    {
                        double t = original[c, i] / 65535.0;
                        double curved = t + s * t * (1.0 - t) * (2.0 * t - 1.0);
                        ramp[c, i] = (ushort)(Math.Clamp(curved * channelScale[c], 0.0, 1.0) * 65535.0);
                    }
                return ramp;
            }
        },

        // ── Focus Mode — warm-neutral, moderate blue cut ────────────
        new()
        {
            Name = "Focus Mode",
            Description = "Gentle gamma lift with moderate blue reduction \u2014 easy on the eyes during long coding sessions.",
            BuildRamp = original =>
            {
                // Gamma 0.88 brightens mid-tones slightly for crisp text,
                // then channel scales apply a warm-neutral tint:
                //   R ×1.02, G ×0.97, B ×0.75  (25 % blue cut).
                // Worst-case deviation from identity at i=255: ~16 384  ✓
                double[] channelScale = [1.02, 0.97, 0.75];
                var ramp = new ushort[3, 256];
                for (int c = 0; c < 3; c++)
                    for (int i = 0; i < 256; i++)
                    {
                        double t = original[c, i] / 65535.0;
                        double lifted = Math.Pow(t, 0.88);
                        ramp[c, i] = (ushort)(Math.Clamp(lifted * channelScale[c], 0.0, 1.0) * 65535.0);
                    }
                return ramp;
            }
        },
    ];
}
