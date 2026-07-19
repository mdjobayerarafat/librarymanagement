using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;

namespace LibraryManagementSystem.UI.Controls
{
    public class BooksControl : UserControl
    {
        private readonly BookService bookService = new();
        private readonly TextBox txtSearch;
        private readonly FlowLayoutPanel flpBooks;
        private static readonly Color BackgroundColor = Color.FromArgb(245, 242, 235);
        private static readonly Color CardBackgroundColor = Color.White;
        private static readonly Color TextPrimaryColor = Color.FromArgb(26, 32, 44);
        private static readonly Color TextSecondaryColor = Color.FromArgb(100, 116, 139);
        private static readonly Color AccentColor = Color.FromArgb(20, 83, 45);
        private static readonly Color InputBackgroundColor = Color.FromArgb(230, 224, 213);
        private static readonly Color TagColor = Color.FromArgb(230, 224, 213);
        private static readonly Color TagTextColor = Color.FromArgb(75, 85, 99);
        public BooksControl()
        {
            BackColor = BackgroundColor;
            Dock = DockStyle.Fill;
            Padding = new Padding(32, 24, 32, 24);

            // Title Section
            var lblTitle = new Label
            {
                Text = "Books",
                Font = new Font("Georgia", 36, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var lblSubtitle = new Label
            {
                Text = "8 titles in collection",
                Font = new Font("Segoe UI", 14),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(0, 52)
            };

            // Toolbar Section
            var toolbarPanel = new Panel
            {
                Location = new Point(0, 96),
                Size = new Size(ClientSize.Width - 64, 56),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            txtSearch = CreateStyledTextBox("Search by title, author or ISBN...", new Point(0, 0), 400);
            txtSearch.TextChanged += (_, _) => LoadBooks(txtSearch.Text.Trim());

            var btnAdd = CreateStyledButton("➕ Add Book", AccentColor, Color.White, (_, _) => AddBook());
            btnAdd.Location = new Point(toolbarPanel.ClientSize.Width - 180, 0);
            toolbarPanel.Resize += (_, _) =>
            {
                btnAdd.Location = new Point(toolbarPanel.ClientSize.Width - 180, 0);
            };

            toolbarPanel.Controls.AddRange(new Control[] { txtSearch, btnAdd });

            // Books Container
            flpBooks = new FlowLayoutPanel
            {
                Location = new Point(0, 176),
                Size = new Size(ClientSize.Width - 64, ClientSize.Height - 200),
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            Resize += (_, _) =>
            {
                toolbarPanel.Size = new Size(ClientSize.Width - 64, 56);
                flpBooks.Size = new Size(ClientSize.Width - 64, ClientSize.Height - 200);
            };

            Controls.AddRange(new Control[] { lblTitle, lblSubtitle, toolbarPanel, flpBooks });

            Load += (_, _) => LoadBooks();
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
                Size = new Size(180, 48),
                BackColor = backColor,
                ForeColor = foreColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
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

        private Panel CreateBookCard(Book book)
        {
            var card = new Panel
            {
                Size = new Size(420, 170),
                BackColor = CardBackgroundColor,
                Margin = new Padding(0, 0, 24, 24)
            };
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                int cr = 12;
                path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                path.AddArc(card.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                path.AddArc(card.Width - cr * 2, card.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                path.AddArc(0, card.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                path.CloseAllFigures();
                card.Region = new Region(path);
            };

            var coverPanel = new Panel
            {
                Size = new Size(100, 138),
                Location = new Point(24, 16),
                BackColor = InputBackgroundColor
            };
            coverPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                int cr = 6;
                path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                path.AddArc(coverPanel.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                path.AddArc(coverPanel.Width - cr * 2, coverPanel.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                path.AddArc(0, coverPanel.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                path.CloseAllFigures();
                coverPanel.Region = new Region(path);
            };

            var picCover = new PictureBox
            {
                Size = new Size(100, 138),
                Location = new Point(0, 0),
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            if (!string.IsNullOrWhiteSpace(book.CoverImagePath) && System.IO.File.Exists(book.CoverImagePath))
            {
                try
                {
                    picCover.Image = Image.FromFile(book.CoverImagePath);
                }
                catch
                {
                    // Ignore if can't load
                }
            }
            coverPanel.Controls.Add(picCover);

            var lblTitle = new Label
            {
                Text = book.Title,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                Size = new Size(256, 44),
                Location = new Point(140, 20),
                AutoSize = false,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.TopLeft
            };

            var lblAuthorYear = new Label
            {
                Text = $"{book.Author} · {book.YearPublished}",
                Font = new Font("Segoe UI", 12),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(140, 60),
                MaximumSize = new Size(256, 0)
            };

            var tagPanel = new Panel
            {
                Size = new Size(180, 32),
                Location = new Point(140, 92),
                BackColor = TagColor
            };
            tagPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                int cr = 16;
                path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                path.AddArc(tagPanel.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                path.AddArc(tagPanel.Width - cr * 2, tagPanel.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                path.AddArc(0, tagPanel.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                path.CloseAllFigures();
                tagPanel.Region = new Region(path);
            };
            var lblTag = new Label
            {
                Text = book.Category,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = TagTextColor,
                AutoSize = true,
                Location = new Point(16, 6),
                BackColor = Color.Transparent,
                MaximumSize = new Size(148, 0)
            };
            tagPanel.Controls.Add(lblTag);

            var availabilityColor = book.Available > 0 ? AccentColor : Color.FromArgb(239, 68, 68);
            var availabilityPanel = new Panel
            {
                Size = new Size(64, 32),
                Location = new Point(332, 92),
                BackColor = book.Available > 0 ? AccentColor : Color.FromArgb(254, 226, 226)
            };
            availabilityPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                int cr = 16;
                path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                path.AddArc(availabilityPanel.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                path.AddArc(availabilityPanel.Width - cr * 2, availabilityPanel.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                path.AddArc(0, availabilityPanel.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                path.CloseAllFigures();
                availabilityPanel.Region = new Region(path);
            };
            var lblAvailability = new Label
            {
                Text = book.Available > 0 ? $"{book.Available}" : "0",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = book.Available > 0 ? Color.White : Color.FromArgb(239, 68, 68),
                AutoSize = true,
                Location = new Point(24, 6),
                BackColor = Color.Transparent
            };
            availabilityPanel.Controls.Add(lblAvailability);

            card.Controls.AddRange(new Control[] { coverPanel, lblTitle, lblAuthorYear, tagPanel, availabilityPanel });
            return card;
        }

        private void LoadBooks(string keyword = "")
        {
            try
            {
                flpBooks.Controls.Clear();
                var books = string.IsNullOrWhiteSpace(keyword)
                    ? bookService.GetAllBooks()
                    : bookService.SearchBooks(keyword);

                foreach (var book in books)
                {
                    flpBooks.Controls.Add(CreateBookCard(book));
                }

                // Update subtitle count
                foreach (Control control in Controls)
                {
                    if (control is Label lbl && lbl.Text.EndsWith("titles in collection"))
                    {
                        lbl.Text = $"{books.Count} titles in collection";
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load books.\n\n{ex.Message}", "Books", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            private readonly PictureBox picCover = new() { Left = 30, Top = 340, Width = 120, Height = 160, BackColor = InputBackgroundColor, SizeMode = PictureBoxSizeMode.Zoom };
            private readonly Button btnBrowseCover = new() { Text = "Browse Cover", Left = 170, Top = 340, Width = 220, Height = 35 };
            private string _coverImagePath = string.Empty;

            private static readonly Color BackgroundColor = Color.FromArgb(245, 242, 235);
            private static readonly Color CardBackgroundColor = Color.White;
            private static readonly Color TextPrimaryColor = Color.FromArgb(26, 32, 44);
            private static readonly Color AccentColor = Color.FromArgb(20, 83, 45);
            private static readonly Color InputBackgroundColor = Color.FromArgb(230, 224, 213);
            private static readonly Color WarningColor = Color.FromArgb(239, 68, 68);
            private static readonly Color WarningLightColor = Color.FromArgb(254, 226, 226);

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
                BackColor = BackgroundColor;
                ForeColor = TextPrimaryColor;

                // Style the browse button
                btnBrowseCover.BackColor = AccentColor;
                btnBrowseCover.ForeColor = Color.White;
                btnBrowseCover.FlatStyle = FlatStyle.Flat;
                btnBrowseCover.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                btnBrowseCover.Cursor = Cursors.Hand;
                btnBrowseCover.FlatAppearance.BorderSize = 0;
                btnBrowseCover.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using var path = new GraphicsPath();
                    int cr = 6;
                    path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                    path.AddArc(btnBrowseCover.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                    path.AddArc(btnBrowseCover.Width - cr * 2, btnBrowseCover.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                    path.AddArc(0, btnBrowseCover.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                    path.CloseAllFigures();
                    btnBrowseCover.Region = new Region(path);
                };
                btnBrowseCover.Click += (s, e) => BrowseForCover();
                btnBrowseCover.MouseEnter += (s, e) => btnBrowseCover.BackColor = ControlPaint.Light(AccentColor, 0.1f);
                btnBrowseCover.MouseLeave += (s, e) => btnBrowseCover.BackColor = AccentColor;

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
                    ForeColor = TextPrimaryColor,
                    Font = new Font("Segoe UI", 10)
                };
            }

            private static TextBox StyleTextBox(TextBox textBox)
            {
                textBox.BackColor = InputBackgroundColor;
                textBox.ForeColor = TextPrimaryColor;
                textBox.BorderStyle = BorderStyle.FixedSingle;
                textBox.Font = new Font("Segoe UI", 10);
                return textBox;
            }

            private static NumericUpDown StyleNumericUpDown(NumericUpDown numericUpDown)
            {
                numericUpDown.BackColor = InputBackgroundColor;
                numericUpDown.ForeColor = TextPrimaryColor;
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
                    BackColor = text == "Save" ? AccentColor : Color.White,
                    ForeColor = text == "Save" ? Color.White : TextPrimaryColor,
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
                button.MouseEnter += (s, e) => button.BackColor = text == "Save" ? ControlPaint.Light(AccentColor, 0.1f) : InputBackgroundColor;
                button.MouseLeave += (s, e) => button.BackColor = text == "Save" ? AccentColor : Color.White;
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