using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ChildGuard.UI.FluentUI;

namespace ChildGuard.UI
{
    /// <summary>
    /// Beautiful Fluent Design demo - Windows 11 style with proper animations and effects
    /// </summary>
    public partial class FluentDemoForm : Form
    {
        private Panel titleBar = null!;
        private Panel sidebar = null!;
        private Panel mainContent = null!;

        public FluentDemoForm()
        {
            InitializeComponent();
            SetupFluentDesign();
        }

        private void InitializeComponent()
        {
            Text = "ChildGuard - Fluent Design";
            Size = new Size(1400, 900);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = FluentColors.Background;
            Font = new Font("Segoe UI", 9F);
            
            // Enable double buffering
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.UserPaint | 
                    ControlStyles.DoubleBuffer, true);
        }

        private void SetupFluentDesign()
        {
            CreateTitleBar();
            CreateMainLayout();
        }

        private void CreateTitleBar()
        {
            titleBar = new Panel
            {
                Height = 48,
                Dock = DockStyle.Top,
                BackColor = FluentColors.Surface
            };

            // App title
            var titleLabel = new Label
            {
                Text = "ChildGuard",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = FluentColors.TextPrimary,
                Location = new Point(20, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Window controls
            var controlsPanel = new Panel
            {
                Width = 138,
                Height = 48,
                Dock = DockStyle.Right,
                BackColor = Color.Transparent
            };

            var minimizeBtn = CreateWindowButton("−", 0);
            var maximizeBtn = CreateWindowButton("□", 46);
            var closeBtn = CreateWindowButton("✕", 92);

            minimizeBtn.Click += (s, e) => WindowState = FormWindowState.Minimized;
            maximizeBtn.Click += (s, e) => 
            {
                WindowState = WindowState == FormWindowState.Maximized ? 
                    FormWindowState.Normal : FormWindowState.Maximized;
            };
            closeBtn.Click += (s, e) => Close();

            controlsPanel.Controls.AddRange(new Control[] { minimizeBtn, maximizeBtn, closeBtn });
            titleBar.Controls.AddRange(new Control[] { titleLabel, controlsPanel });

            // Make draggable
            titleBar.MouseDown += TitleBar_MouseDown;
            titleLabel.MouseDown += TitleBar_MouseDown;

            Controls.Add(titleBar);
        }

        private Button CreateWindowButton(string text, int x)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(46, 48),
                Location = new Point(x, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = FluentColors.TextSecondary,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand,
                TabStop = false
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = FluentColors.Hover;
            btn.FlatAppearance.MouseDownBackColor = FluentColors.Pressed;

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
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320));
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
                BackColor = FluentColors.BackgroundSecondary,
                Padding = new Padding(20)
            };

            // Brand section
            var brandCard = new FluentCard
            {
                Dock = DockStyle.Top,
                Height = 100,
                Elevation = FluentCard.CardElevation.Medium,
                HasAcrylicEffect = true,
                Margin = new Padding(0, 0, 0, 20)
            };

            var brandLabel = new Label
            {
                Text = "ChildGuard",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = FluentColors.Primary,
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
                ("Dashboard", "🏠"),
                ("Protection", "🛡️"),
                ("Reports", "📊"),
                ("Settings", "⚙️"),
                ("About", "ℹ️")
            };

