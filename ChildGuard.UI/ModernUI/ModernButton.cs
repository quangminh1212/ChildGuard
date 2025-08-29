using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChildGuard.UI.ModernUI
{
    /// <summary>
    /// Modern button inspired by GitHub, VS Code, and modern web apps
    /// </summary>
    public class ModernButton : Button
    {
        public enum ButtonVariant
        {
            Primary,    // Blue filled
            Secondary,  // Gray outlined  
            Success,    // Green filled
            Danger,     // Red filled
            Ghost,      // Transparent with hover
            Link        // Text only
        }

        private ButtonVariant _variant = ButtonVariant.Primary;
        private bool _isHovered = false;
        private bool _isPressed = false;
        private int _borderRadius = 6;

        public ButtonVariant Variant
        {
            get => _variant;
            set { _variant = value; Invalidate(); }
        }

        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; Invalidate(); }
        }

        public ModernButton()
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
            Size = new Size(100, 32);
            Cursor = Cursors.Hand;
            TabStop = true;
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
            
            // Get colors based on variant and state
            var colors = GetColors();
            
            // Draw background
            if (_variant != ButtonVariant.Link)
            {
                using (var brush = new SolidBrush(colors.Background))
                using (var path = CreateRoundedPath(rect, _borderRadius))
                {
                    g.FillPath(brush, path);
                }

                // Draw border for outlined variants
                if (_variant == ButtonVariant.Secondary || _variant == ButtonVariant.Ghost)
                {
                    using (var pen = new Pen(colors.Border, 1))
                    using (var path = CreateRoundedPath(rect, _borderRadius))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            // Draw text
            var textRect = new Rectangle(8, 0, Width - 16, Height);
            using (var brush = new SolidBrush(colors.Text))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                
                g.DrawString(Text, Font, brush, textRect, sf);
            }

            // Draw focus indicator
            if (Focused && TabStop)
            {
                var focusRect = new Rectangle(2, 2, Width - 4, Height - 4);
                using (var pen = new Pen(Color.FromArgb(0, 120, 215), 2))
                using (var path = CreateRoundedPath(focusRect, _borderRadius - 1))
                {
                    pen.DashStyle = DashStyle.Dot;
                    g.DrawPath(pen, path);
                }
            }
        }

        private (Color Background, Color Text, Color Border) GetColors()
        {
            return _variant switch
            {
                ButtonVariant.Primary => GetPrimaryColors(),
                ButtonVariant.Secondary => GetSecondaryColors(),
                ButtonVariant.Success => GetSuccessColors(),
                ButtonVariant.Danger => GetDangerColors(),
                ButtonVariant.Ghost => GetGhostColors(),
                ButtonVariant.Link => GetLinkColors(),
                _ => GetPrimaryColors()
            };
        }

        private (Color Background, Color Text, Color Border) GetPrimaryColors()
        {
            if (!Enabled)
                return (Color.FromArgb(229, 229, 229), Color.FromArgb(166, 166, 166), Color.FromArgb(229, 229, 229));

            if (_isPressed)
                return (Color.FromArgb(0, 84, 166), Color.White, Color.FromArgb(0, 84, 166));
            
            if (_isHovered)
                return (Color.FromArgb(0, 102, 204), Color.White, Color.FromArgb(0, 102, 204));
            
            return (Color.FromArgb(0, 120, 215), Color.White, Color.FromArgb(0, 120, 215));
        }

        private (Color Background, Color Text, Color Border) GetSecondaryColors()
        {
            if (!Enabled)
                return (Color.FromArgb(250, 250, 250), Color.FromArgb(166, 166, 166), Color.FromArgb(229, 229, 229));

            if (_isPressed)
                return (Color.FromArgb(243, 244, 246), Color.FromArgb(55, 65, 81), Color.FromArgb(209, 213, 219));
            
            if (_isHovered)
                return (Color.FromArgb(249, 250, 251), Color.FromArgb(55, 65, 81), Color.FromArgb(209, 213, 219));
            
            return (Color.White, Color.FromArgb(55, 65, 81), Color.FromArgb(209, 213, 219));
        }

        private (Color Background, Color Text, Color Border) GetSuccessColors()
        {
            if (!Enabled)
                return (Color.FromArgb(229, 229, 229), Color.FromArgb(166, 166, 166), Color.FromArgb(229, 229, 229));

            if (_isPressed)
                return (Color.FromArgb(21, 128, 61), Color.White, Color.FromArgb(21, 128, 61));
            
            if (_isHovered)
                return (Color.FromArgb(34, 197, 94), Color.White, Color.FromArgb(34, 197, 94));
            
            return (Color.FromArgb(34, 197, 94), Color.White, Color.FromArgb(34, 197, 94));
        }

        private (Color Background, Color Text, Color Border) GetDangerColors()
        {
            if (!Enabled)
                return (Color.FromArgb(229, 229, 229), Color.FromArgb(166, 166, 166), Color.FromArgb(229, 229, 229));

            if (_isPressed)
                return (Color.FromArgb(185, 28, 28), Color.White, Color.FromArgb(185, 28, 28));
            
            if (_isHovered)
                return (Color.FromArgb(220, 38, 38), Color.White, Color.FromArgb(220, 38, 38));
            
            return (Color.FromArgb(239, 68, 68), Color.White, Color.FromArgb(239, 68, 68));
        }

        private (Color Background, Color Text, Color Border) GetGhostColors()
        {
            if (!Enabled)
                return (Color.Transparent, Color.FromArgb(166, 166, 166), Color.FromArgb(229, 229, 229));

            if (_isPressed)
                return (Color.FromArgb(243, 244, 246), Color.FromArgb(55, 65, 81), Color.FromArgb(209, 213, 219));
            
            if (_isHovered)
                return (Color.FromArgb(249, 250, 251), Color.FromArgb(55, 65, 81), Color.FromArgb(209, 213, 219));
            
            return (Color.Transparent, Color.FromArgb(55, 65, 81), Color.FromArgb(229, 231, 235));
        }

        private (Color Background, Color Text, Color Border) GetLinkColors()
        {
            if (!Enabled)
                return (Color.Transparent, Color.FromArgb(166, 166, 166), Color.Transparent);

            if (_isPressed)
                return (Color.Transparent, Color.FromArgb(0, 84, 166), Color.Transparent);
            
            if (_isHovered)
                return (Color.Transparent, Color.FromArgb(0, 102, 204), Color.Transparent);
            
            return (Color.Transparent, Color.FromArgb(0, 120, 215), Color.Transparent);
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
    }
}
