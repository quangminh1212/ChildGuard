using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChildGuard.UI.FluentUI
{
    /// <summary>
    /// Fluent Design Button - Windows 11 style with proper animations and states
    /// </summary>
    public class FluentButton : Button
    {
        public enum FluentStyle
        {
            Standard,   // Default Windows 11 button
            Accent,     // Primary action button
            Subtle,     // Secondary action
            Outline,    // Outlined button
            Text        // Text-only button
        }

        private FluentStyle _style = FluentStyle.Standard;
        private bool _isHovered = false;
        private bool _isPressed = false;
        private System.Windows.Forms.Timer? _animationTimer;
        private float _animationProgress = 0f;
        private int _cornerRadius = 4;
        private bool _showFocusRect = true;

        public FluentStyle Style
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }

        public int CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = value; Invalidate(); }
        }

        public bool ShowFocusRect
        {
            get => _showFocusRect;
            set { _showFocusRect = value; Invalidate(); }
        }

        public FluentButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.UserPaint | 
                    ControlStyles.DoubleBuffer | 
                    ControlStyles.ResizeRedraw | 
                    ControlStyles.SupportsTransparentBackColor, true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            Size = new Size(120, 32);
            Cursor = Cursors.Hand;
            TabStop = true;
            UseVisualStyleBackColor = false;

            // Initialize animation timer
            _animationTimer = new System.Windows.Forms.Timer { Interval = 16 }; // 60 FPS
            _animationTimer.Tick += AnimationTimer_Tick;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            StartAnimation(true);
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            StartAnimation(false);
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            _isPressed = true;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _isPressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        private void StartAnimation(bool forward)
        {
            if (_animationTimer != null)
            {
                _animationTimer.Stop();
                _animationTimer.Start();
            }
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            const float animationSpeed = 0.15f;
            
            if (_isHovered)
            {
                _animationProgress = Math.Min(1f, _animationProgress + animationSpeed);
            }
            else
            {
                _animationProgress = Math.Max(0f, _animationProgress - animationSpeed);
            }

            Invalidate();

            if ((_isHovered && _animationProgress >= 1f) || (!_isHovered && _animationProgress <= 0f))
            {
                _animationTimer?.Stop();
            }
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, Width, Height);
            
            // Draw background
            DrawBackground(g, rect);
            
            // Draw border
            DrawBorder(g, rect);
            
            // Draw text
            DrawText(g, rect);
            
            // Draw focus indicator
            if (Focused && _showFocusRect && TabStop)
            {
                DrawFocusRect(g, rect);
            }
        }

        private void DrawBackground(Graphics g, Rectangle rect)
        {
            var bgColor = GetBackgroundColor();
            
            using (var brush = new SolidBrush(bgColor))
            using (var path = CreateRoundedPath(rect, _cornerRadius))
            {
                g.FillPath(brush, path);
            }

            // Add subtle gradient for depth
            if (_style == FluentStyle.Standard || _style == FluentStyle.Accent)
            {
                var gradientRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height / 2);
                using (var gradientBrush = new LinearGradientBrush(
                    gradientRect,
                    Color.FromArgb(20, 255, 255, 255),
                    Color.FromArgb(5, 255, 255, 255),
                    LinearGradientMode.Vertical))
                using (var path = CreateRoundedPath(gradientRect, _cornerRadius))
                {
                    g.FillPath(gradientBrush, path);
                }
            }
        }

        private void DrawBorder(Graphics g, Rectangle rect)
        {
            if (_style == FluentStyle.Outline || _style == FluentStyle.Standard)
            {
                var borderColor = GetBorderColor();
                using (var pen = new Pen(borderColor, 1))
                using (var path = CreateRoundedPath(rect, _cornerRadius))
                {
                    g.DrawPath(pen, path);
                }
            }
        }

        private void DrawText(Graphics g, Rectangle rect)
        {
            var textColor = GetTextColor();
            var textRect = new Rectangle(rect.X + 12, rect.Y, rect.Width - 24, rect.Height);
            
            using (var brush = new SolidBrush(textColor))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                };
                
                g.DrawString(Text, Font, brush, textRect, sf);
            }
        }

        private void DrawFocusRect(Graphics g, Rectangle rect)
        {
            var focusRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
            using (var pen = new Pen(FluentColors.Focus, 2))
            using (var path = CreateRoundedPath(focusRect, Math.Max(0, _cornerRadius - 2)))
            {
                pen.DashStyle = DashStyle.Dot;
                g.DrawPath(pen, path);
            }
        }

        private Color GetBackgroundColor()
        {
            if (!Enabled)
                return FluentColors.Gray30;

            var baseColor = _style switch
            {
                FluentStyle.Standard => FluentColors.Surface,
                FluentStyle.Accent => FluentColors.Primary,
                FluentStyle.Subtle => Color.Transparent,
                FluentStyle.Outline => Color.Transparent,
                FluentStyle.Text => Color.Transparent,
                _ => FluentColors.Surface
            };

            if (_isPressed)
            {
                return _style switch
                {
                    FluentStyle.Standard => FluentColors.Gray40,
                    FluentStyle.Accent => FluentColors.PrimaryDark,
                    FluentStyle.Subtle => FluentColors.Gray30,
                    FluentStyle.Outline => FluentColors.Gray20,
                    FluentStyle.Text => FluentColors.Gray20,
                    _ => FluentColors.Gray40
                };
            }

            if (_animationProgress > 0)
            {
                var hoverColor = _style switch
                {
                    FluentStyle.Standard => FluentColors.Gray20,
                    FluentStyle.Accent => FluentColors.PrimaryLight,
                    FluentStyle.Subtle => FluentColors.Gray20,
                    FluentStyle.Outline => FluentColors.Gray10,
                    FluentStyle.Text => FluentColors.Gray10,
                    _ => FluentColors.Gray20
                };

                return BlendColors(baseColor, hoverColor, _animationProgress);
            }

            return baseColor;
        }

        private Color GetBorderColor()
        {
            if (!Enabled)
                return FluentColors.Gray60;

            if (_isPressed)
                return FluentColors.Gray80;

            if (_isHovered)
                return FluentColors.Gray70;

            return _style switch
            {
                FluentStyle.Standard => FluentColors.Border,
                FluentStyle.Outline => FluentColors.Primary,
                _ => FluentColors.Border
            };
        }

        private Color GetTextColor()
        {
            if (!Enabled)
                return FluentColors.TextDisabled;

            return _style switch
            {
                FluentStyle.Standard => FluentColors.TextPrimary,
                FluentStyle.Accent => FluentColors.TextOnPrimary,
                FluentStyle.Subtle => FluentColors.TextPrimary,
                FluentStyle.Outline => FluentColors.Primary,
                FluentStyle.Text => FluentColors.Primary,
                _ => FluentColors.TextPrimary
            };
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
}
