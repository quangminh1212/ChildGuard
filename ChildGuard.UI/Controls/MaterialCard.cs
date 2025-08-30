using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ChildGuard.UI.Theming;

namespace ChildGuard.UI.Controls
{
    /// <summary>
    /// Modern card component inspired by Google Material Design
    /// </summary>
    public class MaterialCard : Panel
    {
        private int _cornerRadius = 12;
        private int _elevation = 1;
        private bool _isHovered = false;
        private Color _backgroundColor = ColorScheme.MaterialFluent.Surface;

        public int CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = value; Invalidate(); }
        }

        public int Elevation
        {
            get => _elevation;
            set { _elevation = value; Invalidate(); }
        }

        public Color CardBackgroundColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; Invalidate(); }
        }

        public MaterialCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.DoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Padding = new Padding(24);
            Margin = new Padding(8);

            // Default Material Design values
            _cornerRadius = 16;
            _elevation = 2;
            _backgroundColor = ColorScheme.MaterialFluent.Surface;
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
            if (_elevation > 0)
            {
                DrawShadow(g, rect);
            }

            // Draw card background
            DrawCard(g, rect);
        }

        private void DrawShadow(Graphics g, Rectangle rect)
        {
            var shadowOffset = _elevation;
            var shadowRect = new Rectangle(
                rect.X + shadowOffset, 
                rect.Y + shadowOffset, 
                rect.Width, 
                rect.Height
            );

            var shadowColor = _elevation switch
            {
                1 => ColorScheme.MaterialFluent.Shadow1,
                2 => ColorScheme.MaterialFluent.Shadow2,
                3 => ColorScheme.MaterialFluent.Shadow3,
                _ => ColorScheme.MaterialFluent.Shadow4
            };

            // Create gradient shadow for more realistic effect
            using (var shadowPath = CreateRoundedPath(shadowRect, _cornerRadius))
            {
                var shadowBounds = shadowPath.GetBounds();
                using (var shadowBrush = new PathGradientBrush(shadowPath))
                {
                    shadowBrush.CenterColor = shadowColor;
                    shadowBrush.SurroundColors = new[] { Color.Transparent };
                    shadowBrush.FocusScales = new PointF(0.8f, 0.8f);
                    
                    g.FillPath(shadowBrush, shadowPath);
                }
            }
        }

        private void DrawCard(Graphics g, Rectangle rect)
        {
            var cardColor = _isHovered ? 
                ColorScheme.Lighten(_backgroundColor, 0.02f) : 
                _backgroundColor;

            using (var brush = new SolidBrush(cardColor))
            using (var path = CreateRoundedPath(rect, _cornerRadius))
            {
                g.FillPath(brush, path);
            }

            // Draw subtle border
            var borderColor = ColorScheme.MaterialFluent.BorderLight;
            using (var pen = new Pen(borderColor, 1))
            using (var path = CreateRoundedPath(rect, _cornerRadius))
            {
                g.DrawPath(pen, path);
            }
        }

        private GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            var diameter = radius * 2;
            
            if (diameter > rect.Width || diameter > rect.Height)
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

            // Apply modern styling to child controls
            if (e.Control != null)
                ApplyModernStyling(e.Control);
        }

        private void ApplyModernStyling(Control? control)
        {
            if (control == null) return;

            if (control is Label label)
            {
                label.Font = new Font("Segoe UI", label.Font.Size, label.Font.Style);
                label.ForeColor = ColorScheme.MaterialFluent.TextPrimary;
                label.BackColor = Color.Transparent;
            }
            else if (control is TextBox textBox)
            {
                textBox.Font = new Font("Segoe UI", textBox.Font.Size);
                textBox.BorderStyle = BorderStyle.FixedSingle;
                textBox.BackColor = ColorScheme.MaterialFluent.SurfaceVariant;
                textBox.ForeColor = ColorScheme.MaterialFluent.TextPrimary;
            }

            // Recursively apply to child controls
            foreach (Control child in control.Controls)
            {
                ApplyModernStyling(child);
            }
        }
    }
}
