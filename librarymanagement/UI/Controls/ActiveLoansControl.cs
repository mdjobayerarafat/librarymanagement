using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LibraryManagementSystem.UI.Controls
{
    public class ActiveLoansControl : UserControl
    {
        private readonly TransactionService _transactionService = new();
        private readonly DataGridView _dgvActiveLoans;
        private readonly TextBox _txtSearch;

        public ActiveLoansControl()
        {
            BackColor = Color.FromArgb(15, 23, 42);
            Dock = DockStyle.Fill;
            Padding = new Padding(20);

            // Title Panel
            var titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(30, 41, 59)
            };

            var lblTitle = new Label
            {
                Text = "📖 Active Loans",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 18)
            };

            titlePanel.Controls.Add(lblTitle);

            // Toolbar Panel
            var toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(30, 41, 59)
            };

            _txtSearch = new TextBox
            {
                PlaceholderText = "🔍 Search by book, member...",
                Width = 400,
                Height = 36,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                Location = new Point(20, 17)
            };
            _txtSearch.TextChanged += (_, _) => LoadActiveLoans();

            var btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Width = 120,
                Height = 36,
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(440, 17),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (_, _) => LoadActiveLoans();
            btnRefresh.MouseEnter += (s, e) => btnRefresh.BackColor = ControlPaint.Light(Color.FromArgb(59, 130, 246), 0.1f);
            btnRefresh.MouseLeave += (s, e) => btnRefresh.BackColor = Color.FromArgb(59, 130, 246);

            toolbarPanel.Controls.AddRange(new Control[] { _txtSearch, btnRefresh });

            // Data Grid
            _dgvActiveLoans = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.FromArgb(30, 41, 59),
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(51, 65, 85),
                Font = new Font("Segoe UI", 9),
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                AllowUserToResizeColumns = true,
                ColumnHeadersDefaultCellStyle =
                {
                    BackColor = Color.FromArgb(51, 65, 85),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    WrapMode = DataGridViewTriState.True
                },
                DefaultCellStyle =
                {
                    BackColor = Color.FromArgb(30, 41, 59),
                    ForeColor = Color.White,
                    SelectionBackColor = Color.FromArgb(59, 130, 246),
                    SelectionForeColor = Color.White,
                    Padding = new Padding(5),
                    WrapMode = DataGridViewTriState.True
                },
                AlternatingRowsDefaultCellStyle =
                {
                    BackColor = Color.FromArgb(30, 41, 59),
                    ForeColor = Color.White,
                    WrapMode = DataGridViewTriState.True
                },
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
            };

            Controls.Add(_dgvActiveLoans);
            Controls.Add(toolbarPanel);
            Controls.Add(titlePanel);

            Load += (_, _) => LoadActiveLoans();
        }

        private void LoadActiveLoans()
        {
            var searchText = _txtSearch.Text.Trim().ToLower();
            var allTransactions = _transactionService.GetAllTransactions();
            var activeLoans = allTransactions.Where(t => t.ReturnDate == null).ToList();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                activeLoans = activeLoans.Where(t =>
                    t.BookTitle.ToLower().Contains(searchText) ||
                    t.MemberName.ToLower().Contains(searchText) ||
                    t.Status.ToLower().Contains(searchText)
                ).ToList();
            }

            _dgvActiveLoans.DataSource = activeLoans;
        }
    }
}
