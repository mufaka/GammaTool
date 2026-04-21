using System.Diagnostics;
using System.Text;

namespace GammaTool;

public partial class Form1 : Form
{
    private readonly List<MonitorGamma> _monitors = [];
    private readonly List<WindowInfo> _windows = [];
    private readonly GammaPreset[] _sortedPresets = [.. GammaPreset.All.OrderBy(preset => preset.Name, StringComparer.CurrentCultureIgnoreCase)];
    private readonly Dictionary<int, (int Brightness, int Contrast, int PresetIndex)> _settings = [];
    private bool _suppressSliderEvents;
    private bool _suppressPresetEvent;

    public Form1()
    {
        InitializeComponent();
        LoadPresets();
        LoadMonitors();
        RefreshWindows();
    }

    // ------------------------------------------------------------------
    //  Monitor management
    // ------------------------------------------------------------------

    private void LoadPresets()
    {
        cboPresets.Items.Clear();
        cboPresets.Items.Add("(None)");
        foreach (var preset in _sortedPresets)
            cboPresets.Items.Add(preset.Name);
        cboPresets.SelectedIndex = 0;
    }

    private void LoadMonitors()
    {
        foreach (var m in _monitors) m.Dispose();
        _monitors.Clear();
        _settings.Clear();
        cboMonitors.Items.Clear();

        foreach (var screen in Screen.AllScreens)
        {
            var monitor = new MonitorGamma(screen);
            _monitors.Add(monitor);
            cboMonitors.Items.Add(monitor.DisplayName);
            _settings[_monitors.Count - 1] = (0, 0, 0);
        }

        if (cboMonitors.Items.Count > 0)
            cboMonitors.SelectedIndex = 0;

        UpdateStatus();
    }

    private MonitorGamma? SelectedMonitor =>
        cboMonitors.SelectedIndex >= 0 && cboMonitors.SelectedIndex < _monitors.Count
            ? _monitors[cboMonitors.SelectedIndex]
            : null;

    // ------------------------------------------------------------------
    //  Window enumeration (for "Target App" picker)
    // ------------------------------------------------------------------

    private void RefreshWindows()
    {
        _windows.Clear();
        cboWindows.Items.Clear();
        cboWindows.Items.Add("(None \u2014 select monitor manually)");

        var sb = new StringBuilder(256);
        GammaInterop.EnumWindows((hWnd, _) =>
        {
            if (!GammaInterop.IsWindowVisible(hWnd))
                return true;

            sb.Clear();
            GammaInterop.GetWindowText(hWnd, sb, sb.Capacity);
            var title = sb.ToString();
            if (string.IsNullOrWhiteSpace(title))
                return true;

            string processName = "";
            try
            {
                GammaInterop.GetWindowThreadProcessId(hWnd, out uint pid);
                processName = Process.GetProcessById((int)pid).ProcessName;
            }
            catch { /* access denied or process exited */ }

            _windows.Add(new WindowInfo
            {
                Handle = hWnd,
                Title = title,
                ProcessName = processName
            });
            cboWindows.Items.Add(_windows[^1].ToString());
            return true;
        }, IntPtr.Zero);

        cboWindows.SelectedIndex = 0;
    }

    // ------------------------------------------------------------------
    //  UI event handlers
    // ------------------------------------------------------------------

