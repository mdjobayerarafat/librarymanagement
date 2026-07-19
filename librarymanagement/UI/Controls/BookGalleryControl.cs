using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace LibraryManagementSystem.UI.Controls
{
    public class BookGalleryControl : UserControl
    {
        private readonly BookService _bookService = new();
        private readonly FlowLayoutPanel _galleryPanel;
        private readonly ComboBox _cboCategory;
        private readonly TextBox _txtSearch;
        private readonly Button _btnRefresh;
        private List<Book> _allBooks = new();
        private List<Book>? _currentBooks;
        private static readonly Color BackgroundColor = Color.FromArgb(247, 247, 255);
        private static readonly Color CardBackgroundColor = Color.White;
        private static readonly Color TextPrimaryColor = Color.FromArgb(41, 47, 54);
        private static readonly Color TextSecondaryColor = Color.FromArgb(73, 88, 103);
        private static readonly Color AccentColor = Color.FromArgb(87, 115, 153);
        private static readonly Color InputBackgroundColor = Color.FromArgb(247, 247, 255);

        public BookGalleryControl()
        {
            BackColor = BackgroundColor;
            Dock = DockStyle.Fill;
            Padding = new Padding(32, 24, 32, 24);

            // Title Section
            var lblTitle = new Label
            {
                Text = "Book Gallery",
                Font = new Font("Georgia", 36, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            // Filter Panel
            var filterPanel = new Panel
            {
                Location = new Point(0, 80),
                Size = new Size(ClientSize.Width - 64, 56),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _txtSearch = CreateStyledTextBox("Search by title, author...", new Point(0, 4), 300, filterPanel);
            _txtSearch.TextChanged += (_, _) => FilterBooks();

            var lblCategory = new Label
            {
                Text = "Category:",
                Font = new Font("Segoe UI", 11),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(332, 16)
            };

            _cboCategory = new ComboBox
            {
                Width = 200,
                Height = 40,
                Font = new Font("Segoe UI", 11),
                BackColor = InputBackgroundColor,
                ForeColor = TextPrimaryColor,
                Location = new Point(412, 8),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            _cboCategory.SelectedIndexChanged += (_, _) => FilterBooks();

            // Refresh button
            _btnRefresh = CreateStyledButton("Refresh", Color.White, TextPrimaryColor, async (_, _) => await RefreshBooksAsync());
            _btnRefresh.Location = new Point(640, 4);

            filterPanel.Controls.AddRange(new Control[] { lblCategory, _cboCategory, _btnRefresh });

            // Gallery Panel
            _galleryPanel = new FlowLayoutPanel
            {
                Location = new Point(0, 156),
                Size = new Size(ClientSize.Width - 64, ClientSize.Height - 180),
                BackColor = BackgroundColor,
                Padding = new Padding(10),
                AutoScroll = true,
                WrapContents = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            Controls.AddRange(new Control[] { lblTitle, filterPanel, _galleryPanel });

            Resize += (_, _) =>
            {
                filterPanel.Size = new Size(ClientSize.Width - 64, 56);
                _galleryPanel.Size = new Size(ClientSize.Width - 64, ClientSize.Height - 180);
                UpdateGalleryLayout();
            };

            Load += (_, _) => LoadBooks();
        }

        private TextBox CreateStyledTextBox(string placeholder, Point location, int width, Panel parent)
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
            parent.Controls.Add(txtBox);
            return txtBox;
        }

        private Button CreateStyledButton(string text, Color backColor, Color foreColor, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(120, 48),
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
            btn.MouseEnter += (s, e) => btn.BackColor = backColor == Color.White ? InputBackgroundColor : ControlPaint.Light(backColor, 0.1f);
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;
            return btn;
        }

        private void LoadBooks()
        {
            _allBooks = _bookService.GetAllBooks();
            LoadCategories();
            RenderBooks(_allBooks);
        }

        public async Task RefreshBooksAsync()
        {
            try
            {
                _btnRefresh.Enabled = false;
                var originalText = _btnRefresh.Text;
                _btnRefresh.Text = "Refreshing...";

                // Load books on background thread to avoid UI freeze
                await Task.Run(() => { _allBooks = _bookService.GetAllBooks(); });

                // Update UI
                LoadCategories();
                RenderBooks(_allBooks);

                _btnRefresh.Text = originalText;
            }
            catch
            {
                // ignore for now
            }
            finally
            {
                _btnRefresh.Enabled = true;
            }
        }

        private void LoadCategories()
        {
            var categories = _allBooks.Select(b => b.Category).Distinct().OrderBy(c => c).ToList();
            categories.Insert(0, "All Categories");
            
            _cboCategory.DataSource = categories;
        }

        private void FilterBooks()
        {
            var searchText = _txtSearch.Text.Trim().ToLower();
            var selectedCategory = _cboCategory.SelectedItem?.ToString() ?? "All Categories";

            var filtered = _allBooks.Where(b =>
                (selectedCategory == "All Categories" || b.Category == selectedCategory) &&
                (string.IsNullOrWhiteSpace(searchText) ||
                 b.Title.ToLower().Contains(searchText) ||
                 b.Author.ToLower().Contains(searchText) ||
                 b.ISBN.ToLower().Contains(searchText))
            ).ToList();

            RenderBooks(filtered);
        }

        private void RenderBooks(List<Book> books)
        {
            _currentBooks = books;
            _galleryPanel.Controls.Clear();
            foreach (var book in books)
            {
                _galleryPanel.Controls.Add(CreateBookCard(book));
            }
            UpdateGalleryLayout();
        }

        private void UpdateGalleryLayout()
        {
            if (_currentBooks is null) return;

            // Calculate responsive card size based on available width
            int availableWidth = _galleryPanel.ClientSize.Width - 40; // Account for padding and scrollbar
            int cardsPerRow = Math.Max(3, availableWidth / 220); // At least 3, use 220px as base
            int cardWidth = Math.Max(160, availableWidth / cardsPerRow - 40); // Min 160px
            int cardHeight = (int)(cardWidth * 1.6); // Proportional height (aspect ratio ~ 0.625)

            foreach (Control control in _galleryPanel.Controls)
            {
                if (control is Panel card)
                {
                    card.Size = new Size(cardWidth, cardHeight);
                    // Update child elements
                    foreach (Control child in card.Controls)
                    {
                        if (child is PictureBox picCover)
                        {
                            int picWidth = (int)(cardWidth * 0.8);
                            int picHeight = (int)(cardHeight * 0.7);
                            picCover.Size = new Size(picWidth, picHeight);
                            picCover.Location = new Point((cardWidth - picWidth) / 2, 10);
                        }
                        else if (child is Label lbl)
                        {
                            int lblWidth = (int)(cardWidth * 0.85);
                            if (lbl.Name == "lblTitle")
                            {
                                lbl.Size = new Size(lblWidth, (int)(cardHeight * 0.12));
                                lbl.Font = new Font("Georgia", Math.Max(8, cardWidth / 18), FontStyle.Bold);
                                lbl.Location = new Point((cardWidth - lblWidth) / 2, (int)(cardHeight * 0.72));
                            }
                            else if (lbl.Name == "lblAuthor")
                            {
                                lbl.Size = new Size(lblWidth, (int)(cardHeight * 0.08));
                                lbl.Font = new Font("Segoe UI", Math.Max(7, cardWidth / 25));
                                lbl.Location = new Point((cardWidth - lblWidth) / 2, (int)(cardHeight * 0.84));
                            }
                            else if (lbl.Name == "lblAvailable")
                            {
                                lbl.Size = new Size(lblWidth, (int)(cardHeight * 0.08));
                                lbl.Font = new Font("Segoe UI", Math.Max(7, cardWidth / 25), FontStyle.Bold);
                                lbl.Location = new Point((cardWidth - lblWidth) / 2, (int)(cardHeight * 0.91));
                            }
                        }
                    }
                }
            }
        }

        private Panel CreateBookCard(Book book)
        {
            var card = new Panel
            {
                BackColor = CardBackgroundColor,
                Margin = new Padding(10),
                Cursor = Cursors.Hand
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

            // Cover Image
            var picCover = new PictureBox
            {
                Name = "picCover",
                BackColor = InputBackgroundColor,
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
                    // Use placeholder if image load fails
                }
            }

            // Title
            var lblTitle = new Label
            {
                Name = "lblTitle",
                Text = book.Title.Length > 30 ? book.Title.Substring(0, 30) + "..." : book.Title,
                ForeColor = TextPrimaryColor,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Author
            var lblAuthor = new Label
            {
                Name = "lblAuthor",
                Text = book.Author,
                ForeColor = TextSecondaryColor,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Available/Total
            var lblAvailable = new Label
            {
                Name = "lblAvailable",
                Text = $"{book.Available}/{book.Quantity} Available",
                ForeColor = book.Available > 0 ? AccentColor : Color.FromArgb(220, 38, 38),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter
            };

            card.Controls.AddRange(new Control[] { picCover, lblTitle, lblAuthor, lblAvailable });
            return card;
        }
    }
}
