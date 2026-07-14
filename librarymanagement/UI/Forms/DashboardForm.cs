using LibraryManagementSystem.Services;
using LibraryManagementSystem.UI.Controls;
using System;
using System.Drawing;
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
        private Panel sidebarPanel = null!;
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

        public DashboardForm(string username, string role)
        {
            this.username = username;
            this.role = role;

            Text = "Library Management System - Dashboard";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(15, 23, 42); // Dark mode background

            // Sidebar Panel
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(0, 20, 0, 0)
            };

            // Sidebar Header
            var sidebarHeader = new Label
            {
                Text = "📚 LibraryMS",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 15),
                Margin = new Padding(0, 0, 0, 30)
            };
            sidebarPanel.Controls.Add(sidebarHeader);

            // Sidebar Buttons
            var btnDashboard = CreateSidebarButton("🏠 Dashboard", Color.FromArgb(59, 130, 246), (_, _) => ShowDashboard());
            btnDashboard.Location = new Point(10, 70);
            var btnBooks = CreateSidebarButton("📖 Books", Color.Transparent, (_, _) => ShowBooks());
            btnBooks.Location = new Point(10, 120);
            var btnBookGallery = CreateSidebarButton("🖼️ Book Gallery", Color.Transparent, (_, _) => ShowBookGallery());
            btnBookGallery.Location = new Point(10, 170);
            var btnMembers = CreateSidebarButton("👥 Members", Color.Transparent, (_, _) => ShowMembers());
            btnMembers.Location = new Point(10, 220);
            var btnTransactions = CreateSidebarButton("🔄 Transactions", Color.Transparent, (_, _) => ShowTransactions());
            btnTransactions.Location = new Point(10, 270);
            var btnActiveLoans = CreateSidebarButton("📋 Active Loans", Color.Transparent, (_, _) => ShowActiveLoans());
            btnActiveLoans.Location = new Point(10, 320);
            var btnSettings = CreateSidebarButton("⚙️ Settings", Color.Transparent, (_, _) => ShowSettings());
            btnSettings.Location = new Point(10, 370);
            var btnLogout = CreateSidebarButton("🚪 Log Out", Color.Transparent, (_, _) => Close());
            btnLogout.Location = new Point(10, sidebarPanel.Height - 60);

            activeSidebarButton = btnDashboard;

            // Keep logout button at bottom on resize
            sidebarPanel.Resize += (_, _) =>
            {
                btnLogout.Location = new Point(10, sidebarPanel.Height - 60);
            };

            sidebarPanel.Controls.AddRange(new Control[] { btnDashboard, btnBooks, btnBookGallery, btnMembers, btnTransactions, btnActiveLoans, btnSettings, btnLogout });

            // Top Bar
            var topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(30, 41, 59)
            };

            var lblUserInfo = new Label
            {
                Text = $"👤 {username} | {role}",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(260, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            var lblDate = new Label
            {
                Text = $"📅 {DateTime.Now:dddd, MMMM dd, yyyy}",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Location = new Point(topBar.Width - 300, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            topBar.Controls.AddRange(new Control[] { lblUserInfo, lblDate });
            topBar.Resize += (_, _) =>
            {
                lblDate.Location = new Point(topBar.Width - 300, 20);
            };

            // Main Content Panel
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(20)
            };

            // Add controls
            Controls.Add(contentPanel);
            Controls.Add(topBar);
            Controls.Add(sidebarPanel);

            // Handle resize for dashboard stats
            Resize += (_, _) => UpdateStatsLayout();

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
                activeSidebarButton.ForeColor = Color.FromArgb(148, 163, 184);
            }
            button.BackColor = Color.FromArgb(59, 130, 246);
            button.ForeColor = Color.White;
            activeSidebarButton = button;
        }

        private void ShowDashboard()
        {
            // Find and set dashboard button as active
            foreach (Control control in sidebarPanel.Controls)
            {
                if (control is Button btn && btn.Text.StartsWith("🏠"))
                {
                    SetActiveButton(btn);
                    break;
                }
            }

            contentPanel.Controls.Clear();
            currentControl = null;

            // Stats Cards
            statsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Margin = new Padding(0),
                AutoScroll = true
            };

            var pnlBooks = CreateStatsCard("Total Books", Color.FromArgb(59, 130, 246), out lblBooksCount);
            var pnlMembers = CreateStatsCard("Total Members", Color.FromArgb(16, 185, 129), out lblMembersCount);
            var pnlLoans = CreateStatsCard("Active Loans", Color.FromArgb(245, 158, 11), out lblLoansCount);

            statsPanel.Controls.Add(pnlBooks);
            statsPanel.Controls.Add(pnlMembers);
            statsPanel.Controls.Add(pnlLoans);

            contentPanel.Controls.Add(statsPanel);
            UpdateStatsLayout();
            LoadSummary();
        }

        private void UpdateStatsLayout()
        {
            if (statsPanel is null) return;

            // Calculate responsive card size based on window width
            int availableWidth = contentPanel.ClientSize.Width - 40; // Account for padding
            int cardWidth = Math.Max(240, availableWidth / 3 - 40); // Min 240, divide by 3 with margin
            int cardHeight = Math.Max(120, cardWidth / 2); // Proportional height

            foreach (Control control in statsPanel.Controls)
            {
                if (control is Panel panel)
                {
                    panel.Size = new Size(cardWidth, cardHeight);
                    // Update fonts based on card size
                    foreach (Control child in panel.Controls)
                    {
                        if (child is Label label)
                        {
                            if (label.Font.Bold && label.Font.Size > 20)
                            {
                                label.Font = new Font("Segoe UI", Math.Max(20, cardWidth / 7), FontStyle.Bold);
                            }
                            else
                            {
                                label.Font = new Font("Segoe UI", Math.Max(10, cardWidth / 22));
                            }
                            // Re-center labels
                            label.Location = new Point(20, 20);
                            if (label.Name == "lblCount") // Find count label (we can add a name in CreateStatsCard)
                            {
                                label.Location = new Point(20, 50);
                            }
                        }
                    }
                }
            }
        }

        private void ShowBooks()
        {
            foreach (Control control in sidebarPanel.Controls)
            {
                if (control is Button btn && btn.Text.StartsWith("📖"))
                {
                    SetActiveButton(btn);
                    break;
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
            foreach (Control control in sidebarPanel.Controls)
            {
                if (control is Button btn && btn.Text.StartsWith("👥"))
                {
                    SetActiveButton(btn);
                    break;
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
            foreach (Control control in sidebarPanel.Controls)
            {
                if (control is Button btn && btn.Text.StartsWith("🔄"))
                {
                    SetActiveButton(btn);
                    break;
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
            foreach (Control control in sidebarPanel.Controls)
            {
                if (control is Button btn && btn.Text.StartsWith("⚙️"))
                {
                    SetActiveButton(btn);
                    break;
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
            foreach (Control control in sidebarPanel.Controls)
            {
                if (control is Button btn && btn.Text.StartsWith("🖼️"))
                {
                    SetActiveButton(btn);
                    break;
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
            foreach (Control control in sidebarPanel.Controls)
            {
                if (control is Button btn && btn.Text.StartsWith("📋"))
                {
                    SetActiveButton(btn);
                    break;
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

        private Button CreateSidebarButton(string text, Color activeColor, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = activeColor == Color.Transparent ? Color.FromArgb(148, 163, 184) : Color.White,
                BackColor = activeColor == Color.Transparent ? Color.Transparent : activeColor,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(220, 40),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                UseVisualStyleBackColor = false
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
            return btn;
        }

        private Panel CreateStatsCard(string title, Color color, out Label lblCount)
        {
            var panel = new Panel
            {
                BackColor = color,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(20),
                Margin = new Padding(10)
            };

            var lblTitle = new Label
            {
                Name = "lblTitle",
                Text = title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            lblCount = new Label
            {
                Name = "lblCount",
                Text = "0",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 50)
            };

            panel.Controls.AddRange(new Control[] { lblTitle, lblCount });
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

                if (lblBooksCount != null) lblBooksCount.Text = bookCount.ToString();
                if (lblMembersCount != null) lblMembersCount.Text = memberCount.ToString();
                if (lblLoansCount != null) lblLoansCount.Text = openTransactions.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load summary: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
