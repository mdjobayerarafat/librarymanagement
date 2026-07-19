using LibraryManagementSystem.Data;
using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LibraryManagementSystem.UI.Controls
{
    public class SettingsControl : UserControl
    {
        private readonly TextBox txtDefaultBorrowDays;
        private readonly CheckBox chkAllowMultipleBorrows;
        private readonly TextBox txtMaxBooksPerMember;
        private readonly Button btnSaveSettings;
        private static readonly Color BackgroundColor = Color.FromArgb(247, 247, 255);
        private static readonly Color CardBackgroundColor = Color.White;
        private static readonly Color TextPrimaryColor = Color.FromArgb(41, 47, 54);
        private static readonly Color TextSecondaryColor = Color.FromArgb(73, 88, 103);
        private static readonly Color AccentColor = Color.FromArgb(87, 115, 153);
        private static readonly Color InputBackgroundColor = Color.FromArgb(247, 247, 255);

        public SettingsControl()
        {
            BackColor = BackgroundColor;
            Dock = DockStyle.Fill;
            Padding = new Padding(32, 24, 32, 24);

            // Title Section
            var lblTitle = new Label
            {
                Text = "Settings",
                Font = new Font("Georgia", 36, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            // Settings Container Panel
            var settingsPanel = new Panel
            {
                Location = new Point(0, 80),
                Size = new Size(ClientSize.Width - 64, ClientSize.Height - 104),
                BackColor = Color.Transparent,
                AutoScroll = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Borrow Settings Group
            var borrowGroup = new Panel
            {
                Text = "Borrow Settings",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = CardBackgroundColor,
                Size = new Size(600, 220),
                Location = new Point(0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            borrowGroup.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                int cr = 12;
                path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                path.AddArc(borrowGroup.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                path.AddArc(borrowGroup.Width - cr * 2, borrowGroup.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                path.AddArc(0, borrowGroup.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                path.CloseAllFigures();
                borrowGroup.Region = new Region(path);
            };

            var lblGroupTitle = new Label
            {
                Text = "Borrow Settings",
                Font = new Font("Georgia", 18, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(24, 24)
            };

            // Default Borrow Days
            var lblBorrowDays = CreateLabel("Default Borrow Days (e.g., 14):", 60);
            borrowGroup.Controls.Add(lblBorrowDays);
            txtDefaultBorrowDays = CreateTextBox("14", 24, 84, borrowGroup);

            // Allow Multiple Borrows
            chkAllowMultipleBorrows = new CheckBox
            {
                Text = "Allow members to borrow multiple books at once",
                Font = new Font("Segoe UI", 11),
                ForeColor = TextPrimaryColor,
                BackColor = Color.Transparent,
                Location = new Point(24, 132),
                AutoSize = true,
                Checked = true
            };
            borrowGroup.Controls.Add(chkAllowMultipleBorrows);

            // Max Books Per Member
            var lblMaxBooks = CreateLabel("Max Books Per Member:", 164);
            borrowGroup.Controls.Add(lblMaxBooks);
            txtMaxBooksPerMember = CreateTextBox("5", 24, 188, borrowGroup);

            borrowGroup.Controls.Add(lblGroupTitle);

            // Save Button
            btnSaveSettings = CreateStyledButton("Save Settings", AccentColor, Color.White, (_, _) => SaveSettings());
            btnSaveSettings.Location = new Point(0, 244);

            settingsPanel.Controls.AddRange(new Control[] { borrowGroup, btnSaveSettings });

            Controls.AddRange(new Control[] { lblTitle, settingsPanel });

            Resize += (_, _) =>
            {
                settingsPanel.Size = new Size(ClientSize.Width - 64, ClientSize.Height - 104);
                borrowGroup.Width = Math.Max(600, settingsPanel.ClientSize.Width);
            };

            // Load existing settings
            LoadSettings();
        }

        private Label CreateLabel(string text, int top)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 11),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(24, top)
            };
        }

        private TextBox CreateTextBox(string placeholder, int left, int top, Panel parent)
        {
            var panel = new Panel
            {
                Location = new Point(left, top),
                Size = new Size(250, 40),
                BackColor = InputBackgroundColor
            };
            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                int cr = 8;
                path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                path.AddArc(panel.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                path.AddArc(panel.Width - cr * 2, panel.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                path.AddArc(0, panel.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                path.CloseAllFigures();
                panel.Region = new Region(path);
            };

            var txtBox = new TextBox
            {
                Text = placeholder,
                Font = new Font("Segoe UI", 11),
                Size = new Size(226, 40),
                Location = new Point(12, 8),
                BorderStyle = BorderStyle.None,
                BackColor = InputBackgroundColor,
                ForeColor = TextPrimaryColor
            };

            panel.Controls.Add(txtBox);
            parent.Controls.Add(panel);

            return txtBox;
        }

        private Button CreateStyledButton(string text, Color backColor, Color foreColor, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(200, 48),
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
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(backColor, 0.1f);
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;
            return btn;
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
