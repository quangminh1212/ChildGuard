using System;
using System.Drawing;
using System.Windows.Forms;
using ChildGuard.UI.ModernUI;

namespace ChildGuard.UI
{
    /// <summary>
    /// Modern demo form using the new ModernUI library - inspired by GitHub, VS Code, Notion
    /// </summary>
    public partial class ModernDemoForm : ModernForm
    {
        public ModernDemoForm()
        {
            InitializeComponent();
            SetupModernLayout();
        }

        private void InitializeComponent()
        {
            Text = "ChildGuard - Modern UI Demo";
            Size = new Size(1200, 800);
            BackColor = Color.FromArgb(248, 249, 250);
            Font = new Font("Segoe UI", 9F);
        }

        private void SetupModernLayout()
        {
            // Main container
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280));
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Sidebar
            var sidebar = CreateSidebar();
            mainContainer.Controls.Add(sidebar, 0, 0);

            // Main content
            var content = CreateMainContent();
            mainContainer.Controls.Add(content, 1, 0);

            Controls.Add(mainContainer);
        }

        private Panel CreateSidebar()
        {
            var sidebar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16)
            };

            // Logo/Brand
            var brandCard = new ModernCard
            {
                Dock = DockStyle.Top,
                Height = 80,
                HasShadow = false,
                BorderColor = Color.FromArgb(229, 231, 235),
                BackColor = Color.FromArgb(59, 130, 246),
                Margin = new Padding(0, 0, 0, 16)
            };

            var brandLabel = new Label
            {
                Text = "ChildGuard",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
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
                ("🏠", "Dashboard"),
                ("🛡️", "Protection"),
                ("📊", "Reports"),
                ("⚙️", "Settings"),
                ("ℹ️", "About")
            };

            var y = 0;
            foreach (var (icon, text) in navItems)
            {
                var navButton = new ModernButton
                {
                    Text = $"{icon}  {text}",
                    Variant = y == 0 ? ModernButton.ButtonVariant.Primary : ModernButton.ButtonVariant.Ghost,
                    Size = new Size(248, 40),
                    Location = new Point(0, y),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 10F)
                };
                navPanel.Controls.Add(navButton);
                y += 48;
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
                Padding = new Padding(24)
            };

            // Page header
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.Transparent
            };

            var titleLabel = new Label
            {
                Text = "Modern UI Components",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                AutoSize = true,
                Location = new Point(0, 16)
            };
            header.Controls.Add(titleLabel);

            // Content area with scroll
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            var contentContainer = new Panel
            {
                Width = 800,
                Height = 1200,
                BackColor = Color.Transparent
            };

            // Buttons section
            var buttonsCard = CreateButtonsSection();
            buttonsCard.Location = new Point(0, 16);
            contentContainer.Controls.Add(buttonsCard);

            // Cards section
            var cardsSection = CreateCardsSection();
            cardsSection.Location = new Point(0, 200);
            contentContainer.Controls.Add(cardsSection);

            // Stats section
            var statsSection = CreateStatsSection();
            statsSection.Location = new Point(0, 400);
            contentContainer.Controls.Add(statsSection);

            scrollPanel.Controls.Add(contentContainer);
            content.Controls.Add(scrollPanel);
            content.Controls.Add(header);

            return content;
        }

        private ModernCard CreateButtonsSection()
        {
            var card = new ModernCard
            {
                Size = new Size(780, 160),
                HasShadow = true
            };

            var header = new ModernCardHeader
            {
                Title = "Button Variants"
            };

            var content = new ModernCardContent();
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0, 8, 0, 0)
            };

            var buttons = new[]
            {
                ("Primary", ModernButton.ButtonVariant.Primary),
                ("Secondary", ModernButton.ButtonVariant.Secondary),
                ("Success", ModernButton.ButtonVariant.Success),
                ("Danger", ModernButton.ButtonVariant.Danger),
                ("Ghost", ModernButton.ButtonVariant.Ghost),
                ("Link", ModernButton.ButtonVariant.Link)
            };

            foreach (var (text, variant) in buttons)
            {
                var btn = new ModernButton
                {
                    Text = text,
                    Variant = variant,
                    Size = new Size(100, 36),
                    Margin = new Padding(0, 0, 12, 12)
                };
                buttonPanel.Controls.Add(btn);
            }

            content.Controls.Add(buttonPanel);
            card.Controls.Add(content);
            card.Controls.Add(header);

            return card;
        }

        private ModernCard CreateCardsSection()
        {
            var card = new ModernCard
            {
                Size = new Size(780, 180),
                HasShadow = true
            };

            var header = new ModernCardHeader
            {
                Title = "Card Examples"
            };

            var content = new ModernCardContent();
            var cardPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0, 8, 0, 0)
            };

            for (int i = 1; i <= 4; i++)
            {
                var miniCard = new ModernCard
                {
                    Size = new Size(160, 100),
                    Margin = new Padding(0, 0, 16, 16),
                    HasShadow = true
                };

                var miniHeader = new Label
                {
                    Text = $"Card {i}",
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(17, 24, 39),
                    Location = new Point(16, 16),
                    AutoSize = true
                };

                var miniContent = new Label
                {
                    Text = "Sample content with modern styling",
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Location = new Point(16, 40),
                    Size = new Size(128, 40)
                };

                miniCard.Controls.Add(miniHeader);
                miniCard.Controls.Add(miniContent);
                cardPanel.Controls.Add(miniCard);
            }

            content.Controls.Add(cardPanel);
            card.Controls.Add(content);
            card.Controls.Add(header);

            return card;
        }

        private ModernCard CreateStatsSection()
        {
            var card = new ModernCard
            {
                Size = new Size(780, 200),
                HasShadow = true
            };

            var header = new ModernCardHeader
            {
                Title = "Statistics Dashboard"
            };

            var content = new ModernCardContent();
            var statsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(0, 8, 0, 0)
            };
            statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            var stats = new[]
            {
                ("Active Users", "1,234", Color.FromArgb(34, 197, 94)),
                ("Threats Blocked", "567", Color.FromArgb(239, 68, 68)),
                ("Uptime", "99.9%", Color.FromArgb(59, 130, 246))
            };

            for (int i = 0; i < stats.Length; i++)
            {
                var (label, value, color) = stats[i];
                var statPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent,
                    Padding = new Padding(8)
                };

                var valueLabel = new Label
                {
                    Text = value,
                    Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                    ForeColor = color,
                    Dock = DockStyle.Top,
                    Height = 40,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                var labelText = new Label
                {
                    Text = label,
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Dock = DockStyle.Top,
                    Height = 30,
                    TextAlign = ContentAlignment.TopCenter
                };

                statPanel.Controls.Add(labelText);
                statPanel.Controls.Add(valueLabel);
                statsPanel.Controls.Add(statPanel, i, 0);
            }

            content.Controls.Add(statsPanel);
            card.Controls.Add(content);
            card.Controls.Add(header);

            return card;
        }
    }
}
