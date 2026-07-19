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
        private readonly Label lblTitle;
        private readonly Label lblSubtitle;
        private readonly Label lblUsername;
        private readonly Label lblPassword;
        private readonly Button btnClose;

        // Theme colors
        private static readonly Color BackgroundColor = Color.FromArgb(245, 242, 235);
        private static readonly Color CardBackgroundColor = Color.White;
        private static readonly Color TextPrimaryColor = Color.FromArgb(26, 32, 44);
        private static readonly Color TextSecondaryColor = Color.FromArgb(100, 116, 139);
        private static readonly Color AccentColor = Color.FromArgb(20, 83, 45);
        private static readonly Color InputBackgroundColor = Color.FromArgb(230, 224, 213);

        public LoginForm()
        {
            Text = "Library Management System";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            BackColor = BackgroundColor;

            // Login Card Panel - fixed size for reliability
            const int cardWidth = 440;
            const int cardHeight = 540;
            loginCard = new Panel
            {
                BackColor = Color.Transparent,
                Size = new Size(cardWidth, cardHeight),
                Anchor = AnchorStyles.None
            };
            // Add rounded corners and shadow to loginCard
            loginCard.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Draw shadow
                using var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0));
                var shadowPath = new GraphicsPath();
                int cr = 16;
                shadowPath.AddArc(4, 4, cr * 2, cr * 2, 180, 90);
                shadowPath.AddArc(loginCard.Width - cr * 2 - 4, 4, cr * 2, cr * 2, 270, 90);
                shadowPath.AddArc(loginCard.Width - cr * 2 - 4, loginCard.Height - cr * 2 - 4, cr * 2, cr * 2, 0, 90);
                shadowPath.AddArc(4, loginCard.Height - cr * 2 - 4, cr * 2, cr * 2, 90, 90);
                shadowPath.CloseAllFigures();
                g.FillPath(shadowBrush, shadowPath);

                // Draw card
                var path = new GraphicsPath();
                path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                path.AddArc(loginCard.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                path.AddArc(loginCard.Width - cr * 2, loginCard.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                path.AddArc(0, loginCard.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                path.CloseAllFigures();
                using var brush = new SolidBrush(CardBackgroundColor);
                g.FillPath(brush, path);
            };

            // Content Panel (inside rounded card)
            var contentPanel = new Panel
            {
                Size = new Size(cardWidth, cardHeight),
                BackColor = Color.Transparent,
                Location = new Point(0, 0)
            };

            // Logo/Symbol
           
            // Title
            lblTitle = new Label
            {
                Text = "LM",
                Font = new Font("Georgia", 28, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                TextAlign = ContentAlignment.TopCenter
            };
            lblTitle.Location = new Point((cardWidth - lblTitle.PreferredWidth) / 2, 100);

            lblSubtitle = new Label
            {
                Text = "Sign in to your account",
                Font = new Font("Segoe UI", 14),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                TextAlign = ContentAlignment.TopCenter
            };
            lblSubtitle.Location = new Point((cardWidth - lblSubtitle.PreferredWidth) / 2, 148);

            // Username
            lblUsername = new Label
            {
                Text = "Username",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(40, 200)
            };

            // Password
            lblPassword = new Label
            {
                Text = "Password",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(40, 292)
            };

            // Close button
            btnClose = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 18),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(40, 40),
                ForeColor = TextSecondaryColor,
                Location = new Point(cardWidth - 52, 16),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (_, _) => Close();
            btnClose.MouseEnter += (s, e) => btnClose.ForeColor = Color.FromArgb(239, 68, 68);
            btnClose.MouseLeave += (s, e) => btnClose.ForeColor = TextSecondaryColor;

            // Add controls to content panel
            contentPanel.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, lblUsername, lblPassword, btnClose });

            // Create username and password text boxes
            txtUsername = CreateStyledTextBox("Enter your username", 224, 360, contentPanel);
            txtUsername.Text = "admin";

            txtPassword = CreateStyledTextBox("Enter your password", 316, 360, contentPanel);
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.Text = "admin123";
            txtPassword.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    AttemptLogin();
                }
            };

            btnLogin = CreateStyledButton("Sign In", AccentColor, Color.White, 392, 360, contentPanel);
            btnLogin.Click += (_, _) => AttemptLogin();

            lblHint = new Label
            {
                Text = "Default login: admin / admin123",
                Font = new Font("Segoe UI", 11),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                TextAlign = ContentAlignment.TopCenter
            };
            lblHint.Location = new Point((cardWidth - lblHint.PreferredWidth) / 2, 468);
            contentPanel.Controls.Add(lblHint);

            // Add content panel to login card
            loginCard.Controls.Add(contentPanel);
            Controls.Add(loginCard);

            // Center the login card
            CenterLoginCard();
            Resize += (_, _) => CenterLoginCard();
        }

        private TextBox CreateStyledTextBox(string placeholder, int top, int width, Panel parent)
        {
            var tb = new TextBox
            {
                PlaceholderText = placeholder,
                Font = new Font("Segoe UI", 12),
                Size = new Size(width, 48),
                Location = new Point(40, top),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = InputBackgroundColor,
                ForeColor = TextPrimaryColor
            };

            parent.Controls.Add(tb);
            return tb;
        }

        private Button CreateStyledButton(string text, Color backColor, Color foreColor, int top, int width, Panel parent)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = foreColor,
                BackColor = backColor,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(width, 52),
                Location = new Point(40, top),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                int cr = 12;
                path.AddArc(0, 0, cr * 2, cr * 2, 180, 90);
                path.AddArc(btn.Width - cr * 2, 0, cr * 2, cr * 2, 270, 90);
                path.AddArc(btn.Width - cr * 2, btn.Height - cr * 2, cr * 2, cr * 2, 0, 90);
                path.AddArc(0, btn.Height - cr * 2, cr * 2, cr * 2, 90, 90);
                path.CloseAllFigures();
                btn.Region = new Region(path);
            };
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(backColor, 0.1f);
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;

            parent.Controls.Add(btn);
            return btn;
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
