using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;
using ChildGuard.Core.Configuration;
using ChildGuard.Core.Models;
using ChildGuard.Hooking;
using ChildGuard.UI.Controls;
using ChildGuard.UI.Localization;
using ChildGuard.UI.Theming;
using System.ComponentModel;

using System.IO;

namespace ChildGuard.UI
{
    /// <summary>
    /// Form chính với giao diện hiện đại theo phong cách Windows 11 và Facebook
    /// </summary>
    public partial class ModernMainForm : Form
    {
        // Panels
        private Panel sidebarPanel = default!;
        private Panel headerPanel = default!;
        private Panel contentPanel = default!;
        // Removed unused currentContentPanel field to eliminate warnings

        // Header controls
        private Label titleLabel = default!;
        private PictureBox logoImage = default!;
        private ModernButton profileButton = default!;

        // Sidebar items
        private List<SidebarItem> sidebarItems = default!;
        private SidebarItem? activeSidebarItem;

        // Protection
        private readonly AdvancedProtectionManager _protectionManager = new();
        private volatile bool _running;
        private AppConfig _config = new();

        // Stats
        private long _lastKeys;
        private long _lastMouse;
        private long _threatsDetected;

        // Timers
        private System.Windows.Forms.Timer updateTimer = default!;
        private System.Windows.Forms.Timer animationTimer = default!;

#if DEBUG
        // Debug status strip for visual overlap diagnostics
        private StatusStrip? debugStatusStrip;
        private ToolStripStatusLabel? debugStatusLabel;
#endif

        // Activity log
        private ListBox? activityListBox;
        private readonly ConcurrentQueue<string> _logQueue = new();

        public ModernMainForm()
        {
            InitializeForm();
            InitializeComponents();
            LoadConfiguration();
            SetupEventHandlers();
            ApplyTheme();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Normalize and ensure DataDirectory exists after config is loaded
            NormalizeDataDirectory();
#if DEBUG
            if (object.Equals(this.Tag, "DEBUG_UI"))
            {
                ChildGuard.UI.Diagnostics.OverlapDiagnostics.Attach(contentPanel, this, msg =>
                {
                    try
                    {
                        var baseTitle = this.Text;
                        var diagIndex = baseTitle.IndexOf(" [DIAG]", StringComparison.OrdinalIgnoreCase);
                        if (diagIndex >= 0) baseTitle = baseTitle.Substring(0, diagIndex);
                        this.Text = $"{baseTitle}  [DIAG] {msg}";
                        if (debugStatusLabel != null) debugStatusLabel.Text = msg;
                    }
                    catch { }
                });
            }
#endif
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                updateTimer?.Stop();
                animationTimer?.Stop();
                if (_running)
                {
                    try { _protectionManager.Stop(); } catch { }
                    _running = false;
                }
                _protectionManager.OnActivity -= OnActivity;
                _protectionManager.OnStatisticsUpdated -= OnStatisticsUpdated;
                updateTimer?.Dispose();
                animationTimer?.Dispose();
            }
            catch { }
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    updateTimer?.Stop();

                    animationTimer?.Stop();
                    _protectionManager.OnActivity -= OnActivity;
                    _protectionManager.OnStatisticsUpdated -= OnStatisticsUpdated;
                    updateTimer?.Dispose();
                    animationTimer?.Dispose();
                    activityListBox?.Dispose();
                    logoImage?.Image?.Dispose();
                    logoImage?.Dispose();
                    profileButton?.Dispose();
                    titleLabel?.Dispose();
                    headerPanel?.Dispose();
                    sidebarPanel?.Dispose();
                    contentPanel?.Dispose();
                }
                catch { }
            }
            base.Dispose(disposing);
        }


        private void InitializeForm()
        {
            this.Text = "ChildGuard Protection";
            this.Size = new Size(1200, 700);
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.BackColor = ColorScheme.Modern.Background;
            this.AutoScaleMode = AutoScaleMode.Dpi;

            // Enable double buffering for smooth animations
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.OptimizedDoubleBuffer, true);
        }

        private void InitializeComponents()
        {
            // Add in proper dock order: header (Top), sidebar (Left), then content (Fill) LAST
            // Docking uses z-order; adding Fill last ensures it occupies the remaining space below/after others.
            CreateHeaderPanel();
            CreateSidebarPanel();
            CreateContentPanel();

            // After contentPanel created, ensure default section selected if none
            if (activeSidebarItem == null && sidebarItems != null && sidebarItems.Count > 0)
            {
                var defaultItem = sidebarItems.FirstOrDefault(i => i.Text == "Dashboard") ?? sidebarItems[0];
                SetActiveSidebarItem(defaultItem);
            }

#if DEBUG
            if (object.Equals(this.Tag, "DEBUG_UI"))
            {
                CreateDebugStatusBar();
            }
#endif

            // Initialize timers
            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 1000;
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();

            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 10;
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
        }