    private void cboMonitors_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cboMonitors.SelectedIndex < 0) return;

        _suppressSliderEvents = true;
        _suppressPresetEvent = true;
        var (brightness, contrast, presetIndex) = _settings.GetValueOrDefault(cboMonitors.SelectedIndex);
        trkBrightness.Value = brightness;
        trkContrast.Value = contrast;
        lblBrightnessValue.Text = brightness.ToString();
        lblContrastValue.Text = contrast.ToString();
        cboPresets.SelectedIndex = presetIndex;
        _suppressPresetEvent = false;
        _suppressSliderEvents = false;

        UpdateStatus();
    }

    private void cboWindows_SelectedIndexChanged(object? sender, EventArgs e)
    {
        int windowIndex = cboWindows.SelectedIndex - 1; // index 0 is the "(None)" placeholder
        if (windowIndex < 0 || windowIndex >= _windows.Count) return;

        var windowScreen = Screen.FromHandle(_windows[windowIndex].Handle);
        for (int i = 0; i < _monitors.Count; i++)
        {
            if (_monitors[i].Screen.DeviceName == windowScreen.DeviceName)
            {
                cboMonitors.SelectedIndex = i;
                break;
            }
        }
    }

    private void btnRefresh_Click(object? sender, EventArgs e) => RefreshWindows();

    private void cboPresets_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_suppressPresetEvent) return;
        var monitor = SelectedMonitor;
        if (monitor == null) return;

        int presetIdx = cboPresets.SelectedIndex;
        GammaPreset? preset = presetIdx > 0 ? _sortedPresets[presetIdx - 1] : null;
        monitor.SetPreset(preset);

        int idx = cboMonitors.SelectedIndex;
        var (brightness, contrast, _) = _settings.GetValueOrDefault(idx);
        _settings[idx] = (brightness, contrast, presetIdx);

        ApplyCurrentSettings();
    }

    private void trkBrightness_ValueChanged(object? sender, EventArgs e)
    {
        if (_suppressSliderEvents) return;
        lblBrightnessValue.Text = trkBrightness.Value.ToString();
        ApplyCurrentSettings();
    }

    private void trkContrast_ValueChanged(object? sender, EventArgs e)
    {
        if (_suppressSliderEvents) return;
        lblContrastValue.Text = trkContrast.Value.ToString();
        ApplyCurrentSettings();
    }

    private void btnReset_Click(object? sender, EventArgs e)
    {
        var monitor = SelectedMonitor;
        if (monitor == null) return;

        monitor.RestoreOriginal();
        _settings[cboMonitors.SelectedIndex] = (0, 0, 0);

        _suppressSliderEvents = true;
        _suppressPresetEvent = true;
        trkBrightness.Value = 0;
        trkContrast.Value = 0;
        lblBrightnessValue.Text = "0";
        lblContrastValue.Text = "0";
        cboPresets.SelectedIndex = 0;
        _suppressPresetEvent = false;
        _suppressSliderEvents = false;

        lblStatus.Text = "Reset to original gamma ramp.";
    }

    private void btnResetAll_Click(object? sender, EventArgs e)
    {
        foreach (var m in _monitors)
            m.RestoreOriginal();

        for (int i = 0; i < _monitors.Count; i++)
            _settings[i] = (0, 0, 0);

        _suppressSliderEvents = true;
        _suppressPresetEvent = true;
        trkBrightness.Value = 0;
        trkContrast.Value = 0;
        lblBrightnessValue.Text = "0";
        lblContrastValue.Text = "0";
        cboPresets.SelectedIndex = 0;
        _suppressPresetEvent = false;
        _suppressSliderEvents = false;

        lblStatus.Text = "All monitors reset to original gamma ramps.";
    }

    // ------------------------------------------------------------------
    //  Gamma application
    // ------------------------------------------------------------------

    private void ApplyCurrentSettings()
    {
        var monitor = SelectedMonitor;
        if (monitor == null) return;

        int idx = cboMonitors.SelectedIndex;
        var (_, _, presetIndex) = _settings.GetValueOrDefault(idx);
        _settings[idx] = (trkBrightness.Value, trkContrast.Value, presetIndex);

        bool ok = monitor.ApplyBrightnessContrast(trkBrightness.Value, trkContrast.Value);
        string presetName = presetIndex > 0 ? _sortedPresets[presetIndex - 1].Name : "None";
        lblStatus.Text = ok
            ? $"Applied: Preset={presetName}, Brightness={trkBrightness.Value}, Contrast={trkContrast.Value}"
            : "\u26A0 SetDeviceGammaRamp failed \u2014 values may exceed driver limits.";
    }

    private void UpdateStatus()
    {
        var monitor = SelectedMonitor;
        if (monitor == null)
            lblStatus.Text = "No monitor selected.";
        else if (!monitor.IsValid)
            lblStatus.Text = "\u26A0 Could not read gamma ramp for this monitor.";
        else
            lblStatus.Text = "Ready.";
    }

    // ------------------------------------------------------------------
    //  Cleanup
    // ------------------------------------------------------------------

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        bool restore = chkRestoreOnClose.Checked;
        foreach (var m in _monitors)
            m.RestoreOnDispose = restore;

        base.OnFormClosing(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            foreach (var m in _monitors)
                m.Dispose();
            _monitors.Clear();
        }
        base.Dispose(disposing);
    }
}
