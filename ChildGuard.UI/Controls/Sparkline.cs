using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace ChildGuard.UI.Controls
{
    public class Sparkline : Control
    {
        private readonly List<float> _data = new();
        public IReadOnlyList<float> Data => _data;
        public int MaxPoints { get; set; } = 60;
        public Color LineColor { get; set; } = Color.FromArgb(26, 115, 232); // Material blue
        public int LineWidth { get; set; } = 2;
        public bool Fill { get; set; } = true;
        public Color FillColor { get; set; } = Color.FromArgb(40, 26, 115, 232);

        public Sparkline()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            MinimumSize = new Size(60, 24);
        }

        public void Push(float value)
        {
            _data.Add(Math.Max(0, value));
            while (_data.Count > MaxPoints) _data.RemoveAt(0);
            Invalidate();
        }

        public void Clear()
        {
            _data.Clear();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var bg = new SolidBrush(Parent?.BackColor ?? SystemColors.Window);
            g.FillRectangle(bg, ClientRectangle);

            if (_data.Count < 2) return;

            float max = Math.Max(1f, _data.Max());
            float min = 0f; // clamp at zero for activity metrics
            int w = Width - 1;
            int h = Height - 1;
            float dx = w / Math.Max(1f, (_data.Count - 1));

            var points = new PointF[_data.Count];
            for (int i = 0; i < _data.Count; i++)
            {
                float x = i * dx;
                float y = h - ((Math.Clamp(_data[i], min, max) - min) / (max - min)) * h;
                points[i] = new PointF(x, y);
            }

            if (Fill)
            {
                var fillPts = points.Concat(new[] { new PointF(w, h), new PointF(0, h) }).ToArray();
                using var p = new GraphicsPath();
                p.AddLines(fillPts);
                using var fill = new SolidBrush(FillColor);
                g.FillPath(fill, p);
            }

            using var pen = new Pen(LineColor, LineWidth);
            g.DrawLines(pen, points);
        }
    }
}