#if DEBUG
        private void CreateDebugStatusBar()
        {
            try
            {
                debugStatusStrip = new StatusStrip
                {
                    SizingGrip = false,
                    Dock = DockStyle.Bottom,
                    Visible = true
                };
                debugStatusLabel = new ToolStripStatusLabel { Text = "[DIAG] Ready" };
                debugStatusStrip.Items.Add(debugStatusLabel);
                this.Controls.Add(debugStatusStrip);
                debugStatusStrip.BringToFront();
            }
            catch { }
        }
#endif

        private void CreateHeaderPanel()
        {
            headerPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = ColorScheme.Modern.Surface
            };

            // Right container to avoid overlap with title
            var rightPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 140,
                BackColor = Color.Transparent
            };

            // Profile button (right side)
            profileButton = new ModernButton
            {
                Text = "Admin",
                Size = new Size(100, 36),
                Style = ModernButton.ButtonStyle.Ghost,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            profileButton.Click += ProfileButton_Click;
            rightPanel.Controls.Add(profileButton);
            rightPanel.Resize += (s, e) =>
            {
                // Center vertically, stick to right inside the rightPanel
                profileButton.Location = new Point(rightPanel.Width - profileButton.Width - 12, (rightPanel.Height - profileButton.Height) / 2);
            };

            // Left container hosts logo + title
            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            // Logo
            logoImage = new PictureBox
            {
                Size = new Size(40, 40),
                Location = new Point(15, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            logoImage.Image = CreateLogo();
            leftPanel.Controls.Add(logoImage);

            // Title
            titleLabel = new Label
            {
                Text = "ChildGuard Protection",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                Location = new Point(65, 18),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            leftPanel.Controls.Add(titleLabel);

            // Shadow at bottom
            headerPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using (var shadowBrush = new LinearGradientBrush(
                    new Point(0, headerPanel.Height - 4),
                    new Point(0, headerPanel.Height),
                    ColorScheme.Modern.ShadowLight,
                    Color.Transparent))
                {
                    g.FillRectangle(shadowBrush, 0, headerPanel.Height - 4, headerPanel.Width, 4);
                }
            };

            headerPanel.Controls.Add(leftPanel);
            headerPanel.Controls.Add(rightPanel);
            this.Controls.Add(headerPanel);
        }

        private void CreateSidebarPanel()
        {
            sidebarPanel = new Panel
            {
                Width = 240,
                Dock = DockStyle.Left,
                BackColor = ColorScheme.Modern.Surface,
                Padding = new Padding(0, 10, 0, 10)
            };

            // Add separator line
            sidebarPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using (var pen = new Pen(ColorScheme.Modern.Border, 1))
                {
                    g.DrawLine(pen, sidebarPanel.Width - 1, 0, sidebarPanel.Width - 1, sidebarPanel.Height);
                }
            };

            // Create sidebar items
            sidebarItems = new List<SidebarItem>();

            var dashboardItem = CreateSidebarItem("Dashboard", "🏠", 0);
            var monitoringItem = CreateSidebarItem("Monitoring", "👁", 1);
            var protectionItem = CreateSidebarItem("Protection", "🛡", 2);
            var reportsItem = CreateSidebarItem("Reports", "📊", 3);
            var settingsItem = CreateSidebarItem("Settings", "⚙", 4);

            // Set dashboard as active by default (only if contentPanel already exists)
            if (contentPanel != null)
            {
                SetActiveSidebarItem(dashboardItem);
            }

            this.Controls.Add(sidebarPanel);
        }

        private SidebarItem CreateSidebarItem(string text, string icon, int index)
        {
            var item = new SidebarItem
            {
                Text = text,
                Icon = icon,
                Index = index,
                Size = new Size(220, 48),
                Location = new Point(10, 10 + (index * 52))
            };

            item.Click += (s, e) => SetActiveSidebarItem(item);

            sidebarPanel.Controls.Add(item);
            sidebarItems.Add(item);

            return item;
        }

        private void SetActiveSidebarItem(SidebarItem item)
        {
            if (activeSidebarItem != null)
            {
                activeSidebarItem.IsActive = false;
            }

            item.IsActive = true;
            activeSidebarItem = item;

            // Load corresponding content
            LoadContent(item.Text);
            // Update header title and window title
            string t = $"ChildGuard • {item.Text}";
            titleLabel.Text = t;
            this.Text = t;
        }

        // Navigate to a section from outside (e.g., Program args)
        public void NavigateTo(string section)
        {
            if (string.IsNullOrWhiteSpace(section)) return;
            ChildGuard.Core.Diagnostics.SimpleLogger.Info("ModernMainForm.NavigateTo: {0}", section);
            var target = sidebarItems?.FirstOrDefault(i => string.Equals(i.Text, section, StringComparison.OrdinalIgnoreCase));
            if (target != null)
            {
                SetActiveSidebarItem(target);
            }
            else
            {
                LoadContent(section);
            }
        }

        private void CreateContentPanel()
        {
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorScheme.Modern.Background,
                Padding = new Padding(30),
                AutoScroll = true
            };

            this.Controls.Add(contentPanel);
        }

        private void LoadContent(string section)
        {
            // Temporarily stop periodic UI updates while switching content to avoid
            // touching disposed controls mid-transition
            bool wasUpdating = false;
            try { wasUpdating = updateTimer?.Enabled ?? false; updateTimer?.Stop(); } catch { }

            contentPanel.SuspendLayout();
            try
            {
                // Dispose old controls to avoid timers/events firing on disposed objects
                foreach (Control c in contentPanel.Controls)
                {
                    try { c.Dispose(); } catch { }
                }
                contentPanel.Controls.Clear();
                activityListBox = null; // drop reference to old listbox if any

                switch (section)
                {
                    case "Dashboard":
                        LoadDashboardContent();
                        break;
                    case "Monitoring":
                        LoadMonitoringContent();
                        break;
                    case "Protection":
                        LoadProtectionContent();
                        break;
                    case "Reports":
                        LoadReportsContent();
                        break;
                    case "Settings":
                        LoadSettingsContent();
                        break;
                }
            }
            finally
            {
                contentPanel.ResumeLayout(true);
                // Resume timer if it was running
                try { if (wasUpdating) updateTimer?.Start(); } catch { }
            }

            // Also update header title if sidebar not used (external navigate)
            if (!string.IsNullOrWhiteSpace(section))
            {
                string t = $"ChildGuard • {section}";
                titleLabel.Text = t;
                this.Text = t;
            }

            // DEBUG check overlays
            ValidateNoOverlap();
        }
        // Build responsive grid for cards: switches to a table grid based on available width
        private void BuildResponsiveCardGrid(Panel container, List<Control> cards, int maxColumns, Size cardSize, int hSpacing, int vSpacing)
        {
            // Remove any previous grid panel
            var existing = container.Controls.OfType<TableLayoutPanel>().FirstOrDefault(p => p.Tag as string == "CardGrid");
            if (existing != null)
            {
                container.Controls.Remove(existing);
                existing.Dispose();
            }

            // Compute columns based on available width
            int available = container.ClientSize.Width - container.Padding.Horizontal - 20; // rough padding
            int colWidth = cardSize.Width + hSpacing;
            int cols = Math.Max(1, Math.Min(maxColumns, available > 0 ? Math.Max(1, available / colWidth) : 1));

            var grid = new TableLayoutPanel
            {
                Tag = "CardGrid",
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = cols,
                RowCount = (int)Math.Ceiling(cards.Count / (double)cols),
                Margin = new Padding(0, 8, 0, 8)
            };
            grid.ColumnStyles.Clear();
            for (int i = 0; i < cols; i++)
            {
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cols));
            }

            grid.RowStyles.Clear();
            for (int r = 0; r < grid.RowCount; r++)
            {
                grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            int index = 0;
            for (int r = 0; r < grid.RowCount; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (index >= cards.Count) break;
                    var card = cards[index++];
                    card.Width = cardSize.Width;
                    card.Height = cardSize.Height;
                    card.Margin = new Padding(c == cols - 1 ? 0 : hSpacing, 0, 0, vSpacing);
                    grid.Controls.Add(card, c, r);
                }
            }

            container.Controls.Add(grid);
            grid.BringToFront();
        }


        private void LoadDashboardContent()
        {
            var dashboardPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            // Header
            var hdr = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };

            // Cards
            var statusCard = new ModernHeaderCard { Title = "Protection Status", Subtitle = _running ? "Active" : "Inactive", Size = new Size(250, 100), Margin = new Padding(0, 0, 20, 10) };
            var threatsCard = new ModernHeaderCard { Title = "Threats Detected", Subtitle = _threatsDetected.ToString(), Size = new Size(250, 100), Margin = new Padding(0, 0, 20, 10) };
            var activityCard = new ModernHeaderCard { Title = "System Activity", Subtitle = "Normal", Size = new Size(250, 100), Margin = new Padding(0, 0, 20, 10) };
            var dashboardCards = new List<Control> { statusCard, threatsCard, activityCard };

            // Actions
            var actionsLabel = new Label { Text = "Quick Actions", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = ColorScheme.MaterialFluent.TextPrimary, AutoSize = true, Margin = new Padding(0, 8, 0, 4) };
            var actionsPanel = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
            var startButton = new MaterialButton { Text = "Start Protection", Size = new Size(150, 40), Style = MaterialButton.ButtonStyle.Primary, Margin = new Padding(0, 0, 10, 10) }; startButton.Click += (s, e) => StartProtection(); actionsPanel.Controls.Add(startButton);
            var stopButton = new MaterialButton  { Text = "Stop Protection",  Size = new Size(150, 40), Style = MaterialButton.ButtonStyle.Danger,  Margin = new Padding(0, 0, 10, 10) }; stopButton.Click  += (s, e) => StopProtection(); actionsPanel.Controls.Add(stopButton);
            var scanButton = new MaterialButton  { Text = "Quick Scan",       Size = new Size(150, 40), Style = MaterialButton.ButtonStyle.Secondary, Margin = new Padding(0, 0, 10, 10) }; actionsPanel.Controls.Add(scanButton);

            // Grid host for responsive cards
            var gridHost = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };
            BuildResponsiveCardGrid(gridHost, dashboardCards, maxColumns: 3, cardSize: new Size(250, 100), hSpacing: 20, vSpacing: 10);
            dashboardPanel.Resize += (s, e) => BuildResponsiveCardGrid(gridHost, dashboardCards, 3, new Size(250, 100), 20, 10);

            // Vertical layout via TableLayoutPanel to avoid Dock stacking issues
            var tlp = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, RowCount = 4, Margin = new Padding(0) };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // header
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // grid
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // actions label
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // actions panel
            tlp.Controls.Add(hdr, 0, 0);
            tlp.Controls.Add(gridHost, 0, 1);
            tlp.Controls.Add(actionsLabel, 0, 2);
            tlp.Controls.Add(actionsPanel, 0, 3);

            dashboardPanel.Controls.Add(tlp);
            contentPanel.Controls.Add(dashboardPanel);
        }

        private void LoadMonitoringContent()
        {
            var monitoringPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            // Header
            var hdr = new Label { Text = "Real-time Monitoring", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = ColorScheme.Modern.TextPrimary, AutoSize = true, Margin = new Padding(0, 0, 0, 8) };

            // Cards
            var keyboardCard = new ModernCard { Size = new Size(240, 130), Margin = new Padding(0, 0, 20, 10) };
            keyboardCard.Controls.Add(new PictureBox { Size = new Size(48, 48), Location = new Point(20, 20), SizeMode = PictureBoxSizeMode.Zoom, Image = CreateIcon("⌨", 40, ColorScheme.Modern.Primary) });
            keyboardCard.Controls.Add(new Label { Text = "Keyboard", Font = new Font("Segoe UI", 10), ForeColor = ColorScheme.Modern.TextSecondary, Location = new Point(80, 25), AutoSize = true });
            keyboardCard.Controls.Add(new Label { Name = "keyValueLabel", Text = _lastKeys.ToString("N0"), Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = ColorScheme.Modern.TextPrimary, Location = new Point(80, 45), AutoSize = true });

            var mouseCard = new ModernCard { Size = new Size(240, 130), Margin = new Padding(0, 0, 20, 10) };
            mouseCard.Controls.Add(new PictureBox { Size = new Size(48, 48), Location = new Point(20, 20), SizeMode = PictureBoxSizeMode.Zoom, Image = CreateIcon("🖱", 40, ColorScheme.Modern.Success) });
            mouseCard.Controls.Add(new Label { Text = "Mouse", Font = new Font("Segoe UI", 10), ForeColor = ColorScheme.Modern.TextSecondary, Location = new Point(80, 25), AutoSize = true });
            mouseCard.Controls.Add(new Label { Name = "mouseValueLabel", Text = _lastMouse.ToString("N0"), Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = ColorScheme.Modern.TextPrimary, Location = new Point(80, 45), AutoSize = true });

            var tlp = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, RowCount = 4, Margin = new Padding(0) };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // header
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // grid
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // log label
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // log fill

            tlp.Controls.Add(hdr, 0, 0);

            // Grid host
            var monitoringCards = new List<Control> { keyboardCard, mouseCard };
            var gridHost = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };
            BuildResponsiveCardGrid(gridHost, monitoringCards, maxColumns: 3, cardSize: new Size(240, 130), hSpacing: 20, vSpacing: 10);
            monitoringPanel.Resize += (s, e) => BuildResponsiveCardGrid(gridHost, monitoringCards, 3, new Size(240, 130), 20, 10);
            tlp.Controls.Add(gridHost, 0, 1);

            var logLabel = new Label { Text = "Activity Log", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = ColorScheme.Modern.TextPrimary, AutoSize = true, Margin = new Padding(0, 8, 0, 4) };
            tlp.Controls.Add(logLabel, 0, 2);

            var logCard = new ModernCard { Dock = DockStyle.Fill };
            var logListBox = new ListBox { Name = "activityListBox", Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, Font = new Font("Consolas", 9), BackColor = ColorScheme.Modern.Surface, ForeColor = ColorScheme.Modern.TextPrimary };
            activityListBox = logListBox;
            logCard.Controls.Add(logListBox);
            tlp.Controls.Add(logCard, 0, 3);

            monitoringPanel.Controls.Add(tlp);
            contentPanel.Controls.Add(monitoringPanel);
        }

        private void LoadProtectionContent()
        {
            var protectionPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            var hdr = new Label { Text = "Protection Settings", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = ColorScheme.Modern.TextPrimary, AutoSize = true, Margin = new Padding(0, 0, 0, 8) };

            var optionsCard = new ModernCard { Dock = DockStyle.Top, Padding = new Padding(10) };

            var options = new[]
            {
                ("Real-time Protection", "Monitor and block threats in real-time"),
                ("Web Protection", "Block malicious websites and downloads"),
                ("Application Control", "Monitor and control application access"),
                ("Content Filtering", "Filter inappropriate content"),
                ("Screen Time Limits", "Set daily usage limits")
            };

            // Calculate height based on number of options (each ~70px + padding)
            optionsCard.Height = options.Length * 70 + 40;

            var tlp = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, Margin = new Padding(0) };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // header
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // options card
            tlp.Controls.Add(hdr, 0, 0);

            var optionsHost = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.TopDown, WrapContents = false };

            foreach (var (title, description) in options)
            {
                var optionPanel = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(20, 0, 20, 10) };
                optionPanel.Controls.Add(new ToggleSwitch { Location = new Point(0, 15), Checked = true });
                optionPanel.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(70, 10), AutoSize = true, ForeColor = ColorScheme.Modern.TextPrimary });
                optionPanel.Controls.Add(new Label { Text = description, Font = new Font("Segoe UI", 9), Location = new Point(70, 32), AutoSize = true, ForeColor = ColorScheme.Modern.TextSecondary });
                optionsHost.Controls.Add(optionPanel);
            }

            optionsCard.Controls.Add(optionsHost);
            tlp.Controls.Add(optionsCard, 0, 1);

            protectionPanel.Controls.Add(tlp);
            contentPanel.Controls.Add(protectionPanel);
        }

        private void LoadReportsContent()
        {
            var reportsPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            // Header
            var hdr = new Label { Text = "Reports & Analytics", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = ColorScheme.Modern.TextPrimary, AutoSize = true, Margin = new Padding(0, 0, 0, 8) };

            // Placeholder for charts/table filters
            var filters = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
            filters.Controls.Add(new Label { Text = "Date Range:", AutoSize = true, Margin = new Padding(0, 0, 8, 0) });
            filters.Controls.Add(new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList });

            var chartsHost = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };

            // Vertical TLP
            var tlp = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, Margin = new Padding(0) };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // header
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // filters
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // charts

            tlp.Controls.Add(hdr, 0, 0);
            tlp.Controls.Add(filters, 0, 1);
            tlp.Controls.Add(chartsHost, 0, 2);

            reportsPanel.Controls.Add(tlp);
            contentPanel.Controls.Add(reportsPanel);
        }

        private void LoadSettingsContent()
        {
            var settingsPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            // Header
            var hdr = new Label { Text = "Settings", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = ColorScheme.Modern.TextPrimary, AutoSize = true, Margin = new Padding(0, 0, 0, 8) };

            // Placeholder content split
            var general = new ModernCard { Dock = DockStyle.Top, Padding = new Padding(10), AutoSize = true };
            general.Controls.Add(new Label { Text = "Language", AutoSize = true, Location = new Point(10, 10) });
            general.Controls.Add(new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(120, 8) });

            var paths = new ModernCard { Dock = DockStyle.Top, Padding = new Padding(10), AutoSize = true };
            paths.Controls.Add(new Label { Text = "Data Directory", AutoSize = true, Location = new Point(10, 10) });
            paths.Controls.Add(new TextBox { Width = 340, Location = new Point(120, 8), ReadOnly = true, Text = _config.DataDirectory ?? string.Empty });

            var tlp = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, Margin = new Padding(0) };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // header
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // general
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // paths
            tlp.Controls.Add(hdr, 0, 0);
            tlp.Controls.Add(general, 0, 1);
            tlp.Controls.Add(paths, 0, 2);

            settingsPanel.Controls.Add(tlp);
            contentPanel.Controls.Add(settingsPanel);
        }

        private void NormalizeDataDirectory()
        {
            try
            {
                var dir = _config.DataDirectory ?? string.Empty;
                if (string.IsNullOrWhiteSpace(dir))
                {
                    dir = ConfigManager.GetLocalAppDataDir();
                }
                try { dir = Environment.ExpandEnvironmentVariables(dir); } catch { }
                try { dir = Path.GetFullPath(dir); } catch { }
                try { Directory.CreateDirectory(dir); } catch { }
                // Trim trailing separators for consistency
                _config.DataDirectory = dir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            catch { }
        }


        private void LoadConfiguration()
        {
            // Load config and ensure DataDirectory is valid
            _config = ConfigManager.Load(out var cfgPath);
            if (string.IsNullOrWhiteSpace(_config.DataDirectory))
            {
                try { _config.DataDirectory = Path.GetDirectoryName(cfgPath) ?? ConfigManager.GetLocalAppDataDir(); }
                catch { _config.DataDirectory = ConfigManager.GetLocalAppDataDir(); }
            }
            UIStrings.SetLanguage(_config.UILanguage);
        }

        private void ApplyTheme()
        {
            var isDark = ParseTheme(_config.Theme) == ThemeMode.Dark ||
                        (ParseTheme(_config.Theme) == ThemeMode.System && ThemeHelper.IsSystemDark());

            if (isDark)
            {
                // Apply dark theme colors
                this.BackColor = ColorScheme.Windows11Dark.Background;
                headerPanel.BackColor = ColorScheme.Windows11Dark.Surface;
                sidebarPanel.BackColor = ColorScheme.Windows11Dark.Surface;
                contentPanel.BackColor = ColorScheme.Windows11Dark.Background;
            }
            else
            {
                // Apply light theme colors (Modern scheme)
                this.BackColor = ColorScheme.Modern.Background;
                headerPanel.BackColor = ColorScheme.Modern.Surface;
                sidebarPanel.BackColor = ColorScheme.Modern.Surface;
                contentPanel.BackColor = ColorScheme.Modern.Background;
            }

            // Apply broader modern style (fonts, menu, etc.)
            ModernStyle.Apply(this, ParseTheme(_config.Theme));
        }

        private static ThemeMode ParseTheme(string? s)
        {
            return (s?.ToLowerInvariant()) switch
            {
                "dark" => ThemeMode.Dark,
                "light" => ThemeMode.Light,
                _ => ThemeMode.System
            };
        }

        private void SetupEventHandlers()
        {
            _protectionManager.OnActivity += OnActivity;
            _protectionManager.OnStatisticsUpdated += OnStatisticsUpdated;
        }

        private void OnActivity(object? sender, ChildGuard.Core.Models.ActivityEvent evt)
        {
            try
            {
                string msg = evt.Data switch
                {
                    string s => s,
                    _ => evt.Type + (evt.Data != null ? $": {evt.Data}" : string.Empty)
                };
                var ts = evt.Timestamp.ToLocalTime().ToString("HH:mm:ss");
                _logQueue.Enqueue($"[{ts}] {msg}");
            }
            catch { }
        }

        private void OnStatisticsUpdated(object? sender, StatisticsUpdatedEventArgs e)
        {
            Interlocked.Exchange(ref _lastKeys, e.TotalKeysPressed);
            Interlocked.Exchange(ref _lastMouse, e.TotalMouseClicks);
            Interlocked.Exchange(ref _threatsDetected, e.ThreatsDetected);
        }

        private void StartProtection()
        {
            if (_running) return;
            _protectionManager.Start(_config);
            _running = true;
            UpdateStatus();
        }

        private void StopProtection()
        {
            if (!_running) return;
            _protectionManager.Stop();
            _running = false;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            // Update UI based on protection status
            try
            {
                if (!IsHandleCreated)
                {
                    // Ensure handle exists to avoid invalid operation during startup
                    CreateHandle();
                }
                this.BeginInvoke(new Action(() =>
                {
                    LoadContent(activeSidebarItem?.Text ?? "Dashboard");
                }));
            }
            catch { }
        }

        private void ProfileButton_Click(object? sender, EventArgs e)
        {
            // Show profile menu
            var menu = new ContextMenuStrip();
            menu.Items.Add("Profile Settings");
            menu.Items.Add("Account");
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Sign Out");
            menu.Show(profileButton, new Point(0, profileButton.Height));
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (IsDisposed || Disposing) return;

                // Update monitoring values (guard against disposed controls)
                var keyLabel = contentPanel?.IsDisposed == false ? contentPanel.Controls.Find("keyValueLabel", true).FirstOrDefault() as Label : null;
                if (keyLabel != null && !keyLabel.IsDisposed)
                    keyLabel.Text = _lastKeys.ToString("N0");

                var mouseLabel = contentPanel?.IsDisposed == false ? contentPanel.Controls.Find("mouseValueLabel", true).FirstOrDefault() as Label : null;
                if (mouseLabel != null && !mouseLabel.IsDisposed)
                    mouseLabel.Text = _lastMouse.ToString("N0");

                // Flush queued activity logs in batches to reduce UI thrash
                if (activityListBox != null && !activityListBox.IsDisposed)
                {
                    int maxItems = 500;
                    if (!_logQueue.IsEmpty)
                    {
                        activityListBox.BeginUpdate();
                        try
                        {
                            while (_logQueue.TryDequeue(out var line))
                            {
                                // Insert newest on top
                                activityListBox.Items.Insert(0, line);
                            }
                            while (activityListBox.Items.Count > maxItems)
                            {
                                activityListBox.Items.RemoveAt(activityListBox.Items.Count - 1);
                            }
                        }
                        finally
                        {
                            activityListBox.EndUpdate();
                        }
                    }
                }
            }
            catch { }
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            // Handle animations
        }

        private Image CreateLogo()
        {
            var bitmap = new Bitmap(40, 40);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Draw shield icon
                using (var brush = new SolidBrush(ColorScheme.Modern.Primary))
                {
                    g.FillEllipse(brush, 5, 5, 30, 30);
                }

                using (var font = new Font("Segoe UI", 18, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    var text = "C";
                    var size = g.MeasureString(text, font);
                    g.DrawString(text, font, brush, (40 - size.Width) / 2, (40 - size.Height) / 2);

                }
            }
            return bitmap;
        }

        private Image CreateIcon(string icon, int size, Color color)
        {
            var bitmap = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (var font = new Font("Segoe UI Emoji", size * 0.7f))
                using (var brush = new SolidBrush(color))
                {
                    var text = icon;
                    var measured = g.MeasureString(text, font);
                    g.DrawString(text, font, brush, (size - measured.Width) / 2, (size - measured.Height) / 2);
                }
            }
            return bitmap;
        }

        [Conditional("DEBUG")]
        private void ValidateNoOverlap()
        {
            try
            {
                if (contentPanel == null || contentPanel.Controls.Count == 0) return;
                var root = contentPanel.Controls[0];
                // Force layout
                root.PerformLayout();

                void Check(Control parent)
                {
                    var children = parent.Controls.Cast<Control>().Where(c => c.Visible && c.Width > 0 && c.Height > 0).ToArray();
                    for (int i = 0; i < children.Length; i++)
                    {
                        for (int j = i + 1; j < children.Length; j++)
                        {
                            var a = children[i];
                            var b = children[j];
                            var ra = new Rectangle(a.Left, a.Top, a.Width, a.Height);
                            var rb = new Rectangle(b.Left, b.Top, b.Width, b.Height);
                            if (ra.IntersectsWith(rb))
                            {
                                Debug.WriteLine($"[LAYOUT] Overlap detected: {a.Name ?? a.GetType().Name} vs {b.Name ?? b.GetType().Name} in {parent.Name ?? parent.GetType().Name}");
                            }
                        }
                        if (children[i].HasChildren) Check(children[i]);
                    }
                }

                Check(root);
            }
            catch { }
        }
    }

    /// <summary>
    /// Sidebar navigation item
    /// </summary>
    public class SidebarItem : Panel
    {
        private bool isActive;
        private bool isHovered;
        public string Icon { get; set; } = string.Empty;
        public int Index { get; set; }
        public new string Text { get; set; } = string.Empty;

        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                UpdateAppearance();
            }
        }

        public SidebarItem()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |

                    ControlStyles.ResizeRedraw |
                    ControlStyles.OptimizedDoubleBuffer, true);

            Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Background
            if (isActive)
            {
                using (var brush = new SolidBrush(ColorScheme.Modern.PrimaryLight))
                {
                    g.FillRectangle(brush, ClientRectangle);
                }

                // Active indicator
                using (var brush = new SolidBrush(ColorScheme.Modern.Primary))
                {
                    g.FillRectangle(brush, 0, 8, 3, Height - 16);
                }
            }
            else if (isHovered)
            {
                using (var brush = new SolidBrush(ColorScheme.Modern.HoverOverlay))
                {
                    g.FillRectangle(brush, ClientRectangle);
                }
            }

            // Icon
            using (var font = new Font("Segoe UI Emoji", 16))
            using (var brush = new SolidBrush(isActive ? ColorScheme.Modern.Primary : ColorScheme.Modern.TextSecondary))
            {
                g.DrawString(Icon, font, brush, 20, 12);
            }

            // Text
            using (var font = new Font("Segoe UI", 10, isActive ? FontStyle.Bold : FontStyle.Regular))
            using (var brush = new SolidBrush(isActive ? ColorScheme.Modern.Primary : ColorScheme.Modern.TextPrimary))
            {
                g.DrawString(Text, font, brush, 55, 15);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHovered = false;
            Invalidate();
        }

        private void UpdateAppearance()
        {
            Invalidate();
        }
    }
}
