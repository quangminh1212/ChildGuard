using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using ChildGuard.UI.Theming;

namespace ChildGuard.UI.Controls
{
    /// <summary>
    /// Modern navigation bar inspired by Google and Windows design
    /// </summary>
    public class MaterialNavigationBar : Panel
    {
        public class NavigationItem
        {
            public string Text { get; set; } = "";
            public string Icon { get; set; } = "";
            public bool IsSelected { get; set; } = false;
            public object? Tag { get; set; }
            public Rectangle Bounds { get; set; }
        }

        private List<NavigationItem> _items = new List<NavigationItem>();
        private int _selectedIndex = -1;
        private int _hoveredIndex = -1;
        private int _itemHeight = 48;
        private int _iconSize = 20;

        public event EventHandler<int>? ItemSelected;

        public List<NavigationItem> Items => _items;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value && value >= -1 && value < _items.Count)
                {
                    // Update selection state
                    if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
                        _items[_selectedIndex].IsSelected = false;
                    
                    _selectedIndex = value;
                    
                    if (_selectedIndex >= 0)
                        _items[_selectedIndex].IsSelected = true;
                    
                    Invalidate();
                    ItemSelected?.Invoke(this, _selectedIndex);
                }
            }
        }

        public int ItemHeight
        {
            get => _itemHeight;
            set { _itemHeight = value; Invalidate(); }
        }

        public MaterialNavigationBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.UserPaint | 
                    ControlStyles.DoubleBuffer | 
                    ControlStyles.ResizeRedraw, true);
            
            BackColor = ColorScheme.MaterialFluent.Surface;
            Width = 240;
            Padding = new Padding(8);
        }

        public void AddItem(string text, string icon = "", object? tag = null)
        {
            _items.Add(new NavigationItem 
            { 
                Text = text, 
                Icon = icon, 
                Tag = tag 
            });
            
            UpdateLayout();
            Invalidate();
        }

        public void ClearItems()
        {
            _items.Clear();
            _selectedIndex = -1;
            _hoveredIndex = -1;
            Invalidate();
        }

        private void UpdateLayout()
        {
            var y = Padding.Top;
            var itemWidth = Width - Padding.Horizontal;
            
            for (int i = 0; i < _items.Count; i++)
            {
                _items[i].Bounds = new Rectangle(Padding.Left, y, itemWidth, _itemHeight);
                y += _itemHeight + 4; // 4px spacing between items
            }
            
            Height = y + Padding.Bottom;
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            UpdateLayout();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            
            var newHoveredIndex = -1;
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Bounds.Contains(e.Location))
                {
                    newHoveredIndex = i;
                    break;
                }
            }
            
            if (_hoveredIndex != newHoveredIndex)
            {
                _hoveredIndex = newHoveredIndex;
                Cursor = _hoveredIndex >= 0 ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoveredIndex >= 0)
            {
                _hoveredIndex = -1;
                Cursor = Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Bounds.Contains(e.Location))
                {
                    SelectedIndex = i;
                    break;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Draw background
            using (var brush = new SolidBrush(BackColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            // Draw items
            for (int i = 0; i < _items.Count; i++)
            {
                DrawItem(g, _items[i], i);
            }
        }

        private void DrawItem(Graphics g, NavigationItem item, int index)
        {
            var rect = item.Bounds;
            var isSelected = item.IsSelected;
            var isHovered = index == _hoveredIndex;

            // Draw background
            if (isSelected || isHovered)
            {
                var bgColor = isSelected ? 
                    ColorScheme.MaterialFluent.PrimaryLight : 
                    ColorScheme.MaterialFluent.Hover;
                
                using (var brush = new SolidBrush(bgColor))
                using (var path = CreateRoundedPath(rect, 8))
                {
                    g.FillPath(brush, path);
                }
            }

            // Draw selection indicator (left border)
            if (isSelected)
            {
                var indicatorRect = new Rectangle(rect.X, rect.Y + 8, 3, rect.Height - 16);
                using (var brush = new SolidBrush(ColorScheme.MaterialFluent.Primary))
                using (var path = CreateRoundedPath(indicatorRect, 2))
                {
                    g.FillPath(brush, path);
                }
            }

            // Draw icon (if available)
            var textRect = rect;
            if (!string.IsNullOrEmpty(item.Icon))
            {
                var iconRect = new Rectangle(
                    rect.X + 16, 
                    rect.Y + (rect.Height - _iconSize) / 2, 
                    _iconSize, 
                    _iconSize
                );
                
                // For now, draw a simple circle as icon placeholder
                var iconColor = isSelected ? 
                    ColorScheme.MaterialFluent.Primary : 
                    ColorScheme.MaterialFluent.TextSecondary;
                
                using (var brush = new SolidBrush(iconColor))
                {
                    g.FillEllipse(brush, iconRect);
                }
                
                textRect.X += _iconSize + 12;
                textRect.Width -= _iconSize + 12;
            }
            else
            {
                textRect.X += 16;
                textRect.Width -= 16;
            }

            // Draw text
            var textColor = isSelected ? 
                ColorScheme.MaterialFluent.Primary : 
                ColorScheme.MaterialFluent.TextPrimary;
            
            using (var brush = new SolidBrush(textColor))
            {
                var font = isSelected ?
                    new Font("Segoe UI", 9F, FontStyle.Bold) :
                    new Font("Segoe UI", 9F, FontStyle.Regular);
                
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center
                };
                
                g.DrawString(item.Text, font, brush, textRect, sf);
                font.Dispose();
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
    }
}
