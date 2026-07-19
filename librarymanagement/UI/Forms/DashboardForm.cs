using LibraryManagementSystem.Services;
using LibraryManagementSystem.UI.Controls;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;

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
        private Panel bottomBarPanel = null!;
        private Panel contentPanel = null!;
        private Button? activeSidebarButton;
        private UserControl? currentControl;
        private BooksControl? booksControl;
        private MembersControl? membersControl;
        private TransactionsControl? transactionsControl;
        private SettingsControl? settingsControl;
        private BookGalleryControl? bookGalleryControl;
        private ActiveLoansControl? activeLoansControl;
        private FlowLayoutPanel? statsPanel;

        // Theme colors
        private static readonly Color BackgroundColor = Color.FromArgb(245, 242, 235);
        private static readonly Color CardBackgroundColor = Color.White;
        private static readonly Color BottomBarBackgroundColor = Color.FromArgb(248, 246, 242);
        private static readonly Color TextPrimaryColor = Color.FromArgb(26, 32, 44);
        private static readonly Color TextSecondaryColor = Color.FromArgb(100, 116, 139);
        private static readonly Color AccentColor = Color.FromArgb(20, 83, 45);
        private static readonly Color AccentLightColor = Color.FromArgb(209, 240, 215);
        private static readonly Color WarningColor = Color.FromArgb(239, 68, 68);
        private static readonly Color WarningLightColor = Color.FromArgb(254, 226, 226);
        private static readonly Color InputBackgroundColor = Color.FromArgb(230, 224, 213);

        public DashboardForm(string username, string role)
        {
            this.username = username;
            this.role = role;

            Text = "Library Management System - Dashboard";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BackColor = BackgroundColor;

            // Bottom Bar Panel
            bottomBarPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 72,
                BackColor = BottomBarBackgroundColor
            };

            // Flow Layout for Bottom Bar Buttons
            var buttonFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(24, 12, 24, 12),
                WrapContents = false
            };

            // Bottom Bar Buttons
            var btnDashboard = CreateBottomBarButton("🏠", "", true, (_, _) => ShowDashboard());
            var btnBooks = CreateBottomBarButton("📖", "", false, (_, _) => ShowBooks());
            var btnBookGallery = CreateBottomBarButton("🖼️", "", false, (_, _) => ShowBookGallery());
            var btnMembers = CreateBottomBarButton("👥", "", false, (_, _) => ShowMembers());
            var btnTransactions = CreateBottomBarButton("🔄", "", false, (_, _) => ShowTransactions());
            var btnActiveLoans = CreateBottomBarButton("📋", "", false, (_, _) => ShowActiveLoans());
            var btnSettings = CreateBottomBarButton("⚙️", "", false, (_, _) => ShowSettings());
            var btnLogout = CreateBottomBarButton("🚪", "Log Out", false, (_, _) => Close());

            activeSidebarButton = btnDashboard;

            buttonFlow.Controls.AddRange(new Control[] { btnDashboard, btnBooks, btnBookGallery, btnMembers, btnTransactions, btnActiveLoans, btnSettings, btnLogout });
            bottomBarPanel.Controls.Add(buttonFlow);

            // Main Content Panel
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BackgroundColor,
                Padding = new Padding(32, 24, 32, 24)
            };

            // Add controls
            Controls.Add(contentPanel);
            Controls.Add(bottomBarPanel);

            // Initialize dashboard content
            Shown += (_, _) =>
            {
                ShowDashboard();
                LoadSummary();
            };
        }

        private void SetActiveButton(Button button)
        {
            if (activeSidebarButton != null)
            {
                activeSidebarButton.BackColor = Color.Transparent;
                activeSidebarButton.ForeColor = TextSecondaryColor;
            }
            button.BackColor = AccentLightColor;
            button.ForeColor = AccentColor;
            activeSidebarButton = button;
        }

        private Button CreateBottomBarButton(string icon, string text, bool isActive, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = $"{icon} {text}",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = isActive ? AccentColor : TextSecondaryColor,
                BackColor = isActive ? AccentLightColor : Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(140, 48),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                UseVisualStyleBackColor = false,
                Margin = new Padding(0, 0, 16, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                int cr = 12;
                path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                path.AddArc(btn.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                path.AddArc(btn.Width - cr * 2, btn.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                path.AddArc(0, btn.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                path.CloseAllFigures();
                btn.Region = new Region(path);
            };
            btn.Click += onClick;
            btn.MouseEnter += (s, e) =>
            {
                if (btn != activeSidebarButton)
                {
                    btn.BackColor = InputBackgroundColor;
                    btn.ForeColor = TextPrimaryColor;
                }
            };
            btn.MouseLeave += (s, e) =>
            {
                if (btn != activeSidebarButton)
                {
                    btn.BackColor = Color.Transparent;
                    btn.ForeColor = TextSecondaryColor;
                }
            };
            return btn;
        }

        private void ShowDashboard()
        {
            // Activate dashboard button
            foreach (Control control in bottomBarPanel.Controls)
            {
                // Find the button in the FlowLayoutPanel
                if (control is FlowLayoutPanel flp)
                {
                    foreach (Control flpControl in flp.Controls)
                    {
                        if (flpControl is Button btn && btn.Text.StartsWith("🏠"))
                        {
                            SetActiveButton(btn);
                            break;
                        }
                    }
                }
            }

            contentPanel.Controls.Clear();
            currentControl = null;

            // Header
            var lblHeader = new Label
            {
                Text = "Dashboard",
                Font = new Font("Georgia", 36, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var lblSubHeader = new Label
            {
                Text = "Overview of your library at a glance",
                Font = new Font("Segoe UI", 14),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(0, 52)
            };

            // Stats panel
            statsPanel = new FlowLayoutPanel
            {
                Location = new Point(0, 100),
                Size = new Size(contentPanel.ClientSize.Width - 64, 180),
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Margin = new Padding(0),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            var pnlBooks = CreateStatsCard("📖", "Total Books", out lblBooksCount, AccentLightColor, AccentColor, "15 available");
            var pnlMembers = CreateStatsCard("👥", "Members", out lblMembersCount, Color.FromArgb(255, 237, 213), Color.FromArgb(194, 108, 47), "5 active");
            var pnlLoans = CreateStatsCard("📋", "Borrowed", out lblLoansCount, Color.FromArgb(230, 241, 255), Color.FromArgb(30, 64, 175), "currently out");
            var pnlOverdue = CreateStatsCard("❗", "Overdue", out lblOverdueCount, WarningLightColor, WarningColor, "need attention");

            statsPanel.Controls.AddRange(new Control[] { pnlBooks, pnlMembers, pnlLoans, pnlOverdue });
            statsPanel.Resize += (_, _) => UpdateStatsLayout();

            // Recent Activity Section
            var pnlRecentActivity = CreateCardPanel("Recent Activity", new Point(0, 300), new Size((contentPanel.ClientSize.Width - 64 - 24) / 2, 400));
            var flpRecentActivity = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 0, 24, 24),
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true
            };
            pnlRecentActivity.Controls.Add(flpRecentActivity);

            // Most Borrowed Section
            var pnlMostBorrowed = CreateCardPanel("Most Borrowed", 
                new Point((contentPanel.ClientSize.Width - 64 - 24) / 2 + 24, 300), 
                new Size((contentPanel.ClientSize.Width - 64 - 24) / 2, 400));
            var flpMostBorrowed = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 0, 24, 24),
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true
            };
            pnlMostBorrowed.Controls.Add(flpMostBorrowed);

            contentPanel.Controls.AddRange(new Control[] { lblHeader, lblSubHeader, statsPanel, pnlRecentActivity, pnlMostBorrowed });

            LoadSummary();

            // Load dummy data for recent activity and most borrowed
            try
            {
                // Recent Activity
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
                    flpRecentActivity.Controls.Add(CreateRecentActivityItem(item.Title, item.Member, item.Date, item.Overdue));
                }

                // Most Borrowed
                var mostBorrowedItems = new[]
                {
                    new { Title = "The Dispossessed", Author = "Ursula K. Le Guin", Count = 3, Total = 3 },
                    new { Title = "Normal People", Author = "Sally Rooney", Count = 2, Total = 4 },
                    new { Title = "Beloved", Author = "Toni Morrison", Count = 2, Total = 3 },
                    new { Title = "Middlemarch", Author = "George Eliot", Count = 1, Total = 3 }
                };

                foreach (var item in mostBorrowedItems)
                {
                    flpMostBorrowed.Controls.Add(CreateMostBorrowedItem(item.Title, item.Author, item.Count, item.Total));
                }
            }
            catch { }
        }

        private Panel CreateCardPanel(string title, Point location, Size size)
        {
            var panel = new Panel
            {
                Location = location,
                Size = size,
                BackColor = CardBackgroundColor,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                int cr = 12;
                path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                path.AddArc(panel.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                path.AddArc(panel.Width - cr * 2, panel.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                path.AddArc(0, panel.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                path.CloseAllFigures();
                panel.Region = new Region(path);
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Georgia", 20, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(24, 24)
            };
            panel.Controls.Add(lblTitle);

            return panel;
        }

        private Panel CreateRecentActivityItem(string title, string member, string date, bool overdue)
        {
            var item = new Panel
            {
                Size = new Size(statsPanel!.ClientSize.Width - 48, 72),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 12)
            };

            var dot = new Panel
            {
                Size = new Size(10, 10),
                Location = new Point(4, 31),
                BackColor = overdue ? WarningColor : AccentColor
            };
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
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(28, 16),
                MaximumSize = new Size(item.ClientSize.Width - 140, 0),
                BackColor = Color.Transparent
            };

            var lblMemberDate = new Label
            {
                Text = $"{member} · {date}",
                Font = new Font("Segoe UI", 12),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(28, 40),
                MaximumSize = new Size(item.ClientSize.Width - 140, 0),
                BackColor = Color.Transparent
            };

            if (overdue)
            {
                var lblOverdue = new Label
                {
                    Text = "Overdue",
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    ForeColor = WarningColor,
                    AutoSize = true,
                    BackColor = WarningLightColor,
                    Padding = new Padding(10, 4, 10, 4),
                    Location = new Point(item.ClientSize.Width - 85, 20),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                lblOverdue.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using var path = new GraphicsPath();
                    int cr = 8;
                    path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                    path.AddArc(lblOverdue.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                    path.AddArc(lblOverdue.Width - cr * 2, lblOverdue.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                    path.AddArc(0, lblOverdue.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                    path.CloseAllFigures();
                    lblOverdue.Region = new Region(path);
                };
                item.Controls.Add(lblOverdue);
            }

            item.Controls.AddRange(new Control[] { dot, lblTitle, lblMemberDate });
            return item;
        }

        private Panel CreateMostBorrowedItem(string title, string author, int count, int total)
        {
            var item = new Panel
            {
                Size = new Size(statsPanel!.ClientSize.Width - 48, 72),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 12)
            };

            var lblRank = new Label
            {
                Text = (statsPanel.Controls.IndexOf(item) + 1).ToString(),
                Font = new Font("Georgia", 14, FontStyle.Bold),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(4, 24)
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(36, 16),
                MaximumSize = new Size(item.ClientSize.Width - 180, 0),
                BackColor = Color.Transparent
            };

            var lblAuthor = new Label
            {
                Text = author,
                Font = new Font("Segoe UI", 12),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(36, 40),
                MaximumSize = new Size(item.ClientSize.Width - 180, 0),
                BackColor = Color.Transparent
            };

            // Progress bar
            var progressPanel = new Panel
            {
                Size = new Size(100, 8),
                Location = new Point(item.ClientSize.Width - 140, 32),
                BackColor = InputBackgroundColor,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            progressPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                int cr = 4;
                path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                path.AddArc(progressPanel.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                path.AddArc(progressPanel.Width - cr * 2, progressPanel.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                path.AddArc(0, progressPanel.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                path.CloseAllFigures();
                progressPanel.Region = new Region(path);

                // Fill progress
                float progress = (float)count / total;
                using var progressBrush = new SolidBrush(AccentColor);
                var progressRect = new Rectangle(0, 0, (int)(progressPanel.Width * progress), progressPanel.Height);
                using var progressPath = new GraphicsPath();
                progressPath.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                if (progress >= 1.0f)
                {
                    progressPath.AddArc(progressPanel.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                    progressPath.AddArc(progressPanel.Width - cr * 2, progressPanel.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                }
                progressPath.AddArc(0, progressPanel.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                progressPath.CloseAllFigures();
                g.FillPath(progressBrush, progressPath);
            };

            var lblCount = new Label
            {
                Text = $"{count}/{total}",
                Font = new Font("Segoe UI", 12),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(item.ClientSize.Width - 36, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            item.Controls.AddRange(new Control[] { lblRank, lblTitle, lblAuthor, progressPanel, lblCount });
            return item;
        }

        private void UpdateStatsLayout()
        {
            if (statsPanel is null) return;

            int availableWidth = statsPanel.ClientSize.Width;
            int cardWidth = Math.Max(260, (availableWidth - 72) / 4);
            int cardHeight = 160;

            foreach (Control control in statsPanel.Controls)
            {
                if (control is Panel panel)
                {
                    panel.Size = new Size(cardWidth, cardHeight);
                    panel.Margin = new Padding(0, 0, 24, 0);
                }
            }
        }

        private void ShowBooks()
        {
            foreach (Control control in bottomBarPanel.Controls)
            {
                if (control is FlowLayoutPanel flp)
                {
                    foreach (Control flpControl in flp.Controls)
                    {
                        if (flpControl is Button btn && btn.Text.StartsWith("📖"))
                        {
                            SetActiveButton(btn);
                            break;
                        }
                    }
                }
            }

            if (booksControl == null)
            {
                booksControl = new BooksControl();
            }

            ShowControl(booksControl);
        }

        private void ShowMembers()
        {
            foreach (Control control in bottomBarPanel.Controls)
            {
                if (control is FlowLayoutPanel flp)
                {
                    foreach (Control flpControl in flp.Controls)
                    {
                        if (flpControl is Button btn && btn.Text.StartsWith("👥"))
                        {
                            SetActiveButton(btn);
                            break;
                        }
                    }
                }
            }

            if (membersControl == null)
            {
                membersControl = new MembersControl();
            }

            ShowControl(membersControl);
        }

        private void ShowTransactions()
        {
            foreach (Control control in bottomBarPanel.Controls)
            {
                if (control is FlowLayoutPanel flp)
                {
                    foreach (Control flpControl in flp.Controls)
                    {
                        if (flpControl is Button btn && btn.Text.StartsWith("🔄"))
                        {
                            SetActiveButton(btn);
                            break;
                        }
                    }
                }
            }

            if (transactionsControl == null)
            {
                transactionsControl = new TransactionsControl();
            }

            ShowControl(transactionsControl);
        }

        private void ShowSettings()
        {
            foreach (Control control in bottomBarPanel.Controls)
            {
                if (control is FlowLayoutPanel flp)
                {
                    foreach (Control flpControl in flp.Controls)
                    {
                        if (flpControl is Button btn && btn.Text.StartsWith("⚙️"))
                        {
                            SetActiveButton(btn);
                            break;
                        }
                    }
                }
            }

            if (settingsControl == null)
            {
                settingsControl = new SettingsControl();
            }

            ShowControl(settingsControl);
        }

        private void ShowBookGallery()
        {
            foreach (Control control in bottomBarPanel.Controls)
            {
                if (control is FlowLayoutPanel flp)
                {
                    foreach (Control flpControl in flp.Controls)
                    {
                        if (flpControl is Button btn && btn.Text.StartsWith("🖼️"))
                        {
                            SetActiveButton(btn);
                            break;
                        }
                    }
                }
            }

            if (bookGalleryControl == null)
            {
                bookGalleryControl = new BookGalleryControl();
            }

            ShowControl(bookGalleryControl);
        }

        private void ShowActiveLoans()
        {
            foreach (Control control in bottomBarPanel.Controls)
            {
                if (control is FlowLayoutPanel flp)
                {
                    foreach (Control flpControl in flp.Controls)
                    {
                        if (flpControl is Button btn && btn.Text.StartsWith("📋"))
                        {
                            SetActiveButton(btn);
                            break;
                        }
                    }
                }
            }

            if (activeLoansControl == null)
            {
                activeLoansControl = new ActiveLoansControl();
            }

            ShowControl(activeLoansControl);
        }

        private void ShowControl(UserControl control)
        {
            contentPanel.Controls.Clear();
            control.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(control);
            currentControl = control;
            LoadSummary();
        }


        private Panel CreateStatsCard(string icon, string title, out Label lblCount, Color bgColor, Color accentColor, string subtitle)
        {
            var panel = new Panel
            {
                BackColor = CardBackgroundColor,
                Margin = new Padding(0, 0, 24, 0),
                Size = new Size(260, 160)
            };
            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                int cr = 12;
                path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                path.AddArc(panel.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                path.AddArc(panel.Width - cr * 2, panel.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                path.AddArc(0, panel.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                path.CloseAllFigures();
                panel.Region = new Region(path);
            };

            var iconPanel = new Panel
            {
                Size = new Size(56, 56),
                Location = new Point(24, 24),
                BackColor = bgColor
            };
            iconPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                int cr = 12;
                path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                path.AddArc(iconPanel.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                path.AddArc(iconPanel.Width - cr * 2, iconPanel.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                path.AddArc(0, iconPanel.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                path.CloseAllFigures();
                iconPanel.Region = new Region(path);
            };

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 24),
                AutoSize = true,
                Location = new Point(16, 12),
                BackColor = Color.Transparent
            };
            iconPanel.Controls.Add(lblIcon);

            lblCount = new Label
            {
                Name = "lblCount",
                Text = "0",
                Font = new Font("Georgia", 36, FontStyle.Bold),
                ForeColor = accentColor,
                AutoSize = true,
                Location = new Point(96, 24)
            };

            var lblTitle = new Label
            {
                Name = "lblTitle",
                Text = title,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(96, 72),
                MaximumSize = new Size(140, 0)
            };

            var lblSubtitle = new Label
            {
                Name = "lblSubtitle",
                Text = subtitle,
                Font = new Font("Segoe UI", 12),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(96, 100),
                MaximumSize = new Size(140, 0)
            };

            panel.Controls.AddRange(new Control[] { iconPanel, lblCount, lblTitle, lblSubtitle });
            return panel;
        }

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
                var overdueCount = 4; // Dummy data

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
