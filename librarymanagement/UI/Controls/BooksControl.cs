using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryManagementSystem.UI.Controls
{
    public class BooksControl : UserControl
    {
        private readonly BookService bookService = new();
        private readonly TextBox txtSearch;
        private readonly DataGridView dgvBooks;

        public BooksControl()
        {
            BackColor = Color.FromArgb(15, 23, 42);
            Dock = DockStyle.Fill;
            Padding = new Padding(0);

            // Title Panel
            var titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(30, 41, 59)
            };

            var lblTitle = new Label
            {
                Text = "📚 Books Management",
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

            txtSearch = new TextBox
            {
                PlaceholderText = "🔍 Search by title, author, ISBN, or category...",
                Width = 400,
                Height = 36,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
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

            Controls.Add(dgvBooks);
            Controls.Add(toolbarPanel);
            Controls.Add(titlePanel);

            // Handle resize to scale fonts
            Resize += (_, _) => UpdateGridFont();

            Load += (_, _) =>
            {
                LoadBooks();
                UpdateGridFont();
            };
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

        private void UpdateGridFont()
        {
            int availableWidth = ClientSize.Width;
            float fontSize = Math.Max(8, Math.Min(12, availableWidth / 120f));

            dgvBooks.Font = new Font("Segoe UI", fontSize);
            dgvBooks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", fontSize + 0.5f, FontStyle.Bold);
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
            private readonly TextBox txtIsbn = new() { Left = 150, Top = 20, Width = 240 };
            private readonly TextBox txtTitle = new() { Left = 150, Top = 55, Width = 240 };
            private readonly TextBox txtAuthor = new() { Left = 150, Top = 90, Width = 240 };
            private readonly TextBox txtPublisher = new() { Left = 150, Top = 125, Width = 240 };
            private readonly TextBox txtCategory = new() { Left = 150, Top = 160, Width = 240 };
            private readonly NumericUpDown numYear = new() { Left = 150, Top = 195, Width = 240, Minimum = 1900, Maximum = 2100, Value = 2024 };
            private readonly NumericUpDown numQuantity = new() { Left = 150, Top = 230, Width = 240, Minimum = 0, Maximum = 10000, Value = 1 };
            private readonly NumericUpDown numAvailable = new() { Left = 150, Top = 265, Width = 240, Minimum = 0, Maximum = 10000, Value = 1 };
            private readonly TextBox txtShelf = new() { Left = 150, Top = 300, Width = 240 };
            private readonly PictureBox picCover = new() { Left = 30, Top = 340, Width = 120, Height = 160, BackColor = Color.FromArgb(51, 65, 85), SizeMode = PictureBoxSizeMode.Zoom };
            private readonly Button btnBrowseCover = new() { Text = "Browse Cover", Left = 170, Top = 340, Width = 220, Height = 35 };
            private string _coverImagePath = string.Empty;

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
                        ShelfLocation = book.ShelfLocation,
                        CoverImagePath = book.CoverImagePath
                    };

                Text = book == null ? "Add Book" : "Edit Book";
                ClientSize = new Size(420, 550);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                StartPosition = FormStartPosition.CenterParent;
                BackColor = Color.FromArgb(30, 41, 59);
                ForeColor = Color.White;

                // Style the browse button
                btnBrowseCover.BackColor = Color.FromArgb(59, 130, 246);
                btnBrowseCover.ForeColor = Color.White;
                btnBrowseCover.FlatStyle = FlatStyle.Flat;
                btnBrowseCover.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                btnBrowseCover.Cursor = Cursors.Hand;
                btnBrowseCover.FlatAppearance.BorderSize = 0;
                btnBrowseCover.Click += (s, e) => BrowseForCover();
                btnBrowseCover.MouseEnter += (s, e) => btnBrowseCover.BackColor = ControlPaint.Light(Color.FromArgb(59, 130, 246), 0.1f);
                btnBrowseCover.MouseLeave += (s, e) => btnBrowseCover.BackColor = Color.FromArgb(59, 130, 246);

                Controls.AddRange(new Control[]
                {
                    CreateLabel("ISBN", 20),
                    StyleTextBox(txtIsbn),
                    CreateLabel("Title", 55),
                    StyleTextBox(txtTitle),
                    CreateLabel("Author", 90),
                    StyleTextBox(txtAuthor),
                    CreateLabel("Publisher", 125),
                    StyleTextBox(txtPublisher),
                    CreateLabel("Category", 160),
                    StyleTextBox(txtCategory),
                    CreateLabel("Year", 195),
                    StyleNumericUpDown(numYear),
                    CreateLabel("Quantity", 230),
                    StyleNumericUpDown(numQuantity),
                    CreateLabel("Available", 265),
                    StyleNumericUpDown(numAvailable),
                    CreateLabel("Shelf", 300),
                    StyleTextBox(txtShelf),
                    CreateLabel("Cover", 340),
                    picCover,
                    btnBrowseCover,
                    CreateActionButton("Save", 150, 505, (_, _) => SaveBook()),
                    CreateActionButton("Cancel", 255, 505, (_, _) => DialogResult = DialogResult.Cancel)
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
                _coverImagePath = Book.CoverImagePath;
                if (!string.IsNullOrWhiteSpace(_coverImagePath) && System.IO.File.Exists(_coverImagePath))
                {
                    try
                    {
                        picCover.Image = Image.FromFile(_coverImagePath);
                    }
                    catch
                    {
                        // Ignore if image can't be loaded
                    }
                }
            }

            private void BrowseForCover()
            {
                using var openFileDialog = new OpenFileDialog
                {
                    Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                    Title = "Select Book Cover Image"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _coverImagePath = openFileDialog.FileName;
                    try
                    {
                        picCover.Image = Image.FromFile(_coverImagePath);
                    }
                    catch
                    {
                        MessageBox.Show("Unable to load the selected image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _coverImagePath = string.Empty;
                        picCover.Image = null;
                    }
                }
            }

            private static Label CreateLabel(string text, int top)
            {
                return new Label
                {
                    Text = text,
                    Left = 30,
                    Top = top + 4,
                    Width = 100,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10)
                };
            }

            private static TextBox StyleTextBox(TextBox textBox)
            {
                textBox.BackColor = Color.FromArgb(51, 65, 85);
                textBox.ForeColor = Color.White;
                textBox.BorderStyle = BorderStyle.FixedSingle;
                textBox.Font = new Font("Segoe UI", 10);
                return textBox;
            }

            private static NumericUpDown StyleNumericUpDown(NumericUpDown numericUpDown)
            {
                numericUpDown.BackColor = Color.FromArgb(51, 65, 85);
                numericUpDown.ForeColor = Color.White;
                numericUpDown.Font = new Font("Segoe UI", 10);
                return numericUpDown;
            }

            private static Button CreateActionButton(string text, int left, int top, EventHandler onClick)
            {
                var button = new Button
                {
                    Text = text,
                    Left = left,
                    Top = top,
                    Width = 100,
                    Height = 35,
                    BackColor = Color.FromArgb(59, 130, 246),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                button.FlatAppearance.BorderSize = 0;
                button.Click += onClick;
                button.MouseEnter += (s, e) => button.BackColor = ControlPaint.Light(Color.FromArgb(59, 130, 246), 0.1f);
                button.MouseLeave += (s, e) => button.BackColor = Color.FromArgb(59, 130, 246);
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
                Book.CoverImagePath = _coverImagePath;

                DialogResult = DialogResult.OK;
            }
        }
    }
}