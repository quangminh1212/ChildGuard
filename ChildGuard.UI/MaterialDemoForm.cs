using System;
using System.Drawing;
using System.Windows.Forms;
using ChildGuard.UI.Controls;
using ChildGuard.UI.Theming;

namespace ChildGuard.UI
{
    /// <summary>
    /// Demo form showcasing Material Design components
    /// </summary>
    public partial class MaterialDemoForm : Form
    {
        public MaterialDemoForm()
        {
            InitializeComponent();
            SetupMaterialDesign();
        }

        private void InitializeComponent()
        {
            Text = "ChildGuard - Material Design 3 Demo";
            Size = new Size(1200, 800);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F);
            BackColor = ColorScheme.MaterialFluent.Background;

            // Remove default form border for modern look
            FormBorderStyle = FormBorderStyle.None;

            // Add custom title bar
            var titleBar = CreateTitleBar();
            Controls.Add(titleBar);

            // Apply modern styling
            ModernStyle.Apply(this, ThemeMode.Light);
        }

        private Panel CreateTitleBar()
        {
            var titleBar = new Panel
            {
                Height = 48,
                Dock = DockStyle.Top,
                BackColor = ColorScheme.MaterialFluent.Primary
            };

            var titleLabel = new Label
            {
                Text = "ChildGuard - Material Design 3 Demo",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(16, 12),
                AutoSize = true
            };

            var closeButton = new MaterialButton
            {
                Text = "✕",
                Style = MaterialButton.ButtonStyle.Text,
                Size = new Size(48, 32),
                Location = new Point(Width - 64, 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 12F),
                ForeColor = Color.White
            };
            closeButton.Click += (s, e) => Close();

            titleBar.Controls.Add(titleLabel);
            titleBar.Controls.Add(closeButton);

            // Make title bar draggable
            titleBar.MouseDown += TitleBar_MouseDown;
            titleLabel.MouseDown += TitleBar_MouseDown;

            return titleBar;
        }

        private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Allow dragging the form
                const int WM_NCLBUTTONDOWN = 0xA1;
                const int HT_CAPTION = 0x2;

