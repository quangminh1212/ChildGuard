using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChildGuard.UI.GlassUI
{
    /// <summary>
    /// Glass Morphism Card with Backdrop Blur Effect
    /// Modern gaming/cyberpunk aesthetic
    /// </summary>
    public class GlassCard : Panel
    {
        public enum CardStyle
        {
            Glass,          // Pure glass morphism
            GlassNeon,      // Glass with neon border
            Frosted,        // Frosted glass effect
            Floating,       // Floating with heavy shadow
            Minimal         // Minimal glass
        }

        private CardStyle _style = CardStyle.Glass;
        private Color _neonColor = GlassColors.NeonCyan;
        private bool _isHovered = false;
        private System.Windows.Forms.Timer? _animationTimer;
        private float _hoverProgress = 0f;
        private float _glowIntensity = 0f;
        private int _cornerRadius = 16;
        private bool _hasGlow = false;
        private bool _isFloating = true;

        public CardStyle Style
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

        public bool IsFloating
        {
            get => _isFloating;
            set { _isFloating = value; Invalidate(); }
        }

        public GlassCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.UserPaint | 
                    ControlStyles.DoubleBuffer | 
                    ControlStyles.ResizeRedraw | 
                    ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Padding = new Padding(24);
            Margin = new Padding(12);
            
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
            const float speed = 0.08f;
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

            // Glow animation for neon style
            if (_hasGlow && _style == CardStyle.GlassNeon)
            {
                _glowIntensity = (float)(0.3 + 0.2 * Math.Sin(DateTime.Now.Millisecond * 0.008));
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

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width, Height);
            
            // Draw shadow/glow
            if (_isFloating || _hasGlow)
            {
                DrawShadowAndGlow(g, rect);
            }

            // Draw card background
            DrawCardBackground(g, rect);
            
            // Draw border
            DrawBorder(g, rect);
            
            // Draw backdrop blur simulation
            DrawBackdropEffect(g, rect);
        }

        private void DrawShadowAndGlow(Graphics g, Rectangle rect)
        {
            // Shadow for floating effect
            if (_isFloating)
            {
                var shadowSize = (int)(8 + 4 * _hoverProgress);
                var shadowOpacity = (int)(20 + 10 * _hoverProgress);

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

            // Glow effect for neon style
            if (_hasGlow && _style == CardStyle.GlassNeon)
            {
                var glowSize = (int)(12 * (_glowIntensity + _hoverProgress * 0.5));
                var glowRect = new Rectangle(
                    rect.X - glowSize,
                    rect.Y - glowSize,
                    rect.Width + glowSize * 2,
                    rect.Height + glowSize * 2
                );

                var glowOpacity = (int)(25 * (_glowIntensity + _hoverProgress * 0.3));
                using (var glowBrush = new SolidBrush(Color.FromArgb(glowOpacity, _neonColor)))
                using (var path = CreateRoundedPath(glowRect, _cornerRadius + glowSize))
                {
                    g.FillPath(glowBrush, path);
                }
            }
        }

        private void DrawCardBackground(Graphics g, Rectangle rect)
        {
            using (var path = CreateRoundedPath(rect, _cornerRadius))
            {
                switch (_style)
                {
                    case CardStyle.Glass:
                    case CardStyle.GlassNeon:
                        using (var glassBrush = GlassColors.CreateGlassGradient(rect))
                        {
                            g.FillPath(glassBrush, path);
                        }
                        break;

                    case CardStyle.Frosted:
                        var frostedColor = GlassColors.WithOpacity(GlassColors.Surface, 0.7);
                        if (_hoverProgress > 0)
                        {
                            frostedColor = GlassColors.BlendColors(frostedColor, GlassColors.SurfaceSecondary, _hoverProgress * 0.3);
                        }
                        using (var frostedBrush = new SolidBrush(frostedColor))
                        {
                            g.FillPath(frostedBrush, path);
                        }
                        break;

                    case CardStyle.Floating:
                        var floatingColor = GlassColors.Surface;
                        if (_hoverProgress > 0)
                        {
                            floatingColor = GlassColors.BlendColors(floatingColor, GlassColors.SurfaceSecondary, _hoverProgress * 0.5);
                        }
                        using (var floatingBrush = new SolidBrush(floatingColor))
                        {
                            g.FillPath(floatingBrush, path);
                        }
                        break;

                    case CardStyle.Minimal:
                        var minimalOpacity = 0.3 + _hoverProgress * 0.2;
                        var minimalColor = GlassColors.WithOpacity(GlassColors.Surface, minimalOpacity);
                        using (var minimalBrush = new SolidBrush(minimalColor))
                        {
                            g.FillPath(minimalBrush, path);
                        }
                        break;
                }
            }
        }

        private void DrawBorder(Graphics g, Rectangle rect)
        {
            var borderColor = _style switch
            {
                CardStyle.Glass => GlassColors.GlassBorder,
                CardStyle.GlassNeon => _neonColor,
                CardStyle.Frosted => GlassColors.WithOpacity(GlassColors.TextTertiary, 0.3),
                CardStyle.Floating => GlassColors.Border,
                CardStyle.Minimal => GlassColors.WithOpacity(GlassColors.GlassBorder, 0.5),
                _ => GlassColors.GlassBorder
            };

            // Enhance border on hover
            if (_hoverProgress > 0)
            {
                var enhancedColor = _style == CardStyle.GlassNeon ? 
                    GlassColors.Lighten(_neonColor, 0.2) : 
                    GlassColors.Lighten(borderColor, 0.3);
                borderColor = GlassColors.BlendColors(borderColor, enhancedColor, _hoverProgress);
            }

            var borderWidth = _style == CardStyle.GlassNeon ? 2 : 1;
            using (var pen = new Pen(borderColor, borderWidth))
            using (var path = CreateRoundedPath(rect, _cornerRadius))
            {
                g.DrawPath(pen, path);
            }
        }

        private void DrawBackdropEffect(Graphics g, Rectangle rect)
        {
            // Simulate backdrop blur with noise pattern
            if (_style == CardStyle.Glass || _style == CardStyle.GlassNeon || _style == CardStyle.Frosted)
            {
                var random = new Random(42); // Fixed seed for consistent pattern
                var noiseCount = _style == CardStyle.Frosted ? 80 : 40;
                
                using (var clipPath = CreateRoundedPath(rect, _cornerRadius))
                {
                    g.SetClip(clipPath);
                    
                    for (int i = 0; i < noiseCount; i++)
                    {
                        var x = random.Next(rect.X, rect.Right);
                        var y = random.Next(rect.Y, rect.Bottom);
                        var opacity = random.Next(3, 12);
                        var size = random.Next(1, 3);
                        
                        using (var noiseBrush = new SolidBrush(Color.FromArgb(opacity, 255, 255, 255)))
                        {
                            g.FillEllipse(noiseBrush, x, y, size, size);
                        }
                    }
                    
                    g.ResetClip();
                }
            }

            // Add highlight gradient
            if (_style != CardStyle.Minimal)
            {
                var highlightRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height / 4);
                var highlightOpacity = _style == CardStyle.Frosted ? 15 : 25;
                
                using (var highlightBrush = new LinearGradientBrush(
                    highlightRect,
                    Color.FromArgb((int)(highlightOpacity * (1 + _hoverProgress * 0.5)), 255, 255, 255),
                    Color.FromArgb(2, 255, 255, 255),
                    LinearGradientMode.Vertical))
                using (var highlightPath = CreateRoundedPath(highlightRect, _cornerRadius))
                {
                    g.FillPath(highlightBrush, highlightPath);
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
                ApplyGlassStyling(e.Control);
        }

        private void ApplyGlassStyling(Control? control)
        {
            if (control == null) return;

            if (control is Label label)
            {
                label.Font = new Font("Segoe UI", label.Font.Size, label.Font.Style);
                label.ForeColor = GlassColors.TextPrimary;
                label.BackColor = Color.Transparent;
            }
            else if (control is TextBox textBox)
            {
                textBox.Font = new Font("Segoe UI", textBox.Font.Size);
                textBox.BorderStyle = BorderStyle.None;
                textBox.BackColor = GlassColors.WithOpacity(GlassColors.Surface, 0.5);
                textBox.ForeColor = GlassColors.TextPrimary;
            }

            // Recursively apply to child controls
            foreach (Control child in control.Controls)
            {
                ApplyGlassStyling(child);
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
    /// Glass Card Header with modern typography
    /// </summary>
    public class GlassCardHeader : Panel
    {
        private Label titleLabel;
        private Label subtitleLabel;
        private Panel actionsPanel;

        public string Title
        {
            get => titleLabel.Text;
            set => titleLabel.Text = value;
        }

        public string Subtitle
        {
            get => subtitleLabel.Text;
            set { subtitleLabel.Text = value; subtitleLabel.Visible = !string.IsNullOrEmpty(value); }
        }

        public Panel Actions => actionsPanel;

        public GlassCardHeader()
        {
            Height = 72;
            Dock = DockStyle.Top;
            BackColor = Color.Transparent;
            Padding = new Padding(0, 0, 0, 20);

            titleLabel = new Label
            {
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = GlassColors.TextPrimary,
                AutoSize = false,
                Height = 32,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.BottomLeft,
                BackColor = Color.Transparent
            };

            subtitleLabel = new Label
            {
                Font = new Font("Segoe UI", 11F),
                ForeColor = GlassColors.TextSecondary,
                AutoSize = false,
                Height = 20,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.TopLeft,
                BackColor = Color.Transparent,
                Visible = false
            };

            actionsPanel = new Panel
            {
                Width = 150,
                Dock = DockStyle.Right,
                BackColor = Color.Transparent
            };

            Controls.Add(subtitleLabel);
            Controls.Add(titleLabel);
            Controls.Add(actionsPanel);
        }
    }
}
