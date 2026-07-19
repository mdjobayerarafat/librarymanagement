using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace LibraryManagementSystem.UI.Forms
{
    public class MembersForm : Form
    {
        private readonly MemberService memberService = new();
        private readonly TextBox txtSearch;
        private readonly DataGridView dgvMembers;
        private readonly Panel headerPanel;
        private readonly Label lblSummary;
        private IReadOnlyList<Member> currentMembers = Array.Empty<Member>();

        // ---------- Palette ----------
        private static readonly Color BgColor = Color.FromArgb(246, 247, 251);
        private static readonly Color CardColor = Color.White;
        private static readonly Color BorderColor = Color.FromArgb(189, 213, 234);
        private static readonly Color PrimaryColor = Color.FromArgb(79, 70, 229);
        private static readonly Color PrimaryHover = Color.FromArgb(67, 56, 202);
        private static readonly Color DangerColor = Color.FromArgb(220, 38, 38);
        private static readonly Color DangerBg = Color.FromArgb(254, 226, 226);
        private static readonly Color DangerHoverBg = Color.FromArgb(252, 205, 205);
        private static readonly Color SuccessColor = Color.FromArgb(22, 163, 74);
        private static readonly Color SuccessBg = Color.FromArgb(220, 252, 231);
        private static readonly Color TextPrimary = Color.FromArgb(17, 24, 39);
        private static readonly Color TextSecondary = Color.FromArgb(107, 114, 128);
        private static readonly Color NeutralHover = Color.FromArgb(243, 244, 246);

        public MembersForm()
        {
            Text = "Manage Members";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BackColor = BgColor;
            Font = new Font("Segoe UI", 9.5f);

            // ================= Header =================
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 84,
                BackColor = CardColor
            };
            headerPanel.Paint += (_, e) =>
            {
                using var pen = new Pen(BorderColor, 1);
                e.Graphics.DrawLine(pen, 0, headerPanel.Height - 1, headerPanel.Width, headerPanel.Height - 1);
            };

            var lblTitle = new Label
            {
                Text = "Members",
                Font = new Font("Segoe UI Semibold", 18f, FontStyle.Regular),
                ForeColor = TextPrimary,
                AutoSize = true,
                Location = new Point(32, 14)
            };

            var lblSubtitle = new Label
            {
                Text = "Manage registered library members",
                Font = new Font("Segoe UI", 10f),
                ForeColor = TextSecondary,
                AutoSize = true,
                Location = new Point(32, 50)
            };

            lblSummary = new Label
            {
                Font = new Font("Segoe UI", 10f),
                ForeColor = TextSecondary,
                AutoSize = true
            };
            headerPanel.Resize += (_, _) => PositionSummaryLabel();

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            headerPanel.Controls.Add(lblSummary);

            // ================= Toolbar =================
            var toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 74,
                BackColor = BgColor
            };

            var searchBox = new RoundedTextBox
            {
                Height = 40,
                Width = 360,
                Location = new Point(32, 17),
                CornerRadius = 20,
                IconChar = '\uE721', // Segoe MDL2 Assets: Search
                BackColor = BgColor
            };
            searchBox.Input.PlaceholderText = "Search by name, email, or phone";
            txtSearch = searchBox.Input;
            txtSearch.TextChanged += (_, _) => LoadMembers(txtSearch.Text.Trim());

            var btnRefresh = CreateButton("Refresh", 104, NeutralHover, BorderColor, TextPrimary, BgColor);
            btnRefresh.Click += (_, _) => LoadMembers(txtSearch.Text.Trim());

            var btnEdit = CreateButton("Edit", 92, NeutralHover, BorderColor, TextPrimary, BgColor);
            btnEdit.Click += (_, _) => EditMember();

            var btnDelete = CreateButton("Delete", 100, DangerHoverBg, Color.Transparent, DangerColor, BgColor);
            btnDelete.BaseColor = DangerBg;
            btnDelete.Click += (_, _) => DeleteMember();

            var btnAdd = CreateButton("+  Add Member", 150, PrimaryHover, Color.Transparent, Color.White, BgColor);
            btnAdd.BaseColor = PrimaryColor;
            btnAdd.Click += (_, _) => AddMember();

            void RepositionRightButtons()
            {
                int right = toolbarPanel.Width - 32;
                btnAdd.Location = new Point(right - btnAdd.Width, 17);
                btnDelete.Location = new Point(btnAdd.Left - 10 - btnDelete.Width, 17);
                btnEdit.Location = new Point(btnDelete.Left - 10 - btnEdit.Width, 17);
                btnRefresh.Location = new Point(btnEdit.Left - 10 - btnRefresh.Width, 17);
            }
            toolbarPanel.Resize += (_, _) => RepositionRightButtons();

            toolbarPanel.Controls.AddRange(new Control[] { searchBox, btnAdd, btnDelete, btnEdit, btnRefresh });

            // ================= Data Grid =================
            var gridContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(32, 4, 32, 32),
                BackColor = BgColor
            };

            dgvMembers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = CardColor,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.None,
                GridColor = CardColor,
                Font = new Font("Segoe UI", 9.5f),
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 46,
                AllowUserToResizeRows = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            dgvMembers.RowTemplate.Height = 48;
            dgvMembers.ColumnHeadersDefaultCellStyle.BackColor = CardColor;
            dgvMembers.ColumnHeadersDefaultCellStyle.ForeColor = TextSecondary;
            dgvMembers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5f);
            dgvMembers.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvMembers.ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 0, 0, 0);
            dgvMembers.DefaultCellStyle.BackColor = CardColor;
            dgvMembers.DefaultCellStyle.ForeColor = TextPrimary;
            dgvMembers.DefaultCellStyle.SelectionBackColor = Color.FromArgb(238, 237, 253);
            dgvMembers.DefaultCellStyle.SelectionForeColor = TextPrimary;
            dgvMembers.DefaultCellStyle.Padding = new Padding(12, 4, 4, 4);
            dgvMembers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 252);
            dgvMembers.CellPainting += DgvMembers_CellPainting;

            var gridCard = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = CardColor,
                CornerRadius = 12,
                BorderColor = BorderColor
            };
            gridCard.Controls.Add(dgvMembers);
            gridContainer.Controls.Add(gridCard);

            Controls.Add(gridContainer);
            Controls.Add(toolbarPanel);
            Controls.Add(headerPanel);

            RepositionRightButtons();
            Shown += (_, _) => LoadMembers();
        }

        private static RoundedButton CreateButton(string text, int width, Color hover, Color border, Color foreColor, Color parentBackColor)
        {
            return new RoundedButton
            {
                Text = text,
                Width = width,
                Height = 40,
                BaseColor = Color.White,
                HoverColor = hover,
                BorderColorNormal = border,
                TextColorNormal = foreColor,
                BackColor = parentBackColor,
                CornerRadius = 10
            };
        }

        private void PositionSummaryLabel()
        {
            lblSummary.Location = new Point(Math.Max(32, headerPanel.Width - lblSummary.Width - 32), 40);
        }

        // ---------- Data ----------
        private void LoadMembers(string keyword = "")
        {
            try
            {
                var data = string.IsNullOrWhiteSpace(keyword)
                    ? memberService.GetAllMembers()
                    : memberService.SearchMembers(keyword);

                currentMembers = data as IReadOnlyList<Member> ?? data.ToList();
                dgvMembers.DataSource = currentMembers;
                ConfigureColumns();
                UpdateSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load members.\n\n{ex.Message}", "Members", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureColumns()
        {
            HideColumn("MemberID");
            HideColumn("IsActive");

            SetHeader("FullName", "Name");
            SetHeader("Email", "Email");
            SetHeader("Phone", "Phone");
            SetHeader("Address", "Address");
            SetHeader("MembershipDate", "Joined");
            SetHeader("Status", "Status");

            if (dgvMembers.Columns["MembershipDate"] is { } dateCol)
            {
                dateCol.DefaultCellStyle.Format = "dd MMM yyyy";
            }
        }

        private void HideColumn(string name)
        {
            if (dgvMembers.Columns[name] is { } col) col.Visible = false;
        }

        private void SetHeader(string name, string header)
        {
            if (dgvMembers.Columns[name] is { } col) col.HeaderText = header;
        }

        private void UpdateSummary()
        {
            var total = currentMembers.Count;
            var active = currentMembers.Count(m => m.IsActive);
            lblSummary.Text = $"{active} active of {total} registered";
            PositionSummaryLabel();
        }

        /// <summary>Draws the Status column as a colored pill instead of plain text.</summary>
        private void DgvMembers_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvMembers.Columns[e.ColumnIndex].Name != "Status") return;

            e.PaintBackground(e.CellBounds, true);

            var text = e.Value?.ToString() ?? string.Empty;
            if (text.Length == 0)
            {
                e.Handled = true;
                return;
            }

            bool isActive = string.Equals(text, "Active", StringComparison.OrdinalIgnoreCase);
            var pillBg = isActive ? SuccessBg : DangerBg;
            var pillText = isActive ? SuccessColor : DangerColor;

            var g = e.Graphics!;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var font = new Font("Segoe UI Semibold", 8.5f);
            var textSize = g.MeasureString(text, font);
            int pillWidth = (int)textSize.Width + 26;
            int pillHeight = 24;
            int x = e.CellBounds.X + 12;
            int y = e.CellBounds.Y + (e.CellBounds.Height - pillHeight) / 2;

            using var path = UiHelpers.RoundedRect(new Rectangle(x, y, pillWidth, pillHeight), pillHeight / 2);
            using var brush = new SolidBrush(pillBg);
            g.FillPath(brush, path);

            using var textBrush = new SolidBrush(pillText);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(text, font, textBrush, new RectangleF(x, y, pillWidth, pillHeight), sf);

            e.Handled = true;
        }

        private Member? GetSelectedMember()
        {
            return dgvMembers.CurrentRow?.DataBoundItem as Member;
        }

        private void AddMember()
        {
            using var editor = new MemberEditorForm();
            if (editor.ShowDialog(this) == DialogResult.OK)
            {
                memberService.AddMember(editor.Member);
                LoadMembers(txtSearch.Text.Trim());
            }
        }

        private void EditMember()
        {
            var selected = GetSelectedMember();
            if (selected == null)
            {
                MessageBox.Show("Please select a member first.", "Members", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var editor = new MemberEditorForm(selected);
            if (editor.ShowDialog(this) == DialogResult.OK)
            {
                memberService.UpdateMember(editor.Member);
                LoadMembers(txtSearch.Text.Trim());
            }
        }

        private void DeleteMember()
        {
            var selected = GetSelectedMember();
            if (selected == null)
            {
                MessageBox.Show("Please select a member first.", "Members", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Delete member '{selected.FullName}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            memberService.DeleteMember(selected.MemberID);
            LoadMembers(txtSearch.Text.Trim());
        }

        // ======================================================
        //  Modernized editor dialog
        // ======================================================
        private sealed class MemberEditorForm : Form
        {
            private readonly TextBox txtName = new() { Width = 336, Height = 30, Font = new Font("Segoe UI", 10f) };
            private readonly TextBox txtEmail = new() { Width = 336, Height = 30, Font = new Font("Segoe UI", 10f) };
            private readonly TextBox txtPhone = new() { Width = 336, Height = 30, Font = new Font("Segoe UI", 10f) };
            private readonly TextBox txtAddress = new() { Width = 336, Height = 30, Font = new Font("Segoe UI", 10f) };
            private readonly DateTimePicker dtMembership = new() { Width = 336, Height = 30, Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 10f) };
            private readonly CheckBox chkActive = new() { Width = 336, Text = "Active member", Checked = true, Font = new Font("Segoe UI", 10f) };

            public Member Member { get; }

            public MemberEditorForm(Member? member = null)
            {
                Member = member == null
                    ? new Member()
                    : new Member
                    {
                        MemberID = member.MemberID,
                        FullName = member.FullName,
                        Email = member.Email,
                        Phone = member.Phone,
                        Address = member.Address,
                        MembershipDate = member.MembershipDate,
                        IsActive = member.IsActive,
                        Status = member.Status
                    };

                Text = member == null ? "Add Member" : "Edit Member";
                ClientSize = new Size(432, 470);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                StartPosition = FormStartPosition.CenterParent;
                BackColor = BgColor;
                Font = new Font("Segoe UI", 9.5f);

                var headerBar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = CardColor };
                headerBar.Paint += (_, e) =>
                {
                    using var pen = new Pen(BorderColor, 1);
                    e.Graphics.DrawLine(pen, 0, headerBar.Height - 1, headerBar.Width, headerBar.Height - 1);
                };
                headerBar.Controls.Add(new Label
                {
                    Text = Text,
                    Font = new Font("Segoe UI Semibold", 13f),
                    ForeColor = TextPrimary,
                    AutoSize = true,
                    Location = new Point(24, 15)
                });

                var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 20, 24, 0), BackColor = BgColor };

                int top = 0;
                body.Controls.Add(BuildField("Full name", txtName, ref top));
                body.Controls.Add(BuildField("Email", txtEmail, ref top));
                body.Controls.Add(BuildField("Phone", txtPhone, ref top));
                body.Controls.Add(BuildField("Address", txtAddress, ref top));
                body.Controls.Add(BuildField("Membership date", dtMembership, ref top));

                chkActive.Location = new Point(0, top + 8);
                body.Controls.Add(chkActive);

                var footer = new Panel { Dock = DockStyle.Bottom, Height = 72, BackColor = CardColor };
                footer.Paint += (_, e) =>
                {
                    using var pen = new Pen(BorderColor, 1);
                    e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
                };

                var btnCancel = new RoundedButton
                {
                    Text = "Cancel",
                    Width = 92,
                    Height = 38,
                    BaseColor = Color.White,
                    HoverColor = NeutralHover,
                    BorderColorNormal = BorderColor,
                    TextColorNormal = TextPrimary,
                    BackColor = CardColor,
                    CornerRadius = 10
                };
                btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

                var btnSave = new RoundedButton
                {
                    Text = "Save",
                    Width = 100,
                    Height = 38,
                    BaseColor = PrimaryColor,
                    HoverColor = PrimaryHover,
                    BorderColorNormal = Color.Transparent,
                    TextColorNormal = Color.White,
                    BackColor = CardColor,
                    CornerRadius = 10
                };
                btnSave.Click += (_, _) => SaveMember();

                void PositionFooterButtons()
                {
                    btnSave.Location = new Point(footer.Width - 24 - btnSave.Width, 17);
                    btnCancel.Location = new Point(btnSave.Left - 10 - btnCancel.Width, 17);
                }
                footer.Resize += (_, _) => PositionFooterButtons();
                footer.Controls.Add(btnSave);
                footer.Controls.Add(btnCancel);

                Controls.Add(body);
                Controls.Add(footer);
                Controls.Add(headerBar);

                Shown += (_, _) => PositionFooterButtons();

                txtName.Text = Member.FullName;
                txtEmail.Text = Member.Email;
                txtPhone.Text = Member.Phone;
                txtAddress.Text = Member.Address;
                dtMembership.Value = Member.MembershipDate == default ? DateTime.Now : Member.MembershipDate;
                chkActive.Checked = Member.IsActive;
            }

            private static Panel BuildField(string labelText, Control input, ref int top)
            {
                var wrapper = new Panel { Location = new Point(0, top), Width = 336, Height = 62, BackColor = Color.Transparent };
                wrapper.Controls.Add(new Label
                {
                    Text = labelText,
                    Font = new Font("Segoe UI", 8.5f),
                    ForeColor = TextSecondary,
                    AutoSize = true,
                    Location = new Point(0, 0)
                });
                input.Location = new Point(0, 20);
                wrapper.Controls.Add(input);
                top += 62;
                return wrapper;
            }

            private void SaveMember()
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Member name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Member.FullName = txtName.Text.Trim();
                Member.Email = txtEmail.Text.Trim();
                Member.Phone = txtPhone.Text.Trim();
                Member.Address = txtAddress.Text.Trim();
                Member.MembershipDate = dtMembership.Value.Date;
                Member.IsActive = chkActive.Checked;
                Member.Status = chkActive.Checked ? "Active" : "Inactive";

                DialogResult = DialogResult.OK;
            }
        }
    }

    // ======================================================
    //  Reusable modern controls
    // ======================================================

    /// <summary>Flat button with rounded corners and a hover fill color.</summary>
    internal sealed class RoundedButton : Button
    {
        public int CornerRadius { get; set; } = 10;
        public Color BaseColor { get; set; } = Color.White;
        public Color HoverColor { get; set; } = Color.FromArgb(243, 244, 246);
        public Color TextColorNormal { get; set; } = Color.Black;
        public Color BorderColorNormal { get; set; } = Color.Transparent;

        private bool isHovered;

        public RoundedButton()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = new Font("Segoe UI Semibold", 9.5f);
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = UiHelpers.RoundedRect(rect, CornerRadius);

            using (var brush = new SolidBrush(isHovered ? HoverColor : BaseColor))
            {
                g.FillPath(brush, path);
            }

            if (BorderColorNormal != Color.Transparent)
            {
                using var pen = new Pen(BorderColorNormal, 1);
                g.DrawPath(pen, path);
            }

            TextRenderer.DrawText(g, Text, Font, ClientRectangle, TextColorNormal,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }

    /// <summary>Pill-shaped search box with a leading icon glyph (Segoe MDL2 Assets).</summary>
    internal sealed class RoundedTextBox : Panel
    {
        public TextBox Input { get; }
        public int CornerRadius { get; set; } = 20;
        public Color BorderColorNormal { get; set; } = Color.FromArgb(228, 230, 236);

        private readonly Label iconLabel;

        public char IconChar
        {
            get => iconLabel.Text.Length > 0 ? iconLabel.Text[0] : ' ';
            set => iconLabel.Text = value.ToString();
        }

        public RoundedTextBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            Padding = new Padding(2, 0, 14, 0);

            iconLabel = new Label
            {
                Font = new Font("Segoe MDL2 Assets", 10f),
                ForeColor = Color.FromArgb(156, 163, 175),
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 36,
                Dock = DockStyle.Left,
                BackColor = Color.Transparent
            };

            Input = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10f),
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            Controls.Add(Input);
            Controls.Add(iconLabel);
            Resize += (_, _) => UpdateRegion();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = UiHelpers.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), CornerRadius);
            using var pen = new Pen(BorderColorNormal, 1);
            e.Graphics.DrawPath(pen, path);
        }

        private void UpdateRegion()
        {
            using var path = UiHelpers.RoundedRect(new Rectangle(0, 0, Width, Height), CornerRadius);
            Region = new Region(path);
        }
    }

    /// <summary>Plain panel with rounded corners and a subtle border — used as a "card" container.</summary>
    internal sealed class RoundedPanel : Panel
    {
        public int CornerRadius { get; set; } = 12;
        public Color BorderColor { get; set; } = Color.FromArgb(228, 230, 236);

        public RoundedPanel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = UiHelpers.RoundedRect(rect, CornerRadius);
            using var pen = new Pen(BorderColor, 1);
            e.Graphics.DrawPath(pen, path);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Width <= 0 || Height <= 0) return;
            using var path = UiHelpers.RoundedRect(new Rectangle(0, 0, Width, Height), CornerRadius);
            Region = new Region(path);
        }
    }

    internal static class UiHelpers
    {
        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            if (d > bounds.Width) d = Math.Max(bounds.Width, 1);
            if (d > bounds.Height) d = Math.Max(bounds.Height, 1);

            var path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}