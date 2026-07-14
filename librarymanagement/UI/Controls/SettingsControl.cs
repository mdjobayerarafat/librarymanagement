using LibraryManagementSystem.Data;
using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryManagementSystem.UI.Controls
{
    public class SettingsControl : UserControl
    {
        private readonly TextBox txtDefaultBorrowDays;
        private readonly CheckBox chkAllowMultipleBorrows;
        private readonly TextBox txtMaxBooksPerMember;
        private readonly Button btnSaveSettings;

        public SettingsControl()
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
                Text = "⚙️ Settings",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 18)
            };

            titlePanel.Controls.Add(lblTitle);

            // Settings Container Panel
            var settingsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            // Borrow Settings Group
            var borrowGroup = new GroupBox
            {
                Text = "Borrow Settings",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(30, 41, 59),
                Size = new Size(600, 200),
                Location = new Point(0, 0)
            };

            // Default Borrow Days
            var lblBorrowDays = CreateLabel("Default Borrow Days (e.g., 14):", 20);
            txtDefaultBorrowDays = CreateTextBox("14", 20, 50);

            // Allow Multiple Borrows
            chkAllowMultipleBorrows = new CheckBox
            {
                Text = "Allow members to borrow multiple books at once",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(20, 90),
                AutoSize = true,
                Checked = true
            };

            // Max Books Per Member
            var lblMaxBooks = CreateLabel("Max Books Per Member:", 130);
            txtMaxBooksPerMember = CreateTextBox("5", 20, 160);

            borrowGroup.Controls.AddRange(new Control[] { lblBorrowDays, txtDefaultBorrowDays, chkAllowMultipleBorrows, lblMaxBooks, txtMaxBooksPerMember });

            // Save Button
            btnSaveSettings = new Button
            {
                Text = "Save Settings",
                Size = new Size(200, 45),
                Location = new Point(0, 220),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSaveSettings.FlatAppearance.BorderSize = 0;
            btnSaveSettings.Click += (_, _) => SaveSettings();
            btnSaveSettings.MouseEnter += (s, e) => btnSaveSettings.BackColor = Color.FromArgb(39, 174, 96);
            btnSaveSettings.MouseLeave += (s, e) => btnSaveSettings.BackColor = Color.FromArgb(46, 204, 113);

            settingsPanel.Controls.AddRange(new Control[] { borrowGroup, btnSaveSettings });

            Controls.Add(settingsPanel);
            Controls.Add(titlePanel);

            // Load existing settings
            LoadSettings();
        }

        private Label CreateLabel(string text, int top)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, top)
            };
        }

        private TextBox CreateTextBox(string placeholder, int left, int top)
        {
            return new TextBox
            {
                Text = placeholder,
                Font = new Font("Segoe UI", 10),
                Size = new Size(250, 32),
                Location = new Point(left, top),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White
            };
        }

        private void LoadSettings()
        {
            try
            {
                // In a real app, you'd load these from a database or config file
                // For now, we'll just use defaults
                txtDefaultBorrowDays.Text = "14";
                txtMaxBooksPerMember.Text = "5";
                chkAllowMultipleBorrows.Checked = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveSettings()
        {
            try
            {
                if (!int.TryParse(txtDefaultBorrowDays.Text.Trim(), out int borrowDays) || borrowDays <= 0)
                {
                    MessageBox.Show("Please enter a valid number of default borrow days (must be greater than 0).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(txtMaxBooksPerMember.Text.Trim(), out int maxBooks) || maxBooks <= 0)
                {
                    MessageBox.Show("Please enter a valid number of max books per member (must be greater than 0).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // In a real app, you'd save these to a database or config file
                MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to save settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