            var y = 0;
            for (int i = 0; i < navItems.Length; i++)
            {
                var (text, icon) = navItems[i];
                var navBtn = new FluentButton
                {
                    Text = $"{icon}  {text}",
                    Style = i == 0 ? FluentButton.FluentStyle.Accent : FluentButton.FluentStyle.Subtle,
                    Size = new Size(280, 44),
                    Location = new Point(0, y),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 11F),
                    CornerRadius = 6
                };
                navPanel.Controls.Add(navBtn);
                y += 52;
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
                Padding = new Padding(32)
            };

            // Header
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.Transparent
            };

            var titleLabel = new Label
            {
                Text = "Fluent Design System",
                Font = new Font("Segoe UI", 28F, FontStyle.Bold),
                ForeColor = FluentColors.TextPrimary,
                Location = new Point(0, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var subtitleLabel = new Label
            {
                Text = "Beautiful, modern UI components with Windows 11 design language",
                Font = new Font("Segoe UI", 12F),
                ForeColor = FluentColors.TextSecondary,
                Location = new Point(0, 55),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            header.Controls.AddRange(new Control[] { titleLabel, subtitleLabel });

            // Scrollable content
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            var contentContainer = new Panel
            {
                Width = 1000,
                Height = 1200,
                BackColor = Color.Transparent
            };

            // Button showcase
            var buttonsCard = CreateButtonShowcase();
            buttonsCard.Location = new Point(0, 20);
            contentContainer.Controls.Add(buttonsCard);

            // Cards showcase
            var cardsShowcase = CreateCardsShowcase();
            cardsShowcase.Location = new Point(0, 220);
            contentContainer.Controls.Add(cardsShowcase);

            // Stats dashboard
            var statsCard = CreateStatsCard();
            statsCard.Location = new Point(0, 500);
            contentContainer.Controls.Add(statsCard);

            scrollPanel.Controls.Add(contentContainer);
            content.Controls.Add(scrollPanel);
            content.Controls.Add(header);

            return content;
        }

        private FluentCard CreateButtonShowcase()
        {
            var card = new FluentCard
            {
                Size = new Size(950, 180),
                Elevation = FluentCard.CardElevation.Medium,
                HasAcrylicEffect = true
            };

            var header = new FluentCardHeader
            {
                Title = "Button Variants"
            };

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0, 20, 0, 0)
            };

            var buttons = new[]
            {
                ("Standard", FluentButton.FluentStyle.Standard),
                ("Accent", FluentButton.FluentStyle.Accent),
                ("Subtle", FluentButton.FluentStyle.Subtle),
                ("Outline", FluentButton.FluentStyle.Outline),
                ("Text", FluentButton.FluentStyle.Text)
            };

            foreach (var (text, style) in buttons)
            {
                var btn = new FluentButton
                {
                    Text = text,
                    Style = style,
                    Size = new Size(120, 36),
                    Margin = new Padding(0, 0, 16, 16),
                    Font = new Font("Segoe UI", 10F)
                };
                buttonPanel.Controls.Add(btn);
            }

            card.Controls.Add(buttonPanel);
            card.Controls.Add(header);

            return card;
        }

        private FluentCard CreateCardsShowcase()
        {
            var card = new FluentCard
            {
                Size = new Size(950, 260),
                Elevation = FluentCard.CardElevation.Medium
            };

            var header = new FluentCardHeader
            {
                Title = "Card Elevations"
            };

            var cardsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0, 20, 0, 0)
            };

            var elevations = new[]
            {
                ("None", FluentCard.CardElevation.None),
                ("Low", FluentCard.CardElevation.Low),
                ("Medium", FluentCard.CardElevation.Medium),
                ("High", FluentCard.CardElevation.High),
                ("Very High", FluentCard.CardElevation.VeryHigh)
            };

            foreach (var (name, elevation) in elevations)
            {
                var miniCard = new FluentCard
                {
                    Size = new Size(160, 120),
                    Elevation = elevation,
                    Margin = new Padding(0, 0, 20, 20),
                    HasAcrylicEffect = elevation == FluentCard.CardElevation.High
                };

                var titleLabel = new Label
                {
                    Text = name,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = FluentColors.TextPrimary,
                    Location = new Point(20, 20),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };

                var descLabel = new Label
                {
                    Text = $"Elevation: {(int)elevation}px",
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = FluentColors.TextSecondary,
                    Location = new Point(20, 45),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };

                miniCard.Controls.AddRange(new Control[] { titleLabel, descLabel });
                cardsPanel.Controls.Add(miniCard);
            }

            card.Controls.Add(cardsPanel);
            card.Controls.Add(header);

            return card;
        }

        private FluentCard CreateStatsCard()
        {
            var card = new FluentCard
            {
                Size = new Size(950, 200),
                Elevation = FluentCard.CardElevation.High,
                HasAcrylicEffect = true
            };

            var header = new FluentCardHeader
            {
                Title = "System Statistics"
            };

            var statsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(0, 20, 0, 0)
            };
            statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            var stats = new[]
            {
                ("Active Users", "2,847", FluentColors.AccentBlue),
                ("Threats Blocked", "1,293", FluentColors.AccentRed),
                ("System Uptime", "99.8%", FluentColors.AccentGreen)
            };

            for (int i = 0; i < stats.Length; i++)
            {
                var (label, value, color) = stats[i];
                var statPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent,
                    Padding = new Padding(20)
                };

                var valueLabel = new Label
                {
                    Text = value,
                    Font = new Font("Segoe UI", 32F, FontStyle.Bold),
                    ForeColor = color,
                    Dock = DockStyle.Top,
                    Height = 50,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                };

                var labelText = new Label
                {
                    Text = label,
                    Font = new Font("Segoe UI", 11F),
                    ForeColor = FluentColors.TextSecondary,
                    Dock = DockStyle.Top,
                    Height = 30,
                    TextAlign = ContentAlignment.TopCenter,
                    BackColor = Color.Transparent
                };

                statPanel.Controls.Add(labelText);
                statPanel.Controls.Add(valueLabel);
                statsPanel.Controls.Add(statPanel, i, 0);
            }

            card.Controls.Add(statsPanel);
            card.Controls.Add(header);

            return card;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Draw window border
            using (var pen = new Pen(FluentColors.Border, 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }
    }
}
