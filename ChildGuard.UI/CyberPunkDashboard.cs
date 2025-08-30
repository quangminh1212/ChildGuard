using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ChildGuard.UI.GlassUI;

namespace ChildGuard.UI
{
    /// <summary>
    /// CyberPunk Gaming Dashboard - Glass Morphism + Neon Effects
    /// Ultra-modern, beautiful, and professional
    /// </summary>
    public partial class CyberPunkDashboard : Form
    {
        private Panel titleBar;
        private Panel sidebar;
        private Panel mainContent;
        private System.Windows.Forms.Timer backgroundAnimationTimer;
        private float animationPhase = 0f;

        public CyberPunkDashboard()
        {
            InitializeComponent();
            SetupCyberPunkDesign();
            StartBackgroundAnimation();
        }

        private void InitializeComponent()
        {
            Text = "ChildGuard - CyberPunk Edition";
            Size = new Size(1600, 1000);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = GlassColors.DarkBase;
            Font = new Font("Segoe UI", 9F);
            
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.UserPaint | 
                    ControlStyles.DoubleBuffer, true);
        }

        private void SetupCyberPunkDesign()
        {
            CreateTitleBar();
            CreateMainLayout();
        }

        private void StartBackgroundAnimation()
        {
            backgroundAnimationTimer = new System.Windows.Forms.Timer { Interval = 50 };
            backgroundAnimationTimer.Tick += (s, e) =>
            {
                animationPhase += 0.02f;
                if (animationPhase > Math.PI * 2) animationPhase = 0f;
                Invalidate();
            };
            backgroundAnimationTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawAnimatedBackground(e.Graphics);
        }

        private void DrawAnimatedBackground(Graphics g)
        {
            // Animated gradient background
            using (var bgBrush = GlassColors.CreateBackgroundGradient(ClientRectangle))
            {
                g.FillRectangle(bgBrush, ClientRectangle);
            }

            // Animated particles/stars
            var random = new Random(42);
            for (int i = 0; i < 50; i++)
            {
                var x = random.Next(0, Width);
                var y = random.Next(0, Height);
                var phase = animationPhase + i * 0.1f;
                var opacity = (int)(30 + 20 * Math.Sin(phase));
                var size = 1 + (int)(2 * Math.Sin(phase * 0.5));

                using (var starBrush = new SolidBrush(Color.FromArgb(opacity, GlassColors.NeonCyan)))
                {
                    g.FillEllipse(starBrush, x, y, size, size);
                }
            }

            // Subtle grid pattern
            using (var gridPen = new Pen(Color.FromArgb(8, GlassColors.NeonCyan), 1))
            {
                for (int x = 0; x < Width; x += 100)
                {
                    g.DrawLine(gridPen, x, 0, x, Height);
                }
                for (int y = 0; y < Height; y += 100)
                {
                    g.DrawLine(gridPen, 0, y, Width, y);
                }
            }
        }

        private void CreateTitleBar()
        {
            titleBar = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent
            };

