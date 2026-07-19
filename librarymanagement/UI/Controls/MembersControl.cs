using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;

namespace LibraryManagementSystem.UI.Controls
{
    public class MembersControl : UserControl
    {
        private readonly MemberService memberService = new();
        private readonly TextBox txtSearch;
        private readonly FlowLayoutPanel flpMembers;
        private static readonly Color BackgroundColor = Color.FromArgb(245, 242, 235);
        private static readonly Color CardBackgroundColor = Color.White;
        private static readonly Color TextPrimaryColor = Color.FromArgb(26, 32, 44);
        private static readonly Color TextSecondaryColor = Color.FromArgb(100, 116, 139);
        private static readonly Color AccentColor = Color.FromArgb(20, 83, 45);
        private static readonly Color InputBackgroundColor = Color.FromArgb(230, 224, 213);
        private static readonly Color TagColor = Color.FromArgb(230, 224, 213);
        private static readonly Color TagTextColor = Color.FromArgb(75, 85, 99);
        private static readonly Color WarningColor = Color.FromArgb(239, 68, 68);
        private static readonly Color WarningLightColor = Color.FromArgb(254, 226, 226);

        public MembersControl()
        {
            BackColor = BackgroundColor;
            Dock = DockStyle.Fill;
            Padding = new Padding(32, 24, 32, 24);

            // Title Section
            var lblTitle = new Label
            {
                Text = "Members",
                Font = new Font("Georgia", 36, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var lblSubtitle = new Label
            {
                Text = "5 active of 6 registered",
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

            txtSearch = CreateStyledTextBox("Search members...", new Point(0, 0), 400);
            txtSearch.TextChanged += (_, _) => LoadMembers(txtSearch.Text.Trim());

            var btnAdd = CreateStyledButton("➕ Register Member", AccentColor, Color.White, (_, _) => AddMember());
            btnAdd.Location = new Point(toolbarPanel.ClientSize.Width - 220, 0);
            toolbarPanel.Resize += (_, _) =>
            {
                btnAdd.Location = new Point(toolbarPanel.ClientSize.Width - 220, 0);
            };

            toolbarPanel.Controls.AddRange(new Control[] { txtSearch, btnAdd });

            // Members Container
            flpMembers = new FlowLayoutPanel
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
                flpMembers.Size = new Size(ClientSize.Width - 64, ClientSize.Height - 200);
            };

            Controls.AddRange(new Control[] { lblTitle, lblSubtitle, toolbarPanel, flpMembers });

            Load += (_, _) => LoadMembers();
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
                Size = new Size(220, 48),
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

        private Panel CreateMemberCard(Member member)
        {
            var card = new Panel
            {
                Size = new Size(440, 180),
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

            // Avatar Panel
            var avatarPanel = new Panel
            {
                Size = new Size(80, 80),
                Location = new Point(24, 24),
                BackColor = InputBackgroundColor
            };
            avatarPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                int cr = 40;
                path.AddArc(0, 0, cr * 2, cr * 2, 0, 360);
                path.CloseAllFigures();
                avatarPanel.Region = new Region(path);

                // Draw initials
                string initials = string.Concat(member.FullName.Split(' ').Take(2).Select(n => n[0])).ToUpper();
                using var font = new Font("Georgia", 26, FontStyle.Bold);
                using var brush = new SolidBrush(TextSecondaryColor);
                var size = g.MeasureString(initials, font);
                g.DrawString(initials, font, brush, (avatarPanel.Width - size.Width) / 2, (avatarPanel.Height - size.Height) / 2);
            };

            // Name and Email
            var lblName = new Label
            {
                Text = member.FullName,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(120, 28),
                MaximumSize = new Size(220, 0)
            };

            var lblEmail = new Label
            {
                Text = member.Email,
                Font = new Font("Segoe UI", 12),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(120, 60),
                MaximumSize = new Size(220, 0)
            };

            // Status Tag
            var statusBackColor = member.IsActive ? Color.FromArgb(220, 240, 230) : WarningLightColor;
            var statusForeColor = member.IsActive ? AccentColor : WarningColor;
            var statusPanel = new Panel
            {
                Size = new Size(96, 34),
                Location = new Point(320, 36),
                BackColor = statusBackColor
            };
            statusPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                int cr = 17;
                path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                path.AddArc(statusPanel.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                path.AddArc(statusPanel.Width - cr * 2, statusPanel.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                path.AddArc(0, statusPanel.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                path.CloseAllFigures();
                statusPanel.Region = new Region(path);
            };
            var lblStatus = new Label
            {
                Text = member.IsActive ? "Active" : "Inactive",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = statusForeColor,
                AutoSize = true,
                Location = new Point(18, 8),
                BackColor = Color.Transparent
            };
            statusPanel.Controls.Add(lblStatus);

            // Divider
            var divider = new Panel
            {
                Size = new Size(card.Width - 48, 1),
                Location = new Point(24, 124),
                BackColor = InputBackgroundColor
            };

            // Membership Date and Borrow Count
            var lblMembership = new Label
            {
                Text = $"Joined {member.MembershipDate:dd MMM yyyy}",
                Font = new Font("Segoe UI", 13),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(24, 140)
            };

            var lblBorrowed = new Label
            {
                Text = "📚 2 borrowed",
                Font = new Font("Segoe UI", 13),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(260, 140)
            };

            card.Controls.AddRange(new Control[] { avatarPanel, lblName, lblEmail, statusPanel, divider, lblMembership, lblBorrowed });
            return card;
        }

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

                // Update subtitle count
                int activeCount = members.Count(m => m.IsActive);
                foreach (Control control in Controls)
                {
                    if (control is Label lbl && lbl.Text.Contains("active of"))
                    {
                        lbl.Text = $"{activeCount} active of {members.Count} registered";
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load members.\n\n{ex.Message}", "Members", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private sealed class MemberEditorForm : Form
        {
            private readonly TextBox txtName = new() { Left = 150, Top = 20, Width = 240 };
            private readonly TextBox txtEmail = new() { Left = 150, Top = 55, Width = 240 };
            private readonly TextBox txtPhone = new() { Left = 150, Top = 90, Width = 240 };
            private readonly TextBox txtAddress = new() { Left = 150, Top = 125, Width = 240 };
            private readonly DateTimePicker dtMembership = new() { Left = 150, Top = 160, Width = 240, Format = DateTimePickerFormat.Short };
            private readonly CheckBox chkActive = new() { Left = 150, Top = 198, Width = 240, Text = "Active member", Checked = true };

            private static readonly Color BackgroundColor = Color.FromArgb(245, 242, 235);
            private static readonly Color CardBackgroundColor = Color.White;
            private static readonly Color TextPrimaryColor = Color.FromArgb(26, 32, 44);
            private static readonly Color AccentColor = Color.FromArgb(20, 83, 45);
            private static readonly Color InputBackgroundColor = Color.FromArgb(230, 224, 213);

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
                ClientSize = new Size(420, 280);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                StartPosition = FormStartPosition.CenterParent;
                BackColor = BackgroundColor;
                ForeColor = TextPrimaryColor;

                Controls.AddRange(new Control[]
                {
                    CreateLabel("Full Name", 20),
                    StyleTextBox(txtName),
                    CreateLabel("Email", 55),
                    StyleTextBox(txtEmail),
                    CreateLabel("Phone", 90),
                    StyleTextBox(txtPhone),
                    CreateLabel("Address", 125),
                    StyleTextBox(txtAddress),
                    CreateLabel("Membership", 160),
                    StyleDateTimePicker(dtMembership),
                    StyleCheckBox(chkActive),
                    CreateActionButton("Save", 150, 235, (_, _) => SaveMember()),
                    CreateActionButton("Cancel", 255, 235, (_, _) => DialogResult = DialogResult.Cancel)
                });

                txtName.Text = Member.FullName;
                txtEmail.Text = Member.Email;
                txtPhone.Text = Member.Phone;
                txtAddress.Text = Member.Address;
                dtMembership.Value = Member.MembershipDate == default ? DateTime.Now : Member.MembershipDate;
                chkActive.Checked = Member.IsActive;
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

            private static DateTimePicker StyleDateTimePicker(DateTimePicker dateTimePicker)
            {
                dateTimePicker.BackColor = InputBackgroundColor;
                dateTimePicker.ForeColor = TextPrimaryColor;
                dateTimePicker.Font = new Font("Segoe UI", 10);
                return dateTimePicker;
            }

            private static CheckBox StyleCheckBox(CheckBox checkBox)
            {
                checkBox.BackColor = Color.Transparent;
                checkBox.ForeColor = TextPrimaryColor;
                checkBox.Font = new Font("Segoe UI", 10);
                return checkBox;
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
}