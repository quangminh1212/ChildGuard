using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using ChildGuard.UI.Theming;

namespace ChildGuard.UI.Controls
{
    /// <summary>
    /// Card panel theo phong cách Windows 11 và Facebook
    /// </summary>
    public class ModernCard : Panel
    {
        private int cornerRadius = 12;
        private int shadowSize = 8;
        private bool showShadow = true;
        private Color shadowColor = ColorScheme.Modern.ShadowLight;
        private bool isHovered = false;
        private System.Windows.Forms.Timer animationTimer = default!;
        private int animationStep = 0;

        [Category("Modern Style")]
        [Description("Độ bo góc")]
        public int CornerRadius
        {
            get => cornerRadius;
            set
            {
                cornerRadius = Math.Max(0, value);
                Invalidate();
            }
        }

        [Category("Modern Style")]
        [Description("Kích thước shadow")]
        public int ShadowSize
        {
            get => shadowSize;
            set
            {
                shadowSize = Math.Max(0, value);
                UpdatePadding();
                Invalidate();
            }
        }

        [Category("Modern Style")]
        [Description("Hiển thị shadow")]
        public bool ShowShadow
        {
            get => showShadow;
            set
            {
                showShadow = value;
                UpdatePadding();
                Invalidate();
            }
        }

        [Category("Modern Style")]
        [Description("Màu shadow")]
        public Color ShadowColor
        {
            get => shadowColor;
            set
            {
                shadowColor = value;
                Invalidate();
            }
        }

        [Category("Modern Style")]
        [Description("Hiệu ứng hover")]
        public bool EnableHoverEffect { get; set; } = true;

        public ModernCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.SupportsTransparentBackColor, true);

            BackColor = ColorScheme.Modern.Surface;
            Size = new Size(300, 200);

            UpdatePadding();

