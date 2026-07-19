using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryManagementSystem.UI.Forms
{
    public class BaseForm : Form
    {
        protected Panel SidebarPanel { get; private set; }
        protected Panel TopBar { get; private set; }
        protected Panel ContentPanel { get; private set; }

        public BaseForm(string pageTitle)
        {
            Text = pageTitle;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(15, 23, 42);

            // Sidebar Panel
            SidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(0, 20, 0, 0)
            };

            var sidebarHeader = new Label
            {
                Text = "📚 LibraryM",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 15)
            };
            SidebarPanel.Controls.Add(sidebarHeader);

            // Top Bar
            TopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(30, 41, 59)
            };

            var lblDate = new Label
            {
                Text = $"📅 {DateTime.Now:dddd, MMMM dd, yyyy}",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(TopBar.Width - 300, 20)
            };
            TopBar.Controls.Add(lblDate);
            TopBar.Resize += (_, _) => lblDate.Location = new Point(TopBar.Width - 300, 20);

            // Content Panel
            ContentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(20)
            };

            // Bottom navigation (responsive)
            var bottomNav = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 72,
                BackColor = Color.FromArgb(248, 248, 246)
            };

            var bottomFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(12),
                BackColor = Color.Transparent
            };

            bottomNav.Controls.Add(bottomFlow);

            Controls.Add(ContentPanel);
            Controls.Add(bottomNav);
            Controls.Add(TopBar);
            Controls.Add(SidebarPanel);

            // Resize handler to make bottom nav buttons responsive
            bottomNav.Resize += (_, _) =>
            {
                var buttons = new System.Collections.Generic.List<Button>();
                foreach (Control c in bottomFlow.Controls)
                {
                    if (c is Button b) buttons.Add(b);
                }
                if (buttons.Count == 0) return;
                int available = Math.Max(80, bottomFlow.ClientSize.Width - 24);
                int btnWidth = Math.Max(64, available / buttons.Count - 8);
                foreach (var b in buttons)
                {
                    b.Width = btnWidth;
                    b.Height = 48;
                    // adjust text and image relation
                    b.TextAlign = ContentAlignment.MiddleCenter;
                    b.ImageAlign = ContentAlignment.TopCenter;
                    b.Padding = new Padding(0, 6, 0, 0);
                }
            }; // end constructor
        }
        protected Button AddSidebarButton(string text, int top, EventHandler onClick, bool active = false)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = active ? Color.White : Color.FromArgb(148, 163, 184),
                BackColor = active ? Color.FromArgb(59, 130, 246) : Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(220, 40),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                UseVisualStyleBackColor = false,
                Location = new Point(10, top)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            btn.MouseEnter += (s, e) =>
            {
                if (btn.BackColor != Color.FromArgb(59, 130, 246))
                {
                    btn.BackColor = Color.FromArgb(51, 65, 85);
                    btn.ForeColor = Color.White;
                }
            };
            btn.MouseLeave += (s, e) =>
            {
                if (btn.BackColor != Color.FromArgb(59, 130, 246))
                {
                    btn.BackColor = Color.Transparent;
                    btn.ForeColor = Color.FromArgb(148, 163, 184);
                }
            };

            SidebarPanel.Controls.Add(btn);
            return btn;
        }

        protected Button CreateStyledButton(string text, Color color, EventHandler onClick)
        {
            var button = new Button
            {
                Text = text,
                Width = 110,
                Height = 36,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += onClick;
            button.MouseEnter += (s, e) => button.BackColor = ControlPaint.Light(color, 0.1f);
            button.MouseLeave += (s, e) => button.BackColor = color;
            return button;
        }
    }
}
