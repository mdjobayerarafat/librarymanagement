using LibraryManagementSystem.Services;
using LibraryManagementSystem.UI.Controls;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LibraryManagementSystem.UI.Forms
{
    public class DashboardForm : Form
    {
        private readonly string username;
        private readonly string role;

        private Label lblBooksCount = null!;
        private Label lblMembersCount = null!;
        private Label lblLoansCount = null!;
        private Label lblOverdueCount = null!;

        private Panel sidebarPanel = null!;
        private Panel bodyContainer = null!;
        private Panel topBar = null!;
        private Panel contentPanel = null!;
        private Label lblPageTitle = null!;
        private Label lblPageSubtitle = null!;

        private Button? activeSidebarButton;
        private Button btnNavDashboard = null!;
        private Button btnNavBooks = null!;
        private Button btnNavGallery = null!;
        private Button btnNavMembers = null!;
        private Button btnNavTransactions = null!;
        private Button btnNavActiveLoans = null!;
        private Button btnNavSettings = null!;
        private Button btnNavLogout = null!;

        private UserControl? currentControl;
        private BooksControl? booksControl;
        private MembersControl? membersControl;
        private TransactionsControl? transactionsControl;
        private SettingsControl? settingsControl;
        private BookGalleryControl? bookGalleryControl;
        private ActiveLoansControl? activeLoansControl;
        private FlowLayoutPanel? statsPanel;

        private const int SidebarWidth = 252;

        // Theme colors — warm library palette
        private static readonly Color BackgroundColor = Color.FromArgb(247, 247, 255);
        private static readonly Color CardBackgroundColor = Color.White;
        private static readonly Color SidebarBackgroundColor = Color.FromArgb(247, 247, 255);
        private static readonly Color TextPrimaryColor = Color.FromArgb(41, 47, 54);
        private static readonly Color TextSecondaryColor = Color.FromArgb(73, 88, 103);
        private static readonly Color AccentColor = Color.FromArgb(87, 115, 153);
        private static readonly Color AccentLightColor = Color.FromArgb(189, 213, 234);
        private static readonly Color WarningColor = Color.FromArgb(255, 102, 94);
        private static readonly Color WarningLightColor = Color.FromArgb(255, 230, 230);
        private static readonly Color InputBackgroundColor = Color.FromArgb(247, 247, 255);
        private static readonly Color BorderColor = Color.FromArgb(189, 213, 234);
        private static readonly Color ShadowColor = Color.FromArgb(14, 0, 0, 0);

        public DashboardForm(string username, string role)
        {
            this.username = username;
            this.role = role;

            Text = "Library Management System - Dashboard";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BackColor = BackgroundColor;
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(1200, 800);
            AutoScaleMode = AutoScaleMode.Dpi;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);

            // ---- Content area (fills remaining space after sidebar) ----
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BackgroundColor,
                Padding = new Padding(32, 24, 32, 24),
                AutoScroll = true
            };

            CreateTopBar();

            bodyContainer = new Panel { Dock = DockStyle.Fill, BackColor = BackgroundColor };
            bodyContainer.Controls.Add(contentPanel);
            bodyContainer.Controls.Add(topBar);

            CreateSidebar();

            Controls.Add(bodyContainer);
            Controls.Add(sidebarPanel);

            Shown += (_, _) =>
            {
                ShowDashboard();
            };

            Resize += (_, _) =>
            {
                if (statsPanel != null) UpdateStatsLayout();
            };
        }

        // =========================================================
        //  SIDEBAR
        // =========================================================

        private void CreateSidebar()
        {
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = SidebarWidth,
                BackColor = SidebarBackgroundColor
            };
            sidebarPanel.Paint += (s, e) =>
            {
                using var pen = new Pen(BorderColor, 1);
                e.Graphics.DrawLine(pen, sidebarPanel.Width - 1, 0, sidebarPanel.Width - 1, sidebarPanel.Height);
            };

            // --- Brand header ---
            var brandPanel = new Panel { Dock = DockStyle.Top, Height = 92, BackColor = Color.Transparent };

            // Fixed-size icon box (not AutoSize) so the emoji glyph can never render wider
            // than expected and encroach on the title text next to it.
            var logoBox = new Panel
            {
                Size = new Size(38, 38),
                Location = new Point(20, 20),
                BackColor = Color.Transparent
            };
            logoBox.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                TextRenderer.DrawText(g, "📚", new Font("Segoe UI Emoji", 17), logoBox.ClientRectangle,
                    TextPrimaryColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            int brandTextX = logoBox.Right + 12;
            var lblBrand = new Label
            {
                Text = "LibraryMS",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(brandTextX, 16),
                BackColor = Color.Transparent
            };
            var lblBrandSub = new Label
            {
                Text = "Management System",
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                // Positioned relative to the title's own measured height so it can never
                // overlap regardless of DPI scaling or font-metric differences.
                Location = new Point(brandTextX, lblBrand.Location.Y + lblBrand.PreferredHeight),
                BackColor = Color.Transparent
            };
            brandPanel.Controls.AddRange(new Control[] { logoBox, lblBrand, lblBrandSub });

            var divider1 = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(73, 88, 103) };

            // --- Navigation ---
            var navFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(14, 18, 14, 18)
            };

            var navLabel = new Label
            {
                Text = "MENU",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Margin = new Padding(10, 0, 0, 10)
            };

            btnNavDashboard = CreateSidebarButton("🏠", "Dashboard", (_, _) => ShowDashboard());
            btnNavBooks = CreateSidebarButton("📖", "Books", (_, _) => ShowBooks());
            btnNavMembers = CreateSidebarButton("👥", "Members", (_, _) => ShowMembers());
            btnNavTransactions = CreateSidebarButton("🔄", "Transactions", (_, _) => ShowTransactions());
            btnNavActiveLoans = CreateSidebarButton("📋", "Active Loans", (_, _) => ShowActiveLoans());
            btnNavSettings = CreateSidebarButton("⚙️", "Settings", (_, _) => ShowSettings());

            navFlow.Controls.AddRange(new Control[]
            {
                navLabel, btnNavDashboard, btnNavBooks, btnNavGallery,
                btnNavMembers, btnNavTransactions, btnNavActiveLoans, btnNavSettings
            });

            activeSidebarButton = btnNavDashboard;

            var divider2 = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = BorderColor };

            // --- Footer: user card + logout ---
            var footerPanel = new Panel { Dock = DockStyle.Bottom, Height = 132, BackColor = Color.Transparent };

            var userCard = new Panel
            {
                Location = new Point(14, 14),
                Size = new Size(SidebarWidth - 28, 60),
                BackColor = Color.Transparent
            };
            userCard.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = CreateRoundedRectangle(userCard.ClientRectangle, 12);
                using var brush = new SolidBrush(InputBackgroundColor);
                g.FillPath(brush, path);
            };

            var avatar = new Panel { Size = new Size(36, 36), Location = new Point(12, 12), BackColor = Color.Transparent };
            avatar.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = new SolidBrush(AccentColor);
                g.FillEllipse(brush, 0, 0, 36, 36);
                var initial = string.IsNullOrWhiteSpace(username) ? "?" : username.Substring(0, 1).ToUpper();
                TextRenderer.DrawText(g, initial, new Font("Segoe UI", 12, FontStyle.Bold), avatar.ClientRectangle,
                    Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            var lblUserName = new Label
            {
                Text = username,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(247, 247, 255),
                AutoSize = true,
                Location = new Point(58, 12),
                MaximumSize = new Size(SidebarWidth - 100, 0),
                BackColor = Color.Transparent
            };
            var lblUserRole = new Label
            {
                Text = role,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(189, 213, 234),
                AutoSize = true,
                Location = new Point(58, 32),
                BackColor = Color.Transparent
            };

            userCard.Controls.AddRange(new Control[] { avatar, lblUserName, lblUserRole });

            btnNavLogout = CreateSidebarButton("🚪", "Log Out", (_, _) => ConfirmLogout());
            btnNavLogout.Location = new Point(14, 82);
            btnNavLogout.Margin = new Padding(0);

            footerPanel.Controls.AddRange(new Control[] { userCard, btnNavLogout });

            // Add in an order such that: brandPanel is outermost-top, divider1 sits just under it,
            // navFlow fills the remaining middle, divider2 sits just above the footer,
            // and footerPanel is outermost-bottom.
            sidebarPanel.Controls.Add(navFlow);
            sidebarPanel.Controls.Add(divider1);
            sidebarPanel.Controls.Add(brandPanel);
            sidebarPanel.Controls.Add(divider2);
            sidebarPanel.Controls.Add(footerPanel);
        }

        private Button CreateSidebarButton(string icon, string text, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = string.Empty,
                Tag = $"{icon}    {text}",
                Font = new Font("Segoe UI", 10.5F),
                Size = new Size(SidebarWidth - 28, 44),
                Margin = new Padding(0, 2, 0, 2),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                BackColor = Color.Transparent,
                TabStop = false
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;

            btn.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                bool isActive = btn == activeSidebarButton;
                bool isHover = btn.ClientRectangle.Contains(btn.PointToClient(Cursor.Position));

                if (isActive)
                {
                    using var bgBrush = new SolidBrush(Color.FromArgb(189, 213, 234));
                    using var path = CreateRoundedRectangle(btn.ClientRectangle, 10);
                    g.FillPath(bgBrush, path);

                    using var accentBrush = new SolidBrush(AccentColor);
                    g.FillRectangle(accentBrush, 0, 8, 4, btn.Height - 16);
                }
                else if (isHover)
                {
                    using var bgBrush = new SolidBrush(Color.FromArgb(73, 88, 103));
                    using var path = CreateRoundedRectangle(btn.ClientRectangle, 10);
                    g.FillPath(bgBrush, path);
                }

                var textColor = isActive ? Color.FromArgb(41, 47, 54) : Color.FromArgb(189, 213, 234);
                var display = btn.Tag?.ToString() ?? string.Empty;
                TextRenderer.DrawText(g, display, btn.Font, new Rectangle(16, 0, btn.Width - 16, btn.Height),
                    textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
            };

            btn.Click += onClick;
            btn.MouseEnter += (s, e) => btn.Invalidate();
            btn.MouseLeave += (s, e) => btn.Invalidate();

            return btn;
        }

        private void SetActiveButton(Button button)
        {
            var previous = activeSidebarButton;
            activeSidebarButton = button;
            previous?.Invalidate();
            button.Invalidate();
        }

        private void ConfirmLogout()
        {
            var result = MessageBox.Show("Are you sure you want to log out?", "Log Out",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Close();
            }
        }

        // =========================================================
        //  TOP BAR
        // =========================================================

        private void CreateTopBar()
        {
            topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 76,
                BackColor = CardBackgroundColor,
                Padding = new Padding(32, 0, 32, 0)
            };
            topBar.Paint += (s, e) =>
            {
                using var pen = new Pen(BorderColor, 1);
                e.Graphics.DrawLine(pen, 0, topBar.Height - 1, topBar.Width, topBar.Height - 1);
            };

            lblPageTitle = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(32, 12)
            };
            lblPageSubtitle = new Label
            {
                Text = "Overview of your library at a glance",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                // Stack below the title using its own measured height so it can't overlap.
                Location = new Point(32, lblPageTitle.Location.Y + lblPageTitle.PreferredHeight - 2)
            };

            var lblDate = new Label
            {
                Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy"),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            lblDate.Location = new Point(topBar.Width - lblDate.PreferredWidth - 32, 30);

            topBar.Resize += (_, _) =>
            {
                lblDate.Location = new Point(topBar.Width - lblDate.PreferredWidth - 32, 30);
            };

            topBar.Controls.AddRange(new Control[] { lblPageTitle, lblPageSubtitle, lblDate });
        }

        private void SetPageHeader(string title, string subtitle)
        {
            lblPageTitle.Text = title;
            lblPageSubtitle.Text = subtitle;
        }

        // =========================================================
        //  NAVIGATION TARGETS
        // =========================================================

        private void ShowDashboard()
        {
            SetActiveButton(btnNavDashboard);
            SetPageHeader("Dashboard", "Overview of your library at a glance");

            contentPanel.Controls.Clear();
            currentControl = null;

            statsPanel = new FlowLayoutPanel
            {
                Location = new Point(0, 0),
                Size = new Size(contentPanel.ClientSize.Width - 64, 180),
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var pnlBooks = CreateStatsCard("📖", "Total Books", out lblBooksCount, AccentLightColor, AccentColor, "available");
            var pnlMembers = CreateStatsCard("👥", "Members", out lblMembersCount, Color.FromArgb(255, 237, 213), Color.FromArgb(194, 108, 47), "active");
            var pnlLoans = CreateStatsCard("📋", "Borrowed", out lblLoansCount, Color.FromArgb(230, 241, 255), Color.FromArgb(30, 64, 175), "currently out");
            var pnlOverdue = CreateStatsCard("❗", "Overdue", out lblOverdueCount, WarningLightColor, WarningColor, "need attention");

            statsPanel.Controls.AddRange(new Control[] { pnlBooks, pnlMembers, pnlLoans, pnlOverdue });
            statsPanel.Resize += (_, _) => UpdateStatsLayout();

            int contentWidth = contentPanel.ClientSize.Width - 64;
            int panelGap = 24;
            int halfWidth = (contentWidth - panelGap) / 2;
            int sectionTop = 204;
            int sectionHeight = Math.Max(360, contentPanel.ClientSize.Height - sectionTop - 24);

            var pnlRecentActivity = CreateCardPanel("Recent Activity", new Point(0, sectionTop), new Size(halfWidth, sectionHeight));
            var flpRecentActivity = new FlowLayoutPanel
            {
                Location = new Point(0, 64),
                Size = new Size(pnlRecentActivity.Width, pnlRecentActivity.Height - 64),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Padding = new Padding(24, 0, 24, 24),
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
            pnlRecentActivity.Controls.Add(flpRecentActivity);

            var pnlMostBorrowed = CreateCardPanel("Most Borrowed", new Point(halfWidth + panelGap, sectionTop), new Size(halfWidth, sectionHeight));
            var flpMostBorrowed = new FlowLayoutPanel
            {
                Location = new Point(0, 64),
                Size = new Size(pnlMostBorrowed.Width, pnlMostBorrowed.Height - 64),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Padding = new Padding(24, 0, 24, 24),
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
            pnlMostBorrowed.Controls.Add(flpMostBorrowed);

            contentPanel.Controls.AddRange(new Control[] { statsPanel, pnlRecentActivity, pnlMostBorrowed });

            LoadSummary();
            LoadDemoData(flpRecentActivity, flpMostBorrowed, halfWidth);
            UpdateStatsLayout();
        }

        private void ShowBooks()
        {
            SetActiveButton(btnNavBooks);
            SetPageHeader("Books", "Browse and manage your book catalog");
            booksControl ??= new BooksControl();
            ShowControl(booksControl);
        }

        private void ShowBookGallery()
        {
            SetActiveButton(btnNavGallery);
            SetPageHeader("Book Gallery", "Visual browsing of your collection");
            bookGalleryControl ??= new BookGalleryControl();
            ShowControl(bookGalleryControl);
        }

        private void ShowMembers()
        {
            SetActiveButton(btnNavMembers);
            SetPageHeader("Members", "Manage registered library members");
            membersControl ??= new MembersControl();
            ShowControl(membersControl);
        }

        private void ShowTransactions()
        {
            SetActiveButton(btnNavTransactions);
            SetPageHeader("Transactions", "Track borrowing and returns");
            transactionsControl ??= new TransactionsControl();
            ShowControl(transactionsControl);
        }

        private void ShowActiveLoans()
        {
            SetActiveButton(btnNavActiveLoans);
            SetPageHeader("Active Loans", "Books currently checked out");
            activeLoansControl ??= new ActiveLoansControl();
            ShowControl(activeLoansControl);
        }

        private void ShowSettings()
        {
            SetActiveButton(btnNavSettings);
            SetPageHeader("Settings", "Application and account preferences");
            settingsControl ??= new SettingsControl();
            ShowControl(settingsControl);
        }

        private void ShowControl(UserControl control)
        {
            contentPanel.Controls.Clear();
            statsPanel = null;
            control.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(control);
            currentControl = control;
        }

        // =========================================================
        //  DASHBOARD BUILDING BLOCKS
        // =========================================================

        private Panel CreateCardPanel(string title, Point location, Size size)
        {
            var panel = new Panel
            {
                Location = location,
                Size = size,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using var shadowPath = CreateRoundedRectangle(new Rectangle(2, 3, panel.Width - 4, panel.Height - 4), 14);
                using var shadowBrush = new SolidBrush(ShadowColor);
                g.FillPath(shadowBrush, shadowPath);

                using var cardPath = CreateRoundedRectangle(new Rectangle(0, 0, panel.Width - 4, panel.Height - 4), 14);
                using var cardBrush = new SolidBrush(CardBackgroundColor);
                g.FillPath(cardBrush, cardPath);

                using var borderPen = new Pen(BorderColor, 1);
                g.DrawPath(borderPen, cardPath);
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(24, 22),
                BackColor = Color.Transparent
            };
            panel.Controls.Add(lblTitle);

            return panel;
        }

        private Panel CreateStatsCard(string icon, string title, out Label lblCount, Color bgColor, Color accentColor, string subtitle)
        {
            var panel = new Panel
            {
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 20, 20),
                Size = new Size(260, 160)
            };
            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using var shadowPath = CreateRoundedRectangle(new Rectangle(2, 3, panel.Width - 4, panel.Height - 4), 14);
                using var shadowBrush = new SolidBrush(ShadowColor);
                g.FillPath(shadowBrush, shadowPath);

                using var cardPath = CreateRoundedRectangle(new Rectangle(0, 0, panel.Width - 4, panel.Height - 4), 14);
                using var cardBrush = new SolidBrush(CardBackgroundColor);
                g.FillPath(cardBrush, cardPath);

                using var accentBrush = new SolidBrush(accentColor);
                var topStripPath = CreateRoundedRectangle(new Rectangle(0, 0, panel.Width - 4, 6), 14);
                g.FillPath(accentBrush, topStripPath);
                g.FillRectangle(accentBrush, 0, 3, panel.Width - 4, 3);

                using var borderPen = new Pen(BorderColor, 1);
                g.DrawPath(borderPen, cardPath);
            };

            var iconPanel = new Panel { Size = new Size(52, 52), Location = new Point(20, 26), BackColor = Color.Transparent };
            iconPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = CreateRoundedRectangle(iconPanel.ClientRectangle, 12);
                using var brush = new SolidBrush(bgColor);
                g.FillPath(brush, path);
            };
            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 20),
                AutoSize = false,
                Size = iconPanel.Size,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            iconPanel.Controls.Add(lblIcon);

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(84, 32),
                BackColor = Color.Transparent
            };
            var lblSubtitle = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                // Stack below the title using its own measured height so the two never
                // overlap, regardless of DPI scaling or font-metric differences.
                Location = new Point(84, lblTitle.Location.Y + lblTitle.PreferredHeight - 2),
                BackColor = Color.Transparent
            };

            lblCount = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(20, lblSubtitle.Location.Y + lblSubtitle.PreferredHeight + 10)
            };

            panel.Controls.AddRange(new Control[] { iconPanel, lblCount, lblTitle, lblSubtitle });
            return panel;
        }

        private void UpdateStatsLayout()
        {
            if (statsPanel is null) return;

            int availableWidth = Math.Max(260, statsPanel.ClientSize.Width);
            const int minCardWidth = 240;
            const int gap = 20;

            int columns = Math.Max(1, Math.Min(4, (availableWidth + gap) / (minCardWidth + gap)));
            int cardWidth = (availableWidth - gap * (columns - 1)) / columns;
            cardWidth = Math.Max(minCardWidth, cardWidth);

            foreach (Control control in statsPanel.Controls)
            {
                if (control is Panel panel)
                {
                    panel.Size = new Size(cardWidth, 160);
                }
            }
        }

        private void LoadDemoData(FlowLayoutPanel flpRecentActivity, FlowLayoutPanel flpMostBorrowed, int containerWidth)
        {
            try
            {
                var recentActivityItems = new[]
                {
                    new { Title = "Normal People", Member = "Elena Vasquez", Date = "10 Jun 2025", Overdue = true },
                    new { Title = "Pachinko", Member = "Amara Osei", Date = "05 Jun 2025", Overdue = true },
                    new { Title = "Middlemarch", Member = "Amara Osei", Date = "01 Jun 2025", Overdue = true },
                    new { Title = "The Great Gatsby", Member = "James Whitfield", Date = "28 May 2025", Overdue = false },
                    new { Title = "1984", Member = "Lucia Ferreira", Date = "25 May 2025", Overdue = false }
                };

                foreach (var item in recentActivityItems)
                {
                    flpRecentActivity.Controls.Add(CreateRecentActivityItem(item.Title, item.Member, item.Date, item.Overdue, containerWidth));
                }

                var mostBorrowedItems = new[]
                {
                    new { Title = "The Dispossessed", Author = "Ursula K. Le Guin", Count = 3, Total = 3 },
                    new { Title = "Normal People", Author = "Sally Rooney", Count = 2, Total = 4 },
                    new { Title = "Beloved", Author = "Toni Morrison", Count = 2, Total = 3 },
                    new { Title = "Middlemarch", Author = "George Eliot", Count = 1, Total = 3 }
                };

                int rank = 1;
                foreach (var item in mostBorrowedItems)
                {
                    flpMostBorrowed.Controls.Add(CreateMostBorrowedItem(item.Title, item.Author, item.Count, item.Total, rank, containerWidth));
                    rank++;
                }
            }
            catch
            {
                // Demo data is best-effort; failures here should never block the dashboard.
            }
        }

        private Panel CreateRecentActivityItem(string title, string member, string date, bool overdue, int containerWidth)
        {
            int itemWidth = Math.Max(200, containerWidth - 48);

            var item = new Panel
            {
                Size = new Size(itemWidth, 68),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 10)
            };

            var dot = new Panel { Size = new Size(10, 10), Location = new Point(4, 29), BackColor = Color.Transparent };
            dot.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = new SolidBrush(overdue ? WarningColor : AccentColor);
                g.FillEllipse(brush, 0, 0, 10, 10);
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(26, 12),
                MaximumSize = new Size(itemWidth - 130, 0),
                BackColor = Color.Transparent
            };

            var lblMemberDate = new Label
            {
                Text = $"{member} · {date}",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(26, 36),
                MaximumSize = new Size(itemWidth - 130, 0),
                BackColor = Color.Transparent
            };

            item.Controls.AddRange(new Control[] { dot, lblTitle, lblMemberDate });

            if (overdue)
            {
                var lblOverdue = new Label
                {
                    Text = "Overdue",
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                    ForeColor = WarningColor,
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Padding = new Padding(10, 4, 10, 4),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                lblOverdue.Location = new Point(itemWidth - lblOverdue.PreferredWidth - 10, 24);
                lblOverdue.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using var path = CreateRoundedRectangle(lblOverdue.ClientRectangle, 8);
                    using var brush = new SolidBrush(WarningLightColor);
                    g.FillPath(brush, path);
                };
                item.Controls.Add(lblOverdue);
            }

            return item;
        }

        private Panel CreateMostBorrowedItem(string title, string author, int count, int total, int rank, int containerWidth)
        {
            int itemWidth = Math.Max(200, containerWidth - 48);

            var item = new Panel
            {
                Size = new Size(itemWidth, 68),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 10)
            };

            var lblRank = new Label
            {
                Text = rank.ToString(),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(2, 22),
                BackColor = Color.Transparent
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(30, 12),
                MaximumSize = new Size(itemWidth - 170, 0),
                BackColor = Color.Transparent
            };

            var lblAuthor = new Label
            {
                Text = author,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(30, 36),
                MaximumSize = new Size(itemWidth - 170, 0),
                BackColor = Color.Transparent
            };

            var progressPanel = new Panel
            {
                Size = new Size(90, 8),
                Location = new Point(itemWidth - 132, 30),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            progressPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using var trackPath = CreateRoundedRectangle(progressPanel.ClientRectangle, 4);
                using var trackBrush = new SolidBrush(InputBackgroundColor);
                g.FillPath(trackBrush, trackPath);

                float progress = total > 0 ? Math.Min(1f, (float)count / total) : 0f;
                int filledWidth = (int)(progressPanel.Width * progress);
                if (filledWidth > 0)
                {
                    using var fillBrush = new SolidBrush(AccentColor);
                    using var fillPath = CreateRoundedRectangle(new Rectangle(0, 0, Math.Max(8, filledWidth), progressPanel.Height), 4);
                    g.FillPath(fillBrush, fillPath);
                }
            };

            var lblCount = new Label
            {
                Text = $"{count}/{total}",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            lblCount.Location = new Point(itemWidth - lblCount.PreferredWidth, 26);

            item.Controls.AddRange(new Control[] { lblRank, lblTitle, lblAuthor, progressPanel, lblCount });
            return item;
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            if (rect.Width < diameter) diameter = Math.Max(2, rect.Width);
            if (rect.Height < diameter) diameter = Math.Max(2, rect.Height);

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseAllFigures();

            return path;
        }

        // =========================================================
        //  DATA LOADING
        // =========================================================

        private void LoadSummary()
        {
            try
            {
                var bookService = new BookService();
                var memberService = new MemberService();
                var transactionService = new TransactionService();

                var bookCount = bookService.GetAllBooks().Count;
                var memberCount = memberService.GetAllMembers().Count;
                var openTransactions = transactionService.GetAllTransactions().FindAll(t => t.ReturnDate == null).Count;
                var overdueCount = 4; // TODO: replace with real overdue calculation

                if (lblBooksCount != null) lblBooksCount.Text = bookCount.ToString();
                if (lblMembersCount != null) lblMembersCount.Text = memberCount.ToString();
                if (lblLoansCount != null) lblLoansCount.Text = openTransactions.ToString();
                if (lblOverdueCount != null) lblOverdueCount.Text = overdueCount.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load summary: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}