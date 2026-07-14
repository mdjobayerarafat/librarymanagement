using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryManagementSystem.UI.Forms
{
    public class MembersForm : Form
    {
        private readonly MemberService memberService = new();
        private readonly TextBox txtSearch;
        private readonly DataGridView dgvMembers;

        public MembersForm()
        {
            Text = "👥 Manage Members";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(240, 242, 245);

            // Header Panel
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(46, 204, 113)
            };

            var lblHeaderTitle = new Label
            {
                Text = "👥 Members Management",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(30, 18)
            };

            headerPanel.Controls.Add(lblHeaderTitle);

            // Toolbar Panel
            var toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            txtSearch = new TextBox
            {
                PlaceholderText = "🔍 Search by name, email, or phone...",
                Width = 400,
                Height = 36,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (_, _) => LoadMembers(txtSearch.Text.Trim());

            var btnAdd = CreateStyledButton("➕ Add Member", Color.FromArgb(46, 204, 113), (_, _) => AddMember());
            var btnEdit = CreateStyledButton("✏️ Edit Member", Color.FromArgb(241, 196, 15), (_, _) => EditMember());
            var btnDelete = CreateStyledButton("🗑️ Delete Member", Color.FromArgb(231, 76, 60), (_, _) => DeleteMember());
            var btnRefresh = CreateStyledButton("🔄 Refresh", Color.FromArgb(52, 73, 94), (_, _) => LoadMembers(txtSearch.Text.Trim()));

            txtSearch.Location = new Point(15, 17);
            btnAdd.Location = new Point(430, 17);
            btnEdit.Location = new Point(550, 17);
            btnDelete.Location = new Point(670, 17);
            btnRefresh.Location = new Point(800, 17);

            toolbarPanel.Controls.AddRange(new Control[] { txtSearch, btnAdd, btnEdit, btnDelete, btnRefresh });

            // Data Grid
            dgvMembers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(220, 220, 220),
                Font = new Font("Segoe UI", 9),
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false
            };
            dgvMembers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(39, 174, 96);
            dgvMembers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvMembers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvMembers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 249, 249);
            dgvMembers.DefaultCellStyle.Padding = new Padding(5);

            Controls.Add(dgvMembers);
            Controls.Add(toolbarPanel);
            Controls.Add(headerPanel);

            Shown += (_, _) => LoadMembers();
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

        private void LoadMembers(string keyword = "")
        {
            try
            {
                dgvMembers.DataSource = string.IsNullOrWhiteSpace(keyword)
                    ? memberService.GetAllMembers()
                    : memberService.SearchMembers(keyword);

                var idColumn = dgvMembers.Columns["MemberID"];
                if (idColumn != null)
                {
                    idColumn.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load members.\n\n{ex.Message}", "Members", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private sealed class MemberEditorForm : Form
        {
            private readonly TextBox txtName = new() { Left = 150, Top = 20, Width = 220 };
            private readonly TextBox txtEmail = new() { Left = 150, Top = 55, Width = 220 };
            private readonly TextBox txtPhone = new() { Left = 150, Top = 90, Width = 220 };
            private readonly TextBox txtAddress = new() { Left = 150, Top = 125, Width = 220 };
            private readonly DateTimePicker dtMembership = new() { Left = 150, Top = 160, Width = 220, Format = DateTimePickerFormat.Short };
            private readonly CheckBox chkActive = new() { Left = 150, Top = 198, Width = 220, Text = "Active member", Checked = true };

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
                ClientSize = new Size(410, 280);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                StartPosition = FormStartPosition.CenterParent;

                Controls.AddRange(new Control[]
                {
                    CreateLabel("Full Name", 20),
                    txtName,
                    CreateLabel("Email", 55),
                    txtEmail,
                    CreateLabel("Phone", 90),
                    txtPhone,
                    CreateLabel("Address", 125),
                    txtAddress,
                    CreateLabel("Membership", 160),
                    dtMembership,
                    chkActive,
                    CreateActionButton("Save", 150, 225, (_, _) => SaveMember()),
                    CreateActionButton("Cancel", 245, 225, (_, _) => DialogResult = DialogResult.Cancel)
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
                return new Label { Text = text, Left = 30, Top = top + 4, Width = 100 };
            }

            private static Button CreateActionButton(string text, int left, int top, EventHandler onClick)
            {
                var button = new Button { Text = text, Left = left, Top = top, Width = 80, Height = 30 };
                button.Click += onClick;
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
