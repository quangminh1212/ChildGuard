using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ChildGuard.UI.Theming;

namespace ChildGuard.UI.Controls
{
    /// <summary>
    /// Modern button inspired by Google Material Design and Windows Fluent Design
    /// </summary>
    public class MaterialButton : Button
    {
        public enum ButtonStyle
        {
            Primary,    // Filled button with primary color
            Secondary,  // Outlined button
            Text,       // Text-only button
            Danger,     // Red filled button
            Success     // Green filled button
        }

        private ButtonStyle _style = ButtonStyle.Primary;
        private bool _isHovered = false;
        private bool _isPressed = false;
        private int _cornerRadius = 8;
        private int _elevation = 2;

        public ButtonStyle Style
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }

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

        public MaterialButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.DoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.SupportsTransparentBackColor, true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            Size = new Size(140, 48);
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;
            UseVisualStyleBackColor = false;

            // Better text rendering
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
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

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, Width, Height);
            
            // Draw shadow for elevation
            if (_elevation > 0 && _style != ButtonStyle.Text)
            {
                DrawShadow(g, rect);
            }

            // Draw button background
            DrawBackground(g, rect);

            // Draw text
            DrawText(g, rect);
        }

        private void DrawShadow(Graphics g, Rectangle rect)
        {
            var shadowRect = new Rectangle(rect.X, rect.Y + _elevation, rect.Width, rect.Height);
            var shadowColor = _elevation switch
            {
                1 => ColorScheme.MaterialFluent.Shadow1,
                2 => ColorScheme.MaterialFluent.Shadow2,
                3 => ColorScheme.MaterialFluent.Shadow3,
                _ => ColorScheme.MaterialFluent.Shadow4
            };

            using (var shadowBrush = new SolidBrush(shadowColor))
            using (var shadowPath = CreateRoundedPath(shadowRect, _cornerRadius))
            {
                g.FillPath(shadowBrush, shadowPath);
            }
        }

        private void DrawBackground(Graphics g, Rectangle rect)
        {
            Color backgroundColor = GetBackgroundColor();
            
            if (_style == ButtonStyle.Text)
            {
                // Text button - only background on hover/press
                if (_isHovered || _isPressed)
                {
                    using (var brush = new SolidBrush(backgroundColor))
                    using (var path = CreateRoundedPath(rect, _cornerRadius))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }
            else
            {
                // Filled or outlined button
                using (var brush = new SolidBrush(backgroundColor))
                using (var path = CreateRoundedPath(rect, _cornerRadius))
                {
                    g.FillPath(brush, path);
                }

                // Draw border for secondary style
                if (_style == ButtonStyle.Secondary)
                {
                    var borderColor = _isHovered ? ColorScheme.MaterialFluent.Primary : ColorScheme.MaterialFluent.Border;
                    using (var pen = new Pen(borderColor, 1))
                    using (var path = CreateRoundedPath(rect, _cornerRadius))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }
        }

        private void DrawText(Graphics g, Rectangle rect)
        {
            var textColor = GetTextColor();
            var textRect = rect;
            
            // Adjust for shadow offset
            if (_elevation > 0 && _style != ButtonStyle.Text)
            {
                textRect.Y -= _elevation / 2;
            }

            using (var brush = new SolidBrush(textColor))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                
                g.DrawString(Text, Font, brush, textRect, sf);
            }
        }

        private Color GetBackgroundColor()
        {
            return _style switch
            {
                ButtonStyle.Primary => _isPressed ? ColorScheme.MaterialFluent.PrimaryPressed :
                                     _isHovered ? ColorScheme.MaterialFluent.PrimaryHover :
                                     ColorScheme.MaterialFluent.Primary,
                
                ButtonStyle.Secondary => _isPressed ? ColorScheme.MaterialFluent.Pressed :
                                       _isHovered ? ColorScheme.MaterialFluent.Hover :
                                       ColorScheme.MaterialFluent.Surface,
                
                ButtonStyle.Text => _isPressed ? ColorScheme.MaterialFluent.Pressed :
                                  _isHovered ? ColorScheme.MaterialFluent.Hover :
                                  Color.Transparent,
                
                ButtonStyle.Danger => _isPressed ? ColorScheme.MaterialFluent.Error :
                                    _isHovered ? ColorScheme.Lighten(ColorScheme.MaterialFluent.Error, 0.1f) :
                                    ColorScheme.MaterialFluent.Error,
                
                ButtonStyle.Success => _isPressed ? ColorScheme.MaterialFluent.Success :
                                     _isHovered ? ColorScheme.Lighten(ColorScheme.MaterialFluent.Success, 0.1f) :
                                     ColorScheme.MaterialFluent.Success,
                
                _ => ColorScheme.MaterialFluent.Primary
            };
        }

        private Color GetTextColor()
        {
            return _style switch
            {
                ButtonStyle.Primary => ColorScheme.MaterialFluent.TextOnPrimary,
                ButtonStyle.Secondary => _isHovered ? ColorScheme.MaterialFluent.Primary : ColorScheme.MaterialFluent.TextPrimary,
                ButtonStyle.Text => ColorScheme.MaterialFluent.Primary,
                ButtonStyle.Danger => ColorScheme.MaterialFluent.TextOnPrimary,
                ButtonStyle.Success => ColorScheme.MaterialFluent.TextOnPrimary,
                _ => ColorScheme.MaterialFluent.TextOnPrimary
            };
        }

        private GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            var diameter = radius * 2;
            
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            
            return path;
        }
    }
}
