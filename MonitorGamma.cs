namespace GammaTool;

/// <summary>
/// Manages the gamma ramp for a single monitor. Captures the original LUT on creation
/// so that all adjustments are relative to the real hardware state.
/// </summary>
internal sealed class MonitorGamma : IDisposable
{
    private readonly IntPtr _hdc;
    private readonly ushort[,] _originalRamp = new ushort[3, 256];
    private ushort[,] _baseRamp = new ushort[3, 256];
    private bool _disposed;

    /// <summary>
    /// Windows enforces that each gamma ramp entry stays within a maximum deviation
    /// from the identity ramp (i × 257) and that the ramp is monotonically non-decreasing.
    /// The exact limit is driver-specific; 32000 is a conservative safe value.
    /// </summary>
    private const int MaxDeviation = 32000;

    public Screen Screen { get; }
    public string DisplayName { get; }
    public bool IsValid { get; }

    /// <summary>
    /// When true, <see cref="Dispose"/> restores the original gamma ramp.
    /// Set to false if the user wants to keep the adjusted gamma after exit.
    /// </summary>
    public bool RestoreOnDispose { get; set; } = true;

    public MonitorGamma(Screen screen)
    {
        Screen = screen;

        var name = screen.DeviceName.Replace(@"\\.\", "");
        DisplayName = screen.Primary
            ? $"{name} \u2014 {screen.Bounds.Width}\u00D7{screen.Bounds.Height} [Primary]"
            : $"{name} \u2014 {screen.Bounds.Width}\u00D7{screen.Bounds.Height}";

        _hdc = GammaInterop.CreateDC(screen.DeviceName, null, null, IntPtr.Zero);
        if (_hdc != IntPtr.Zero)
        {
            IsValid = GammaInterop.GetDeviceGammaRamp(_hdc, _originalRamp);
            if (IsValid)
            {
                Array.Copy(_originalRamp, _baseRamp, _originalRamp.Length);
            }
        }
    }

    /// <summary>
    /// Replaces the base ramp with a preset transformation of the original.
    /// Pass null to clear the preset and revert to the original ramp.
    /// Call <see cref="ApplyBrightnessContrast"/> afterwards to push the result to hardware.
    /// </summary>
    public void SetPreset(GammaPreset? preset)
    {
        if (preset == null)
            Array.Copy(_originalRamp, _baseRamp, _originalRamp.Length);
        else
        {
            _baseRamp = preset.BuildRamp(_originalRamp);
            ClampToDriverLimits(_baseRamp);
        }
    }

    /// <summary>
    /// Applies brightness and contrast adjustments relative to the current base ramp
    /// (which may be the original or a preset-modified version).
    /// </summary>
    /// <param name="brightness">Brightness offset, -100 to +100 (0 = no change).</param>
    /// <param name="contrast">Contrast adjustment, -100 to +100 (0 = no change).</param>
    /// <returns>True if the driver accepted the new ramp.</returns>
    public bool ApplyBrightnessContrast(int brightness, int contrast)
    {
        if (!IsValid) return false;

        var ramp = new ushort[3, 256];

        // Brightness: shift the ramp up/down (±25 % of the full 0-65535 range)
        double brightnessShift = brightness / 100.0 * 16384.0;

        // Contrast: scale around the midpoint.  -100 → 0×, 0 → 1×, +100 → 2×
        double contrastFactor = 1.0 + contrast / 100.0;

        const double midpoint = 32768.0;

        for (int c = 0; c < 3; c++)
        {
            for (int i = 0; i < 256; i++)
            {
                double value = _baseRamp[c, i];

                // Scale around midpoint for contrast
                value = midpoint + (value - midpoint) * contrastFactor;

                // Shift for brightness
                value += brightnessShift;

                ramp[c, i] = (ushort)Math.Clamp(value, 0.0, 65535.0);
            }
        }

        ClampToDriverLimits(ramp);
        return GammaInterop.SetDeviceGammaRamp(_hdc, ramp);
    }

    /// <summary>
    /// Clamps every ramp entry to within <see cref="MaxDeviation"/> of the identity
    /// ramp and enforces monotonically non-decreasing order so that
    /// <c>SetDeviceGammaRamp</c> will not silently reject the values.
    /// </summary>
    private static void ClampToDriverLimits(ushort[,] ramp)
    {
        for (int c = 0; c < 3; c++)
        {
            ushort prev = 0;
            for (int i = 0; i < 256; i++)
            {
                int identity = i * 257;
                int lo = Math.Max(0, identity - MaxDeviation);
                int hi = Math.Min(65535, identity + MaxDeviation);
                int val = Math.Clamp(ramp[c, i], lo, hi);

                // Enforce monotonically non-decreasing
                if (val < prev)
                    val = prev;

                ramp[c, i] = (ushort)val;
                prev = (ushort)val;
            }
        }
    }

    /// <summary>
    /// Restores the original gamma ramp that was captured when this instance was created.
    /// Also resets the base ramp (clears any active preset).
    /// </summary>
    public bool RestoreOriginal()
    {
        if (!IsValid) return false;
        Array.Copy(_originalRamp, _baseRamp, _originalRamp.Length);
        return GammaInterop.SetDeviceGammaRamp(_hdc, _originalRamp);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (IsValid && RestoreOnDispose)
        {
            GammaInterop.SetDeviceGammaRamp(_hdc, _originalRamp);
        }

        if (_hdc != IntPtr.Zero)
        {
            GammaInterop.DeleteDC(_hdc);
        }
    }
}
