using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace LibraryManagementSystem.UI.Controls
{
    public class MembersControl : UserControl
    {
        private readonly MemberService memberService = new();
        private readonly TextBox txtSearch;
        private readonly FlowLayoutPanel flpMembers;
        private readonly Panel toolbarPanel;
        private readonly Label lblSummary;

        // ---------- Layout constants ----------
        private const int PadX = 32;
        private const int TitleY = 24;
        private const int SummaryY = 66;
        private const int ToolbarY = 100;
        private const int ToolbarHeight = 48;
        private const int GridY = 164;
        private const int PadBottom = 24;

        private const int CardWidth = 460;
        private const int CardHeight = 200;

        // ---------- Palette ----------
        private static readonly Color BackgroundColor = Color.FromArgb(247, 247, 255);
        private static readonly Color CardBackgroundColor = Color.White;
        private static readonly Color TextPrimaryColor = Color.FromArgb(41, 47, 54);
        private static readonly Color TextSecondaryColor = Color.FromArgb(73, 88, 103);
        private static readonly Color AccentColor = Color.FromArgb(87, 115, 153);
        private static readonly Color AccentHoverColor = Color.FromArgb(15, 64, 35);
        private static readonly Color InputBackgroundColor = Color.FromArgb(247, 247, 255);
        private static readonly Color BorderColor = Color.FromArgb(189, 213, 234);
        private static readonly Color WarningColor = Color.FromArgb(255, 102, 94);
        private static readonly Color WarningLightColor = Color.FromArgb(255, 230, 230);
        private static readonly Color WarningHoverColor = Color.FromArgb(250, 210, 210);
        private static readonly Color ActiveBadgeBg = Color.FromArgb(219, 237, 226);

        public MembersControl()
        {
            BackColor = BackgroundColor;
            Dock = DockStyle.Fill;

            // ================= Title =================
            var lblTitle = new Label
            {
                Text = "Members",
                Font = new Font("Georgia", 26, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(PadX, TitleY)
            };

            lblSummary = new Label
            {
                Font = new Font("Segoe UI", 11),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(PadX, SummaryY)
            };

            // ================= Toolbar =================
            toolbarPanel = new Panel
            {
                Location = new Point(PadX, ToolbarY),
                Size = new Size(ClientSize.Width - PadX * 2, ToolbarHeight),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var searchBox = new RoundedTextBox
            {
                Height = 44,
                Width = 360,
                Location = new Point(0, 0),
                CornerRadius = 22,
                BorderColorNormal = BorderColor,
                BackColor = BackgroundColor,
                FieldBackColor = InputBackgroundColor,
                IconChar = '\uE721'
            };
            searchBox.Input.PlaceholderText = "Search by name, email, or phone";
            searchBox.Input.ForeColor = TextPrimaryColor;
            txtSearch = searchBox.Input;
            txtSearch.TextChanged += (_, _) => LoadMembers(txtSearch.Text.Trim());

            var btnAdd = new RoundedButton
            {
                Text = "Register Member",
                Width = 200,
                Height = 44,
                BaseColor = AccentColor,
                HoverColor = AccentHoverColor,
                TextColorNormal = Color.White,
                BorderColorNormal = Color.Transparent,
                BackColor = BackgroundColor,
                CornerRadius = 10
            };
            btnAdd.Click += (_, _) => AddMember();

            void PositionToolbarButton()
            {
                btnAdd.Location = new Point(toolbarPanel.Width - btnAdd.Width, 0);
            }
            toolbarPanel.Resize += (_, _) => PositionToolbarButton();

            toolbarPanel.Controls.Add(searchBox);
            toolbarPanel.Controls.Add(btnAdd);

            // ================= Member cards =================
            flpMembers = new FlowLayoutPanel
            {
                Location = new Point(PadX, GridY),
                Size = new Size(ClientSize.Width - PadX * 2, Math.Max(0, ClientSize.Height - GridY - PadBottom)),
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            Resize += (_, _) =>
            {
                toolbarPanel.Width = ClientSize.Width - PadX * 2;
                PositionToolbarButton();
                flpMembers.Size = new Size(ClientSize.Width - PadX * 2, Math.Max(0, ClientSize.Height - GridY - PadBottom));
            };

            Controls.AddRange(new Control[] { lblTitle, lblSummary, toolbarPanel, flpMembers });

            Load += (_, _) => LoadMembers();
            PositionToolbarButton();
        }

        // ---------- Data ----------
        private void LoadMembers(string keyword = "")
        {
            try
            {
                flpMembers.Controls.Clear();
                var members = string.IsNullOrWhiteSpace(keyword)
                    ? memberService.GetAllMembers()
                    : memberService.SearchMembers(keyword);

                foreach (var member in members)
                {
                    flpMembers.Controls.Add(CreateMemberCard(member));
                }

                var total = members.Count;
                var activeCount = members.Count(m => m.IsActive);
                lblSummary.Text = $"{activeCount} active of {total} registered";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load members.\n\n{ex.Message}", "Members", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddMember()
        {
            using var editor = new MemberEditorForm();
            if (editor.ShowDialog(FindForm()) == DialogResult.OK)
            {
                memberService.AddMember(editor.Member);
                LoadMembers(txtSearch.Text.Trim());
            }
        }

        private void EditMember(Member member)
        {
            using var editor = new MemberEditorForm(member);
            if (editor.ShowDialog(FindForm()) == DialogResult.OK)
            {
                memberService.UpdateMember(editor.Member);
                LoadMembers(txtSearch.Text.Trim());
            }
        }

        private void DeleteMember(Member member)
        {
            if (MessageBox.Show($"Delete member '{member.FullName}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            memberService.DeleteMember(member.MemberID);
            LoadMembers(txtSearch.Text.Trim());
        }

        // ---------- Card ----------
        private Panel CreateMemberCard(Member member)
        {
            var card = new RoundedPanel
            {
                Size = new Size(CardWidth, CardHeight),
                BackColor = CardBackgroundColor,
                BorderColor = BorderColor,
                CornerRadius = 14,
                Margin = new Padding(0, 0, 20, 20)
            };

            // Avatar
            var avatarPanel = new RoundedPanel
            {
                Size = new Size(56, 56),
                Location = new Point(20, 20),
                BackColor = InputBackgroundColor,
                BorderColor = Color.Transparent,
                CornerRadius = 28
            };
            string initials = string.Concat(member.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                                            .Take(2)
                                                            .Select(n => char.ToUpper(n[0])));
            avatarPanel.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var font = new Font("Georgia", 16, FontStyle.Bold);
                using var brush = new SolidBrush(TextSecondaryColor);
                var size = e.Graphics.MeasureString(initials, font);
                e.Graphics.DrawString(initials, font, brush,
                    (avatarPanel.Width - size.Width) / 2, (avatarPanel.Height - size.Height) / 2);
            };

            // Name — auto size, maximum size, wrapping correctly
            var lblName = new Label
            {
                Text = member.FullName,
                Font = new Font("Segoe UI Semibold", 13.5f, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                MaximumSize = new Size(200, 0),
                AutoSize = true,
                AutoEllipsis = true,
                Margin = new Padding(0, 0, 0, 4)
            };

            var lblEmail = new Label
            {
                Text = member.Email,
                Font = new Font("Segoe UI", 10f),
                ForeColor = TextSecondaryColor,
                MaximumSize = new Size(200, 0),
                AutoSize = true,
                AutoEllipsis = true,
                Margin = new Padding(0, 0, 0, 2)
            };

            var textPanel = new FlowLayoutPanel
            {
                Location = new Point(88, 22),
                Size = new Size(220, 80),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            textPanel.Controls.AddRange(new Control[] { lblName, lblEmail });

            // Status pill — sized to its own text, anchored to the card's right edge
            bool isActive = member.IsActive;
            var statusBg = isActive ? ActiveBadgeBg : WarningLightColor;
            var statusFg = isActive ? AccentColor : WarningColor;
            var statusText = isActive ? "Active" : "Inactive";

            using (var measureFont = new Font("Segoe UI Semibold", 9f))
            {
                var textSize = TextRenderer.MeasureText(statusText, measureFont);
                int pillWidth = textSize.Width + 28;
                var statusPanel = new RoundedPanel
                {
                    Size = new Size(pillWidth, 28),
                    Location = new Point(card.Width - 20 - pillWidth, 20),
                    BackColor = statusBg,
                    BorderColor = Color.Transparent,
                    CornerRadius = 14
                };
                statusPanel.Controls.Add(new Label
                {
                    Text = statusText,
                    Font = measureFont,
                    ForeColor = statusFg,
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                });
                card.Controls.Add(statusPanel);
            }

            // Divider
            var divider = new Panel
            {
                Size = new Size(CardWidth - 40, 1),
                Location = new Point(20, 92),
                BackColor = BorderColor
            };

            var lblJoined = new Label
            {
                Text = $"Joined {member.MembershipDate:dd MMM yyyy}",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(20, 106)
            };

            // Actions
            var btnDelete = new RoundedButton
            {
                Text = "Delete",
                Width = 76,
                Height = 32,
                BaseColor = WarningLightColor,
                HoverColor = WarningHoverColor,
                TextColorNormal = WarningColor,
                BorderColorNormal = Color.Transparent,
                BackColor = CardBackgroundColor,
                CornerRadius = 8,
                Location = new Point(CardWidth - 20 - 76, CardHeight - 20 - 32)
            };
            btnDelete.Click += (_, _) => DeleteMember(member);

            var btnEdit = new RoundedButton
            {
                Text = "Edit",
                Width = 76,
                Height = 32,
                BaseColor = InputBackgroundColor,
                HoverColor = BorderColor,
                TextColorNormal = TextPrimaryColor,
                BorderColorNormal = Color.Transparent,
                BackColor = CardBackgroundColor,
                CornerRadius = 8,
                Location = new Point(btnDelete.Left - 10 - 76, CardHeight - 20 - 32)
            };
            btnEdit.Click += (_, _) => EditMember(member);

            card.Controls.AddRange(new Control[] { avatarPanel, textPanel, divider, lblJoined, btnEdit, btnDelete });
            return card;
        }

        // ======================================================
        //  Editor dialog
        // ======================================================
        private sealed class MemberEditorForm : Form
        {
            private readonly TextBox txtName = new() { Width = 300, Height = 30, Font = new Font("Segoe UI", 10) };
            private readonly TextBox txtEmail = new() { Width = 300, Height = 30, Font = new Font("Segoe UI", 10) };
            private readonly TextBox txtPhone = new() { Width = 300, Height = 30, Font = new Font("Segoe UI", 10) };
            private readonly TextBox txtAddress = new() { Width = 300, Height = 30, Font = new Font("Segoe UI", 10) };
            private readonly DateTimePicker dtMembership = new() { Width = 300, Height = 30, Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 10) };
            private readonly CheckBox chkActive = new() { Width = 300, Text = "Active member", Checked = true, Font = new Font("Segoe UI", 10) };

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

                Text = member == null ? "Register Member" : "Edit Member";
                ClientSize = new Size(392, 470);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                StartPosition = FormStartPosition.CenterParent;
                BackColor = BackgroundColor;
                Font = new Font("Segoe UI", 9.5f);

                var headerBar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = CardBackgroundColor };
                headerBar.Paint += (_, e) =>
                {
                    using var pen = new Pen(BorderColor, 1);
                    e.Graphics.DrawLine(pen, 0, headerBar.Height - 1, headerBar.Width, headerBar.Height - 1);
                };
                headerBar.Controls.Add(new Label
                {
                    Text = Text,
                    Font = new Font("Segoe UI Semibold", 13f),
                    ForeColor = TextPrimaryColor,
                    AutoSize = true,
                    Location = new Point(24, 15)
                });

                var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 20, 24, 0), BackColor = BackgroundColor };

                int top = 0;
                body.Controls.Add(BuildField("Full name", txtName, ref top));
                body.Controls.Add(BuildField("Email", txtEmail, ref top));
                body.Controls.Add(BuildField("Phone", txtPhone, ref top));
                body.Controls.Add(BuildField("Address", txtAddress, ref top));
                body.Controls.Add(BuildField("Membership date", dtMembership, ref top));

                chkActive.Location = new Point(0, top + 8);
                chkActive.BackColor = Color.Transparent;
                chkActive.ForeColor = TextPrimaryColor;
                body.Controls.Add(chkActive);

                var footer = new Panel { Dock = DockStyle.Bottom, Height = 72, BackColor = CardBackgroundColor };
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
                    HoverColor = InputBackgroundColor,
                    BorderColorNormal = BorderColor,
                    TextColorNormal = TextPrimaryColor,
                    BackColor = CardBackgroundColor,
                    CornerRadius = 10
                };
                btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

                var btnSave = new RoundedButton
                {
                    Text = "Save",
                    Width = 100,
                    Height = 38,
                    BaseColor = AccentColor,
                    HoverColor = AccentHoverColor,
                    BorderColorNormal = Color.Transparent,
                    TextColorNormal = Color.White,
                    BackColor = CardBackgroundColor,
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
                var wrapper = new Panel { Location = new Point(0, top), Width = 300, Height = 62, BackColor = Color.Transparent };
                wrapper.Controls.Add(new Label
                {
                    Text = labelText,
                    Font = new Font("Segoe UI", 8.5f),
                    ForeColor = TextSecondaryColor,
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
    //  Reusable modern controls (self-contained in this namespace)
    // ======================================================

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

        protected override void OnMouseEnter(EventArgs e) { isHovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { isHovered = false; Invalidate(); base.OnMouseLeave(e); }

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

    internal sealed class RoundedTextBox : Panel
    {
        public TextBox Input { get; }
        public int CornerRadius { get; set; } = 20;
        public Color BorderColorNormal { get; set; } = Color.FromArgb(228, 230, 236);

        public Color FieldBackColor
        {
            get => Input.BackColor;
            set { Input.BackColor = value; fieldBackColor = value; }
        }
        private Color fieldBackColor = Color.White;

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
                ForeColor = Color.FromArgb(140, 130, 110),
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
                BackColor = fieldBackColor
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
            if (BorderColor != Color.Transparent)
            {
                using var pen = new Pen(BorderColor, 1);
                e.Graphics.DrawPath(pen, path);
            }
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