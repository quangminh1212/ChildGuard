using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ChildGuard.UI
{
    public class WindowsMainForm : Form
    {
        private Panel sidebarPanel = default!;
        private Panel rightPanel = default!;
        private Label headerLabel = default!;
        private Panel contentPanel = default!;

        private Button btnDashboard = default!;
        private Button btnMonitoring = default!;
        private Button btnProtection = default!;
        private Button btnReports = default!;
        private Button btnSettings = default!;

        public WindowsMainForm()
        {
            Text = "ChildGuard";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1100, 700);
            MinimumSize = new Size(900, 600);

            DoubleBuffered = true;

            BuildLayout();
            WireEvents();

            // Default section (restore last section from config if available)
            try
            {
                var cfg = ChildGuard.Core.Configuration.ConfigManager.Load(out _);
                var sec = string.IsNullOrWhiteSpace(cfg.LastOpenUISection) ? "Dashboard" : cfg.LastOpenUISection;
                NavigateTo(sec);
            }
            catch { NavigateTo("Dashboard"); }
        }

        private void BuildLayout()
        {
            // Root: 2 columns (sidebar fixed, right fill)
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Controls.Add(root);

            // Sidebar
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SystemColors.ControlLight,
                Padding = new Padding(8)
            };
            root.Controls.Add(sidebarPanel, 0, 0);

            // Right area: header + content (vertical)
            rightPanel = new Panel { Dock = DockStyle.Fill, BackColor = SystemColors.Window };
            root.Controls.Add(rightPanel, 1, 0);

            var rightTlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
            };
            rightTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // header
            rightTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // content
            rightPanel.Controls.Add(rightTlp);

            headerLabel = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(8, 12, 8, 8),
                ForeColor = SystemColors.WindowText
            };
            rightTlp.Controls.Add(headerLabel, 0, 0);

            contentPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(16) };
            rightTlp.Controls.Add(contentPanel, 0, 1);

            BuildSidebar();
        }

        private void BuildSidebar()
        {
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0),
            };
            sidebarPanel.Controls.Add(flow);

            btnDashboard = CreateNavButton("🏠  Dashboard");
            btnMonitoring = CreateNavButton("👁️  Monitoring");
            btnProtection = CreateNavButton("🛡️  Protection");
            btnReports = CreateNavButton("📊  Reports");
            btnSettings = CreateNavButton("⚙️  Settings");

            flow.Controls.AddRange(new Control[] { btnDashboard, btnMonitoring, btnProtection, btnReports, btnSettings });

            // Active highlight behavior
            void SetActive(Button b)
            {
                foreach (var ctrl in flow.Controls.OfType<Button>())
                {
                    ctrl.BackColor = SystemColors.Control;
                    ctrl.Font = new Font(ctrl.Font, FontStyle.Regular);
                }
                b.BackColor = Color.FromArgb(210, 225, 255);
                b.Font = new Font(b.Font, FontStyle.Bold);
            }

            btnDashboard.Click += (s, e) => SetActive(btnDashboard);
            btnMonitoring.Click += (s, e) => SetActive(btnMonitoring);
            btnProtection.Click += (s, e) => SetActive(btnProtection);
            btnReports.Click += (s, e) => SetActive(btnReports);
            btnSettings.Click += (s, e) => SetActive(btnSettings);

            // Default highlight
            SetActive(btnDashboard);
        }

        private Button CreateNavButton(string text)
        {
            return new Button
            {
                Text = text,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Width = 180,
                Height = 40,
                Margin = new Padding(8, 4, 8, 4),
                FlatStyle = FlatStyle.System,
            };
        }

        private void WireEvents()
        {
            btnDashboard.Click += (s, e) => NavigateTo("Dashboard");
            btnMonitoring.Click += (s, e) => NavigateTo("Monitoring");
            btnProtection.Click += (s, e) => NavigateTo("Protection");
            btnReports.Click += (s, e) => NavigateTo("Reports");
            btnSettings.Click += (s, e) => NavigateTo("Settings");
        }

        public void NavigateTo(string section)
        {
            if (string.IsNullOrWhiteSpace(section)) return;
            section = section.Trim();
            ChildGuard.Core.Diagnostics.SimpleLogger.Info("WindowsMainForm.NavigateTo: {0}", section);
            headerLabel.Text = section;
            Text = $"ChildGuard • {section}";

            contentPanel.SuspendLayout();
            try
            {
                // Persist last opened section
                try
                {
                    var cfg = ChildGuard.Core.Configuration.ConfigManager.Load(out _);
                    cfg.LastOpenUISection = section;
                    ChildGuard.Core.Configuration.ConfigManager.Save(cfg, out _);
                }
                catch { }

                // Dispose previously embedded forms/controls to avoid leaks
                foreach (Control c in contentPanel.Controls)
                {
                    try
                    {
                        if (c is Form f)
                        {
                            f.Close();
                            f.Dispose();
                        }
                        else
                        {
                            c.Dispose();
                        }
                    }
                    catch { }
                }

                contentPanel.Controls.Clear();
                switch (section)
                {
                    case "Dashboard":
                        LoadDashboard();
                        break;
                    case "Monitoring":
                        LoadMonitoring();
                        break;
                    case "Protection":
                        LoadProtection();
                        break;
                    case "Reports":
                        LoadReports();
                        break;
                    case "Settings":
                        LoadSettings();
                        break;
                }
            }
            finally
            {
                contentPanel.ResumeLayout(true);
            }
        }

        private void LoadDashboard()
        {
            // Quick actions row (Windows-like simple buttons)
            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
            // If navigating to Monitoring, propagate type filter from Reports combo when available later

                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0),
                Margin = new Padding(0, 0, 0, 8)
            };

            actions.Controls.Add(new Button { Text = "Start Protection", AutoSize = true, Margin = new Padding(0, 0, 8, 8) });
            actions.Controls.Add(new Button { Text = "Stop Protection", AutoSize = true, Margin = new Padding(0, 0, 8, 8) });
            actions.Controls.Add(new Button { Text = "Quick Scan", AutoSize = true, Margin = new Padding(0, 0, 8, 8) });

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlp.Controls.Add(new Label { Text = "Quick Actions", Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 0, 0, 6) });
            tlp.Controls.Add(actions, 0, 1);

            contentPanel.Controls.Add(tlp);
        }

        private void LoadMonitoring()
        {
            // Live log viewer + quick link to reports
            var live = new Controls.LiveLogViewer { Dock = DockStyle.Fill };
            live.OnOpenReports += (s, e) => NavigateTo("Reports");
            // Try to start with current config
            try
            {
                var cfg = ChildGuard.Core.Configuration.ConfigManager.Load(out _);
                live.Start(cfg);
            }
            catch { live.Start(); }

            contentPanel.Controls.Add(live);
        }
        private void LoadProtection()
        {
            // Reuse the advanced protection UI inside the content panel
            EmbedFormInContent(new EnhancedForm1());
        }
        private void LoadReports()
        {
            EmbedFormInContent(new ReportsForm());
        }
        private void LoadSettings()
        {
            EmbedFormInContent(new SettingsForm());
        }

        private void EmbedFormInContent(Form form)
        {
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.StartPosition = FormStartPosition.Manual;
            // Ensure consistent background
            try { form.BackColor = contentPanel.BackColor; } catch { }
            contentPanel.Controls.Add(form);
            form.Show();
        }

        private void AddPlaceholder(string text)
        {
            var lbl = new Label { Text = text, AutoSize = true, Font = new Font("Segoe UI", 11), ForeColor = SystemColors.GrayText };
            var host = new Panel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            host.Controls.Add(lbl);
            contentPanel.Controls.Add(host);
        }
    }
}

