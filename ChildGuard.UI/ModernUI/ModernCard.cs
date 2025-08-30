using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChildGuard.UI.ModernUI
{
    /// <summary>
    /// Modern card component inspired by GitHub, Notion, and modern web design
    /// </summary>
    public class ModernCard : Panel
    {
        private int _borderRadius = 8;
        private bool _hasShadow = true;
        private bool _isHovered = false;
        private Color _borderColor = Color.FromArgb(229, 231, 235);
        private Color _shadowColor = Color.FromArgb(16, 0, 0, 0);

        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; Invalidate(); }
        }

        public bool HasShadow
        {
            get => _hasShadow;
            set { _hasShadow = value; Invalidate(); }
        }

        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        public ModernCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.UserPaint | 
                    ControlStyles.DoubleBuffer | 
                    ControlStyles.ResizeRedraw | 
                    ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.White;
            Padding = new Padding(16);
            Margin = new Padding(8);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width, Height);
            
            // Draw shadow
            if (_hasShadow)
            {
                DrawShadow(g, rect);
            }

            // Draw card background
            using (var brush = new SolidBrush(BackColor))
            using (var path = CreateRoundedPath(rect, _borderRadius))
            {
                g.FillPath(brush, path);
            }

            // Draw border
            using (var pen = new Pen(_borderColor, 1))
            using (var path = CreateRoundedPath(rect, _borderRadius))
            {
                g.DrawPath(pen, path);
            }

            // Draw hover effect
            if (_isHovered)
            {
                using (var brush = new SolidBrush(Color.FromArgb(8, 0, 0, 0)))
                using (var path = CreateRoundedPath(rect, _borderRadius))
                {
                    g.FillPath(brush, path);
                }
            }
        }

        private void DrawShadow(Graphics g, Rectangle rect)
        {
            // Create multiple shadow layers for realistic effect
            var shadowOffsets = new[] { 1, 2, 4 };
            var shadowOpacities = new[] { 8, 12, 16 };

            for (int i = 0; i < shadowOffsets.Length; i++)
            {
                var offset = shadowOffsets[i];
                var opacity = shadowOpacities[i];
                
                var shadowRect = new Rectangle(
                    rect.X + offset, 
                    rect.Y + offset, 
                    rect.Width, 
                    rect.Height
                );

                using (var shadowBrush = new SolidBrush(Color.FromArgb(opacity, 0, 0, 0)))
                using (var path = CreateRoundedPath(shadowRect, _borderRadius))
                {
                    g.FillPath(shadowBrush, path);
                }
            }
        }

        private GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            var diameter = radius * 2;
            
            if (diameter > rect.Width || diameter > rect.Height || radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }
            
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            
            return path;
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (e.Control != null)
                ApplyModernStyling(e.Control);
        }

        private void ApplyModernStyling(Control? control)
        {
            if (control == null) return;

            if (control is Label label)
            {
                label.Font = new Font("Segoe UI", label.Font.Size, label.Font.Style);
                label.ForeColor = Color.FromArgb(55, 65, 81);
                label.BackColor = Color.Transparent;
            }
            else if (control is TextBox textBox)
            {
                textBox.Font = new Font("Segoe UI", textBox.Font.Size);
                textBox.BorderStyle = BorderStyle.FixedSingle;
                textBox.BackColor = Color.FromArgb(249, 250, 251);
                textBox.ForeColor = Color.FromArgb(55, 65, 81);
            }

            // Recursively apply to child controls
            foreach (Control child in control.Controls)
            {
                ApplyModernStyling(child);
            }
        }
    }

    /// <summary>
    /// Card header component for titles and actions
    /// </summary>
    public class ModernCardHeader : Panel
    {
        private Label titleLabel;
        private Panel actionsPanel;

        public string Title
        {
            get => titleLabel.Text;
            set => titleLabel.Text = value;
        }

        public Panel Actions => actionsPanel;

        public ModernCardHeader()
        {
            Height = 48;
            Dock = DockStyle.Top;
            BackColor = Color.Transparent;
            Padding = new Padding(0, 0, 0, 8);

            titleLabel = new Label
            {
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            actionsPanel = new Panel
            {
                Width = 100,
                Dock = DockStyle.Right,
                BackColor = Color.Transparent
            };

            Controls.Add(titleLabel);
            Controls.Add(actionsPanel);
        }
    }

    /// <summary>
    /// Card content area
    /// </summary>
    public class ModernCardContent : Panel
    {
        public ModernCardContent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.Transparent;
            Padding = new Padding(0);
        }
    }
}