            // Gradient background for title bar
            titleBar.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    titleBar.ClientRectangle,
                    Color.FromArgb(80, GlassColors.DarkSecondary),
                    Color.FromArgb(40, GlassColors.DarkBase),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, titleBar.ClientRectangle);
                }
            };

            // App title with glow
            var titleLabel = new Label
            {
                Text = "⚡ CHILDGUARD",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = GlassColors.NeonCyan,
                Location = new Point(30, 18),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Subtitle
            var subtitleLabel = new Label
            {
                Text = "CYBERPUNK EDITION",
                Font = new Font("Segoe UI", 8F),
                ForeColor = GlassColors.TextSecondary,
                Location = new Point(200, 22),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Window controls
            var controlsPanel = new Panel
            {
                Width = 150,
                Height = 60,
                Dock = DockStyle.Right,
                BackColor = Color.Transparent
            };

            var minimizeBtn = CreateWindowButton("−", 0, GlassColors.NeonBlue);
            var maximizeBtn = CreateWindowButton("□", 50, GlassColors.NeonOrange);
            var closeBtn = CreateWindowButton("✕", 100, GlassColors.NeonPink);

            minimizeBtn.Click += (s, e) => WindowState = FormWindowState.Minimized;
            maximizeBtn.Click += (s, e) => 
            {
                WindowState = WindowState == FormWindowState.Maximized ? 
                    FormWindowState.Normal : FormWindowState.Maximized;
            };
            closeBtn.Click += (s, e) => Close();

            controlsPanel.Controls.AddRange(new Control[] { minimizeBtn, maximizeBtn, closeBtn });
            titleBar.Controls.AddRange(new Control[] { titleLabel, subtitleLabel, controlsPanel });

            // Make draggable
            titleBar.MouseDown += TitleBar_MouseDown;
            titleLabel.MouseDown += TitleBar_MouseDown;
            subtitleLabel.MouseDown += TitleBar_MouseDown;

            Controls.Add(titleBar);
        }

        private GlassButton CreateWindowButton(string text, int x, Color neonColor)
        {
            var btn = new GlassButton
            {
                Text = text,
                Size = new Size(40, 40),
                Location = new Point(x, 10),
                Style = GlassButton.GlassStyle.Ghost,
                NeonColor = neonColor,
                HasGlow = true,
                CornerRadius = 8,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                TabStop = false
            };

            return btn;
        }

        private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                const int WM_NCLBUTTONDOWN = 0xA1;
                const int HT_CAPTION = 0x2;
                
                var msg = Message.Create(Handle, WM_NCLBUTTONDOWN, new IntPtr(HT_CAPTION), IntPtr.Zero);
                DefWndProc(ref msg);
            }
        }

        private void CreateMainLayout()
        {
            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350));
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            sidebar = CreateSidebar();
            mainContent = CreateMainContent();

            container.Controls.Add(sidebar, 0, 0);
            container.Controls.Add(mainContent, 1, 0);

            Controls.Add(container);
        }

        private Panel CreateSidebar()
        {
            var sidebar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(20)
            };

            // Brand card
            var brandCard = new GlassCard
            {
                Dock = DockStyle.Top,
                Height = 120,
                Style = GlassCard.CardStyle.GlassNeon,
                NeonColor = GlassColors.NeonCyan,
                HasGlow = true,
                Margin = new Padding(0, 0, 0, 20)
            };

            var brandLabel = new Label
            {
                Text = "🛡️ CHILDGUARD",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = GlassColors.NeonCyan,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            brandCard.Controls.Add(brandLabel);

            // Navigation
            var navPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            var navItems = new[]
            {
                ("🏠 Dashboard", GlassColors.NeonCyan, true),
                ("🛡️ Protection", GlassColors.NeonGreen, false),
                ("📊 Analytics", GlassColors.NeonPurple, false),
                ("⚙️ Settings", GlassColors.NeonOrange, false),
                ("🔧 Tools", GlassColors.NeonBlue, false),
                ("ℹ️ About", GlassColors.NeonPink, false)
            };

            var y = 0;
            for (int i = 0; i < navItems.Length; i++)
            {
                var (text, color, isActive) = navItems[i];
                var navBtn = new GlassButton
                {
                    Text = text,
                    Style = isActive ? GlassButton.GlassStyle.Neon : GlassButton.GlassStyle.Glass,
                    NeonColor = color,
                    Size = new Size(310, 50),
                    Location = new Point(0, y),
                    Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                    CornerRadius = 12,
                    HasGlow = isActive
                };
                navPanel.Controls.Add(navBtn);
                y += 60;
            }

            sidebar.Controls.Add(navPanel);
            sidebar.Controls.Add(brandCard);

            return sidebar;
        }

        private Panel CreateMainContent()
        {
            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(40)
            };

            // Header
            var header = CreateHeader();
            
            // Scrollable content
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            var contentContainer = new Panel
            {
                Width = 1100,
                Height = 1400,
                BackColor = Color.Transparent
            };

            // Stats cards
            var statsPanel = CreateStatsPanel();
            statsPanel.Location = new Point(0, 20);
            contentContainer.Controls.Add(statsPanel);

            // Feature showcase
            var featuresPanel = CreateFeaturesPanel();
            featuresPanel.Location = new Point(0, 200);
            contentContainer.Controls.Add(featuresPanel);

            // System info
            var systemCard = CreateSystemInfoCard();
            systemCard.Location = new Point(0, 600);
            contentContainer.Controls.Add(systemCard);

            scrollPanel.Controls.Add(contentContainer);
            content.Controls.Add(scrollPanel);
            content.Controls.Add(header);

            return content;
        }

        private Panel CreateHeader()
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.Transparent
            };

            var titleLabel = new Label
            {
                Text = "CYBERPUNK DASHBOARD",
                Font = new Font("Segoe UI", 32F, FontStyle.Bold),
                ForeColor = GlassColors.TextPrimary,
                Location = new Point(0, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var subtitleLabel = new Label
            {
                Text = "Advanced protection with glass morphism design",
                Font = new Font("Segoe UI", 14F),
                ForeColor = GlassColors.TextSecondary,
                Location = new Point(0, 65),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            header.Controls.AddRange(new Control[] { titleLabel, subtitleLabel });
            return header;
        }

        private Panel CreateStatsPanel()
        {
            var panel = new TableLayoutPanel
            {
                Size = new Size(1050, 160),
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            var stats = new[]
            {
                ("ACTIVE USERS", "3,847", GlassColors.NeonCyan),
                ("THREATS BLOCKED", "1,293", GlassColors.NeonPink),
                ("SYSTEM UPTIME", "99.9%", GlassColors.NeonGreen)
            };

            for (int i = 0; i < stats.Length; i++)
            {
                var (label, value, color) = stats[i];
                var statCard = new GlassCard
                {
                    Dock = DockStyle.Fill,
                    Style = GlassCard.CardStyle.GlassNeon,
                    NeonColor = color,
                    HasGlow = true,
                    Margin = new Padding(10)
                };

                var valueLabel = new Label
                {
                    Text = value,
                    Font = new Font("Segoe UI", 28F, FontStyle.Bold),
                    ForeColor = color,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                };

                var labelText = new Label
                {
                    Text = label,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = GlassColors.TextSecondary,
                    Dock = DockStyle.Bottom,
                    Height = 25,
                    TextAlign = ContentAlignment.TopCenter,
                    BackColor = Color.Transparent
                };

                statCard.Controls.Add(valueLabel);
                statCard.Controls.Add(labelText);
                panel.Controls.Add(statCard, i, 0);
            }

            return panel;
        }

        private Panel CreateFeaturesPanel()
        {
            var panel = new FlowLayoutPanel
            {
                Size = new Size(1050, 380),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent
            };

            var features = new[]
            {
                ("Glass Morphism", "Modern UI with backdrop blur effects", GlassColors.NeonCyan),
                ("Neon Animations", "Smooth glowing animations and transitions", GlassColors.NeonPink),
                ("CyberPunk Theme", "Futuristic gaming aesthetic design", GlassColors.NeonPurple),
                ("Real-time Stats", "Live monitoring and beautiful visualizations", GlassColors.NeonGreen)
            };

            foreach (var (title, desc, color) in features)
            {
                var featureCard = new GlassCard
                {
                    Size = new Size(500, 170),
                    Style = GlassCard.CardStyle.Floating,
                    Margin = new Padding(10)
                };

                var header = new GlassCardHeader
                {
                    Title = title,
                    Subtitle = desc
                };

                var iconLabel = new Label
                {
                    Text = "⚡",
                    Font = new Font("Segoe UI", 32F),
                    ForeColor = color,
                    Size = new Size(60, 60),
                    Location = new Point(20, 80),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                };

                featureCard.Controls.Add(iconLabel);
                featureCard.Controls.Add(header);
                panel.Controls.Add(featureCard);
            }

            return panel;
        }

        private GlassCard CreateSystemInfoCard()
        {
            var card = new GlassCard
            {
                Size = new Size(1050, 250),
                Style = GlassCard.CardStyle.Glass,
                IsFloating = true
            };

            var header = new GlassCardHeader
            {
                Title = "SYSTEM INFORMATION",
                Subtitle = "Real-time system monitoring and status"
            };

            var infoPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(0, 20, 0, 0)
            };

            var infoItems = new[]
            {
                ("Operating System:", "Windows 11 Pro"),
                ("Memory Usage:", "4.2 GB / 16 GB"),
                ("CPU Usage:", "23%"),
                ("Network Status:", "Connected"),
                ("Last Scan:", "2 minutes ago"),
                ("Protection Level:", "Maximum")
            };

            for (int i = 0; i < infoItems.Length; i++)
            {
                var (label, value) = infoItems[i];
                var row = i / 2;
                var col = i % 2;

                var itemPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent,
                    Padding = new Padding(20, 10, 20, 10)
                };

                var labelText = new Label
                {
                    Text = label,
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    ForeColor = GlassColors.TextSecondary,
                    Dock = DockStyle.Top,
                    Height = 25,
                    BackColor = Color.Transparent
                };

                var valueText = new Label
                {
                    Text = value,
                    Font = new Font("Segoe UI", 12F),
                    ForeColor = GlassColors.TextPrimary,
                    Dock = DockStyle.Top,
                    Height = 25,
                    BackColor = Color.Transparent
                };

                itemPanel.Controls.Add(valueText);
                itemPanel.Controls.Add(labelText);
                infoPanel.Controls.Add(itemPanel, col, row);
            }

            card.Controls.Add(infoPanel);
            card.Controls.Add(header);

            return card;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                backgroundAnimationTimer?.Stop();
                backgroundAnimationTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
