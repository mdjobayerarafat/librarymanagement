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
        private readonly Panel loginCard;
        private readonly Panel topChrome;

        private bool isDragging;
        private Point dragCursorStart;
        private Point dragFormStart;

        // Theme colors
        private static readonly Color BackgroundColor = Color.FromArgb(247, 247, 255);
        private static readonly Color CardBackgroundColor = Color.White;
        private static readonly Color TextPrimaryColor = Color.FromArgb(41, 47, 54);
        private static readonly Color TextSecondaryColor = Color.FromArgb(73, 88, 103);
        private static readonly Color AccentColor = Color.FromArgb(87, 115, 153);
        private static readonly Color AccentDarkColor = Color.FromArgb(41, 47, 54);
        private static readonly Color BrandMutedColor = Color.FromArgb(189, 213, 234);
        private static readonly Color InputBackgroundColor = Color.FromArgb(255, 255, 255);
        private static readonly Color BorderColor = Color.FromArgb(189, 213, 234);

        private const int CardWidth = 900;
        private const int CardHeight = 560;
        private const int BrandWidth = 360;

        public LoginForm()
        {
            Text = "Library Management System";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1000, 650);
            BackColor = BackgroundColor;

            Paint += (s, e) => DrawAmbientBackground(e.Graphics);
            Resize += (_, _) => { CenterLoginCard(); Invalidate(); };

            topChrome = CreateTopChrome();

            // ---- Login card (split brand / form) ----
            loginCard = new Panel
            {
                BackColor = Color.Transparent,
                Size = new Size(CardWidth, CardHeight),
                Anchor = AnchorStyles.None
            };
            loginCard.Paint += (s, e) => DrawLoginCard(e.Graphics);

            var brandPanel = CreateBrandPanel();
            var formPanel = CreateFormPanel(out txtUsername, out txtPassword, out btnLogin);

            loginCard.Controls.Add(brandPanel);
            loginCard.Controls.Add(formPanel);

            Controls.Add(loginCard);
            Controls.Add(topChrome);

            CenterLoginCard();
        }

        // =========================================================
        //  CUSTOM TITLE BAR
        // =========================================================

        private Panel CreateTopChrome()
        {
            var chrome = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.Transparent
            };

            var lblIcon = new Label
            {
                Text = "📚",
                Font = new Font("Segoe UI Emoji", 12),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(16, 11),
                BackColor = Color.Transparent
            };
            var lblAppName = new Label
            {
                Text = "Library Management System",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(42, 13),
                BackColor = Color.Transparent
            };

            var btnMinimize = CreateChromeButton("—");
            var btnClose = CreateChromeButton("✕");

            btnMinimize.Click += (_, _) => WindowState = FormWindowState.Minimized;
            btnClose.Click += (_, _) => Close();
            btnClose.MouseEnter += (s, e) => { btnClose.BackColor = Color.FromArgb(232, 17, 35); btnClose.ForeColor = Color.White; };
            btnClose.MouseLeave += (s, e) => { btnClose.BackColor = Color.Transparent; btnClose.ForeColor = TextSecondaryColor; };
            btnMinimize.MouseEnter += (s, e) => btnMinimize.BackColor = InputBackgroundColor;
            btnMinimize.MouseLeave += (s, e) => btnMinimize.BackColor = Color.Transparent;

            void PositionChromeButtons()
            {
                btnClose.Location = new Point(chrome.Width - 46, 2);
                btnMinimize.Location = new Point(chrome.Width - 92, 2);
            }
            chrome.Resize += (_, _) => PositionChromeButtons();

            chrome.Controls.AddRange(new Control[] { lblIcon, lblAppName, btnMinimize, btnClose });
            PositionChromeButtons();

            // Draggable region — click-drag moves the window (restoring from maximized first).
            void StartDrag(object? s, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left) return;

                if (WindowState == FormWindowState.Maximized)
                {
                    var target = new Size(1280, 800);
                    var cursor = Cursor.Position;
                    WindowState = FormWindowState.Normal;
                    Size = target;
                    Location = new Point(cursor.X - target.Width / 2, Math.Max(0, cursor.Y - 20));
                }

                isDragging = true;
                dragCursorStart = Cursor.Position;
                dragFormStart = Location;
            }

            chrome.MouseDown += StartDrag;
            lblAppName.MouseDown += StartDrag;

            void DoDrag(object? s, MouseEventArgs e)
            {
                if (!isDragging) return;
                var diff = new Size(Cursor.Position.X - dragCursorStart.X, Cursor.Position.Y - dragCursorStart.Y);
                Location = Point.Add(dragFormStart, diff);
            }
            chrome.MouseMove += DoDrag;
            lblAppName.MouseMove += DoDrag;

            void EndDrag(object? s, MouseEventArgs e) => isDragging = false;
            chrome.MouseUp += EndDrag;
            lblAppName.MouseUp += EndDrag;

            void ToggleMaximize(object? s, MouseEventArgs e)
            {
                WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            }
            chrome.MouseDoubleClick += ToggleMaximize;
            lblAppName.MouseDoubleClick += ToggleMaximize;

            return chrome;
        }

        private Button CreateChromeButton(string text)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                ForeColor = TextSecondaryColor,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(44, 40),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                TabStop = false
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;
            return btn;
        }

        // =========================================================
        //  BACKGROUND / CARD PAINTING
        // =========================================================

        private void DrawAmbientBackground(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var glow1 = new SolidBrush(Color.FromArgb(14, AccentColor));
            g.FillEllipse(glow1, -220, -180, 640, 640);

            using var glow2 = new SolidBrush(Color.FromArgb(10, AccentColor));
            g.FillEllipse(glow2, ClientSize.Width - 380, ClientSize.Height - 380, 640, 640);
        }

        private void DrawLoginCard(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = loginCard.Width - 4;
            int h = loginCard.Height - 4;

            using var shadowPath = CreateRoundedRectangle(new Rectangle(3, 5, w, h), 20);
            using var shadowBrush = new SolidBrush(Color.FromArgb(35, 0, 0, 0));
            g.FillPath(shadowBrush, shadowPath);

            using var cardPath = CreateRoundedRectangle(new Rectangle(0, 0, w, h), 20);
            var oldClip = g.Clip;
            g.SetClip(cardPath, CombineMode.Replace);

            using var brandBrush = new LinearGradientBrush(
                new Rectangle(0, 0, BrandWidth, h), AccentColor, AccentDarkColor, 60f);
            g.FillRectangle(brandBrush, 0, 0, BrandWidth, h);

            using var formBrush = new SolidBrush(CardBackgroundColor);
            g.FillRectangle(formBrush, BrandWidth, 0, w - BrandWidth, h);

            g.Clip = oldClip;

            using var borderPen = new Pen(Color.FromArgb(18, 0, 0, 0), 1);
            g.DrawPath(borderPen, cardPath);
        }

        // =========================================================
        //  BRAND PANEL (left)
        // =========================================================

        private Panel CreateBrandPanel()
        {
            var panel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(BrandWidth, CardHeight - 4),
                BackColor = Color.Transparent
            };

            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var c1 = new SolidBrush(Color.FromArgb(22, 255, 255, 255));
                g.FillEllipse(c1, -90, -110, 260, 260);
                using var c2 = new SolidBrush(Color.FromArgb(16, 255, 255, 255));
                g.FillEllipse(c2, BrandWidth - 140, panel.Height - 180, 300, 300);
            };

            var logoCircle = new Panel { Size = new Size(72, 72), BackColor = Color.Transparent };
            logoCircle.Location = new Point((BrandWidth - logoCircle.Width) / 2, 64);
            logoCircle.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = new SolidBrush(Color.FromArgb(45, 255, 255, 255));
                g.FillEllipse(brush, 0, 0, logoCircle.Width, logoCircle.Height);
                TextRenderer.DrawText(g, "📚", new Font("Segoe UI Emoji", 24), logoCircle.ClientRectangle,
                    Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            var lblBrandTitle = new Label
            {
                Text = "LibraryMS",
                Font = new Font("Segoe UI", 19, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            lblBrandTitle.Location = new Point((BrandWidth - lblBrandTitle.PreferredWidth) / 2, 142);

            var lblTagline = new Label
            {
                Text = "Organize. Track. Discover.",
                Font = new Font("Segoe UI", 10),
                ForeColor = BrandMutedColor,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            lblTagline.Location = new Point((BrandWidth - lblTagline.PreferredWidth) / 2, 188);

            panel.Controls.AddRange(new Control[] { logoCircle, lblBrandTitle, lblTagline });

            string[] features =
            {
                "Manage your full book catalog",
                "Track active loans in real time",
                "Keep member records organized"
            };

            int featureY = 254;
            foreach (var feature in features)
            {
                panel.Controls.Add(CreateFeatureRow(feature, featureY));
                featureY += 38;
            }

            var lblVersion = new Label
            {
                Text = "v1.0.0",
                Font = new Font("Segoe UI", 8),
                ForeColor = BrandMutedColor,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(24, panel.Height - 34)
            };
            panel.Controls.Add(lblVersion);

            return panel;
        }

        private Panel CreateFeatureRow(string text, int y)
        {
            var row = new Panel { Location = new Point(48, y), Size = new Size(BrandWidth - 80, 28), BackColor = Color.Transparent };

            var check = new Panel { Size = new Size(20, 20), Location = new Point(0, 4), BackColor = Color.Transparent };
            check.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = new SolidBrush(Color.FromArgb(60, 255, 255, 255));
                g.FillEllipse(brush, 0, 0, 20, 20);
                TextRenderer.DrawText(g, "✓", new Font("Segoe UI", 8, FontStyle.Bold), check.ClientRectangle,
                    Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(235, 240, 236),
                AutoSize = true,
                Location = new Point(30, 3),
                BackColor = Color.Transparent
            };

            row.Controls.AddRange(new Control[] { check, lbl });
            return row;
        }

        // =========================================================
        //  FORM PANEL (right)
        // =========================================================

        private Panel CreateFormPanel(out TextBox username, out TextBox password, out Button loginButton)
        {
            int panelWidth = CardWidth - 4 - BrandWidth;
            int panelHeight = CardHeight - 4;
            const int margin = 54;
            int fieldWidth = panelWidth - margin * 2;

            var panel = new Panel
            {
                Location = new Point(BrandWidth, 0),
                Size = new Size(panelWidth, panelHeight),
                BackColor = Color.Transparent
            };

            var lblWelcome = new Label
            {
                Text = "Welcome back",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = TextPrimaryColor,
                AutoSize = true,
                Location = new Point(margin, 50)
            };
            var lblSubtitle = new Label
            {
                Text = "Sign in to continue to your library",
                Font = new Font("Segoe UI", 10.5F),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(margin, 104)
            };

            var usernameWrapper = CreateModernInput("👤", "Username", false, margin, 152, fieldWidth, out username);
            var passwordWrapper = CreateModernInput("🔒", "Password", true, margin, 220, fieldWidth, out password);

            username.Text = "admin";
            password.Text = "admin123";
            password.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter) AttemptLogin();
            };

            var chkRemember = new CheckBox
            {
                Text = "Remember me",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = TextSecondaryColor,
                AutoSize = true,
                Location = new Point(margin, 286),
                Cursor = Cursors.Hand
            };

            var lblForgot = new Label
            {
                Text = "Forgot password?",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Underline),
                ForeColor = AccentColor,
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            lblForgot.Location = new Point(margin + fieldWidth - lblForgot.PreferredWidth, 287);
            lblForgot.Click += (_, _) =>
                MessageBox.Show("Please contact your library administrator to reset your password.",
                    "Forgot Password", MessageBoxButtons.OK, MessageBoxIcon.Information);

            loginButton = CreateSignInButton(margin, 328, fieldWidth);
            loginButton.Click += (_, _) => AttemptLogin();

            var hintBadge = CreateHintBadge(margin, 396, fieldWidth);

            panel.Controls.AddRange(new Control[]
            {
                lblWelcome, lblSubtitle, usernameWrapper, passwordWrapper,
                chkRemember, lblForgot, loginButton, hintBadge
            });

            return panel;
        }

        private Panel CreateModernInput(string icon, string placeholder, bool isPassword, int x, int y, int width, out TextBox textBox)
        {
            bool focused = false;
            const int height = 50;

            var wrapper = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.Transparent
            };

            wrapper.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = CreateRoundedRectangle(wrapper.ClientRectangle, 10);
                using var fillBrush = new SolidBrush(InputBackgroundColor);
                g.FillPath(fillBrush, path);
                using var borderPen = new Pen(focused ? AccentColor : BorderColor, focused ? 1.6f : 1f);
                g.DrawPath(borderPen, path);
            };

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 11),
                ForeColor = TextSecondaryColor,
                AutoSize = false,
                Size = new Size(34, height),
                Location = new Point(6, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            int toggleWidth = isPassword ? 34 : 8;

            textBox = new TextBox
            {
                PlaceholderText = placeholder,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.None,
                BackColor = InputBackgroundColor,
                ForeColor = TextPrimaryColor,
                Location = new Point(42, 16),
                Size = new Size(width - 42 - toggleWidth, 20)
            };

            var tb = textBox;
            tb.Enter += (s, e) => { focused = true; wrapper.Invalidate(); };
            tb.Leave += (s, e) => { focused = false; wrapper.Invalidate(); };

            wrapper.Controls.Add(lblIcon);
            wrapper.Controls.Add(tb);

            if (isPassword)
            {
                tb.UseSystemPasswordChar = true;
                var lblToggle = new Label
                {
                    Text = "🙈",
                    Font = new Font("Segoe UI Emoji", 10),
                    ForeColor = TextSecondaryColor,
                    AutoSize = false,
                    Size = new Size(30, height),
                    Location = new Point(width - 32, 0),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand
                };
                lblToggle.Click += (s, e) =>
                {
                    tb.UseSystemPasswordChar = !tb.UseSystemPasswordChar;
                    lblToggle.Text = tb.UseSystemPasswordChar ? "🙈" : "👁";
                    tb.Focus();
                };
                wrapper.Controls.Add(lblToggle);
            }

            return wrapper;
        }

        private Button CreateSignInButton(int x, int y, int width)
        {
            var btn = new Button
            {
                Text = "Sign In",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = AccentColor,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(width, 48),
                Location = new Point(x, y),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = CreateRoundedRectangle(btn.ClientRectangle, 10);
                btn.Region = new Region(path);
            };
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(AccentColor, 0.15f);
            btn.MouseDown += (s, e) => btn.BackColor = AccentDarkColor;
            btn.MouseUp += (s, e) => btn.BackColor = ControlPaint.Light(AccentColor, 0.15f);
            btn.MouseLeave += (s, e) => btn.BackColor = AccentColor;
            return btn;
        }

        private Panel CreateHintBadge(int x, int y, int width)
        {
            var badge = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, 32),
                BackColor = Color.Transparent
            };
            badge.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = CreateRoundedRectangle(badge.ClientRectangle, 8);
                using var brush = new SolidBrush(InputBackgroundColor);
                g.FillPath(brush, path);
            };
            var lbl = new Label
            {
                Text = "Demo access — Username: admin  ·  Password: admin123",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = TextSecondaryColor,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            badge.Controls.Add(lbl);
            return badge;
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            if (rect.Width < diameter) diameter = Math.Max(2, rect.Width);
            if (rect.Height < diameter) diameter = Math.Max(2, rect.Height);

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseAllFigures();

            return path;
        }

        private void CenterLoginCard()
        {
            int availableHeight = ClientSize.Height - topChrome.Height;
            loginCard.Location = new Point(
                (ClientSize.Width - loginCard.Width) / 2,
                topChrome.Height + Math.Max(0, (availableHeight - loginCard.Height) / 2)
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

            var originalText = btnLogin.Text;
            btnLogin.Enabled = false;
            btnLogin.Text = "Signing in...";

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
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = originalText;
            }
        }
    }
}