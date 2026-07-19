using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LibraryManagementSystem.UI.Controls
{
    /// <summary>
    /// Full book-gallery page: a searchable, card-based catalog that surfaces every
    /// field on the Book model (cover, title, author, year, category, ISBN,
    /// publisher, shelf location, and live availability) plus Add/Edit/Delete.
    /// </summary>
    public class BooksControl : UserControl
    {
        private readonly BookService bookService = new();

        private TextBox txtSearch = null!;
        private Label lblClearSearch = null!;
        private Panel searchBox = null!;
        private FlowLayoutPanel flpBooks = null!;
        private Label lblEmptyState = null!;
        private Label lblSubtitle = null!;

        // Cards are rebuilt on every load, so selection is tracked by reference.
        private Panel? selectedCard;
        private Book? selectedBook;
        private readonly ToolTip cardToolTip = new();

        // ---- Palette (warm/cream theme) --------------------------------------
        private static readonly Color BackgroundColor = Color.FromArgb(247, 247, 255);
        private static readonly Color CardBackgroundColor = Color.White;
        private static readonly Color CardBorderColor = Color.FromArgb(189, 213, 234);
        private static readonly Color CardBorderHoverColor = Color.FromArgb(87, 115, 153);
        private static readonly Color TextPrimaryColor = Color.FromArgb(41, 47, 54);
        private static readonly Color TextSecondaryColor = Color.FromArgb(73, 88, 103);
        private static readonly Color TextMutedColor = Color.FromArgb(130, 140, 150);
        private static readonly Color AccentColor = Color.FromArgb(87, 115, 153);   // deep green
        private static readonly Color AccentAmber = Color.FromArgb(219, 142, 60);   // edit
        private static readonly Color AccentRed = Color.FromArgb(255, 102, 94);  // delete
        private static readonly Color InputBackgroundColor = Color.FromArgb(247, 247, 255);
        private static readonly Color TagColor = Color.FromArgb(189, 213, 234);
        private static readonly Color TagTextColor = Color.FromArgb(41, 47, 54);
        private static readonly Color AvailableBg = Color.FromArgb(229, 242, 201);
        private static readonly Color AvailableText = Color.FromArgb(41, 47, 54);
        private static readonly Color UnavailableBg = Color.FromArgb(255, 230, 230);
        private static readonly Color UnavailableText = Color.FromArgb(255, 102, 94);

        private static readonly Color[] CoverPalette =
        {
            Color.FromArgb(37, 99, 235),  // blue
            Color.FromArgb(124, 58, 237), // violet
            Color.FromArgb(13, 148, 136), // teal
            Color.FromArgb(190, 24, 93),  // rose
            Color.FromArgb(180, 95, 6),   // amber
            Color.FromArgb(21, 128, 61),  // green
        };

        public BooksControl()
        {
            BackColor = BackgroundColor;
            Dock = DockStyle.Fill;
            Padding = new Padding(32, 24, 32, 24);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = BackgroundColor
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            root.Controls.Add(BuildHeader(), 0, 0);
            root.Controls.Add(BuildToolbar(), 0, 1);
            root.Controls.Add(BuildGrid(), 0, 2);

            Controls.Add(root);

            Load += (_, _) => LoadBooks();
        }

        // ======================================================================
        //  Layout builders
        // ======================================================================

        private Control BuildHeader()
        {
            var panel = new Panel { Dock = DockStyle.Top, BackColor = BackgroundColor, Margin = new Padding(0, 0, 0, 8) };

            var lblTitle = new Label
            {
                Text = "Books",
                Font = new Font("Georgia", 32, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            lblSubtitle = new Label
            {
                Text = "Loading…",
                Font = new Font("Segoe UI", 13),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(2, lblTitle.Bottom + 4)
            };

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblSubtitle);

            // Size the panel to fit its actual content — a hardcoded pixel height
            // is what clipped the subtitle before, since font metrics vary by
            // machine/DPI and a guessed constant can't account for that.
            panel.Height = lblSubtitle.Bottom + 4;
            return panel;
        }

        private Control BuildToolbar()
        {
            var panel = new Panel { Dock = DockStyle.Top, BackColor = BackgroundColor, Margin = new Padding(0, 0, 0, 24) };

            searchBox = BuildSearchBar();
            panel.Controls.Add(searchBox);

            // Buttons live in their own auto-sized flow panel, anchored to the top-right
            // corner and repositioned manually on resize. (Dock = Right previously forced
            // this panel to exactly match the toolbar's height, which is what clipped the
            // buttons — a docked child can never be taller than its parent.)
            var buttonsFlow = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = BackgroundColor,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            var btnAdd = CreateActionButton("➕  Add Book", AccentColor, Color.White, (_, _) => AddBook());
            var btnEdit = CreateActionButton("✏️  Edit", AccentAmber, Color.White, (_, _) => EditBook());
            var btnDelete = CreateActionButton("🗑️  Delete", AccentRed, Color.White, (_, _) => DeleteBook());
            var btnRefresh = CreateOutlineButton("🔄  Refresh", (_, _) => LoadBooks(txtSearch.Text.Trim()));
            foreach (var b in new[] { btnAdd, btnEdit, btnDelete, btnRefresh })
            {
                b.Margin = new Padding(10, 0, 0, 0);
            }
            buttonsFlow.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });
            panel.Controls.Add(buttonsFlow);

            // The panel must be at least as tall as its tallest child (whichever of the
            // search box or the button row ends up taller) — never a guessed constant.
            panel.Height = Math.Max(searchBox.Height, buttonsFlow.Height);

            void Reflow()
            {
                buttonsFlow.Location = new Point(panel.Width - buttonsFlow.Width, (panel.Height - buttonsFlow.Height) / 2);
                searchBox.Location = new Point(0, (panel.Height - searchBox.Height) / 2);
                searchBox.Width = Math.Max(220, buttonsFlow.Left - 16);
            }
            panel.Resize += (_, _) => Reflow();
            Reflow();

            return panel;
        }

        private Panel BuildSearchBar()
        {
            var box = new Panel { Size = new Size(420, 48), BackColor = InputBackgroundColor };

            var lblIcon = new Label
            {
                Text = "🔍",
                Size = new Size(36, 48),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = TextMutedColor,
                BackColor = Color.Transparent,
                Location = new Point(6, 0),
                Font = new Font("Segoe UI", 11)
            };

            txtSearch = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 12),
                BackColor = InputBackgroundColor,
                ForeColor = TextPrimaryColor,
                Location = new Point(42, 14),
                Width = box.Width - 42 - 34,
                PlaceholderText = "Search by title, author or ISBN…"
            };
            txtSearch.TextChanged += (_, _) =>
            {
                lblClearSearch.Visible = txtSearch.Text.Length > 0;
                LoadBooks(txtSearch.Text.Trim());
            };

            lblClearSearch = new Label
            {
                Text = "✕",
                Size = new Size(34, 48),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = TextMutedColor,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Visible = false,
                Font = new Font("Segoe UI", 10)
            };
            lblClearSearch.Click += (_, _) => txtSearch.Clear();
            lblClearSearch.MouseEnter += (_, _) => lblClearSearch.ForeColor = TextPrimaryColor;
            lblClearSearch.MouseLeave += (_, _) => lblClearSearch.ForeColor = TextMutedColor;

            void Reflow()
            {
                txtSearch.Width = Math.Max(60, box.Width - 42 - 34);
                lblClearSearch.Location = new Point(box.Width - 34, 0);
            }
            box.Resize += (_, _) => Reflow();
            Reflow();

            var borderColor = CardBorderColor;
            box.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(new Rectangle(0, 0, box.Width - 1, box.Height - 1), box.Height / 2);
                using var fill = new SolidBrush(InputBackgroundColor);
                g.FillPath(fill, path);
                using var pen = new Pen(borderColor, 1.6f);
                g.DrawPath(pen, path);
                box.Region = new Region(path);
            };
            txtSearch.Enter += (_, _) => { borderColor = AccentColor; box.Invalidate(); };
            txtSearch.Leave += (_, _) => { borderColor = CardBorderColor; box.Invalidate(); };

            box.Controls.Add(txtSearch);
            box.Controls.Add(lblIcon);
            box.Controls.Add(lblClearSearch);
            return box;
        }

        private Control BuildGrid()
        {
            var container = new Panel { Dock = DockStyle.Fill, BackColor = BackgroundColor };

            flpBooks = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                BackColor = BackgroundColor,
                Padding = new Padding(0, 4, 0, 24)
            };

            lblEmptyState = new Label
            {
                Text = "📭   No books found.\nTry a different search, or add a new book.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = TextSecondaryColor,
                Font = new Font("Segoe UI", 13),
                BackColor = BackgroundColor,
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

        private void AddBook()
        {
            using var editor = new BookEditorForm();
            if (editor.ShowDialog(FindForm()) == DialogResult.OK)
            {
                bookService.AddBook(editor.Book);
                LoadBooks(txtSearch.Text.Trim());
            }
        }

        private void EditBook()
        {
            if (selectedBook == null)
            {
                MessageBox.Show("Please select a book first.", "Books", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var editor = new BookEditorForm(selectedBook);
            if (editor.ShowDialog(FindForm()) == DialogResult.OK)
            {
                bookService.UpdateBook(editor.Book);
                LoadBooks(txtSearch.Text.Trim());
            }
        }

        private void DeleteBook()
        {
            if (selectedBook == null)
            {
                MessageBox.Show("Please select a book first.", "Books", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Delete '{selectedBook.Title}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            bookService.DeleteBook(selectedBook.BookID);
            LoadBooks(txtSearch.Text.Trim());
        }

        // ======================================================================
        //  Card — surfaces every field on the Book model
        // ======================================================================

        private Panel CreateBookCard(Book book)
        {
            var card = new Panel
            {
                Size = new Size(460, 200),
                Margin = new Padding(0, 0, 24, 24),
                BackColor = CardBackgroundColor,
                Cursor = Cursors.Hand
            };

            bool hovering = false;

            card.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 14);
                using var fill = new SolidBrush(CardBackgroundColor);
                g.FillPath(fill, path);
                bool isSelected = ReferenceEquals(selectedCard, card);
                var borderColor = isSelected ? AccentColor : (hovering ? CardBorderHoverColor : CardBorderColor);
                using var pen = new Pen(borderColor, isSelected ? 2f : 1.2f);
                g.DrawPath(pen, path);
                card.Region = new Region(path);
            };

            // --- Cover -------------------------------------------------------
            var cover = new Panel { Size = new Size(100, 144), Location = new Point(20, 24) };
            var coverColor = CoverPalette[Math.Abs((book.Category ?? book.Title ?? "").GetHashCode()) % CoverPalette.Length];

            if (!string.IsNullOrWhiteSpace(book.CoverImagePath) && System.IO.File.Exists(book.CoverImagePath))
            {
                var pic = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
                try { pic.Image = Image.FromFile(book.CoverImagePath); } catch { /* fall through to placeholder */ }
                cover.Controls.Add(pic);
            }
            else
            {
                cover.BackColor = coverColor;
                cover.Controls.Add(new Label
                {
                    Text = "📘",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI Emoji", 26),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent
                });
            }
            RoundControl(cover, 8);

            // --- Text column ---------------------------------------------------
            int textLeft = cover.Right + 18;
            int badgeWidth = 64;
            int titleWidth = card.Width - textLeft - 20 - badgeWidth - 8;
            int fullTextWidth = card.Width - textLeft - 20;

            var textPanel = new FlowLayoutPanel
            {
                Location = new Point(textLeft, 22),
                Size = new Size(fullTextWidth, card.Height - 32),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            var lblTitle = new Label
            {
                Text = book.Title,
                Font = new Font("Segoe UI Semibold", 13.5f, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                MaximumSize = new Size(titleWidth, 0),
                AutoSize = true,
                AutoEllipsis = true,
                Margin = new Padding(0, 0, 0, 4)
            };
            cardToolTip.SetToolTip(lblTitle, book.Title);

            var lblAuthorYear = new Label
            {
                Text = $"{book.Author} · {book.YearPublished}",
                Font = new Font("Segoe UI", 10f),
                ForeColor = TextSecondaryColor,
                MaximumSize = new Size(fullTextWidth, 0),
                AutoSize = true,
                AutoEllipsis = true,
                Margin = new Padding(0, 0, 0, 6)
            };

            var lblIsbnPublisher = new Label
            {
                Text = $"ISBN {book.ISBN}" + (string.IsNullOrWhiteSpace(book.Publisher) ? "" : $"  ·  {book.Publisher}"),
                Font = new Font("Segoe UI", 9f),
                ForeColor = TextMutedColor,
                MaximumSize = new Size(fullTextWidth, 0),
                AutoSize = true,
                AutoEllipsis = true,
                Margin = new Padding(0, 0, 0, 2)
            };
            cardToolTip.SetToolTip(lblIsbnPublisher, lblIsbnPublisher.Text);

            var lblShelf = new Label
            {
                Text = string.IsNullOrWhiteSpace(book.ShelfLocation) ? "Shelf not set" : $"📍 Shelf {book.ShelfLocation}",
                Font = new Font("Segoe UI", 9f),
                ForeColor = TextMutedColor,
                MaximumSize = new Size(fullTextWidth, 0),
                AutoSize = true,
                AutoEllipsis = true,
                Margin = new Padding(0, 0, 0, 10)
            };

            var chip = CreateChip(string.IsNullOrWhiteSpace(book.Category) ? "Uncategorized" : book.Category, fullTextWidth - 4);
            chip.Margin = new Padding(0);

            textPanel.Controls.AddRange(new Control[] { lblTitle, lblAuthorYear, lblIsbnPublisher, lblShelf, chip });

            // --- Availability badge (top-right) --------------------------------
            bool available = book.Available > 0;
            var badge = new Label
            {
                Text = $"{book.Available}/{book.Quantity}",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = available ? AvailableText : UnavailableText,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = available ? AvailableBg : UnavailableBg,
                Size = new Size(badgeWidth, 26)
            };
            badge.Location = new Point(card.Width - 20 - badge.Width, 20);
            RoundControl(badge, 13);
            cardToolTip.SetToolTip(badge, available ? $"{book.Available} of {book.Quantity} copies available" : "No copies available");

            card.Controls.AddRange(new Control[] { cover, textPanel, badge });
            badge.BringToFront(); // Fix: ensure the badge renders on top of the textPanel to avoid being erased by Fake Transparency

            // --- Selection / interaction ----------------------------------------
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
            foreach (Control c in new Control[] { card, cover, textPanel, lblTitle, lblAuthorYear, lblIsbnPublisher, lblShelf, chip })
            {
                WireInteractions(c);
            }

            return card;
        }

        private Label CreateChip(string text, int maxWidth)
        {
            maxWidth = Math.Max(50, maxWidth);
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = TagTextColor,
                BackColor = TagColor,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = true,
                Padding = new Padding(8, 0, 8, 0)
            };

            int measured = TextRenderer.MeasureText(text, lbl.Font).Width;
            int width = Math.Min(maxWidth, measured + 32);
            lbl.Size = new Size(Math.Max(50, width), 26);

            RoundControl(lbl, 13);
            return lbl;
        }

        // ======================================================================
        //  Shared button + rounding helpers
        // ======================================================================

        private static Button CreateActionButton(string text, Color backColor, Color foreColor, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(16, 8, 16, 8),
                Height = 44,
                BackColor = backColor,
                ForeColor = foreColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            btn.MouseEnter += (_, _) => btn.BackColor = ControlPaint.Light(backColor, 0.15f);
            btn.MouseLeave += (_, _) => btn.BackColor = backColor;
            RoundControl(btn, 10);
            return btn;
        }

        private static Button CreateOutlineButton(string text, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(16, 8, 16, 8),
                Height = 44,
                BackColor = CardBackgroundColor,
                ForeColor = TextSecondaryColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = CardBorderColor;
            btn.Click += onClick;
            btn.MouseEnter += (_, _) => { btn.BackColor = InputBackgroundColor; btn.ForeColor = TextPrimaryColor; };
            btn.MouseLeave += (_, _) => { btn.BackColor = CardBackgroundColor; btn.ForeColor = TextSecondaryColor; };
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
        //  Add / Edit dialog — every field on the Book model
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
            private readonly PictureBox picCover = new() { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = InputBackgroundColor };
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
                ClientSize = new Size(460, 490);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                StartPosition = FormStartPosition.CenterParent;
                Padding = new Padding(20, 16, 20, 16);
                BackColor = BackgroundColor;
                ForeColor = TextPrimaryColor;

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
                    BackColor = BackgroundColor
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
                        ForeColor = TextSecondaryColor,
                        TextAlign = ContentAlignment.MiddleLeft
                    }, 0, i);
                    layout.Controls.Add(fields[i].Input, 1, i);
                }

                // Cover row
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
                layout.Controls.Add(new Label
                {
                    Text = "Cover",
                    Dock = DockStyle.Fill,
                    ForeColor = TextSecondaryColor,
                    TextAlign = ContentAlignment.MiddleLeft
                }, 0, fields.Length);

                var coverRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
                coverRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));
                coverRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                var coverHolder = new Panel { Size = new Size(76, 96), BackColor = InputBackgroundColor };
                coverHolder.Controls.Add(picCover);
                RoundControl(coverHolder, 8);
                var btnBrowseCover = CreateOutlineButton("🖼️  Browse…", (_, _) => BrowseForCover());
                btnBrowseCover.Anchor = AnchorStyles.Left;
                coverRow.Controls.Add(coverHolder, 0, 0);
                coverRow.Controls.Add(btnBrowseCover, 1, 0);
                layout.Controls.Add(coverRow, 1, fields.Length);

                // Button row spans both columns
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
                var buttonPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    WrapContents = false,
                    Padding = new Padding(0, 10, 0, 0)
                };
                var btnCancel = CreateOutlineButton("Cancel", (_, _) => DialogResult = DialogResult.Cancel);
                var btnSave = CreateActionButton("Save", AccentColor, Color.White, (_, _) => SaveBook());
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
                input.ForeColor = TextPrimaryColor;
                input.BackColor = InputBackgroundColor;
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