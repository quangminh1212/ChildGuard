using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ChildGuard.UI.ModernUI
{
    /// <summary>
    /// Modern borderless form with custom title bar - inspired by VS Code, Discord, Spotify
    /// </summary>
    public class ModernForm : Form
    {
        private Panel titleBar = null!;
        private Label titleLabel = null!;
        private Panel buttonPanel = null!;
        private Button minimizeButton = null!, maximizeButton = null!, closeButton = null!;

        // Windows API for window manipulation
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        public ModernForm()
        {
            InitializeModernForm();
        }

        private void InitializeModernForm()
        {
            // Remove default border
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            
            // Modern colors
            BackColor = Color.FromArgb(248, 249, 250); // Light gray background
            Font = new Font("Segoe UI", 9F);
            
            // Enable double buffering for smooth rendering
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.UserPaint | 
                    ControlStyles.DoubleBuffer | 
                    ControlStyles.ResizeRedraw, true);

            CreateTitleBar();
            
            // Add subtle drop shadow
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        private void CreateTitleBar()
        {
            titleBar = new Panel
            {
                Height = 40,
                Dock = DockStyle.Top,
                BackColor = Color.White,
                Padding = new Padding(0)
            };

            // Title
            titleLabel = new Label
            {
                Text = this.Text,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(32, 33, 36),
                AutoSize = false,
                Size = new Size(200, 40),
                Location = new Point(16, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Window control buttons
            buttonPanel = new Panel
            {
                Width = 138,
                Height = 40,
                Dock = DockStyle.Right,
                BackColor = Color.Transparent
            };

            // Minimize button
            minimizeButton = CreateWindowButton("−", Color.FromArgb(99, 102, 106));
            minimizeButton.Location = new Point(0, 0);
            minimizeButton.Click += (s, e) => WindowState = FormWindowState.Minimized;

            // Maximize/Restore button
            maximizeButton = CreateWindowButton("□", Color.FromArgb(99, 102, 106));
            maximizeButton.Location = new Point(46, 0);
            maximizeButton.Click += (s, e) => 
            {
                WindowState = WindowState == FormWindowState.Maximized ? 
                    FormWindowState.Normal : FormWindowState.Maximized;
                maximizeButton.Text = WindowState == FormWindowState.Maximized ? "❐" : "□";
            };

            // Close button
            closeButton = CreateWindowButton("✕", Color.FromArgb(196, 43, 28));
            closeButton.Location = new Point(92, 0);
            closeButton.Click += (s, e) => Close();
            closeButton.MouseEnter += (s, e) => closeButton.BackColor = Color.FromArgb(232, 17, 35);
            closeButton.MouseLeave += (s, e) => closeButton.BackColor = Color.Transparent;

            buttonPanel.Controls.AddRange(new Control[] { minimizeButton, maximizeButton, closeButton });
            titleBar.Controls.AddRange(new Control[] { titleLabel, buttonPanel });

            // Make title bar draggable
            titleBar.MouseDown += TitleBar_MouseDown;
            titleLabel.MouseDown += TitleBar_MouseDown;

            Controls.Add(titleBar);
        }

        private Button CreateWindowButton(string text, Color hoverColor)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(46, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(99, 102, 106),
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand,
                TabStop = false
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(229, 229, 229);

            return button;
        }

        private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (titleLabel != null)
                titleLabel.Text = Text;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Draw subtle border
            using (var pen = new Pen(Color.FromArgb(218, 220, 224), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        // Override WndProc for better window behavior
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HTBOTTOMRIGHT = 17;
            const int HTRIGHT = 11;
            const int HTBOTTOM = 15;

            if (m.Msg == WM_NCHITTEST)
            {
                Point pos = PointToClient(new Point(m.LParam.ToInt32()));
                
                // Allow resizing from edges
                if (pos.X >= Width - 10 && pos.Y >= Height - 10)
                {
                    m.Result = (IntPtr)HTBOTTOMRIGHT;
                    return;
                }
                else if (pos.X >= Width - 10)
                {
                    m.Result = (IntPtr)HTRIGHT;
                    return;
                }
                else if (pos.Y >= Height - 10)
                {
                    m.Result = (IntPtr)HTBOTTOM;
                    return;
                }
            }

            base.WndProc(ref m);
        }
    }
}
