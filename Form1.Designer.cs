namespace GammaTool
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            lblMonitor = new Label();
            cboMonitors = new ComboBox();
            lblWindow = new Label();
            cboWindows = new ComboBox();
            btnRefresh = new Button();
            lblPreset = new Label();
            cboPresets = new ComboBox();
            lblBrightness = new Label();
            trkBrightness = new TrackBar();
            lblBrightnessValue = new Label();
            lblContrast = new Label();
            trkContrast = new TrackBar();
            lblContrastValue = new Label();
            btnReset = new Button();
            btnResetAll = new Button();
            chkRestoreOnClose = new CheckBox();
            lblInfo = new Label();
            lblStatus = new Label();

            ((System.ComponentModel.ISupportInitialize)trkBrightness).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkContrast).BeginInit();
            SuspendLayout();

            // lblMonitor
            lblMonitor.AutoSize = true;
            lblMonitor.Location = new Point(14, 17);
            lblMonitor.Text = "Monitor:";

            // cboMonitors
            cboMonitors.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMonitors.Location = new Point(100, 14);
            cboMonitors.Size = new Size(424, 23);
            cboMonitors.SelectedIndexChanged += cboMonitors_SelectedIndexChanged;

            // lblWindow
            lblWindow.AutoSize = true;
            lblWindow.Location = new Point(14, 49);
            lblWindow.Text = "Target App:";

            // cboWindows
            cboWindows.DropDownStyle = ComboBoxStyle.DropDownList;
            cboWindows.Location = new Point(100, 46);
            cboWindows.Size = new Size(338, 23);
            cboWindows.SelectedIndexChanged += cboWindows_SelectedIndexChanged;

            // btnRefresh
            btnRefresh.Location = new Point(444, 45);
            btnRefresh.Size = new Size(80, 25);
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;

            // lblPreset
            lblPreset.AutoSize = true;
            lblPreset.Location = new Point(14, 81);
            lblPreset.Text = "Preset:";

            // cboPresets
            cboPresets.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPresets.Location = new Point(100, 78);
            cboPresets.Size = new Size(424, 23);
            cboPresets.MaxDropDownItems = 13;
            cboPresets.SelectedIndexChanged += cboPresets_SelectedIndexChanged;

            // lblBrightness
            lblBrightness.AutoSize = true;
            lblBrightness.Location = new Point(14, 130);
            lblBrightness.Text = "Brightness:";

            // trkBrightness
            trkBrightness.Location = new Point(100, 118);
            trkBrightness.Size = new Size(380, 45);
            trkBrightness.Minimum = -100;
            trkBrightness.Maximum = 100;
            trkBrightness.Value = 0;
            trkBrightness.TickFrequency = 10;
            trkBrightness.LargeChange = 10;
            trkBrightness.SmallChange = 1;
            trkBrightness.ValueChanged += trkBrightness_ValueChanged;

            // lblBrightnessValue
            lblBrightnessValue.Location = new Point(486, 130);
            lblBrightnessValue.Size = new Size(38, 15);
            lblBrightnessValue.Text = "0";
            lblBrightnessValue.TextAlign = ContentAlignment.MiddleRight;

            // lblContrast
            lblContrast.AutoSize = true;
            lblContrast.Location = new Point(14, 178);
            lblContrast.Text = "Contrast:";

            // trkContrast
            trkContrast.Location = new Point(100, 166);
            trkContrast.Size = new Size(380, 45);
            trkContrast.Minimum = -100;
            trkContrast.Maximum = 100;
            trkContrast.Value = 0;
            trkContrast.TickFrequency = 10;
            trkContrast.LargeChange = 10;
            trkContrast.SmallChange = 1;
            trkContrast.ValueChanged += trkContrast_ValueChanged;

            // lblContrastValue
            lblContrastValue.Location = new Point(486, 178);
            lblContrastValue.Size = new Size(38, 15);
            lblContrastValue.Text = "0";
            lblContrastValue.TextAlign = ContentAlignment.MiddleRight;

            // btnReset
            btnReset.Location = new Point(100, 222);
            btnReset.Size = new Size(130, 30);
            btnReset.Text = "Reset Selected";
            btnReset.UseVisualStyleBackColor = true;
            btnReset.Click += btnReset_Click;

            // btnResetAll
            btnResetAll.Location = new Point(240, 222);
            btnResetAll.Size = new Size(130, 30);
            btnResetAll.Text = "Reset All";
            btnResetAll.UseVisualStyleBackColor = true;
            btnResetAll.Click += btnResetAll_Click;

            // chkRestoreOnClose
            chkRestoreOnClose.AutoSize = true;
            chkRestoreOnClose.Checked = true;
            chkRestoreOnClose.CheckState = CheckState.Checked;
            chkRestoreOnClose.Location = new Point(100, 264);
            chkRestoreOnClose.Text = "Restore original gamma on exit";

            // lblInfo
            lblInfo.AutoSize = true;
            lblInfo.ForeColor = SystemColors.GrayText;
            lblInfo.Location = new Point(14, 302);
            lblInfo.MaximumSize = new Size(510, 0);
            lblInfo.Text = "Adjustments apply to the entire monitor (not per-application).\r\n"
                + "Use \u2018Target App\u2019 to auto-select the monitor for a running window.\r\n"
                + "Extreme values may be silently rejected by the display driver.";

            // lblStatus
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(14, 364);
            lblStatus.Text = "Ready.";

            // Form1
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(540, 392);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Gamma Tool";

            Controls.Add(lblMonitor);
            Controls.Add(cboMonitors);
            Controls.Add(lblWindow);
            Controls.Add(cboWindows);
            Controls.Add(btnRefresh);
            Controls.Add(lblPreset);
            Controls.Add(cboPresets);
            Controls.Add(lblBrightness);
            Controls.Add(trkBrightness);
            Controls.Add(lblBrightnessValue);
            Controls.Add(lblContrast);
            Controls.Add(trkContrast);
            Controls.Add(lblContrastValue);
            Controls.Add(btnReset);
            Controls.Add(btnResetAll);
            Controls.Add(chkRestoreOnClose);
            Controls.Add(lblInfo);
            Controls.Add(lblStatus);

            ((System.ComponentModel.ISupportInitialize)trkBrightness).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkContrast).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblMonitor;
        private ComboBox cboMonitors;
        private Label lblWindow;
        private ComboBox cboWindows;
        private Button btnRefresh;
        private Label lblPreset;
        private ComboBox cboPresets;
        private Label lblBrightness;
        private TrackBar trkBrightness;
        private Label lblBrightnessValue;
        private Label lblContrast;
        private TrackBar trkContrast;
        private Label lblContrastValue;
        private Button btnReset;
        private Button btnResetAll;
        private CheckBox chkRestoreOnClose;
        private Label lblInfo;
        private Label lblStatus;
    }
}
