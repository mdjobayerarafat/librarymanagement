using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LibraryManagementSystem.UI.Forms
{
    /// <summary>
    /// Modern, card-based "Books" page. Replaces the old DataGridView list with a
    /// responsive gallery that matches BaseForm's dark Fluent-style theme, plus a
    /// pill-shaped search bar and icon toolbar buttons.
    /// </summary>
    public class BooksForm : BaseForm
    {
        private readonly BookService bookService = new();

        // Search + toolbar
        private TextBox txtSearch = null!;
        private Label lblClearSearch = null!;
        private Panel searchBox = null!;

        // Grid
        private FlowLayoutPanel flpBooks = null!;
        private Label lblEmptyState = null!;
        private Label lblSubtitle = null!;

        // Selection state (cards are rebuilt on every load, so selection is tracked
        // by reference rather than by index)
        private Panel? selectedCard;
        private Book? selectedBook;
        private readonly ToolTip cardToolTip = new();

        // ---- Palette --------------------------------------------------------
        private static readonly Color PageBg = Color.FromArgb(15, 23, 42);     // slate-900
        private static readonly Color CardBg = Color.FromArgb(30, 41, 59);     // slate-800
        private static readonly Color CardBgHover = Color.FromArgb(37, 50, 71);
        private static readonly Color CardBorder = Color.FromArgb(51, 65, 85);     // slate-700
        private static readonly Color InputBg = Color.FromArgb(24, 33, 48);
        private static readonly Color TextPrimary = Color.White;
        private static readonly Color TextSecondary = Color.FromArgb(148, 163, 184); // slate-400
        private static readonly Color TextMuted = Color.FromArgb(100, 116, 139); // slate-500
        private static readonly Color AccentBlue = Color.FromArgb(59, 130, 246);  // blue-500
        private static readonly Color AccentGreen = Color.FromArgb(34, 197, 94);   // green-500
        private static readonly Color AccentAmber = Color.FromArgb(219, 142, 60);  // amber-500
        private static readonly Color AccentRed = Color.FromArgb(255, 102, 94);   // red-500
        private static readonly Color ChipBg = Color.FromArgb(51, 65, 85);
        private static readonly Color ChipText = Color.FromArgb(203, 213, 225);

        private static readonly Color[] CoverPalette =
        {
            Color.FromArgb(37, 99, 235),  // blue
            Color.FromArgb(124, 58, 237), // violet
            Color.FromArgb(13, 148, 136), // teal
            Color.FromArgb(217, 70, 160), // pink
            Color.FromArgb(217, 119, 6),  // amber
            Color.FromArgb(22, 163, 74),  // green
        };

        public BooksForm() : base("Manage Books")
        {
            BackColor = PageBg;

            // --- Sidebar ------------------------------------------------------
            AddSidebarButton("🏠  Dashboard", 70, (_, _) => Close());
            AddSidebarButton("📖  Books", 120, (_, _) => { }, active: true);
            AddSidebarButton("👥  Members", 170, (_, _) => { });
            AddSidebarButton("🔄  Transactions", 220, (_, _) => { });
            AddSidebarButton("⚙️  Settings", 270, (_, _) =>
                MessageBox.Show("Settings coming soon!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information));
            AddSidebarButton("🚪  Log Out", 320, (_, _) => Close());

            // --- Page layout: header row / toolbar row / grid (fills rest) ----
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = PageBg
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            root.Controls.Add(BuildHeader(), 0, 0);
            root.Controls.Add(BuildToolbar(), 0, 1);
            root.Controls.Add(BuildGrid(), 0, 2);

            ContentPanel.Controls.Add(root);

            Shown += (_, _) => LoadBooks();
        }

        // ======================================================================
        //  Layout builders
        // ======================================================================

        private Control BuildHeader()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 78,
                BackColor = PageBg,
                Padding = new Padding(4, 12, 4, 0)
            };

            var lblTitle = new Label
            {
                Text = "Books",
                Font = new Font("Segoe UI Semibold", 26, FontStyle.Bold),
                ForeColor = TextPrimary,
                AutoSize = true,
                Location = new Point(4, 8)
            };

            lblSubtitle = new Label
            {
                Text = "Loading…",
                Font = new Font("Segoe UI", 11),
                ForeColor = TextSecondary,
                AutoSize = true,
                Location = new Point(4, lblTitle.Bottom + 2)
            };

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblSubtitle);
            return panel;
        }

        private Control BuildToolbar()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = PageBg,
                Padding = new Padding(0, 4, 0, 16)
            };

            searchBox = BuildSearchBar();
            searchBox.Location = new Point(0, 0);
            panel.Controls.Add(searchBox);

            // Action buttons live in their own right-docked flow panel, so they stay
            // pinned to the right edge (and keep their left-to-right order) no matter
            // how the window is resized, instead of relying on fixed X offsets.
            var buttonsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = PageBg,
                Margin = new Padding(0)
            };

            var btnAdd = CreateActionButton("➕  Add Book", AccentGreen, (_, _) => AddBook());
            var btnEdit = CreateActionButton("✏️  Edit", AccentAmber, (_, _) => EditBook());
            var btnDelete = CreateActionButton("🗑️  Delete", AccentRed, (_, _) => DeleteBook());
            var btnRefresh = CreateOutlineButton("🔄  Refresh", (_, _) => LoadBooks(txtSearch.Text.Trim()));
            foreach (var b in new[] { btnAdd, btnEdit, btnDelete, btnRefresh })
            {
                b.Margin = new Padding(8, 0, 0, 0);
            }

            buttonsFlow.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });
            panel.Controls.Add(buttonsFlow);

            // Search bar fills whatever room is left between its left edge and the button flow.
            panel.Resize += (_, _) => searchBox.Width = Math.Max(200, panel.Width - buttonsFlow.Width - 16);
            searchBox.Width = Math.Max(200, panel.Width - buttonsFlow.Width - 16);

            return panel;
        }

        private Panel BuildSearchBar()
        {
            var box = new Panel
            {
                Size = new Size(360, 40),
                BackColor = InputBg
            };

            var lblIcon = new Label
            {
                Text = "🔍",
                AutoSize = false,
                Size = new Size(30, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = TextMuted,
                BackColor = Color.Transparent,
                Location = new Point(6, 0),
                Font = new Font("Segoe UI", 10)
            };

            txtSearch = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10.5f),
                BackColor = InputBg,
                ForeColor = TextPrimary,
                Location = new Point(38, 11),
                Width = box.Width - 38 - 30,
                PlaceholderText = "Search by title, author, or ISBN…"
            };
            txtSearch.TextChanged += (_, _) =>
            {
                lblClearSearch.Visible = txtSearch.Text.Length > 0;
                LoadBooks(txtSearch.Text.Trim());
            };

            lblClearSearch = new Label
            {
                Text = "✕",
                AutoSize = false,
                Size = new Size(30, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = TextMuted,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Visible = false,
                Font = new Font("Segoe UI", 9)
            };
            lblClearSearch.Click += (_, _) => txtSearch.Clear();
            lblClearSearch.MouseEnter += (_, _) => lblClearSearch.ForeColor = TextPrimary;
            lblClearSearch.MouseLeave += (_, _) => lblClearSearch.ForeColor = TextMuted;

            void PositionClear() => lblClearSearch.Location = new Point(box.Width - 30, 0);
            box.Resize += (_, _) =>
            {
                txtSearch.Width = Math.Max(60, box.Width - 38 - 30);
                PositionClear();
            };
            PositionClear();

            // Rounded pill border that lights up in accent blue while focused.
            var borderColor = CardBorder;
            box.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(new Rectangle(0, 0, box.Width - 1, box.Height - 1), box.Height / 2);
                using var fill = new SolidBrush(InputBg);
                g.FillPath(fill, path);
                using var pen = new Pen(borderColor, 1.4f);
                g.DrawPath(pen, path);
                box.Region = new Region(path);
            };
            txtSearch.Enter += (_, _) => { borderColor = AccentBlue; box.Invalidate(); };
            txtSearch.Leave += (_, _) => { borderColor = CardBorder; box.Invalidate(); };

            box.Controls.Add(txtSearch);
            box.Controls.Add(lblIcon);
            box.Controls.Add(lblClearSearch);
            return box;
        }

        private Control BuildGrid()
        {
            var container = new Panel { Dock = DockStyle.Fill, BackColor = PageBg };

            flpBooks = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                BackColor = PageBg,
                Padding = new Padding(0, 4, 0, 20)
            };

            lblEmptyState = new Label
            {
                Text = "📭   No books found.\nTry a different search, or add a new book.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = TextSecondary,
                Font = new Font("Segoe UI", 12),
                BackColor = PageBg,
                Visible = false
            };

            container.Controls.Add(flpBooks);
            container.Controls.Add(lblEmptyState);
            return container;
        }

        // ======================================================================
        //  Data
        // ======================================================================

        private void LoadBooks(string keyword = "")
        {
            try
            {
                flpBooks.SuspendLayout();
                flpBooks.Controls.Clear();
                selectedCard = null;
                selectedBook = null;

                var books = string.IsNullOrWhiteSpace(keyword)
                    ? bookService.GetAllBooks()
                    : bookService.SearchBooks(keyword);

                foreach (var book in books)
                {
                    flpBooks.Controls.Add(CreateBookCard(book));
                }

                int count = books.Count;
                lblSubtitle.Text = string.IsNullOrWhiteSpace(keyword)
                    ? $"{count} title{(count == 1 ? "" : "s")} in collection"
                    : $"{count} result{(count == 1 ? "" : "s")} for \"{keyword}\"";

                bool isEmpty = count == 0;
                lblEmptyState.Visible = isEmpty;
                flpBooks.Visible = !isEmpty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load books.\n\n{ex.Message}", "Books", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                flpBooks.ResumeLayout();
            }
        }

        private Book? GetSelectedBook() => selectedBook;

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

        // ======================================================================
        //  Card
        // ======================================================================

        private Panel CreateBookCard(Book book)
        {
            var card = new Panel
            {
                Size = new Size(360, 900),
                Margin = new Padding(0, 0, 16, 16),
                BackColor = CardBg,
                Cursor = Cursors.Hand
            };

            bool hovering = false;

            card.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 14);
                using var fill = new SolidBrush(hovering ? CardBgHover : CardBg);
                g.FillPath(fill, path);
                bool isSelected = ReferenceEquals(selectedCard, card);
                using var pen = new Pen(isSelected ? AccentBlue : CardBorder, isSelected ? 2f : 1f);
                g.DrawPath(pen, path);
                card.Region = new Region(path);
            };

            // --- Cover -----------------------------------------------------
            var cover = new Panel { Size = new Size(84, 116), Location = new Point(16, 16) };
            var coverColor = CoverPalette[Math.Abs((book.Category ?? book.Title ?? "").GetHashCode()) % CoverPalette.Length];

            if (!string.IsNullOrWhiteSpace(book.CoverImagePath) && System.IO.File.Exists(book.CoverImagePath))
            {
                var pic = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
                try { pic.Image = Image.FromFile(book.CoverImagePath); } catch { /* fall through to placeholder */ }
                cover.Controls.Add(pic);
                RoundControl(cover, 8);
            }
            else
            {
                var lblGlyph = new Label
                {
                    Text = "📘",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI Emoji", 22),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent
                };
                cover.BackColor = coverColor;
                cover.Controls.Add(lblGlyph);
                RoundControl(cover, 8);
            }

            // --- Title / author -------------------------------------------
            int textLeft = cover.Right + 16;
            int textWidth = card.Width - textLeft - 14;

            var lblTitle = new Label
            {
                Text = book.Title,
                Font = new Font("Segoe UI Semibold", 12.5f, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(textLeft, 12),
                Size = new Size(textWidth, 22),
                AutoEllipsis = true,
                BackColor = Color.Transparent
            };
            cardToolTip.SetToolTip(lblTitle, book.Title);

            var lblAuthorYear = new Label
            {
                Text = $"{book.Author} · {book.YearPublished}",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = TextSecondary,
                Location = new Point(textLeft, lblTitle.Bottom + 2),
                Size = new Size(textWidth, 18),
                AutoEllipsis = true,
                BackColor = Color.Transparent
            };

            // --- Category chip ----------------------------------------------
            var chip = CreateChip(string.IsNullOrWhiteSpace(book.Category) ? "Uncategorized" : book.Category,
                                   Math.Min(textWidth - 74, 170));
            chip.Location = new Point(textLeft, lblAuthorYear.Bottom + 10);

            // --- Availability badge -----------------------------------------
            bool available = book.Available > 0;
            var badge = new Label
            {
                Text = $"{book.Available}/{book.Quantity}",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = available ? Color.White : Color.FromArgb(254, 202, 202),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = available ? AccentGreen : AccentRed,
                Size = new Size(58, 26)
            };
            badge.Location = new Point(card.Width - 16 - badge.Width, chip.Top - 1);
            RoundControl(badge, 13);
            cardToolTip.SetToolTip(badge, available ? $"{book.Available} of {book.Quantity} available" : "No copies available");

            card.Controls.AddRange(new Control[] { cover, lblTitle, lblAuthorYear, chip, badge });

            // --- Selection / interaction --------------------------------------
            void Select()
            {
                var previous = selectedCard;
                selectedCard = card;
                selectedBook = book;
                previous?.Invalidate();
                card.Invalidate();
            }

            var menu = new ContextMenuStrip();
            menu.Items.Add("Edit", null, (_, _) => { Select(); EditBook(); });
            menu.Items.Add("Delete", null, (_, _) => { Select(); DeleteBook(); });

            void WireInteractions(Control c)
            {
                c.Click += (_, _) => Select();
                c.DoubleClick += (_, _) => { Select(); EditBook(); };
                c.MouseEnter += (_, _) => { hovering = true; card.Invalidate(); };
                c.MouseLeave += (_, _) => { hovering = false; card.Invalidate(); };
                c.ContextMenuStrip = menu;
            }
            WireInteractions(card);
            WireInteractions(lblTitle);
            WireInteractions(lblAuthorYear);
            WireInteractions(cover);

            return card;
        }

        private Label CreateChip(string text, int maxWidth)
        {
            maxWidth = Math.Max(50, maxWidth);
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ChipText,
                BackColor = ChipBg,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = true,
                Padding = new Padding(4, 0, 4, 0)
            };

            var measured = TextRenderer.MeasureText(text, lbl.Font).Width;
            int width = Math.Min(maxWidth, measured + 24);
            lbl.Size = new Size(Math.Max(50, width), 26);

            RoundControl(lbl, 13);
            return lbl;
        }

        // ======================================================================
        //  Shared button + rounding helpers
        // ======================================================================

        private static Button CreateActionButton(string text, Color color, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(118, 40),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            btn.MouseEnter += (_, _) => btn.BackColor = ControlPaint.Light(color, 0.15f);
            btn.MouseLeave += (_, _) => btn.BackColor = color;
            RoundControl(btn, 10);
            return btn;
        }

        private static Button CreateOutlineButton(string text, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(112, 40),
                BackColor = CardBg,
                ForeColor = TextSecondary,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = CardBorder;
            btn.Click += onClick;
            btn.MouseEnter += (_, _) => { btn.BackColor = CardBgHover; btn.ForeColor = TextPrimary; };
            btn.MouseLeave += (_, _) => { btn.BackColor = CardBg; btn.ForeColor = TextSecondary; };
            RoundControl(btn, 10);
            return btn;
        }

        private static void RoundControl(Control control, int radius)
        {
            void Apply(object? s, EventArgs e)
            {
                if (control.Width <= 0 || control.Height <= 0) return;
                using var path = RoundedRect(new Rectangle(0, 0, control.Width, control.Height), radius);
                control.Region = new Region(path);
            }
            control.Resize += Apply;
            Apply(control, EventArgs.Empty);
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ======================================================================
        //  Add / Edit dialog
        // ======================================================================

        private sealed class BookEditorForm : Form
        {
            private readonly TextBox txtIsbn = new() { Dock = DockStyle.Fill };
            private readonly TextBox txtTitle = new() { Dock = DockStyle.Fill };
            private readonly TextBox txtAuthor = new() { Dock = DockStyle.Fill };
            private readonly TextBox txtPublisher = new() { Dock = DockStyle.Fill };
            private readonly TextBox txtCategory = new() { Dock = DockStyle.Fill };
            private readonly NumericUpDown numYear = new() { Dock = DockStyle.Fill, Minimum = 1900, Maximum = 2100, Value = 2024 };
            private readonly NumericUpDown numQuantity = new() { Dock = DockStyle.Fill, Minimum = 0, Maximum = 10000, Value = 1 };
            private readonly NumericUpDown numAvailable = new() { Dock = DockStyle.Fill, Minimum = 0, Maximum = 10000, Value = 1 };
            private readonly TextBox txtShelf = new() { Dock = DockStyle.Fill };
            private readonly PictureBox picCover = new() { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = InputBg };
            private string _coverImagePath = string.Empty;

            private readonly Label lblValidation = new()
            {
                Dock = DockStyle.Top,
                Height = 26,
                ForeColor = AccentRed,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleLeft,
                Visible = false
            };

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
                ClientSize = new Size(460, 480);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                StartPosition = FormStartPosition.CenterParent;
                Padding = new Padding(20, 16, 20, 16);
                BackColor = PageBg;
                ForeColor = TextPrimary;

                var fields = new (string Label, Control Input)[]
                {
                    ("ISBN", txtIsbn),
                    ("Title", txtTitle),
                    ("Author", txtAuthor),
                    ("Publisher", txtPublisher),
                    ("Category", txtCategory),
                    ("Year", numYear),
                    ("Quantity", numQuantity),
                    ("Available", numAvailable),
                    ("Shelf", txtShelf),
                };
                foreach (var (_, input) in fields) StyleInput(input);

                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = fields.Length + 2, // + cover row + button row
                    AutoSize = true,
                    BackColor = PageBg
                };
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                for (int i = 0; i < fields.Length; i++)
                {
                    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
                    layout.Controls.Add(new Label
                    {
                        Text = fields[i].Label,
                        Dock = DockStyle.Fill,
                        ForeColor = TextSecondary,
                        TextAlign = ContentAlignment.MiddleLeft
                    }, 0, i);
                    layout.Controls.Add(fields[i].Input, 1, i);
                }

                // Cover row
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
                layout.Controls.Add(new Label
                {
                    Text = "Cover",
                    Dock = DockStyle.Fill,
                    ForeColor = TextSecondary,
                    TextAlign = ContentAlignment.MiddleLeft
                }, 0, fields.Length);

                var coverRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
                coverRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));
                coverRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                var coverHolder = new Panel { Size = new Size(76, 96), BackColor = InputBg };
                coverHolder.Controls.Add(picCover);
                RoundControl(coverHolder, 8);
                var btnBrowseCover = CreateOutlineButton("🖼️  Browse…", (_, _) => BrowseForCover());
                btnBrowseCover.Anchor = AnchorStyles.Left;
                coverRow.Controls.Add(coverHolder, 0, 0);
                coverRow.Controls.Add(btnBrowseCover, 1, 0);
                layout.Controls.Add(coverRow, 1, fields.Length);

                // Button row spans both columns
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
                var buttonPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    WrapContents = false,
                    Padding = new Padding(0, 10, 0, 0)
                };
                var btnCancel = CreateOutlineButton("Cancel", (_, _) => DialogResult = DialogResult.Cancel);
                var btnSave = CreateActionButton("Save", AccentBlue, (_, _) => SaveBook());
                buttonPanel.Controls.Add(btnCancel);
                buttonPanel.Controls.Add(btnSave);
                layout.Controls.Add(buttonPanel, 0, fields.Length + 1);
                layout.SetColumnSpan(buttonPanel, 2);

                Controls.Add(layout);
                Controls.Add(lblValidation);

                txtIsbn.Text = Book.ISBN;
                txtTitle.Text = Book.Title;
                txtAuthor.Text = Book.Author;
                txtPublisher.Text = Book.Publisher;
                txtCategory.Text = Book.Category;
                numYear.Value = Math.Max(numYear.Minimum, Math.Min(numYear.Maximum, Book.YearPublished == 0 ? 2024 : Book.YearPublished));
                numQuantity.Value = Book.Quantity;
                numAvailable.Value = Book.Available == 0 && book == null ? 1 : Book.Available;
                txtShelf.Text = Book.ShelfLocation;
                _coverImagePath = Book.CoverImagePath ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(_coverImagePath) && System.IO.File.Exists(_coverImagePath))
                {
                    try { picCover.Image = Image.FromFile(_coverImagePath); } catch { /* ignore */ }
                }

                AcceptButton = btnSave;
                CancelButton = btnCancel;
            }

            private static void StyleInput(Control input)
            {
                input.ForeColor = TextPrimary;
                input.BackColor = InputBg;
                input.Font = new Font("Segoe UI", 10);
                if (input is TextBox tb) tb.BorderStyle = BorderStyle.FixedSingle;
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

            private void SaveBook()
            {
                if (string.IsNullOrWhiteSpace(txtIsbn.Text) || string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtAuthor.Text))
                {
                    ShowValidationError("ISBN, title, and author are required.");
                    return;
                }

                if (numAvailable.Value > numQuantity.Value)
                {
                    ShowValidationError("Available copies cannot exceed total quantity.");
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

            private void ShowValidationError(string message)
            {
                lblValidation.Text = message;
                lblValidation.Visible = true;
            }
        }
    }
}