            // Animation timer
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 20;
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void UpdatePadding()
        {
            if (showShadow)
            {
                Padding = new Padding(
                    shadowSize + 12,
                    shadowSize + 12,
                    shadowSize + 12,
                    shadowSize + 12
                );
            }
            else
            {
                Padding = new Padding(12);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (IsDisposed || Disposing) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;

            Rectangle cardRect = GetCardRectangle();

            // Draw shadow
            if (showShadow)
            {
                DrawCardShadow(g, cardRect);
            }

            // Draw card background
            using (GraphicsPath cardPath = CreateRoundedRectangle(cardRect, cornerRadius))
            {
                // Fill background
                using (SolidBrush bgBrush = new SolidBrush(BackColor))
                {
                    g.FillPath(bgBrush, cardPath);
                }

                // Draw hover effect
                if (isHovered && EnableHoverEffect)
                {
                    DrawHoverEffect(g, cardPath);
                }

                // Draw border
                using (Pen borderPen = new Pen(ColorScheme.Modern.Border, 1))
                {
                    g.DrawPath(borderPen, cardPath);
                }
            }

            // Let the base class render children
            base.OnPaint(e);
        }

        private Rectangle GetCardRectangle()
        {
            if (showShadow)
            {
                return new Rectangle(
                    shadowSize / 2,
                    shadowSize / 2,
                    Width - shadowSize,
                    Height - shadowSize
                );
            }
            else
            {
                return new Rectangle(0, 0, Width - 1, Height - 1);
            }
        }

        private void DrawCardShadow(Graphics g, Rectangle cardRect)
        {
            // Multi-layer shadow for depth effect
            for (int i = shadowSize; i > 0; i--)
            {
                Rectangle shadowRect = new Rectangle(
                    cardRect.X - i / 2 + 2,
                    cardRect.Y - i / 2 + 2,
                    cardRect.Width + i,
                    cardRect.Height + i
                );

                int alpha = (int)(40 * (1 - (float)i / shadowSize));
                Color currentShadowColor = Color.FromArgb(alpha, shadowColor);

                using (GraphicsPath shadowPath = CreateRoundedRectangle(shadowRect, cornerRadius + i / 2))
                using (PathGradientBrush shadowBrush = new PathGradientBrush(shadowPath))
                {
                    shadowBrush.CenterColor = currentShadowColor;
                    shadowBrush.SurroundColors = new[] { Color.Transparent };
                    shadowBrush.FocusScales = new PointF(0.85f, 0.85f);

                    g.FillPath(shadowBrush, shadowPath);
                }
            }
        }

        private void DrawHoverEffect(Graphics g, GraphicsPath path)
        {
            // Elevation effect on hover
            int elevationAlpha = (int)(animationStep * 2.5f);

            using (SolidBrush hoverBrush = new SolidBrush(Color.FromArgb(elevationAlpha, ColorScheme.Modern.Primary)))
            {
                g.FillPath(hoverBrush, path);
            }

            // Glow effect
            Rectangle glowRect = GetCardRectangle();
            glowRect.Inflate(2, 2);

            using (GraphicsPath glowPath = CreateRoundedRectangle(glowRect, cornerRadius))
            using (PathGradientBrush glowBrush = new PathGradientBrush(glowPath))
            {
                Color glowColor = Color.FromArgb((int)(animationStep * 5), ColorScheme.Modern.Primary);
                glowBrush.CenterColor = Color.Transparent;
                glowBrush.SurroundColors = new[] { glowColor };
                glowBrush.FocusScales = new PointF(0.9f, 0.9f);

                g.FillPath(glowBrush, glowPath);
            }
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            // Ensure radius doesn't exceed half of the smallest dimension
            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2);

            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddLine(rect.X + radius, rect.Y, rect.Right - radius * 2, rect.Y);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddLine(rect.Right, rect.Y + radius, rect.Right, rect.Bottom - radius * 2);
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddLine(rect.Right - radius * 2, rect.Bottom, rect.X + radius, rect.Bottom);
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.AddLine(rect.X, rect.Bottom - radius * 2, rect.X, rect.Y + radius);
            path.CloseFigure();

            return path;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (IsDisposed || Disposing) return;
            base.OnMouseEnter(e);
            if (EnableHoverEffect)
            {
                isHovered = true;
                animationStep = 0;
                try { animationTimer?.Start(); } catch { }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (IsDisposed || Disposing) return;
            base.OnMouseLeave(e);
            if (EnableHoverEffect)
            {
                isHovered = false;
                try { animationTimer?.Stop(); } catch { }
                animationStep = 0;
                try { Invalidate(); } catch { }
            }
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            // Guard against ticks after disposal or before handle creation
            if (IsDisposed || Disposing || !IsHandleCreated)
            {
                try { animationTimer?.Stop(); } catch { }
                return;
            }

            if (isHovered && animationStep < 10)
            {
                animationStep++;
            }
            else if (!isHovered && animationStep > 0)
            {
                animationStep--;
            }

            if (animationStep == 0 || animationStep == 10)
            {
                try { animationTimer.Stop(); } catch { }
            }
            try { Invalidate(); } catch { }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            try { animationTimer?.Stop(); } catch { }
            base.OnHandleDestroyed(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            try
            {
                if (!Visible) animationTimer?.Stop();
            }
            catch { }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (animationTimer != null)
                {
                    try { animationTimer.Stop(); } catch { }
                    try { animationTimer.Tick -= AnimationTimer_Tick; } catch { }
                    try { animationTimer.Dispose(); } catch { }
                }
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Card với header theo phong cách Facebook
    /// </summary>
    public class ModernHeaderCard : ModernCard
    {
        private Panel headerPanel = default!;
        private Label titleLabel = default!;
        private Label subtitleLabel = default!;
        private PictureBox iconPicture = default!;

        [Category("Header")]
        [Description("Tiêu đề card")]
        public string Title
        {
            get => titleLabel.Text;
            set => titleLabel.Text = value;
        }

        [Category("Header")]
        [Description("Phụ đề card")]
        public string Subtitle
        {
            get => subtitleLabel.Text;
            set
            {
                subtitleLabel.Text = value;
                subtitleLabel.Visible = !string.IsNullOrEmpty(value);
            }
        }

        [Category("Header")]
        [Description("Icon của card")]
        public Image Icon
        {
            get => iconPicture.Image;
            set
            {
                iconPicture.Image = value;
                iconPicture.Visible = value != null;
                UpdateHeaderLayout();
            }
        }

        [Category("Header")]
        [Description("Màu nền header")]
        public Color HeaderBackColor
        {
            get => headerPanel.BackColor;
            set => headerPanel.BackColor = value;
        }

        public ModernHeaderCard()
        {
            InitializeHeader();
        }

        private void InitializeHeader()
        {
            // Header panel auto-size to content to avoid overlaps
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent
            };
            Controls.Add(headerPanel);

            // Layout inside header: icon + text (title/subtitle) using TableLayoutPanel
            var headerLayout = new TableLayoutPanel
            {
                Name = "headerLayout",
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(12, 8, 12, 8)
            };
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            headerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            headerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            headerPanel.Controls.Add(headerLayout);

            // Icon
            iconPicture = new PictureBox
            {
                Size = new Size(40, 40),
                Margin = new Padding(4, 4, 8, 4),
                SizeMode = PictureBoxSizeMode.Zoom,
                Visible = false
            };
            headerLayout.Controls.Add(iconPicture, 0, 0);
            headerLayout.SetRowSpan(iconPicture, 2);

            // Title
            titleLabel = new Label
            {
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = ColorScheme.Modern.TextPrimary,
                AutoSize = true,
                Margin = new Padding(0, 2, 0, 0)
            };
            headerLayout.Controls.Add(titleLabel, 1, 0);

            // Subtitle
            subtitleLabel = new Label
            {
                Font = new Font("Segoe UI", 9f),
                ForeColor = ColorScheme.Modern.TextSecondary,
                AutoSize = true,
                Visible = false,
                Margin = new Padding(0, 2, 0, 4)
            };
            headerLayout.Controls.Add(subtitleLabel, 1, 1);

            UpdateHeaderLayout();
        }

        private void UpdateHeaderLayout()
        {
            // No absolute positioning: just toggle visibility, TableLayoutPanel handles layout
            var layout = headerPanel.Controls.OfType<TableLayoutPanel>().FirstOrDefault(l => l.Name == "headerLayout");
            if (layout == null) return;
            // Ensure first column (icon) collapses when icon hidden
            layout.ColumnStyles[0].SizeType = iconPicture.Visible ? SizeType.AutoSize : SizeType.Absolute;
            layout.ColumnStyles[0].Width = iconPicture.Visible ? 1f : 0f;
            layout.PerformLayout();
        }
    }
}
