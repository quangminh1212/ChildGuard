using ChildGuard.Core.Configuration;
using ChildGuard.Core.Models;
using ChildGuard.Hooking;
using ChildGuard.UI.Localization;
using ChildGuard.UI.Theming;

namespace ChildGuard.UI;

public partial class Form1 : Form
{
    private readonly AdvancedProtectionManager _protectionManager = new();
    private long _lastKeys = 0;
    private long _lastMouse = 0;
    private volatile bool _running;
    private AppConfig _cfg = new();

    public Form1()
    {
        InitializeComponent();
        _cfg = ConfigManager.Load(out _);
        UIStrings.SetLanguage(_cfg.UILanguage);

        SetupSimpleUI();
        ApplyLocalization();

        uiTimer.Start();
    }

    private void SetupSimpleUI()
    {
        // Ẩn các control cũ
        lblKeys.Visible = false;
        lblMouse.Visible = false;
        chkEnableInput.Visible = false;
        btnStart.Visible = false;
        btnStop.Visible = false;

        // Thiết lập form đơn giản
        this.BackColor = Color.White;
        this.Font = new Font("Segoe UI", 9F);
        this.Size = new Size(500, 400);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;

        // Thêm icon nếu có
        try
        {
            var iconPath = Path.Combine(Application.StartupPath, "Assets", "app.ico");
            if (File.Exists(iconPath))
                this.Icon = new Icon(iconPath);
        }
        catch { }

        // Panel chính
        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(30),
            BackColor = Color.White
        };