                var msg = Message.Create(Handle, WM_NCLBUTTONDOWN, new IntPtr(HT_CAPTION), IntPtr.Zero);
                DefWndProc(ref msg);
            }
        }

        private void SetupMaterialDesign()
        {
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0),
                BackColor = ColorScheme.MaterialFluent.Background
            };
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320));
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Left navigation panel
            var navPanel = CreateNavigationPanel();
            mainContainer.Controls.Add(navPanel, 0, 0);

            // Right content panel
            var contentPanel = CreateContentPanel();
            mainContainer.Controls.Add(contentPanel, 1, 0);

            Controls.Add(mainContainer);
        }

        private Panel CreateNavigationPanel()
        {
            var navContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorScheme.MaterialFluent.SurfaceVariant,
                Padding = new Padding(24, 24, 0, 24)
            };

            // Navigation header
            var headerCard = new MaterialCard
            {
                Dock = DockStyle.Top,
                Height = 120,
                Elevation = 0,
                CornerRadius = 16,
                Margin = new Padding(0, 0, 24, 32),
                CardBackgroundColor = ColorScheme.MaterialFluent.Primary
            };

            var headerLabel = new Label
            {
                Text = "ChildGuard",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var subtitleLabel = new Label
            {
                Text = "Material Design 3",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(200, 255, 255, 255),
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.TopCenter
            };

            headerCard.Controls.Add(headerLabel);
            headerCard.Controls.Add(subtitleLabel);

            // Navigation bar
            var navBar = new MaterialNavigationBar
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 24, 0)
            };

            navBar.AddItem("🏠  Dashboard", "");
            navBar.AddItem("🛡️  Protection", "");
            navBar.AddItem("📊  Reports", "");
            navBar.AddItem("⚙️  Settings", "");
            navBar.AddItem("ℹ️  About", "");

            navBar.SelectedIndex = 0;

            navContainer.Controls.Add(navBar);
            navContainer.Controls.Add(headerCard);

            return navContainer;
        }

        private Panel CreateContentPanel()
        {
            var contentContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(16)
            };

            // Page title
            var titleLabel = new Label
            {
                Text = "Material Design Components",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = ColorScheme.MaterialFluent.TextPrimary,
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.BottomLeft
            };

            // Buttons section
            var buttonsCard = CreateButtonsSection();
            buttonsCard.Dock = DockStyle.Top;
            buttonsCard.Margin = new Padding(0, 16, 0, 16);

            // Text inputs section
            var inputsCard = CreateInputsSection();
            inputsCard.Dock = DockStyle.Top;
            inputsCard.Margin = new Padding(0, 0, 0, 16);

            // Cards section
            var cardsSection = CreateCardsSection();
            cardsSection.Dock = DockStyle.Top;

            scrollPanel.Controls.Add(cardsSection);
            scrollPanel.Controls.Add(inputsCard);
            scrollPanel.Controls.Add(buttonsCard);
            scrollPanel.Controls.Add(titleLabel);

            contentContainer.Controls.Add(scrollPanel);
            return contentContainer;
        }

        private MaterialCard CreateButtonsSection()
        {
            var card = new MaterialCard
            {
                Height = 200,
                Elevation = 1,
                CornerRadius = 12
            };

            var titleLabel = new Label
            {
                Text = "Buttons",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = ColorScheme.MaterialFluent.TextPrimary,
                Dock = DockStyle.Top,
                Height = 40
            };

            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0, 8, 0, 0)
            };

            // Primary button
            var primaryBtn = new MaterialButton
            {
                Text = "Primary",
                Style = MaterialButton.ButtonStyle.Primary,
                Size = new Size(120, 40),
                Margin = new Padding(0, 0, 12, 12)
            };

            // Secondary button
            var secondaryBtn = new MaterialButton
            {
                Text = "Secondary",
                Style = MaterialButton.ButtonStyle.Secondary,
                Size = new Size(120, 40),
                Margin = new Padding(0, 0, 12, 12)
            };

            // Text button
            var textBtn = new MaterialButton
            {
                Text = "Text",
                Style = MaterialButton.ButtonStyle.Text,
                Size = new Size(120, 40),
                Margin = new Padding(0, 0, 12, 12)
            };

            // Success button
            var successBtn = new MaterialButton
            {
                Text = "Success",
                Style = MaterialButton.ButtonStyle.Success,
                Size = new Size(120, 40),
                Margin = new Padding(0, 0, 12, 12)
            };

            // Danger button
            var dangerBtn = new MaterialButton
            {
                Text = "Danger",
                Style = MaterialButton.ButtonStyle.Danger,
                Size = new Size(120, 40),
                Margin = new Padding(0, 0, 12, 12)
            };

            buttonsPanel.Controls.AddRange(new Control[] { primaryBtn, secondaryBtn, textBtn, successBtn, dangerBtn });

            card.Controls.Add(buttonsPanel);
            card.Controls.Add(titleLabel);

            return card;
        }

        private MaterialCard CreateInputsSection()
        {
            var card = new MaterialCard
            {
                Height = 180,
                Elevation = 1,
                CornerRadius = 12
            };

            var titleLabel = new Label
            {
                Text = "Text Inputs",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = ColorScheme.MaterialFluent.TextPrimary,
                Dock = DockStyle.Top,
                Height = 40
            };

            var inputsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0, 8, 0, 0)
            };

            var textInput1 = new MaterialTextBox
            {
                LabelText = "Username",
                Size = new Size(200, 56),
                Margin = new Padding(0, 0, 16, 16)
            };

            var textInput2 = new MaterialTextBox
            {
                LabelText = "Password",
                PasswordChar = '*',
                Size = new Size(200, 56),
                Margin = new Padding(0, 0, 16, 16)
            };

            var textInput3 = new MaterialTextBox
            {
                LabelText = "Email",
                Size = new Size(200, 56),
                Margin = new Padding(0, 0, 16, 16)
            };

            inputsPanel.Controls.AddRange(new Control[] { textInput1, textInput2, textInput3 });

            card.Controls.Add(inputsPanel);
            card.Controls.Add(titleLabel);

            return card;
        }

        private MaterialCard CreateCardsSection()
        {
            var card = new MaterialCard
            {
                Height = 120,
                Elevation = 1,
                CornerRadius = 12
            };

            var titleLabel = new Label
            {
                Text = "Cards & Elevation",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = ColorScheme.MaterialFluent.TextPrimary,
                Dock = DockStyle.Top,
                Height = 40
            };

            var cardsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0, 8, 0, 0)
            };

            for (int i = 1; i <= 4; i++)
            {
                var miniCard = new MaterialCard
                {
                    Size = new Size(120, 60),
                    Elevation = i,
                    CornerRadius = 8,
                    Margin = new Padding(0, 0, 16, 16)
                };

                var label = new Label
                {
                    Text = $"Elevation {i}",
                    Font = new Font("Segoe UI", 9),
                    ForeColor = ColorScheme.MaterialFluent.TextSecondary,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                miniCard.Controls.Add(label);
                cardsPanel.Controls.Add(miniCard);
            }

            card.Controls.Add(cardsPanel);
            card.Controls.Add(titleLabel);

            return card;
        }
    }
}
