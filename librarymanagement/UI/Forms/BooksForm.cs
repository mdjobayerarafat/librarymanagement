using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryManagementSystem.UI.Forms
{
    public class BooksForm : BaseForm
    {
        private readonly BookService bookService = new();
        private readonly TextBox txtSearch;
        private readonly DataGridView dgvBooks;
        public BooksForm() : base("📚 Manage Books")
        {
            // Add sidebar buttons (reuse base layout)
            AddSidebarButton("🏠 Dashboard", 70, (_, _) => Close());
            AddSidebarButton("📖 Books", 120, (_, _) => { });
            AddSidebarButton("👥 Members", 170, (_, _) => { });
            AddSidebarButton("🔄 Transactions", 220, (_, _) => { });
            AddSidebarButton("⚙️ Settings", 270, (_, _) => MessageBox.Show("Settings coming soon!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information));
            AddSidebarButton("🚪 Log Out", 320, (_, _) => Close());

            // Toolbar Panel (inside content)
            var toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(20, 30, 40)
            };

            txtSearch = new TextBox
            {
                PlaceholderText = "🔍 Search by title, author, ISBN, or category...",
                Width = 400,
                Height = 36,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(20, 17)
            };
            txtSearch.TextChanged += (_, _) => LoadBooks(txtSearch.Text.Trim());

            var btnAdd = CreateStyledButton("➕ Add Book", Color.FromArgb(46, 204, 113), (_, _) => AddBook());
            btnAdd.Location = new Point(440, 17);
            var btnEdit = CreateStyledButton("✏️ Edit Book", Color.FromArgb(241, 196, 15), (_, _) => EditBook());
            btnEdit.Location = new Point(560, 17);
            var btnDelete = CreateStyledButton("🗑️ Delete Book", Color.FromArgb(231, 76, 60), (_, _) => DeleteBook());
            btnDelete.Location = new Point(680, 17);
            var btnRefresh = CreateStyledButton("🔄 Refresh", Color.FromArgb(52, 73, 94), (_, _) => LoadBooks(txtSearch.Text.Trim()));
            btnRefresh.Location = new Point(810, 17);

            toolbarPanel.Controls.AddRange(new Control[] { txtSearch, btnAdd, btnEdit, btnDelete, btnRefresh });

            // Data Grid
            dgvBooks = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.FromArgb(20, 30, 40),
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(60, 70, 80),
                Font = new Font("Segoe UI", 9),
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false
            };
            dgvBooks.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            dgvBooks.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvBooks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvBooks.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(18, 27, 40);
            dgvBooks.DefaultCellStyle.Padding = new Padding(5);

            // Add toolbar and grid into the shared content panel
            ContentPanel.Controls.Add(dgvBooks);
            ContentPanel.Controls.Add(toolbarPanel);

            Shown += (_, _) => LoadBooks();
        }

        private Button CreateSidebarButton(string text, Color activeColor, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = activeColor == Color.Transparent ? Color.FromArgb(148, 163, 184) : Color.White,
                BackColor = activeColor,
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

        private Button CreateStyledButton(string text, Color color, EventHandler onClick)
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
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            button.Click += onClick;
            button.MouseEnter += (s, e) => button.BackColor = ControlPaint.Light(color, 0.1f);
            button.MouseLeave += (s, e) => button.BackColor = color;
            return button;
        }

        private void LoadBooks(string keyword = "")
        {
            try
            {
                dgvBooks.DataSource = string.IsNullOrWhiteSpace(keyword)
                    ? bookService.GetAllBooks()
                    : bookService.SearchBooks(keyword);

                var idColumn = dgvBooks.Columns["BookID"];
                if (idColumn != null)
                {
                    idColumn.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load books.\n\n{ex.Message}", "Books", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Book? GetSelectedBook()
        {
            return dgvBooks.CurrentRow?.DataBoundItem as Book;
        }

        private void AddBook()
        {
            using var editor = new BookEditorForm();
            if (editor.ShowDialog(this) == DialogResult.OK)
            {
                bookService.AddBook(editor.Book);
                LoadBooks(txtSearch.Text.Trim());
            }
        }

        private void EditBook()
        {
            var selected = GetSelectedBook();
            if (selected == null)
            {
                MessageBox.Show("Please select a book first.", "Books", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var editor = new BookEditorForm(selected);
            if (editor.ShowDialog(this) == DialogResult.OK)
            {
                bookService.UpdateBook(editor.Book);
                LoadBooks(txtSearch.Text.Trim());
            }
        }

        private void DeleteBook()
        {
            var selected = GetSelectedBook();
            if (selected == null)
            {
                MessageBox.Show("Please select a book first.", "Books", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Delete '{selected.Title}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            bookService.DeleteBook(selected.BookID);
            LoadBooks(txtSearch.Text.Trim());
        }

        private sealed class BookEditorForm : Form
        {
            private readonly TextBox txtIsbn = new() { Left = 150, Top = 20, Width = 220 };
            private readonly TextBox txtTitle = new() { Left = 150, Top = 55, Width = 220 };
            private readonly TextBox txtAuthor = new() { Left = 150, Top = 90, Width = 220 };
            private readonly TextBox txtPublisher = new() { Left = 150, Top = 125, Width = 220 };
            private readonly TextBox txtCategory = new() { Left = 150, Top = 160, Width = 220 };
            private readonly NumericUpDown numYear = new() { Left = 150, Top = 195, Width = 220, Minimum = 1900, Maximum = 2100, Value = 2024 };
            private readonly NumericUpDown numQuantity = new() { Left = 150, Top = 230, Width = 220, Minimum = 0, Maximum = 10000, Value = 1 };
            private readonly NumericUpDown numAvailable = new() { Left = 150, Top = 265, Width = 220, Minimum = 0, Maximum = 10000, Value = 1 };
            private readonly TextBox txtShelf = new() { Left = 150, Top = 300, Width = 220 };

            public Book Book { get; }

            public BookEditorForm(Book? book = null)
            {
                Book = book == null
                    ? new Book()
                    : new Book
                    {
                        BookID = book.BookID,
                        ISBN = book.ISBN,
                        Title = book.Title,
                        Author = book.Author,
                        Publisher = book.Publisher,
                        Category = book.Category,
                        YearPublished = book.YearPublished,
                        Quantity = book.Quantity,
                        Available = book.Available,
                        ShelfLocation = book.ShelfLocation
                    };

                Text = book == null ? "Add Book" : "Edit Book";
                ClientSize = new Size(400, 390);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                StartPosition = FormStartPosition.CenterParent;

                Controls.AddRange(new Control[]
                {
                    CreateLabel("ISBN", 20),
                    txtIsbn,
                    CreateLabel("Title", 55),
                    txtTitle,
                    CreateLabel("Author", 90),
                    txtAuthor,
                    CreateLabel("Publisher", 125),
                    txtPublisher,
                    CreateLabel("Category", 160),
                    txtCategory,
                    CreateLabel("Year", 195),
                    numYear,
                    CreateLabel("Quantity", 230),
                    numQuantity,
                    CreateLabel("Available", 265),
                    numAvailable,
                    CreateLabel("Shelf", 300),
                    txtShelf,
                    CreateActionButton("Save", 150, 340, (_, _) => SaveBook()),
                    CreateActionButton("Cancel", 245, 340, (_, _) => DialogResult = DialogResult.Cancel)
                });

                txtIsbn.Text = Book.ISBN;
                txtTitle.Text = Book.Title;
                txtAuthor.Text = Book.Author;
                txtPublisher.Text = Book.Publisher;
                txtCategory.Text = Book.Category;
                numYear.Value = Math.Max(numYear.Minimum, Math.Min(numYear.Maximum, Book.YearPublished == 0 ? 2024 : Book.YearPublished));
                numQuantity.Value = Book.Quantity;
                numAvailable.Value = Book.Available == 0 && book == null ? 1 : Book.Available;
                txtShelf.Text = Book.ShelfLocation;
            }

            private static Label CreateLabel(string text, int top)
            {
                return new Label { Text = text, Left = 30, Top = top + 4, Width = 100 };
            }

            private static Button CreateActionButton(string text, int left, int top, EventHandler onClick)
            {
                var button = new Button { Text = text, Left = left, Top = top, Width = 80, Height = 30 };
                button.Click += onClick;
                return button;
            }

            private void SaveBook()
            {
                if (string.IsNullOrWhiteSpace(txtIsbn.Text) || string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtAuthor.Text))
                {
                    MessageBox.Show("ISBN, title, and author are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (numAvailable.Value > numQuantity.Value)
                {
                    MessageBox.Show("Available copies cannot exceed total quantity.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Book.ISBN = txtIsbn.Text.Trim();
                Book.Title = txtTitle.Text.Trim();
                Book.Author = txtAuthor.Text.Trim();
                Book.Publisher = txtPublisher.Text.Trim();
                Book.Category = txtCategory.Text.Trim();
                Book.YearPublished = (int)numYear.Value;
                Book.Quantity = (int)numQuantity.Value;
                Book.Available = (int)numAvailable.Value;
                Book.ShelfLocation = txtShelf.Text.Trim();

                DialogResult = DialogResult.OK;
            }
        }
    }
}
