using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ChildGuard.UI.Theming;

namespace ChildGuard.UI.Controls
{
    /// <summary>
    /// Modern TextBox with Material Design styling
    /// </summary>
    public class MaterialTextBox : UserControl
    {
        private TextBox _textBox = null!;
        private Label _labelText = null!;
        private string _labelString = "";
        private bool _isFocused = false;
        private bool _isHovered = false;
        private int _cornerRadius = 8;

        public string LabelText
        {
            get => _labelString;
            set { _labelString = value; _labelText.Text = value; }
        }

        public override string Text
        {
            get => _textBox.Text;
            set => _textBox.Text = value ?? "";
        }

        public bool Multiline
        {
            get => _textBox.Multiline;
            set { _textBox.Multiline = value; AdjustHeight(); }
        }

        public bool ReadOnly
        {
            get => _textBox.ReadOnly;
            set => _textBox.ReadOnly = value;
        }

        public char PasswordChar
        {
            get => _textBox.PasswordChar;
            set => _textBox.PasswordChar = value;
        }

        public int CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = value; Invalidate(); }
        }

        public MaterialTextBox()
        {
            InitializeComponents();
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.UserPaint | 
                    ControlStyles.DoubleBuffer | 
                    ControlStyles.ResizeRedraw, true);
        }

        private void InitializeComponents()
        {
            Size = new Size(200, 56);
            BackColor = Color.Transparent;

            // Label
            _labelText = new Label
            {
                Text = _labelString,
                Font = new Font("Segoe UI", 9F),
                ForeColor = ColorScheme.MaterialFluent.TextSecondary,
                BackColor = Color.Transparent,
                Location = new Point(12, 8),
                AutoSize = true
            };

            // TextBox
            _textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10F),
                ForeColor = ColorScheme.MaterialFluent.TextPrimary,
                BackColor = ColorScheme.MaterialFluent.SurfaceVariant,
                Location = new Point(12, 28),
                Size = new Size(Width - 24, 20)
            };

            _textBox.GotFocus += (s, e) => { _isFocused = true; Invalidate(); };
            _textBox.LostFocus += (s, e) => { _isFocused = false; Invalidate(); };
            _textBox.TextChanged += (s, e) => OnTextChanged(e);
            _textBox.KeyDown += (s, e) => OnKeyDown(e);
            _textBox.KeyPress += (s, e) => OnKeyPress(e);

            Controls.Add(_labelText);
            Controls.Add(_textBox);

            MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
            MouseLeave += (s, e) => { _isHovered = false; Invalidate(); };
        }

        private void AdjustHeight()
        {
            if (_textBox.Multiline)
            {
                Height = Math.Max(80, _textBox.PreferredHeight + 36);
                _textBox.Size = new Size(Width - 24, Height - 36);
            }
            else
            {
                Height = 56;
                _textBox.Size = new Size(Width - 24, 20);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_textBox != null)
            {
                _textBox.Size = new Size(Width - 24, _textBox.Multiline ? Height - 36 : 20);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width, Height);
            
            // Draw background
            DrawBackground(g, rect);
            
            // Draw border
            DrawBorder(g, rect);
        }

        private void DrawBackground(Graphics g, Rectangle rect)
        {
            var backgroundColor = _textBox.ReadOnly ? 
                ColorScheme.MaterialFluent.SurfaceContainer : 
                ColorScheme.MaterialFluent.SurfaceVariant;

            using (var brush = new SolidBrush(backgroundColor))
            using (var path = CreateRoundedPath(rect, _cornerRadius))
            {
                g.FillPath(brush, path);
            }
        }

        private void DrawBorder(Graphics g, Rectangle rect)
        {
            Color borderColor;
            int borderWidth = 1;

            if (_isFocused)
            {
                borderColor = ColorScheme.MaterialFluent.Primary;
                borderWidth = 2;
            }
            else if (_isHovered)
            {
                borderColor = ColorScheme.MaterialFluent.Border;
            }
            else
            {
                borderColor = ColorScheme.MaterialFluent.BorderLight;
            }

            using (var pen = new Pen(borderColor, borderWidth))
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

        public new void Focus()
        {
            _textBox.Focus();
        }

        public void SelectAll()
        {
            _textBox.SelectAll();
        }

        public void Clear()
        {
            _textBox.Clear();
        }

        // Event forwarding
        public new event EventHandler? TextChanged;
        public new event KeyEventHandler? KeyDown;
        public new event KeyPressEventHandler? KeyPress;

        protected override void OnTextChanged(EventArgs e)
        {
            TextChanged?.Invoke(this, e);
            base.OnTextChanged(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            KeyDown?.Invoke(this, e);
            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            KeyPress?.Invoke(this, e);
            base.OnKeyPress(e);
        }
    }
}
