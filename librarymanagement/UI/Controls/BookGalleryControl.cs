using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        private List<Book> _allBooks = new();
        private List<Book>? _currentBooks;

        public BookGalleryControl()
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
                Text = "📚 Book Gallery",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 18)
            };

            titlePanel.Controls.Add(lblTitle);

            // Filter Panel
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(20)
            };

            _txtSearch = new TextBox
            {
                PlaceholderText = "🔍 Search by title, author...",
                Width = 300,
                Height = 36,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                Location = new Point(20, 17)
            };
            _txtSearch.TextChanged += (_, _) => FilterBooks();

            var lblCategory = new Label
            {
                Text = "Category:",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(340, 22)
            };

            _cboCategory = new ComboBox
            {
                Width = 200,
                Height = 36,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                Location = new Point(420, 17),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cboCategory.SelectedIndexChanged += (_, _) => FilterBooks();

            filterPanel.Controls.AddRange(new Control[] { _txtSearch, lblCategory, _cboCategory });

            // Gallery Panel
            _galleryPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(10),
                AutoScroll = true,
                WrapContents = true
            };

            Controls.Add(_galleryPanel);
            Controls.Add(filterPanel);
            Controls.Add(titlePanel);

            // Handle resize
            Resize += (_, _) => UpdateGalleryLayout();

            Load += (_, _) => LoadBooks();
        }

        private void LoadBooks()
        {
            _allBooks = _bookService.GetAllBooks();
            LoadCategories();
            RenderBooks(_allBooks);
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
                                lbl.Font = new Font("Segoe UI", Math.Max(8, cardWidth / 18), FontStyle.Bold);
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
                                lbl.Font = new Font("Segoe UI", Math.Max(7, cardWidth / 25));
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
                BackColor = Color.FromArgb(30, 41, 59),
                Margin = new Padding(10),
                Cursor = Cursors.Hand
            };

            // Cover Image
            var picCover = new PictureBox
            {
                Name = "picCover",
                BackColor = Color.FromArgb(51, 65, 85),
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
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Author
            var lblAuthor = new Label
            {
                Name = "lblAuthor",
                Text = book.Author,
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Available/Total
            var lblAvailable = new Label
            {
                Name = "lblAvailable",
                Text = $"{book.Available}/{book.Quantity} Available",
                ForeColor = book.Available > 0 ? Color.FromArgb(16, 185, 129) : Color.FromArgb(239, 68, 68),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter
            };

            card.Controls.AddRange(new Control[] { picCover, lblTitle, lblAuthor, lblAvailable });
            return card;
        }
    }
}
