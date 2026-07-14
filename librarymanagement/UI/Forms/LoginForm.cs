using LibraryManagementSystem.Data;
using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LibraryManagementSystem.UI.Forms
{
    public class LoginForm : Form
    {
        private readonly TextBox txtUsername;
        private readonly TextBox txtPassword;
        private readonly Button btnLogin;
        private readonly Label lblHint;
        private readonly Panel loginCard;
        private readonly Label lblLogo;
        private readonly Label lblTitle;
        private readonly Label lblSubtitle;
        private readonly Label lblUsername;
        private readonly Label lblPassword;
        private readonly Button btnClose;

        public LoginForm()
        {
            Text = "Library Management System";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(15, 23, 42);

            // Login Card Panel - fixed size for reliability
            const int cardWidth = 420;
            const int cardHeight = 520;
            loginCard = new Panel
            {
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(30),
                Size = new Size(cardWidth, cardHeight),
                Anchor = AnchorStyles.None
            };
            // Add rounded corners to loginCard using paint event
            loginCard.Paint += (s, e) =>
            {
                var path = new GraphicsPath();
                int cornerRadius = 10;
                path.AddArc(0, 0, cornerRadius * 2, cornerRadius * 2, 180, 90);
                path.AddArc(loginCard.Width - cornerRadius * 2, 0, cornerRadius * 2, cornerRadius * 2, 270, 90);
                path.AddArc(loginCard.Width - cornerRadius * 2, loginCard.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
                path.AddArc(0, loginCard.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
                path.CloseAllFigures();
                loginCard.Region = new Region(path);
            };

            // Logo/Symbol
            lblLogo = new Label
            {
                Text = "📚",
                Font = new Font("Segoe UI Emoji", 52),
                ForeColor = Color.FromArgb(59, 130, 246),
                AutoSize = true,
                Location = new Point((cardWidth - 60) / 2, 30)
            };

            // Title
            lblTitle = new Label
            {
                Text = "Library Management",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(226, 232, 240),
                AutoSize = true,
                Location = new Point((cardWidth - 280) / 2, 100)
            };

            lblSubtitle = new Label
            {
                Text = "Sign in to access your account",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Location = new Point((cardWidth - 240) / 2, 140)
            };

            // Username
            lblUsername = new Label
            {
                Text = "Username",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(226, 232, 240),
                AutoSize = true,
                Location = new Point(30, 180)
            };

            txtUsername = new TextBox
            {
                PlaceholderText = "Enter your username",
                Font = new Font("Segoe UI", 11),
                Size = new Size(360, 38),
                Location = new Point(30, 205),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White
            };
            txtUsername.Text = "admin";

            // Password
            lblPassword = new Label
            {
                Text = "Password",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(226, 232, 240),
                AutoSize = true,
                Location = new Point(30, 260)
            };

            txtPassword = new TextBox
            {
                PlaceholderText = "Enter your password",
                Font = new Font("Segoe UI", 11),
                Size = new Size(360, 38),
                Location = new Point(30, 285),
                UseSystemPasswordChar = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White
            };
            txtPassword.Text = "admin123";
            txtPassword.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    AttemptLogin();
                }
            };

            btnLogin = new Button
            {
                Text = "Sign In",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(52, 120, 246),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(360, 45),
                Location = new Point(30, 345),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += (_, _) => AttemptLogin();
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(37, 99, 235);
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(52, 120, 246);

            lblHint = new Label
            {
                Text = "Default login: admin / admin123",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Location = new Point((cardWidth - 220) / 2, 410)
            };

            // Close button
            btnClose = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 12),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 30),
                ForeColor = Color.Gray,
                Location = new Point(cardWidth - 40, 10),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (_, _) => Close();
            btnClose.MouseEnter += (s, e) => btnClose.ForeColor = Color.Red;
            btnClose.MouseLeave += (s, e) => btnClose.ForeColor = Color.Gray;

            // Add controls
            loginCard.Controls.AddRange(new Control[] { lblLogo, lblTitle, lblSubtitle, lblUsername, txtUsername, lblPassword, txtPassword, btnLogin, lblHint, btnClose });
            Controls.Add(loginCard);

            // Center the login card
            CenterLoginCard();
            Resize += (_, _) => CenterLoginCard();
        }

        private void CenterLoginCard()
        {
            loginCard.Location = new Point(
                (ClientSize.Width - loginCard.Width) / 2,
                (ClientSize.Height - loginCard.Height) / 2
            );
        }

        private void AttemptLogin()
        {
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            const string query = "SELECT Username, Role FROM Users WHERE Username = @User AND PasswordHash = SHA2(@Pass, 256)";
            var parameters = new[]
            {
                new MySqlParameter("@User", username),
                new MySqlParameter("@Pass", password)
            };

            try
            {
                var result = DatabaseHelper.ExecuteQuery(query, parameters);
                if (result.Rows.Count == 0)
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE Users SET LastLogin = NOW() WHERE Username = @User",
                    new[] { new MySqlParameter("@User", username) });

                var role = result.Rows[0]["Role"]?.ToString() ?? "Admin";

                Hide();
                using var dashboard = new DashboardForm(username, role);
                dashboard.ShowDialog(this);
                Show();
                txtPassword.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to complete login.\n\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