        // Tiêu đề
        var titleLabel = new Label
        {
            Text = "ChildGuard Monitor",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 51, 51),
            AutoSize = true,
            Location = new Point(0, 0)
        };

        // Thống kê đơn giản
        var statsPanel = new Panel
        {
            Location = new Point(0, 50),
            Size = new Size(400, 80),
            BackColor = Color.FromArgb(248, 249, 250),
            BorderStyle = BorderStyle.FixedSingle
        };

        var keysLabel = new Label
        {
            Text = "Phím bấm:",
            Font = new Font("Segoe UI", 10),
            Location = new Point(15, 15),
            AutoSize = true
        };

        var keysValue = new Label
        {
            Name = "keysValue",
            Text = "0",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 120, 212),
            Location = new Point(100, 13),
            AutoSize = true
        };

        var mouseLabel = new Label
        {
            Text = "Chuột:",
            Font = new Font("Segoe UI", 10),
            Location = new Point(15, 45),
            AutoSize = true
        };

        var mouseValue = new Label
        {
            Name = "mouseValue",
            Text = "0",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(16, 124, 16),
            Location = new Point(100, 43),
            AutoSize = true
        };

        statsPanel.Controls.AddRange(new Control[] { keysLabel, keysValue, mouseLabel, mouseValue });

        // Checkbox đơn giản
        var enableCheck = new CheckBox
        {
            Name = "enableCheck",
            Text = "Bật giám sát",
            Font = new Font("Segoe UI", 10),
            Location = new Point(0, 150),
            AutoSize = true,
            Checked = true
        };

        // Nút điều khiển với hiệu ứng hover
        var startButton = new Button
        {
            Name = "startButton",
            Text = "Bắt đầu",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(100, 35),
            Location = new Point(0, 190),
            BackColor = Color.FromArgb(0, 120, 212),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        startButton.FlatAppearance.BorderSize = 0;

        // Hiệu ứng hover cho nút Start
        startButton.MouseEnter += (s, e) => {
            startButton.BackColor = Color.FromArgb(0, 100, 180);
        };
        startButton.MouseLeave += (s, e) => {
            startButton.BackColor = Color.FromArgb(0, 120, 212);
        };

        var stopButton = new Button
        {
            Name = "stopButton",
            Text = "Dừng",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(100, 35),
            Location = new Point(110, 190),
            BackColor = Color.FromArgb(200, 200, 200),
            ForeColor = Color.FromArgb(51, 51, 51),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        stopButton.FlatAppearance.BorderSize = 0;

        // Hiệu ứng hover cho nút Stop
        stopButton.MouseEnter += (s, e) => {
            stopButton.BackColor = Color.FromArgb(180, 180, 180);
        };
        stopButton.MouseLeave += (s, e) => {
            stopButton.BackColor = Color.FromArgb(200, 200, 200);
        };

        // Trạng thái
        var statusLabel = new Label
        {
            Name = "statusLabel",
            Text = "Sẵn sàng",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(100, 100, 100),
            Location = new Point(0, 240),
            AutoSize = true
        };

        // Thêm sự kiện
        startButton.Click += (s, e) => {
            if (_running) return;
            var cfg = new AppConfig { EnableInputMonitoring = enableCheck.Checked };
            _protectionManager.Start(cfg);
            _running = true;
            statusLabel.Text = "Đang chạy...";
            statusLabel.ForeColor = Color.FromArgb(16, 124, 16);
        };

        stopButton.Click += (s, e) => {
            if (!_running) return;
            _protectionManager.Stop();
            _running = false;
            statusLabel.Text = "Đã dừng";
            statusLabel.ForeColor = Color.FromArgb(200, 50, 50);
        };

        // Thêm tooltip
        var toolTip = new ToolTip();
        toolTip.SetToolTip(enableCheck, "Bật/tắt giám sát hoạt động bàn phím và chuột");
        toolTip.SetToolTip(startButton, "Bắt đầu giám sát hệ thống");
        toolTip.SetToolTip(stopButton, "Dừng giám sát hệ thống");
        toolTip.SetToolTip(statsPanel, "Thống kê hoạt động real-time");

        // Thêm controls vào panel
        mainPanel.Controls.AddRange(new Control[] {
            titleLabel, statsPanel, enableCheck, startButton, stopButton, statusLabel
        });

        // Thêm panel vào form
        this.Controls.Add(mainPanel);
        mainPanel.BringToFront();
        if (menuStrip1 != null) menuStrip1.BringToFront();
    }

    private void uiTimer_Tick(object? sender, EventArgs e)
    {
        var keysValue = this.Controls.Find("keysValue", true).FirstOrDefault() as Label;
        if (keysValue != null)
        {
            var newText = _lastKeys.ToString("N0");
            if (keysValue.Text != newText)
            {
                keysValue.Text = newText;
                // Hiệu ứng nhấp nháy nhẹ khi có thay đổi
                keysValue.ForeColor = Color.FromArgb(0, 150, 255);
                var timer = new System.Windows.Forms.Timer { Interval = 200 };
                timer.Tick += (s, args) => {
                    keysValue.ForeColor = Color.FromArgb(0, 120, 212);
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }
        }

        var mouseValue = this.Controls.Find("mouseValue", true).FirstOrDefault() as Label;
        if (mouseValue != null)
        {
            var newText = _lastMouse.ToString("N0");
            if (mouseValue.Text != newText)
            {
                mouseValue.Text = newText;
                // Hiệu ứng nhấp nháy nhẹ khi có thay đổi
                mouseValue.ForeColor = Color.FromArgb(20, 150, 20);
                var timer = new System.Windows.Forms.Timer { Interval = 200 };
                timer.Tick += (s, args) => {
                    mouseValue.ForeColor = Color.FromArgb(16, 124, 16);
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }
        }
    }

    private void mnuSettings_Click(object? sender, EventArgs e)
    {
        using var dlg = new SettingsForm();
        dlg.ShowDialog(this);
    }

    private void mnuReports_Click(object? sender, EventArgs e)
    {
        using var dlg = new ReportsForm();
        dlg.ShowDialog(this);
    }

    private void mnuPolicy_Click(object? sender, EventArgs e)
    {
        using var dlg = new PolicyEditorForm();
        dlg.ShowDialog(this);
    }
    private void ApplyLocalization()
    {
        this.Text = "ChildGuard - Giám sát bảo vệ trẻ em";
        try { this.MainMenuStrip!.Font = new Font("Segoe UI", 10F); } catch { }
        if (mnuSettings != null) mnuSettings.Text = "Cài đặt";
        if (mnuReports != null) mnuReports.Text = "Báo cáo";
        if (mnuPolicy != null) mnuPolicy.Text = "Chính sách";

        EnsureHelpMenu();
    }

    private void EnsureHelpMenu()
    {
        try
        {
            if (menuStrip1 == null) return;

            // Avoid duplicate
            var exists = this.menuStrip1.Items.OfType<ToolStripMenuItem>().Any(mi => (string?)mi.Tag == "help");
            if (exists) return;

            var help = new ToolStripMenuItem { Name = "mnuHelp", Tag = "help", Text = "Trợ giúp" };
            var about = new ToolStripMenuItem { Name = "mnuAbout", Text = "Giới thiệu" };
            about.Click += (s, e) =>
            {
                try { using var dlg = new AboutForm(); dlg.ShowDialog(this); } catch { }
            };
            help.DropDownItems.Add(about);
            this.menuStrip1.Items.Add(help);
        }
        catch { }
    }

    private void btnStart_Click(object? sender, EventArgs e)
    {
        if (_running) return;
        var enableCheck = this.Controls.Find("enableCheck", true).FirstOrDefault() as CheckBox;
        var cfg = new AppConfig { EnableInputMonitoring = enableCheck?.Checked ?? false };
        _protectionManager.Start(cfg);
        _running = true;

        var statusLabel = this.Controls.Find("statusLabel", true).FirstOrDefault() as Label;
        if (statusLabel != null)
        {
            statusLabel.Text = "Đang chạy...";
            statusLabel.ForeColor = Color.FromArgb(16, 124, 16);
        }
    }

    private void btnStop_Click(object? sender, EventArgs e)
    {
        if (!_running) return;
        _protectionManager.Stop();
        _running = false;

        var statusLabel = this.Controls.Find("statusLabel", true).FirstOrDefault() as Label;
        if (statusLabel != null)
        {
            statusLabel.Text = "Đã dừng";
            statusLabel.ForeColor = Color.FromArgb(200, 50, 50);
        }
    }
}
