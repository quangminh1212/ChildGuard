using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChildGuard.UI.GlassUI
{
    /// <summary>
    /// Glass Morphism Button with Neon Glow Effects
    /// Modern gaming/cyberpunk aesthetic
    /// </summary>
    public class GlassButton : Button
    {
        public enum GlassStyle
        {
            Glass,      // Pure glass morphism
            Neon,       // Neon glow effect
            Solid,      // Solid with glass border
            Outline,    // Outline with glow
            Ghost       // Minimal ghost button
        }

        private GlassStyle _style = GlassStyle.Glass;
        private Color _neonColor = GlassColors.NeonCyan;
        private bool _isHovered = false;
        private bool _isPressed = false;
        private System.Windows.Forms.Timer? _animationTimer;
        private float _glowIntensity = 0f;
        private float _hoverProgress = 0f;
        private int _cornerRadius = 12;
        private bool _hasGlow = true;

        public GlassStyle Style
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }

        public Color NeonColor
        {
            get => _neonColor;
            set { _neonColor = value; Invalidate(); }
        }

        public int CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = value; Invalidate(); }
        }

        public bool HasGlow
        {
            get => _hasGlow;
            set { _hasGlow = value; Invalidate(); }
        }

        public GlassButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.UserPaint | 
                    ControlStyles.DoubleBuffer | 
                    ControlStyles.ResizeRedraw | 
                    ControlStyles.SupportsTransparentBackColor, true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            Size = new Size(140, 40);
            Cursor = Cursors.Hand;
            TabStop = true;
            UseVisualStyleBackColor = false;

            // Initialize animation
            _animationTimer = new System.Windows.Forms.Timer { Interval = 16 }; // 60 FPS
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

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            const float speed = 0.12f;
            bool needsUpdate = false;

            // Hover animation
            if (_isHovered)
            {
                if (_hoverProgress < 1f)
                {
                    _hoverProgress = Math.Min(1f, _hoverProgress + speed);
                    needsUpdate = true;
                }
            }
            else
            {
                if (_hoverProgress > 0f)
                {
                    _hoverProgress = Math.Max(0f, _hoverProgress - speed);
                    needsUpdate = true;
                }
            }

            // Glow animation
            if (_hasGlow && _style == GlassStyle.Neon)
            {
                _glowIntensity = (float)(0.5 + 0.3 * Math.Sin(DateTime.Now.Millisecond * 0.01));
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                Invalidate();
            }

            if (!needsUpdate && (!_isHovered || _hoverProgress >= 1f))
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
            
            // Draw glow effect
            if (_hasGlow && (_style == GlassStyle.Neon || _hoverProgress > 0))
            {
                DrawGlowEffect(g, rect);
            }
            
            // Draw main button
            DrawButton(g, rect);
            
            // Draw text with glow
            DrawText(g, rect);
            
            // Draw focus indicator
            if (Focused && TabStop)
            {
                DrawFocusIndicator(g, rect);
            }
        }

        private void DrawGlowEffect(Graphics g, Rectangle rect)
        {
            var glowSize = (int)(8 * (_hoverProgress + _glowIntensity));
            var glowRect = new Rectangle(
                rect.X - glowSize,
                rect.Y - glowSize,
                rect.Width + glowSize * 2,
                rect.Height + glowSize * 2
            );

            var glowColor = _style == GlassStyle.Neon ? _neonColor : GlassColors.Primary;
            var glowOpacity = (int)(30 * (_hoverProgress + _glowIntensity * 0.5));

            using (var glowBrush = new SolidBrush(Color.FromArgb(glowOpacity, glowColor)))
            using (var path = CreateRoundedPath(glowRect, _cornerRadius + glowSize))
            {
                g.FillPath(glowBrush, path);
            }
        }

        private void DrawButton(Graphics g, Rectangle rect)
        {
            using (var path = CreateRoundedPath(rect, _cornerRadius))
            {
                // Draw background
                DrawBackground(g, rect, path);
                
                // Draw border
                DrawBorder(g, rect, path);
            }
        }

        private void DrawBackground(Graphics g, Rectangle rect, GraphicsPath path)
        {
            switch (_style)
            {
                case GlassStyle.Glass:
                    using (var glassBrush = GlassColors.CreateGlassGradient(rect))
                    {
                        g.FillPath(glassBrush, path);
                    }
                    break;

                case GlassStyle.Neon:
                    using (var neonBrush = GlassColors.CreateNeonGradient(rect, _neonColor))
                    {
                        g.FillPath(neonBrush, path);
                    }
                    break;

                case GlassStyle.Solid:
                    var solidColor = _isPressed ? GlassColors.SurfaceHover : 
                                   _hoverProgress > 0 ? GlassColors.BlendColors(GlassColors.Surface, GlassColors.SurfaceSecondary, _hoverProgress) :
                                   GlassColors.Surface;
                    using (var solidBrush = new SolidBrush(solidColor))
                    {
                        g.FillPath(solidBrush, path);
                    }
                    break;

                case GlassStyle.Outline:
                case GlassStyle.Ghost:
                    if (_hoverProgress > 0 || _isPressed)
                    {
                        var hoverColor = GlassColors.WithOpacity(_neonColor, 0.1 * _hoverProgress);
                        using (var hoverBrush = new SolidBrush(hoverColor))
                        {
                            g.FillPath(hoverBrush, path);
                        }
                    }
                    break;
            }

            // Add highlight
            if (_style != GlassStyle.Ghost)
            {
                var highlightRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height / 3);
                using (var highlightBrush = new LinearGradientBrush(
                    highlightRect,
                    Color.FromArgb((int)(40 * (1 + _hoverProgress)), 255, 255, 255),
                    Color.FromArgb(5, 255, 255, 255),
                    LinearGradientMode.Vertical))
                using (var highlightPath = CreateRoundedPath(highlightRect, _cornerRadius))
                {
                    g.FillPath(highlightBrush, highlightPath);
                }
            }
        }

        private void DrawBorder(Graphics g, Rectangle rect, GraphicsPath path)
        {
            var borderColor = _style switch
            {
                GlassStyle.Glass => GlassColors.GlassBorder,
                GlassStyle.Neon => _neonColor,
                GlassStyle.Outline => _neonColor,
                GlassStyle.Solid => GlassColors.Border,
                GlassStyle.Ghost => GlassColors.WithOpacity(_neonColor, 0.3),
                _ => GlassColors.Border
            };

            if (_hoverProgress > 0)
            {
                borderColor = GlassColors.BlendColors(borderColor, GlassColors.Lighten(borderColor, 0.3), _hoverProgress);
            }

            using (var pen = new Pen(borderColor, _style == GlassStyle.Outline ? 2 : 1))
            {
                g.DrawPath(pen, path);
            }
        }

        private void DrawText(Graphics g, Rectangle rect)
        {
            var textColor = GetTextColor();
            var textRect = new Rectangle(rect.X + 16, rect.Y, rect.Width - 32, rect.Height);

            // Draw text glow for neon style
            if (_style == GlassStyle.Neon && _hasGlow)
            {
                using (var glowBrush = new SolidBrush(Color.FromArgb((int)(60 * _glowIntensity), _neonColor)))
                {
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    
                    // Draw glow text slightly offset
                    for (int i = 1; i <= 2; i++)
                    {
                        var glowRect = new Rectangle(textRect.X + i, textRect.Y + i, textRect.Width, textRect.Height);
                        g.DrawString(Text, Font, glowBrush, glowRect, sf);
                    }
                }
            }

            // Draw main text
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

        private void DrawFocusIndicator(Graphics g, Rectangle rect)
        {
            var focusRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
            using (var pen = new Pen(GlassColors.BorderFocus, 2))
            using (var path = CreateRoundedPath(focusRect, Math.Max(0, _cornerRadius - 2)))
            {
                pen.DashStyle = DashStyle.Dot;
                g.DrawPath(pen, path);
            }
        }

        private Color GetTextColor()
        {
            if (!Enabled)
                return GlassColors.TextDisabled;

            return _style switch
            {
                GlassStyle.Glass => GlassColors.TextPrimary,
                GlassStyle.Neon => GlassColors.TextPrimary,
                GlassStyle.Solid => GlassColors.TextPrimary,
                GlassStyle.Outline => _neonColor,
                GlassStyle.Ghost => GlassColors.TextSecondary,
                _ => GlassColors.TextPrimary
            };
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
