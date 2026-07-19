﻿using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace LibraryManagementSystem.UI.Controls
{
    public class ActiveLoansControl : UserControl
    {
        private readonly TransactionService _transactionService = new();
        private readonly DataGridView _dgvActiveLoans;
        private readonly TextBox _txtSearch;
        private static readonly Color BackgroundColor = Color.FromArgb(247, 247, 255);
        private static readonly Color CardBackgroundColor = Color.White;
        private static readonly Color TextPrimaryColor = Color.FromArgb(41, 47, 54);
        private static readonly Color TextSecondaryColor = Color.FromArgb(73, 88, 103);
        private static readonly Color AccentColor = Color.FromArgb(87, 115, 153);
        private static readonly Color InputBackgroundColor = Color.FromArgb(247, 247, 255);
        private static readonly Color AccentLightColor = Color.FromArgb(189, 213, 234);

        public ActiveLoansControl()
        {
            BackColor = BackgroundColor;
            Dock = DockStyle.Fill;
            Padding = new Padding(32, 24, 32, 24);

            // Title Section
            var lblTitle = new Label
            {
                Text = "Active Books",
                Font = new Font("Georgia", 36, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            // Toolbar Panel
            var toolbarPanel = new Panel
            {
                Location = new Point(0, 80),
                Size = new Size(ClientSize.Width - 64, 56),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _txtSearch = CreateStyledTextBox("Search by book, member...", new Point(0, 4), 400, toolbarPanel);
            _txtSearch.TextChanged += (_, _) => LoadActiveLoans();

            var btnRefresh = CreateStyledButton("🔄 Refresh", Color.White, TextPrimaryColor, (_, _) => LoadActiveLoans());
            btnRefresh.Location = new Point(432, 4);

            toolbarPanel.Controls.Add(btnRefresh);

            // Data Grid Container
            var gridContainer = new Panel
            {
                Location = new Point(0, 156),
                Size = new Size(ClientSize.Width - 64, ClientSize.Height - 180),
                BackColor = CardBackgroundColor,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(2)
            };
            gridContainer.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                int cr = 12;
                path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                path.AddArc(gridContainer.Width - 1 - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                path.AddArc(gridContainer.Width - 1 - cr * 2, gridContainer.Height - 1 - cr * 2, cr * 2, cr * 2, 0, 90);
                path.AddArc(0, gridContainer.Height - 1 - cr * 2, cr * 2, cr * 2, 90, 90);
                path.CloseAllFigures();
                
                using var borderPen = new Pen(Color.FromArgb(189, 213, 234), 2f);
                g.DrawPath(borderPen, path);
            };

            // Data Grid
            _dgvActiveLoans = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = CardBackgroundColor,
                BorderStyle = BorderStyle.None,
                GridColor = InputBackgroundColor,
                Font = new Font("Segoe UI", 9),
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                AllowUserToResizeColumns = true,
                ColumnHeadersDefaultCellStyle =
                {
                    BackColor = InputBackgroundColor,
                    ForeColor = TextPrimaryColor,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    WrapMode = DataGridViewTriState.True
                },
                DefaultCellStyle =
                {
                    BackColor = CardBackgroundColor,
                    ForeColor = TextPrimaryColor,
                    SelectionBackColor = Color.FromArgb(189, 213, 234),
                    SelectionForeColor = TextPrimaryColor,
                    Padding = new Padding(8, 4, 8, 4),
                    WrapMode = DataGridViewTriState.True
                },
                AlternatingRowsDefaultCellStyle =
                {
                    BackColor = Color.FromArgb(247, 247, 255),
                    ForeColor = TextPrimaryColor,
                    WrapMode = DataGridViewTriState.True
                },
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
            };

            gridContainer.Controls.Add(_dgvActiveLoans);

            Controls.AddRange(new Control[] { lblTitle, toolbarPanel, gridContainer });

            Resize += (_, _) =>
            {
                toolbarPanel.Size = new Size(ClientSize.Width - 64, 56);
                gridContainer.Size = new Size(ClientSize.Width - 64, ClientSize.Height - 180);
            };

            Load += (_, _) => LoadActiveLoans();
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
            btn.MouseEnter += (s, e) => btn.BackColor = backColor == Color.White ? InputBackgroundColor : ControlPaint.Light(backColor, 0.1f);
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;
            return btn;
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
