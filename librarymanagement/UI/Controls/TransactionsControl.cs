using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace LibraryManagementSystem.UI.Controls
{
    public class TransactionsControl : UserControl
    {
        private readonly TransactionService transactionService = new();
        private readonly BookService bookService = new();
        private readonly MemberService memberService = new();
        private readonly TextBox txtSearch;
        private readonly DataGridView dgvTransactions;
        private static readonly Color BackgroundColor = Color.FromArgb(245, 242, 235);
        private static readonly Color CardBackgroundColor = Color.White;
        private static readonly Color TextPrimaryColor = Color.FromArgb(26, 32, 44);
        private static readonly Color TextSecondaryColor = Color.FromArgb(100, 116, 139);
        private static readonly Color AccentColor = Color.FromArgb(20, 83, 45);
        private static readonly Color InputBackgroundColor = Color.FromArgb(230, 224, 213);

        public TransactionsControl()
        {
            BackColor = BackgroundColor;
            Dock = DockStyle.Fill;
            Padding = new Padding(32, 24, 32, 24);

            // Title Section
            var lblTitle = new Label
            {
                Text = "Transactions",
                Font = new Font("Georgia", 36, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            // Toolbar Section
            var toolbarPanel = new Panel
            {
                Location = new Point(0, 80),
                Size = new Size(ClientSize.Width - 64, 56),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            txtSearch = CreateStyledTextBox("Search by book, member, or status...", new Point(0, 0), 400);
            txtSearch.TextChanged += (_, _) => LoadTransactions(txtSearch.Text.Trim());

            var btnIssue = CreateStyledButton("➕ Issue Book", AccentColor, Color.White, (_, _) => IssueBook());
            btnIssue.Location = new Point(432, 0);

            var btnReturn = CreateStyledButton("✅ Return Book", Color.FromArgb(25, 118, 210), Color.White, (_, _) => ReturnBook());
            btnReturn.Location = new Point(584, 0);

            toolbarPanel.Controls.AddRange(new Control[] { txtSearch, btnIssue, btnReturn });

            // Data Grid Container
            var gridContainer = new Panel
            {
                Location = new Point(0, 156),
                Size = new Size(ClientSize.Width - 64, ClientSize.Height - 180),
                BackColor = CardBackgroundColor,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            gridContainer.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                int cr = 12;
                path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                path.AddArc(gridContainer.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                path.AddArc(gridContainer.Width - cr * 2, gridContainer.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                path.AddArc(0, gridContainer.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                path.CloseAllFigures();
                gridContainer.Region = new Region(path);
            };

            // Data Grid
            dgvTransactions = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = CardBackgroundColor,
                BorderStyle = BorderStyle.None,
                GridColor = InputBackgroundColor,
                Font = new Font("Segoe UI", 10),
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                AllowUserToResizeColumns = true,
                ColumnHeadersDefaultCellStyle =
                {
                    BackColor = InputBackgroundColor,
                    ForeColor = TextPrimaryColor,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    WrapMode = DataGridViewTriState.True
                },
                DefaultCellStyle =
                {
                    BackColor = CardBackgroundColor,
                    ForeColor = TextPrimaryColor,
                    SelectionBackColor = AccentLightColor,
                    SelectionForeColor = AccentColor,
                    Padding = new Padding(5),
                    WrapMode = DataGridViewTriState.True
                },
                AlternatingRowsDefaultCellStyle =
                {
                    BackColor = Color.FromArgb(249, 247, 243),
                    ForeColor = TextPrimaryColor,
                    WrapMode = DataGridViewTriState.True
                },
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
            };

            gridContainer.Controls.Add(dgvTransactions);

            Controls.AddRange(new Control[] { lblTitle, toolbarPanel, gridContainer });

            Resize += (_, _) =>
            {
                toolbarPanel.Size = new Size(ClientSize.Width - 64, 56);
                gridContainer.Size = new Size(ClientSize.Width - 64, ClientSize.Height - 180);
            };

            Load += (_, _) => LoadTransactions();
        }

        private TextBox CreateStyledTextBox(string placeholder, Point location, int width)
        {
            var txtBox = new TextBox
            {
                PlaceholderText = placeholder,
                Font = new Font("Segoe UI", 14),
                Size = new Size(width, 48),
                Location = location,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = InputBackgroundColor,
                ForeColor = TextPrimaryColor
            };
            Controls.Add(txtBox);
            return txtBox;
        }

        private Button CreateStyledButton(string text, Color backColor, Color foreColor, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(140, 48),
                BackColor = backColor,
                ForeColor = foreColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                int cr = 8;
                path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                path.AddArc(btn.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                path.AddArc(btn.Width - cr * 2, btn.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                path.AddArc(0, btn.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                path.CloseAllFigures();
                btn.Region = new Region(path);
            };
            btn.Click += onClick;
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(backColor, 0.1f);
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;
            return btn;
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
            private static readonly Color BackgroundColor = Color.FromArgb(245, 242, 235);
            private static readonly Color TextPrimaryColor = Color.FromArgb(26, 32, 44);
            private static readonly Color AccentColor = Color.FromArgb(20, 83, 45);
            private static readonly Color InputBackgroundColor = Color.FromArgb(230, 224, 213);

            public int SelectedBookId => (cboBooks.SelectedItem as Book)?.BookID ?? 0;
            public int SelectedMemberId => (cboMembers.SelectedItem as Member)?.MemberID ?? 0;
            public DateTime DueDate => dtDueDate.Value.Date;

            public IssueTransactionForm(System.Collections.Generic.List<Book> books, System.Collections.Generic.List<Member> members)
            {
                Text = "Issue Book";
                ClientSize = new Size(420, 190);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                StartPosition = FormStartPosition.CenterParent;
                BackColor = BackgroundColor;
                ForeColor = TextPrimaryColor;

                cboBooks.DataSource = books;
                cboBooks.DisplayMember = "Title";
                cboBooks.BackColor = InputBackgroundColor;
                cboBooks.ForeColor = TextPrimaryColor;
                cboBooks.Font = new Font("Segoe UI", 10);

                cboMembers.DataSource = members;
                cboMembers.DisplayMember = "FullName";
                cboMembers.BackColor = InputBackgroundColor;
                cboMembers.ForeColor = TextPrimaryColor;
                cboMembers.Font = new Font("Segoe UI", 10);

                dtDueDate.Value = DateTime.Now.AddDays(14);
                dtDueDate.BackColor = InputBackgroundColor;
                dtDueDate.ForeColor = TextPrimaryColor;
                dtDueDate.Font = new Font("Segoe UI", 10);

                Controls.AddRange(new Control[]
                {
                    CreateLabel("Book", 20),
                    cboBooks,
                    CreateLabel("Member", 60),
                    cboMembers,
                    CreateLabel("Due Date", 100),
                    dtDueDate,
                    CreateButton("Issue", 130, 145, (_, _) => DialogResult = DialogResult.OK),
                    CreateButton("Cancel", 235, 145, (_, _) => DialogResult = DialogResult.Cancel)
                });
            }

            private static Label CreateLabel(string text, int top)
            {
                return new Label
                {
                    Text = text,
                    Left = 30,
                    Top = top + 4,
                    Width = 90,
                    ForeColor = TextPrimaryColor,
                    Font = new Font("Segoe UI", 10)
                };
            }

            private static Button CreateButton(string text, int left, int top, EventHandler onClick)
            {
                var button = new Button
                {
                    Text = text,
                    Left = left,
                    Top = top,
                    Width = 90,
                    Height = 35,
                    BackColor = text == "Issue" ? AccentColor : Color.White,
                    ForeColor = text == "Issue" ? Color.White : TextPrimaryColor,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                button.FlatAppearance.BorderSize = 0;
                button.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using var path = new GraphicsPath();
                    int cr = 6;
                    path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                    path.AddArc(button.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                    path.AddArc(button.Width - cr * 2, button.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                    path.AddArc(0, button.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                    path.CloseAllFigures();
                    button.Region = new Region(path);
                };
                button.Click += onClick;
                button.MouseEnter += (s, e) => button.BackColor = text == "Issue" ? ControlPaint.Light(AccentColor, 0.1f) : InputBackgroundColor;
                button.MouseLeave += (s, e) => button.BackColor = text == "Issue" ? AccentColor : Color.White;
                return button;
            }
        }
        private static readonly Color AccentLightColor = Color.FromArgb(209, 240, 215);
    }
}