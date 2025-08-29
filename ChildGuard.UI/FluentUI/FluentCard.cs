using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChildGuard.UI.FluentUI
{
    /// <summary>
    /// Fluent Design Card with Acrylic effect and proper elevation
    /// </summary>
    public class FluentCard : Panel
    {
        public enum CardElevation
        {
            None = 0,
            Low = 2,
            Medium = 4,
            High = 8,
            VeryHigh = 16
        }

        private CardElevation _elevation = CardElevation.Low;
        private int _cornerRadius = 8;
        private bool _hasAcrylicEffect = false;
        private bool _isHovered = false;
        private System.Windows.Forms.Timer? _animationTimer;
        private float _hoverProgress = 0f;

        public CardElevation Elevation
        {
            get => _elevation;
            set { _elevation = value; Invalidate(); }
        }

        public int CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = value; Invalidate(); }
        }

        public bool HasAcrylicEffect
        {
            get => _hasAcrylicEffect;
            set { _hasAcrylicEffect = value; Invalidate(); }
        }

        public FluentCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.UserPaint | 
                    ControlStyles.DoubleBuffer | 
                    ControlStyles.ResizeRedraw | 
                    ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Padding = new Padding(20);
            Margin = new Padding(8);
            
            // Initialize animation
            _animationTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _animationTimer.Tick += AnimationTimer_Tick;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            _animationTimer?.Start();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            _animationTimer?.Start();
            base.OnMouseLeave(e);
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            const float speed = 0.1f;
            
            if (_isHovered)
            {
                _hoverProgress = Math.Min(1f, _hoverProgress + speed);
            }
            else
            {
                _hoverProgress = Math.Max(0f, _hoverProgress - speed);
            }

            Invalidate();

            if ((_isHovered && _hoverProgress >= 1f) || (!_isHovered && _hoverProgress <= 0f))
            {
                _animationTimer?.Stop();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width, Height);
            
            // Draw shadow
            if (_elevation != CardElevation.None)
            {
                DrawShadow(g, rect);
            }

            // Draw card background
            DrawCardBackground(g, rect);
            
            // Draw border
            DrawBorder(g, rect);
            
            // Draw acrylic effect
            if (_hasAcrylicEffect)
            {
                DrawAcrylicEffect(g, rect);
            }
        }

        private void DrawShadow(Graphics g, Rectangle rect)
        {
            var shadowSize = (int)_elevation;
            var shadowOpacity = _elevation switch
            {
                CardElevation.Low => 12,
                CardElevation.Medium => 20,
                CardElevation.High => 28,
                CardElevation.VeryHigh => 36,
                _ => 0
            };

            // Add hover effect to shadow
            if (_hoverProgress > 0)
            {
                shadowSize += (int)(2 * _hoverProgress);
                shadowOpacity += (int)(8 * _hoverProgress);
            }

            // Draw multiple shadow layers for realistic effect
            for (int i = 1; i <= shadowSize; i++)
            {
                var shadowRect = new Rectangle(
                    rect.X + i,
                    rect.Y + i,
                    rect.Width,
                    rect.Height
                );

                var layerOpacity = (int)(shadowOpacity * (1.0 - (double)i / shadowSize));
                using (var shadowBrush = new SolidBrush(Color.FromArgb(layerOpacity, 0, 0, 0)))
                using (var path = CreateRoundedPath(shadowRect, _cornerRadius))
                {
                    g.FillPath(shadowBrush, path);
                }
            }
        }

        private void DrawCardBackground(Graphics g, Rectangle rect)
        {
            var bgColor = FluentColors.Surface;
            
            // Add hover effect
            if (_hoverProgress > 0)
            {
                bgColor = BlendColors(bgColor, FluentColors.Gray10, _hoverProgress * 0.5f);
            }

            using (var brush = new SolidBrush(bgColor))
            using (var path = CreateRoundedPath(rect, _cornerRadius))
            {
                g.FillPath(brush, path);
            }
        }

        private void DrawBorder(Graphics g, Rectangle rect)
        {
            var borderColor = FluentColors.BorderSecondary;
            
            // Enhance border on hover
            if (_hoverProgress > 0)
            {
                borderColor = BlendColors(borderColor, FluentColors.Border, _hoverProgress);
            }

            using (var pen = new Pen(borderColor, 1))
            using (var path = CreateRoundedPath(rect, _cornerRadius))
            {
                g.DrawPath(pen, path);
            }
        }

        private void DrawAcrylicEffect(Graphics g, Rectangle rect)
        {
            // Create acrylic-like effect with subtle noise and transparency
            using (var acrylicBrush = new LinearGradientBrush(
                rect,
                Color.FromArgb(20, 255, 255, 255),
                Color.FromArgb(5, 255, 255, 255),
                LinearGradientMode.Vertical))
            using (var path = CreateRoundedPath(rect, _cornerRadius))
            {
                g.FillPath(acrylicBrush, path);
            }

            // Add subtle noise pattern
            var random = new Random(42); // Fixed seed for consistent pattern
            for (int i = 0; i < 50; i++)
            {
                var x = random.Next(rect.X, rect.Right);
                var y = random.Next(rect.Y, rect.Bottom);
                var opacity = random.Next(5, 15);
                
                using (var noiseBrush = new SolidBrush(Color.FromArgb(opacity, 255, 255, 255)))
                {
                    g.FillEllipse(noiseBrush, x, y, 1, 1);
                }
            }
        }

        private Color BlendColors(Color color1, Color color2, float ratio)
        {
            ratio = Math.Max(0, Math.Min(1, ratio));
            
            return Color.FromArgb(
                (int)(color1.A + (color2.A - color1.A) * ratio),
                (int)(color1.R + (color2.R - color1.R) * ratio),
                (int)(color1.G + (color2.G - color1.G) * ratio),
                (int)(color1.B + (color2.B - color1.B) * ratio)
            );
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
            ApplyFluentStyling(e.Control);
        }

        private void ApplyFluentStyling(Control control)
        {
            if (control is Label label)
            {
                label.Font = new Font("Segoe UI", label.Font.Size, label.Font.Style);
                label.ForeColor = FluentColors.TextPrimary;
                label.BackColor = Color.Transparent;
            }
            else if (control is TextBox textBox)
            {
                textBox.Font = new Font("Segoe UI", textBox.Font.Size);
                textBox.BorderStyle = BorderStyle.FixedSingle;
                textBox.BackColor = FluentColors.BackgroundSecondary;
                textBox.ForeColor = FluentColors.TextPrimary;
            }

            // Recursively apply to child controls
            foreach (Control child in control.Controls)
            {
                ApplyFluentStyling(child);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer?.Stop();
                _animationTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Fluent Card Header with proper typography
    /// </summary>
    public class FluentCardHeader : Panel
    {
        private Label titleLabel;
        private Panel actionsPanel;

        public string Title
        {
            get => titleLabel.Text;
            set => titleLabel.Text = value;
        }

        public Panel Actions => actionsPanel;

        public FluentCardHeader()
        {
            Height = 56;
            Dock = DockStyle.Top;
            BackColor = Color.Transparent;
            Padding = new Padding(0, 0, 0, 16);

            titleLabel = new Label
            {
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = FluentColors.TextPrimary,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                BackColor = Color.Transparent
            };

            actionsPanel = new Panel
            {
                Width = 120,
                Dock = DockStyle.Right,
                BackColor = Color.Transparent
            };

            Controls.Add(titleLabel);
            Controls.Add(actionsPanel);
        }
    }
}
