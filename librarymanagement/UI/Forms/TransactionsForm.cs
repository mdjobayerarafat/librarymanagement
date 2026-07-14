using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LibraryManagementSystem.UI.Forms
{
    public class TransactionsForm : Form
    {
        private readonly TransactionService transactionService = new();
        private readonly BookService bookService = new();
        private readonly MemberService memberService = new();
        private readonly TextBox txtSearch;
        private readonly DataGridView dgvTransactions;

        public TransactionsForm()
        {
            Text = "🔄 Manage Transactions";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(240, 242, 245);

            // Header Panel
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(241, 196, 15)
            };

            var lblHeaderTitle = new Label
            {
                Text = "🔄 Transactions Management",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(30, 18)
            };

            headerPanel.Controls.Add(lblHeaderTitle);

            // Toolbar Panel
            var toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            txtSearch = new TextBox
            {
                PlaceholderText = "🔍 Search by book, member, or status...",
                Width = 400,
                Height = 36,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (_, _) => LoadTransactions(txtSearch.Text.Trim());

            var btnIssue = CreateStyledButton("📖 Issue Book", Color.FromArgb(46, 204, 113), (_, _) => IssueBook());
            var btnReturn = CreateStyledButton("✅ Return Book", Color.FromArgb(52, 152, 219), (_, _) => ReturnBook());
            var btnRefresh = CreateStyledButton("🔄 Refresh", Color.FromArgb(52, 73, 94), (_, _) => LoadTransactions(txtSearch.Text.Trim()));

            txtSearch.Location = new Point(15, 17);
            btnIssue.Location = new Point(430, 17);
            btnReturn.Location = new Point(560, 17);
            btnRefresh.Location = new Point(690, 17);

            toolbarPanel.Controls.AddRange(new Control[] { txtSearch, btnIssue, btnReturn, btnRefresh });

            // Data Grid
            dgvTransactions = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(220, 220, 220),
                Font = new Font("Segoe UI", 9),
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false
            };
            dgvTransactions.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(243, 156, 18);
            dgvTransactions.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvTransactions.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvTransactions.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 249, 249);
            dgvTransactions.DefaultCellStyle.Padding = new Padding(5);

            Controls.Add(dgvTransactions);
            Controls.Add(toolbarPanel);
            Controls.Add(headerPanel);

            Shown += (_, _) => LoadTransactions();
        }

        private Button CreateStyledButton(string text, Color color, EventHandler onClick)
        {
            var button = new Button
            {
                Text = text,
                Width = 120,
                Height = 36,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            button.Click += onClick;
            button.MouseEnter += (s, e) => button.BackColor = ControlPaint.Light(color, 0.1f);
            button.MouseLeave += (s, e) => button.BackColor = color;
            return button;
        }

        private void LoadTransactions(string keyword = "")
        {
            try
            {
                dgvTransactions.DataSource = string.IsNullOrWhiteSpace(keyword)
                    ? transactionService.GetAllTransactions()
                    : transactionService.SearchTransactions(keyword);

                var idColumn = dgvTransactions.Columns["TransactionID"];
                if (idColumn != null)
                {
                    idColumn.HeaderText = "ID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load transactions.\n\n{ex.Message}", "Transactions", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Transaction? GetSelectedTransaction()
        {
            return dgvTransactions.CurrentRow?.DataBoundItem as Transaction;
        }

        private void IssueBook()
        {
            var books = bookService.GetAvailableBooks();
            var members = memberService.GetActiveMembers();

            if (!books.Any())
            {
                MessageBox.Show("There are no available books to issue.", "Transactions", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!members.Any())
            {
                MessageBox.Show("There are no active members available.", "Transactions", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new IssueTransactionForm(books, members);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var success = transactionService.IssueBook(dialog.SelectedBookId, dialog.SelectedMemberId, dialog.DueDate);
            if (!success)
            {
                MessageBox.Show("The selected book is no longer available.", "Transactions", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            LoadTransactions(txtSearch.Text.Trim());
        }

        private void ReturnBook()
        {
            var selected = GetSelectedTransaction();
            if (selected == null)
            {
                MessageBox.Show("Please select a transaction first.", "Transactions", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (selected.ReturnDate != null)
            {
                MessageBox.Show("This transaction is already marked as returned.", "Transactions", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (transactionService.ReturnBook(selected.TransactionID))
            {
                LoadTransactions(txtSearch.Text.Trim());
            }
        }

        private sealed class IssueTransactionForm : Form
        {
            private readonly ComboBox cboBooks = new() { Left = 130, Top = 20, Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            private readonly ComboBox cboMembers = new() { Left = 130, Top = 60, Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            private readonly DateTimePicker dtDueDate = new() { Left = 130, Top = 100, Width = 240, Format = DateTimePickerFormat.Short };

            public int SelectedBookId => (cboBooks.SelectedItem as Book)?.BookID ?? 0;
            public int SelectedMemberId => (cboMembers.SelectedItem as Member)?.MemberID ?? 0;
            public DateTime DueDate => dtDueDate.Value.Date;

            public IssueTransactionForm(System.Collections.Generic.List<Book> books, System.Collections.Generic.List<Member> members)
            {
                Text = "Issue Book";
                ClientSize = new Size(410, 190);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                StartPosition = FormStartPosition.CenterParent;

                cboBooks.DataSource = books;
                cboBooks.DisplayMember = "Title";

                cboMembers.DataSource = members;
                cboMembers.DisplayMember = "FullName";

                dtDueDate.Value = DateTime.Now.AddDays(14);

                Controls.AddRange(new Control[]
                {
                    CreateLabel("Book", 20),
                    cboBooks,
                    CreateLabel("Member", 60),
                    cboMembers,
                    CreateLabel("Due Date", 100),
                    dtDueDate,
                    CreateButton("Issue", 130, 140, (_, _) => DialogResult = DialogResult.OK),
                    CreateButton("Cancel", 225, 140, (_, _) => DialogResult = DialogResult.Cancel)
                });
            }

            private static Label CreateLabel(string text, int top)
            {
                return new Label { Text = text, Left = 30, Top = top + 4, Width = 90 };
            }

            private static Button CreateButton(string text, int left, int top, EventHandler onClick)
            {
                var button = new Button { Text = text, Left = left, Top = top, Width = 80, Height = 30 };
                button.Click += onClick;
                return button;
            }
        }
    }
}
