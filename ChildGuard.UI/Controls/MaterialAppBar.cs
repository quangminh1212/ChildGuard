using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ChildGuard.UI.Theming;

namespace ChildGuard.UI.Controls
{
    /// <summary>
    /// Material Design App Bar / Top Navigation
    /// </summary>
    public class MaterialAppBar : Panel
    {
        private string _title = "";
        private bool _showBackButton = false;
        private MaterialButton? _backButton;
        private Label? _titleLabel;
        private Panel? _actionsPanel;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                if (_titleLabel != null)
                    _titleLabel.Text = value;
            }
        }

        public bool ShowBackButton
        {
            get => _showBackButton;
            set
            {
                _showBackButton = value;
                if (_backButton != null)
                    _backButton.Visible = value;
                UpdateLayout();
            }
        }

        public Panel ActionsPanel => _actionsPanel!;

        public MaterialAppBar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Height = 64;
            Dock = DockStyle.Top;
            BackColor = ColorScheme.MaterialFluent.Surface;
            
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.UserPaint | 
                    ControlStyles.DoubleBuffer | 
                    ControlStyles.ResizeRedraw, true);

            // Back button
            _backButton = new MaterialButton
            {
                Text = "←",
                Style = MaterialButton.ButtonStyle.Text,
                Size = new Size(48, 48),
                Location = new Point(8, 8),
                Font = new Font("Segoe UI", 16F),
                Visible = _showBackButton
            };
            _backButton.Click += (s, e) => OnBackButtonClick();

            // Title
            _titleLabel = new Label
            {
                Text = _title,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ColorScheme.MaterialFluent.TextPrimary,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            // Actions panel (right side)
            _actionsPanel = new Panel
            {
                Height = 48,
                Width = 200,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.Transparent
            };

            Controls.Add(_backButton);
            Controls.Add(_titleLabel);
            Controls.Add(_actionsPanel);

            UpdateLayout();
        }

        private void UpdateLayout()
        {
            if (_titleLabel == null || _actionsPanel == null) return;

            var leftMargin = _showBackButton ? 64 : 24;
            var rightMargin = 24;

            _titleLabel.Location = new Point(leftMargin, 0);
            _titleLabel.Size = new Size(Width - leftMargin - _actionsPanel.Width - rightMargin, Height);

            _actionsPanel.Location = new Point(Width - _actionsPanel.Width - rightMargin, 8);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            UpdateLayout();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Background
            using (var brush = new SolidBrush(BackColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            // Bottom border
            using (var pen = new Pen(ColorScheme.MaterialFluent.Divider, 1))
            {
                g.DrawLine(pen, 0, Height - 1, Width, Height - 1);
            }

            // Subtle shadow
            var shadowRect = new Rectangle(0, Height - 4, Width, 4);
            using (var shadowBrush = new LinearGradientBrush(
                shadowRect, 
                ColorScheme.MaterialFluent.Shadow1, 
                Color.Transparent, 
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(shadowBrush, shadowRect);
            }
        }

        public event EventHandler? BackButtonClick;

        protected virtual void OnBackButtonClick()
        {
            BackButtonClick?.Invoke(this, EventArgs.Empty);
        }

        public void AddAction(Control action)
        {
            if (_actionsPanel != null)
            {
                _actionsPanel.Controls.Add(action);
                // Auto-arrange actions
                ArrangeActions();
            }
        }

        private void ArrangeActions()
        {
            if (_actionsPanel == null) return;

            var x = _actionsPanel.Width;
            foreach (Control control in _actionsPanel.Controls)
            {
                x -= control.Width + 8;
                control.Location = new Point(Math.Max(0, x), (48 - control.Height) / 2);
            }
        }
    }
}
