using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KIWIARENAA
{
    public partial class Form3 : Form
    {
        private static readonly HttpClient HttpClientInstance = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(8)
        };

        private static readonly Dictionary<string, Image> ImageCache = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        private readonly Color colorNavyDark = Color.FromArgb(11, 19, 43);
        private readonly Color colorNavyCard = Color.FromArgb(23, 35, 69);
        private readonly Color colorNavyElevated = Color.FromArgb(30, 46, 90);
        private readonly Color colorOrange = Color.FromArgb(240, 84, 35);
        private readonly Color colorGold = Color.FromArgb(245, 158, 11);
        private readonly Color colorTextMuted = Color.FromArgb(148, 163, 184);
        private readonly Color colorBorder = Color.FromArgb(51, 65, 85);

        private readonly Dictionary<string, Button> navButtons = new Dictionary<string, Button>();
        private readonly List<MatchRecord> allMatches = new List<MatchRecord>();
        private readonly List<CartItem> cartItems = new List<CartItem>();
        private readonly List<TicketRecord> purchasedTickets = new List<TicketRecord>();
        private readonly List<string> notifications = new List<string>();

        private string currentSection = "Home";
        private string selectedSportFilter = "All";
        private string selectedVenueFilter = "All";
        private string currentSearchText = string.Empty;
        private bool blackFridayActive = true;

        private string accountName = "Kiwi Fan";
        private string accountEmail = "fan@kiwiarena.co.nz";
        private string accountCity = "Auckland, New Zealand";
        private Image accountAvatar = null;

        public Form3()
        {
            InitializeComponent();
            ConfigureHttpHeaders();
            LoadMatchesFromCsv();
            SeedInitialNotifications();
            BuildSidebarNavigation();
            WireTopBarEvents();
            NavigateTo("Home");
        }

        private void ConfigureHttpHeaders()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            if (!HttpClientInstance.DefaultRequestHeaders.UserAgent.Any())
            {
                HttpClientInstance.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                HttpClientInstance.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://commons.wikimedia.org/");
            }
        }

        private void SeedInitialNotifications()
        {
            AddNotification("Welcome to KIWIARENA! 105 New Zealand matches are live.");
            AddNotification("🔥 Black Friday Sale active: 25% OFF eligible NZ event tickets.");
        }

        private void AddNotification(string message)
        {
            string stamp = DateTime.Now.ToString("hh:mm tt");
            notifications.Insert(0, "[" + stamp + "] " + message);
            UpdateNavBadges();
        }

        private string ResolveMatchesCsvPath()
        {
            string[] candidateRoots =
            {
                AppDomain.CurrentDomain.BaseDirectory,
                Directory.GetCurrentDirectory()
            };

            foreach (string root in candidateRoots)
            {
                DirectoryInfo current = new DirectoryInfo(root);
                for (int depth = 0; depth < 7 && current != null; depth++)
                {
                    string directCandidate = Path.Combine(current.FullName, "Data", "matches.csv");
                    if (File.Exists(directCandidate))
                    {
                        return directCandidate;
                    }

                    string nestedProjectCandidate = Path.Combine(current.FullName, "KIWIARENAA", "Data", "matches.csv");
                    if (File.Exists(nestedProjectCandidate))
                    {
                        return nestedProjectCandidate;
                    }

                    current = current.Parent;
                }
            }

            return string.Empty;
        }

        private void LoadMatchesFromCsv()
        {
            allMatches.Clear();
            string csvPath = ResolveMatchesCsvPath();

            if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath))
            {
                AddNotification("Warning: Data\\matches.csv was not found near project folder.");
                return;
            }

            string[] lines = File.ReadAllLines(csvPath);
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] cols = line.Split(',');
                if (cols.Length < 14)
                {
                    continue;
                }

                int id = int.TryParse(cols[0].Trim(), out int parsedId) ? parsedId : i;
                decimal price = decimal.TryParse(cols[9].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedPrice)
                    ? parsedPrice
                    : 39.00m;

                allMatches.Add(new MatchRecord
                {
                    Id = id,
                    Sport = cols[1].Trim(),
                    Symbol = cols[2].Trim(),
                    Title = cols[3].Trim(),
                    Teams = cols[4].Trim(),
                    Venue = cols[5].Trim(),
                    City = cols[6].Trim(),
                    Date = cols[7].Trim(),
                    Time = cols[8].Trim(),
                    Price = price,
                    IsFeatured = string.Equals(cols[10].Trim(), "Yes", StringComparison.OrdinalIgnoreCase),
                    HighlightPlayer = cols[11].Trim(),
                    ImageUrl = cols[12].Trim(),
                    PlayerImageUrl = cols[13].Trim()
                });
            }
        }

        private void BuildSidebarNavigation()
        {
            flowNavButtons.Controls.Clear();
            navButtons.Clear();

            string[] sections =
            {
                "Home",
                "Sports",
                "Matches",
                "My Cart",
                "My Tickets",
                "Notifications",
                "My Account"
            };

            string[] icons =
            {
                "🏠",
                "🏆",
                "🎟️",
                "🛒",
                "🎫",
                "🔔",
                "👤"
            };

            for (int i = 0; i < sections.Length; i++)
            {
                string sec = sections[i];
                Button btn = new Button
                {
                    Size = new Size(220, 44),
                    Margin = new Padding(0, 0, 0, 6),
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = colorNavyDark,
                    Cursor = Cursors.Hand,
                    Tag = sec,
                    Text = "  " + icons[i] + "   " + sec
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, e) => NavigateTo(sec);

                flowNavButtons.Controls.Add(btn);
                navButtons[sec] = btn;
            }

            UpdateNavBadges();
        }

        private void UpdateNavBadges()
        {
            int cartCount = cartItems.Sum(x => x.Quantity);
            int ticketCount = purchasedTickets.Sum(x => x.Quantity);
            int notifCount = notifications.Count;

            if (navButtons.ContainsKey("My Cart"))
            {
                navButtons["My Cart"].Text = cartCount > 0
                    ? $"  🛒   My Cart ({cartCount})"
                    : "  🛒   My Cart";
            }

            if (navButtons.ContainsKey("My Tickets"))
            {
                navButtons["My Tickets"].Text = ticketCount > 0
                    ? $"  🎫   My Tickets ({ticketCount})"
                    : "  🎫   My Tickets";
            }

            if (navButtons.ContainsKey("Notifications"))
            {
                navButtons["Notifications"].Text = notifCount > 0
                    ? $"  🔔   Notifications ({notifCount})"
                    : "  🔔   Notifications";
            }

            lblSidebarUser.Text = $"{accountName} • {allMatches.Count} NZ Matches";
        }

        private void WireTopBarEvents()
        {
            btnQuickSearch.Click += (s, e) =>
            {
                currentSearchText = txtQuickSearch.Text.Trim();
                NavigateTo("Matches");
            };

            txtQuickSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    currentSearchText = txtQuickSearch.Text.Trim();
                    NavigateTo("Matches");
                }
            };

            btnBlackFridayBadge.Click += (s, e) =>
            {
                blackFridayActive = !blackFridayActive;
                btnBlackFridayBadge.Text = blackFridayActive
                    ? "🔥 Black Friday 25% OFF"
                    : "Black Friday OFF (Click)";
                btnBlackFridayBadge.BackColor = blackFridayActive ? colorGold : colorNavyElevated;
                btnBlackFridayBadge.ForeColor = blackFridayActive ? colorNavyDark : Color.White;
                AddNotification(blackFridayActive
                    ? "Black Friday 25% discount enabled on tickets."
                    : "Black Friday 25% discount paused.");
                NavigateTo(currentSection);
            };
        }

        private void NavigateTo(string section)
        {
            currentSection = section;

            foreach (KeyValuePair<string, Button> kvp in navButtons)
            {
                bool active = string.Equals(kvp.Key, section, StringComparison.OrdinalIgnoreCase);
                kvp.Value.BackColor = active ? colorOrange : colorNavyDark;
                kvp.Value.ForeColor = Color.White;
            }

            panelContentHost.SuspendLayout();
            panelContentHost.Controls.Clear();

            switch (section)
            {
                case "Home":
                    lblPageTitle.Text = "Home";
                    lblPageSubtitle.Text = "Featured matches, About KIWIARENA, Blogs & News, Highlights, Players & Black Friday Sale";
                    BuildHomeView();
                    break;

                case "Sports":
                    lblPageTitle.Text = "New Zealand Sports";
                    lblPageSubtitle.Text = "Explore all sports across Aotearoa with icons and click any sport to view matches";
                    BuildSportsView();
                    break;

                case "Matches":
                    lblPageTitle.Text = "All New Zealand Matches";
                    lblPageSubtitle.Text = $"Browse {allMatches.Count} matches — search and filter by sport, venue/city, or match name";
                    BuildMatchesView();
                    break;

                case "My Cart":
                    lblPageTitle.Text = "My Cart";
                    lblPageSubtitle.Text = "Review your selected unpurchased tickets before checkout";
                    BuildCartView();
                    break;

                case "My Tickets":
                    lblPageTitle.Text = "My Tickets";
                    lblPageSubtitle.Text = "Your confirmed and purchased KIWIARENA match tickets";
                    BuildTicketsView();
                    break;

                case "Notifications":
                    lblPageTitle.Text = "Notifications";
                    lblPageSubtitle.Text = "Real-time activity updates for tickets, cart, sales, and profile";
                    BuildNotificationsView();
                    break;

                case "My Account":
                    lblPageTitle.Text = "My Account";
                    lblPageSubtitle.Text = "Manage your profile details, edit your name, and upload your profile photo";
                    BuildAccountView();
                    break;
            }

            UpdateNavBadges();
            panelContentHost.ResumeLayout(true);
        }

        // ==================== 1. HOME VIEW ====================
        private void BuildHomeView()
        {
            FlowLayoutPanel root = CreatePageFlowContainer();

            Panel bfBanner = new Panel
            {
                Size = new Size(1180, 110),
                Margin = new Padding(0, 0, 0, 18),
                BackColor = blackFridayActive ? Color.FromArgb(124, 45, 18) : colorNavyCard
            };
            bfBanner.Paint += (s, e) =>
            {
                using (Pen p = new Pen(blackFridayActive ? colorGold : colorBorder, 2))
                {
                    e.Graphics.DrawRectangle(p, 1, 1, bfBanner.Width - 2, bfBanner.Height - 2);
                }
            };

            Label lblBfTitle = new Label
            {
                Location = new Point(22, 16),
                Size = new Size(820, 34),
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = colorGold,
                Text = blackFridayActive
                    ? "🔥 BLACK FRIDAY SALE — 25% OFF ALL NZ MATCH TICKETS!"
                    : "BLACK FRIDAY SALE PAUSED — CLICK TO ACTIVATE 25% OFF"
            };

            Label lblBfDesc = new Label
            {
                Location = new Point(24, 54),
                Size = new Size(820, 36),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.White,
                Text = "Save 25% automatically across Rugby Union, Cricket, Football, Basketball, Netball, Rugby League, Motorsport & Tennis."
            };

            Button btnBrowseBf = new Button
            {
                Location = new Point(940, 32),
                Size = new Size(210, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = colorOrange,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Text = "Shop Sale Matches →"
            };
            btnBrowseBf.FlatAppearance.BorderSize = 0;
            btnBrowseBf.Click += (s, e) => NavigateTo("Matches");

            bfBanner.Controls.AddRange(new Control[] { lblBfTitle, lblBfDesc, btnBrowseBf });
            root.Controls.Add(bfBanner);

            root.Controls.Add(CreateSectionHeader("⭐ Featured Matches", "Top stadium fixtures across Auckland, Wellington, Christchurch, Hamilton & Dunedin"));
            FlowLayoutPanel featuredRow = new FlowLayoutPanel
            {
                Size = new Size(1200, 340),
                Margin = new Padding(0, 0, 0, 20),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent
            };

            List<MatchRecord> featured = allMatches.Where(m => m.IsFeatured).Take(4).ToList();
            foreach (MatchRecord m in featured)
            {
                featuredRow.Controls.Add(CreateMatchCard(m));
            }
            root.Controls.Add(featuredRow);

            root.Controls.Add(CreateSectionHeader("🌟 Highlight Players", "Star athletes lighting up New Zealand arenas & stadiums"));
            FlowLayoutPanel playersRow = new FlowLayoutPanel
            {
                Size = new Size(1200, 225),
                Margin = new Padding(0, 0, 0, 20),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent
            };

            List<MatchRecord> playerShowcases = allMatches
                .GroupBy(m => m.HighlightPlayer)
                .Select(g => g.First())
                .Take(5)
                .ToList();

            foreach (MatchRecord m in playerShowcases)
            {
                playersRow.Controls.Add(CreatePlayerCard(m));
            }
            root.Controls.Add(playersRow);

            FlowLayoutPanel infoRow = new FlowLayoutPanel
            {
                Size = new Size(1200, 260),
                Margin = new Padding(0, 0, 0, 20),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent
            };

            infoRow.Controls.Add(CreateHighlightMatchesBox());
            infoRow.Controls.Add(CreateBlogsNewsBox());
            root.Controls.Add(infoRow);

            root.Controls.Add(CreateAboutKiwiArenaBox());

            panelContentHost.Controls.Add(root);
        }

        private Panel CreateHighlightMatchesBox()
        {
            Panel box = new Panel
            {
                Size = new Size(585, 245),
                Margin = new Padding(0, 0, 16, 0),
                BackColor = colorNavyCard
            };
            box.Paint += DrawCardBorder;

            Label title = new Label
            {
                Location = new Point(18, 14),
                Size = new Size(540, 28),
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = colorGold,
                Text = "⚡ Highlight Matches"
            };
            box.Controls.Add(title);

            List<MatchRecord> topHighlights = allMatches.Where(m => m.IsFeatured).Skip(4).Take(4).ToList();
            int y = 50;
            foreach (MatchRecord m in topHighlights)
            {
                Label row = new Label
                {
                    Location = new Point(18, y),
                    Size = new Size(548, 40),
                    Font = new Font("Segoe UI", 9.5F),
                    ForeColor = Color.White,
                    Text = $"{m.Symbol} {m.Title} ({m.Teams}) — {m.Venue}, {m.City} [{m.Date}]"
                };
                box.Controls.Add(row);
                y += 45;
            }

            return box;
        }

        private Panel CreateBlogsNewsBox()
        {
            Panel box = new Panel
            {
                Size = new Size(580, 245),
                Margin = new Padding(0),
                BackColor = colorNavyCard
            };
            box.Paint += DrawCardBorder;

            Label title = new Label
            {
                Location = new Point(18, 14),
                Size = new Size(540, 28),
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = colorOrange,
                Text = "📰 Blogs & News"
            };
            box.Controls.Add(title);

            string[] newsItems =
            {
                "• Eden Park & Sky Stadium announce expanded fan zones for summer fixtures.",
                "• Super Rugby Pacific & A-League NZ derby ticket demand surges nationwide.",
                "• ASB Classic & Summer T20 fixtures bring top international stars to NZ.",
                "• Digital matchday pass tips: instant access inside My Tickets."
            };

            int y = 52;
            foreach (string item in newsItems)
            {
                Label lbl = new Label
                {
                    Location = new Point(18, y),
                    Size = new Size(542, 42),
                    Font = new Font("Segoe UI", 9.5F),
                    ForeColor = Color.FromArgb(226, 232, 240),
                    Text = item
                };
                box.Controls.Add(lbl);
                y += 45;
            }

            return box;
        }

        private Panel CreateAboutKiwiArenaBox()
        {
            Panel box = new Panel
            {
                Size = new Size(1180, 145),
                Margin = new Padding(0, 0, 0, 24),
                BackColor = colorNavyCard
            };
            box.Paint += DrawCardBorder;

            PictureBox picBadge = new PictureBox
            {
                Location = new Point(18, 18),
                Size = new Size(110, 108),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            picBadge.Image = CreateFallbackSportsGraphic("KIWI", "ARENA", "🏟️", colorOrange);

            Label lblHeading = new Label
            {
                Location = new Point(146, 18),
                Size = new Size(980, 30),
                Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Text = "About KIWIARENA App"
            };

            Label lblBody = new Label
            {
                Location = new Point(146, 52),
                Size = new Size(1005, 75),
                Font = new Font("Segoe UI", 9.8F),
                ForeColor = colorTextMuted,
                Text = "KIWIARENA is your all-in-one New Zealand sports ticketing and matchday companion. Browse 105+ fixtures across Rugby Union, Cricket, Football, Basketball, Netball, Rugby League, Motorsport, and Tennis, filter by venue or sport, save tickets in your cart, view purchased passes, and track live notifications."
            };

            box.Controls.AddRange(new Control[] { picBadge, lblHeading, lblBody });
            return box;
        }

        // ==================== 2. SPORTS VIEW ====================
        private void BuildSportsView()
        {
            FlowLayoutPanel root = CreatePageFlowContainer();
            root.Controls.Add(CreateSectionHeader("🏆 All New Zealand Sports", "Select any sport card to filter matches for that sport"));

            var sportsList = new[]
            {
                new { Name = "Rugby Union", Symbol = "🏉", Desc = "Super Rugby Pacific, All Blacks, Black Ferns & NPC", Color = Color.FromArgb(15, 23, 42) },
                new { Name = "Cricket", Symbol = "🏏", Desc = "Black Caps, White Ferns, Super Smash & Test Series", Color = Color.FromArgb(20, 83, 45) },
                new { Name = "Football", Symbol = "⚽", Desc = "Auckland FC, Wellington Phoenix, All Whites & Cups", Color = Color.FromArgb(30, 58, 138) },
                new { Name = "Basketball", Symbol = "🏀", Desc = "NZ Breakers, Tall Blacks, Tall Ferns & NZNBL", Color = Color.FromArgb(124, 45, 18) },
                new { Name = "Netball", Symbol = "🏐", Desc = "Silver Ferns, ANZ Premiership & Constellation Cup", Color = Color.FromArgb(131, 24, 67) },
                new { Name = "Rugby League", Symbol = "🏈", Desc = "NZ Warriors, NZ Kiwis & Kiwi Ferns fixtures", Color = Color.FromArgb(88, 28, 135) },
                new { Name = "Motorsport", Symbol = "🏎️", Desc = "Taupo 400, NZ Grand Prix, Highlands & D1NZ", Color = Color.FromArgb(127, 29, 29) },
                new { Name = "Tennis", Symbol = "🎾", Desc = "ASB Classic ATP/WTA, Davis Cup & NZ Opens", Color = Color.FromArgb(19, 78, 74) }
            };

            FlowLayoutPanel grid = new FlowLayoutPanel
            {
                Size = new Size(1200, 620),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent
            };

            foreach (var sp in sportsList)
            {
                int matchCount = allMatches.Count(m => string.Equals(m.Sport, sp.Name, StringComparison.OrdinalIgnoreCase));

                Panel card = new Panel
                {
                    Size = new Size(280, 255),
                    Margin = new Padding(0, 0, 16, 16),
                    BackColor = colorNavyCard,
                    Cursor = Cursors.Hand
                };
                card.Paint += DrawCardBorder;

                PictureBox iconPic = new PictureBox
                {
                    Location = new Point(16, 16),
                    Size = new Size(74, 74),
                    SizeMode = PictureBoxSizeMode.Zoom
                };
                iconPic.Image = CreateFallbackSportsGraphic(sp.Name, sp.Symbol, sp.Symbol, sp.Color);

                Label lblCount = new Label
                {
                    Location = new Point(104, 24),
                    Size = new Size(155, 24),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = colorGold,
                    Text = $"{matchCount} Matches"
                };

                Label lblName = new Label
                {
                    Location = new Point(16, 100),
                    Size = new Size(248, 28),
                    Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Text = $"{sp.Symbol}  {sp.Name}"
                };

                Label lblDesc = new Label
                {
                    Location = new Point(16, 132),
                    Size = new Size(248, 56),
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = colorTextMuted,
                    Text = sp.Desc
                };

                Button btnViewSport = new Button
                {
                    Location = new Point(16, 198),
                    Size = new Size(248, 38),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = colorOrange,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Text = $"View {sp.Name} →"
                };
                btnViewSport.FlatAppearance.BorderSize = 0;
                btnViewSport.Click += (s, e) =>
                {
                    selectedSportFilter = sp.Name;
                    NavigateTo("Matches");
                };

                card.Controls.AddRange(new Control[] { iconPic, lblCount, lblName, lblDesc, btnViewSport });
                grid.Controls.Add(card);
            }

            root.Controls.Add(grid);
            panelContentHost.Controls.Add(root);
        }

        // ==================== 3. MATCHES VIEW (SEARCH + FILTERS + 105 MATCHES) ====================
        private void BuildMatchesView()
        {
            Panel pageHost = new Panel
            {
                Location = new Point(20, 16),
                Size = new Size(1210, 1800),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            Panel filterBar = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1180, 72),
                BackColor = colorNavyCard
            };
            filterBar.Paint += DrawCardBorder;

            TextBox txtMatchSearch = new TextBox
            {
                Location = new Point(16, 19),
                Size = new Size(320, 32),
                BackColor = colorNavyElevated,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                Text = currentSearchText
            };

            ComboBox cmbSport = new ComboBox
            {
                Location = new Point(350, 19),
                Size = new Size(220, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = colorNavyElevated,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F)
            };
            cmbSport.Items.Add("All");
            foreach (string sName in allMatches.Select(m => m.Sport).Distinct().OrderBy(x => x))
            {
                cmbSport.Items.Add(sName);
            }
            cmbSport.SelectedItem = cmbSport.Items.Contains(selectedSportFilter) ? selectedSportFilter : "All";

            ComboBox cmbVenue = new ComboBox
            {
                Location = new Point(584, 19),
                Size = new Size(270, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = colorNavyElevated,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F)
            };
            cmbVenue.Items.Add("All");
            foreach (string vName in allMatches.Select(m => $"{m.Venue} ({m.City})").Distinct().OrderBy(x => x))
            {
                cmbVenue.Items.Add(vName);
            }
            cmbVenue.SelectedItem = cmbVenue.Items.Contains(selectedVenueFilter) ? selectedVenueFilter : "All";

            Button btnApplyFilter = new Button
            {
                Location = new Point(868, 19),
                Size = new Size(140, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = colorOrange,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Text = "Filter Matches"
            };
            btnApplyFilter.FlatAppearance.BorderSize = 0;

            Button btnResetFilter = new Button
            {
                Location = new Point(1018, 19),
                Size = new Size(140, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = colorNavyElevated,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Text = "Reset Filters"
            };
            btnResetFilter.FlatAppearance.BorderSize = 0;

            filterBar.Controls.AddRange(new Control[] { txtMatchSearch, cmbSport, cmbVenue, btnApplyFilter, btnResetFilter });

            Label lblCountHeader = new Label
            {
                Location = new Point(0, 84),
                Size = new Size(1180, 28),
                Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Text = $"Showing {allMatches.Count} of {allMatches.Count} Matches"
            };

            Label lblCountSub = new Label
            {
                Location = new Point(0, 114),
                Size = new Size(1180, 22),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = colorTextMuted,
                Text = "Click 'Add to Cart' to save tickets for checkout"
            };

            FlowLayoutPanel resultsGrid = new FlowLayoutPanel
            {
                Location = new Point(0, 144),
                Size = new Size(1200, 1600),
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent
            };

            Action renderFilteredMatches = () =>
            {
                currentSearchText = txtMatchSearch.Text.Trim();
                txtQuickSearch.Text = currentSearchText;
                selectedSportFilter = cmbSport.SelectedItem?.ToString() ?? "All";
                selectedVenueFilter = cmbVenue.SelectedItem?.ToString() ?? "All";

                IEnumerable<MatchRecord> query = allMatches;

                if (!string.Equals(selectedSportFilter, "All", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(m => string.Equals(m.Sport, selectedSportFilter, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.Equals(selectedVenueFilter, "All", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(m => string.Equals($"{m.Venue} ({m.City})", selectedVenueFilter, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(currentSearchText))
                {
                    string q = currentSearchText.ToLowerInvariant();
                    query = query.Where(m =>
                        m.Title.ToLowerInvariant().Contains(q) ||
                        m.Teams.ToLowerInvariant().Contains(q) ||
                        m.Sport.ToLowerInvariant().Contains(q) ||
                        m.Venue.ToLowerInvariant().Contains(q) ||
                        m.City.ToLowerInvariant().Contains(q) ||
                        m.HighlightPlayer.ToLowerInvariant().Contains(q));
                }

                List<MatchRecord> matchedList = query.ToList();
                lblCountHeader.Text = $"Showing {matchedList.Count} of {allMatches.Count} Matches";
                resultsGrid.Controls.Clear();

                if (matchedList.Count == 0)
                {
                    resultsGrid.Controls.Add(new Label
                    {
                        Size = new Size(1160, 60),
                        Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                        ForeColor = colorGold,
                        Text = "No matches found for your current search or filter selection."
                    });
                    return;
                }

                foreach (MatchRecord m in matchedList)
                {
                    resultsGrid.Controls.Add(CreateMatchCard(m));
                }
            };

            btnApplyFilter.Click += (s, e) => renderFilteredMatches();
            txtMatchSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    renderFilteredMatches();
                }
            };
            btnResetFilter.Click += (s, e) =>
            {
                txtMatchSearch.Text = string.Empty;
                txtQuickSearch.Text = string.Empty;
                currentSearchText = string.Empty;
                selectedSportFilter = "All";
                selectedVenueFilter = "All";
                cmbSport.SelectedItem = "All";
                cmbVenue.SelectedItem = "All";
                renderFilteredMatches();
            };

            pageHost.Controls.AddRange(new Control[] { filterBar, lblCountHeader, lblCountSub, resultsGrid });
            panelContentHost.Controls.Add(pageHost);
            renderFilteredMatches();
        }

        // ==================== 4. MY CART VIEW ====================
        private void BuildCartView()
        {
            FlowLayoutPanel root = CreatePageFlowContainer();
            root.Controls.Add(CreateSectionHeader("🛒 My Cart", "Unpurchased tickets saved in your cart"));

            if (cartItems.Count == 0)
            {
                Panel emptyPanel = new Panel
                {
                    Size = new Size(1180, 140),
                    BackColor = colorNavyCard
                };
                emptyPanel.Paint += DrawCardBorder;

                Label lblEmpty = new Label
                {
                    Location = new Point(22, 24),
                    Size = new Size(700, 32),
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Text = "Your cart is currently empty."
                };

                Button btnGoMatches = new Button
                {
                    Location = new Point(22, 68),
                    Size = new Size(190, 40),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = colorOrange,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Text = "Browse NZ Matches →"
                };
                btnGoMatches.FlatAppearance.BorderSize = 0;
                btnGoMatches.Click += (s, e) => NavigateTo("Matches");

                emptyPanel.Controls.AddRange(new Control[] { lblEmpty, btnGoMatches });
                root.Controls.Add(emptyPanel);
                panelContentHost.Controls.Add(root);
                return;
            }

            decimal subtotal = cartItems.Sum(i => i.UnitPrice * i.Quantity);
            decimal discount = blackFridayActive ? Math.Round(subtotal * 0.25m, 2) : 0m;
            decimal total = subtotal - discount;

            foreach (CartItem item in cartItems.ToList())
            {
                Panel row = new Panel
                {
                    Size = new Size(1180, 96),
                    Margin = new Padding(0, 0, 0, 12),
                    BackColor = colorNavyCard
                };
                row.Paint += DrawCardBorder;

                Label lblInfo = new Label
                {
                    Location = new Point(20, 14),
                    Size = new Size(680, 28),
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Text = $"{item.Match.Symbol} {item.Match.Title} ({item.Match.Teams})"
                };

                Label lblSub = new Label
                {
                    Location = new Point(20, 46),
                    Size = new Size(680, 28),
                    Font = new Font("Segoe UI", 9.5F),
                    ForeColor = colorTextMuted,
                    Text = $"{item.Match.Venue}, {item.Match.City} • {item.Match.Date} {item.Match.Time} • ${item.UnitPrice:0.00} NZD each"
                };

                Button btnMinus = new Button
                {
                    Location = new Point(730, 30),
                    Size = new Size(36, 34),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = colorNavyElevated,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    Text = "-"
                };
                btnMinus.FlatAppearance.BorderSize = 0;
                btnMinus.Click += (s, e) =>
                {
                    if (item.Quantity > 1)
                    {
                        item.Quantity--;
                    }
                    else
                    {
                        cartItems.Remove(item);
                    }
                    AddNotification($"Updated cart quantity for '{item.Match.Title}'.");
                    NavigateTo("My Cart");
                };

                Label lblQty = new Label
                {
                    Location = new Point(772, 30),
                    Size = new Size(46, 34),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Text = item.Quantity.ToString()
                };

                Button btnPlus = new Button
                {
                    Location = new Point(824, 30),
                    Size = new Size(36, 34),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = colorNavyElevated,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    Text = "+"
                };
                btnPlus.FlatAppearance.BorderSize = 0;
                btnPlus.Click += (s, e) =>
                {
                    item.Quantity++;
                    AddNotification($"Added another ticket for '{item.Match.Title}' to cart.");
                    NavigateTo("My Cart");
                };

                Label lblLineTotal = new Label
                {
                    Location = new Point(880, 30),
                    Size = new Size(150, 34),
                    TextAlign = ContentAlignment.MiddleRight,
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    ForeColor = colorGold,
                    Text = $"${item.UnitPrice * item.Quantity:0.00} NZD"
                };

                Button btnRemove = new Button
                {
                    Location = new Point(1050, 30),
                    Size = new Size(105, 34),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(153, 27, 27),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Text = "Remove"
                };
                btnRemove.FlatAppearance.BorderSize = 0;
                btnRemove.Click += (s, e) =>
                {
                    cartItems.Remove(item);
                    AddNotification($"Removed '{item.Match.Title}' from cart.");
                    NavigateTo("My Cart");
                };

                row.Controls.AddRange(new Control[] { lblInfo, lblSub, btnMinus, lblQty, btnPlus, lblLineTotal, btnRemove });
                root.Controls.Add(row);
            }

            Panel summary = new Panel
            {
                Size = new Size(1180, 125),
                Margin = new Padding(0, 8, 0, 24),
                BackColor = colorNavyCard
            };
            summary.Paint += DrawCardBorder;

            Label lblSummaryText = new Label
            {
                Location = new Point(22, 18),
                Size = new Size(780, 86),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Text = $"Subtotal: ${subtotal:0.00} NZD\n" +
                       $"Black Friday Discount (25%): -${discount:0.00} NZD\n" +
                       $"Total Due: ${total:0.00} NZD"
            };

            Button btnCheckout = new Button
            {
                Location = new Point(920, 38),
                Size = new Size(230, 48),
                FlatStyle = FlatStyle.Flat,
                BackColor = colorOrange,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Text = "Checkout & Get Tickets"
            };
            btnCheckout.FlatAppearance.BorderSize = 0;
            btnCheckout.Click += (s, e) =>
            {
                foreach (CartItem item in cartItems)
                {
                    decimal itemTotal = item.UnitPrice * item.Quantity;
                    if (blackFridayActive)
                    {
                        itemTotal = Math.Round(itemTotal * 0.75m, 2);
                    }

                    purchasedTickets.Insert(0, new TicketRecord
                    {
                        TicketCode = "KA-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant(),
                        Match = item.Match,
                        Quantity = item.Quantity,
                        TotalPaid = itemTotal,
                        PurchasedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
                    });
                }

                int totalQty = cartItems.Sum(x => x.Quantity);
                cartItems.Clear();

                AddNotification($"Ticket purchase confirmed! {totalQty} ticket(s) moved to My Tickets.");
                MessageBox.Show($"Purchase complete! {totalQty} ticket(s) are now available in My Tickets.",
                    "KIWIARENA Checkout", MessageBoxButtons.OK, MessageBoxIcon.Information);
                NavigateTo("My Tickets");
            };

            summary.Controls.AddRange(new Control[] { lblSummaryText, btnCheckout });
            root.Controls.Add(summary);

            panelContentHost.Controls.Add(root);
        }

        // ==================== 5. MY TICKETS VIEW ====================
        private void BuildTicketsView()
        {
            FlowLayoutPanel root = CreatePageFlowContainer();
            root.Controls.Add(CreateSectionHeader("🎫 My Tickets", "All purchased tickets ready for matchday entry"));

            if (purchasedTickets.Count == 0)
            {
                Panel emptyPanel = new Panel
                {
                    Size = new Size(1180, 120),
                    BackColor = colorNavyCard
                };
                emptyPanel.Paint += DrawCardBorder;

                Label lblEmpty = new Label
                {
                    Location = new Point(22, 42),
                    Size = new Size(900, 32),
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    ForeColor = colorTextMuted,
                    Text = "No purchased tickets yet. Add matches to My Cart and complete checkout to see your tickets here."
                };

                emptyPanel.Controls.Add(lblEmpty);
                root.Controls.Add(emptyPanel);
                panelContentHost.Controls.Add(root);
                return;
            }

            foreach (TicketRecord t in purchasedTickets)
            {
                Panel card = new Panel
                {
                    Size = new Size(1180, 108),
                    Margin = new Padding(0, 0, 0, 12),
                    BackColor = colorNavyCard
                };
                card.Paint += DrawCardBorder;

                Label lblCode = new Label
                {
                    Location = new Point(20, 14),
                    Size = new Size(240, 26),
                    Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                    ForeColor = colorGold,
                    Text = $"PASS ID: {t.TicketCode}"
                };

                Label lblMatch = new Label
                {
                    Location = new Point(20, 42),
                    Size = new Size(760, 28),
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Text = $"{t.Match.Symbol} {t.Match.Title} — {t.Match.Teams} ({t.Quantity}x)"
                };

                Label lblMeta = new Label
                {
                    Location = new Point(20, 72),
                    Size = new Size(880, 24),
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = colorTextMuted,
                    Text = $"Venue: {t.Match.Venue}, {t.Match.City} | Match Date: {t.Match.Date} {t.Match.Time} | Purchased: {t.PurchasedAt}"
                };

                Label lblPaid = new Label
                {
                    Location = new Point(920, 36),
                    Size = new Size(230, 34),
                    TextAlign = ContentAlignment.MiddleRight,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(74, 222, 128),
                    Text = $"PAID ${t.TotalPaid:0.00} NZD"
                };

                card.Controls.AddRange(new Control[] { lblCode, lblMatch, lblMeta, lblPaid });
                root.Controls.Add(card);
            }

            panelContentHost.Controls.Add(root);
        }

        // ==================== 6. NOTIFICATIONS VIEW ====================
        private void BuildNotificationsView()
        {
            FlowLayoutPanel root = CreatePageFlowContainer();
            root.Controls.Add(CreateSectionHeader("🔔 Notifications", "Activity log for your actions across KIWIARENA"));

            Button btnClear = new Button
            {
                Size = new Size(160, 36),
                Margin = new Padding(0, 0, 0, 14),
                FlatStyle = FlatStyle.Flat,
                BackColor = colorNavyElevated,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Text = "Clear Notifications"
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, e) =>
            {
                notifications.Clear();
                NavigateTo("Notifications");
            };
            root.Controls.Add(btnClear);

            if (notifications.Count == 0)
            {
                Label lblNone = new Label
                {
                    Size = new Size(1180, 40),
                    Font = new Font("Segoe UI", 10.5F),
                    ForeColor = colorTextMuted,
                    Text = "No notifications right now."
                };
                root.Controls.Add(lblNone);
            }
            else
            {
                foreach (string note in notifications)
                {
                    Panel row = new Panel
                    {
                        Size = new Size(1180, 54),
                        Margin = new Padding(0, 0, 0, 10),
                        BackColor = colorNavyCard
                    };
                    row.Paint += DrawCardBorder;

                    Label lbl = new Label
                    {
                        Location = new Point(18, 14),
                        Size = new Size(1140, 26),
                        Font = new Font("Segoe UI", 10F),
                        ForeColor = Color.White,
                        Text = note
                    };
                    row.Controls.Add(lbl);
                    root.Controls.Add(row);
                }
            }

            panelContentHost.Controls.Add(root);
        }

        // ==================== 7. MY ACCOUNT VIEW ====================
        private void BuildAccountView()
        {
            FlowLayoutPanel root = CreatePageFlowContainer();
            root.Controls.Add(CreateSectionHeader("👤 My Account", "View account details, edit your display name, and upload your photo"));

            Panel profileCard = new Panel
            {
                Size = new Size(1180, 360),
                BackColor = colorNavyCard
            };
            profileCard.Paint += DrawCardBorder;

            PictureBox picAvatar = new PictureBox
            {
                Location = new Point(26, 26),
                Size = new Size(140, 140),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = accountAvatar ?? CreateFallbackSportsGraphic(accountName, "MEMBER", "👤", colorOrange)
            };

            Button btnUploadPhoto = new Button
            {
                Location = new Point(26, 178),
                Size = new Size(140, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = colorNavyElevated,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Text = "Add / Change Photo"
            };
            btnUploadPhoto.FlatAppearance.BorderSize = 0;

            Label lblNameField = new Label
            {
                Location = new Point(196, 28),
                Size = new Size(300, 24),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = colorTextMuted,
                Text = "Display Name"
            };

            TextBox txtEditName = new TextBox
            {
                Location = new Point(196, 56),
                Size = new Size(360, 34),
                BackColor = colorNavyElevated,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5F),
                Text = accountName
            };

            Label lblEmailInfo = new Label
            {
                Location = new Point(196, 108),
                Size = new Size(520, 26),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.White,
                Text = $"Email: {accountEmail}"
            };

            Label lblCityInfo = new Label
            {
                Location = new Point(196, 138),
                Size = new Size(520, 26),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.White,
                Text = $"Location: {accountCity}"
            };

            Label lblStatsInfo = new Label
            {
                Location = new Point(196, 168),
                Size = new Size(650, 26),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = colorGold,
                Text = $"Cart Items: {cartItems.Sum(x => x.Quantity)}  •  Purchased Tickets: {purchasedTickets.Sum(x => x.Quantity)}"
            };

            Button btnSaveAccount = new Button
            {
                Location = new Point(196, 215),
                Size = new Size(180, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = colorOrange,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Text = "Save Changes"
            };
            btnSaveAccount.FlatAppearance.BorderSize = 0;

            btnUploadPhoto.Click += (s, e) =>
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = "Select Profile Photo";
                    ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        using (Image temp = Image.FromFile(ofd.FileName))
                        {
                            accountAvatar = new Bitmap(temp);
                        }
                        picAvatar.Image = accountAvatar;
                        AddNotification("Profile photo updated in My Account.");
                        MessageBox.Show("Profile photo updated!", "My Account", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };

            btnSaveAccount.Click += (s, e) =>
            {
                string updated = txtEditName.Text.Trim();
                if (string.IsNullOrEmpty(updated))
                {
                    MessageBox.Show("Name cannot be empty.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                accountName = updated;
                if (accountAvatar == null)
                {
                    picAvatar.Image = CreateFallbackSportsGraphic(accountName, "MEMBER", "👤", colorOrange);
                }
                UpdateNavBadges();
                AddNotification($"Account display name updated to '{accountName}'.");
                MessageBox.Show("Account details saved!", "My Account", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            profileCard.Controls.AddRange(new Control[]
            {
                picAvatar, btnUploadPhoto, lblNameField, txtEditName,
                lblEmailInfo, lblCityInfo, lblStatsInfo, btnSaveAccount
            });

            root.Controls.Add(profileCard);
            panelContentHost.Controls.Add(root);
        }

        // ==================== SHARED CARDS & IMAGE HELPERS ====================
        private FlowLayoutPanel CreatePageFlowContainer()
        {
            return new FlowLayoutPanel
            {
                Location = new Point(22, 18),
                Size = new Size(1220, 2000),
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent
            };
        }

        private Panel CreateSectionHeader(string heading, string subtext)
        {
            Panel p = new Panel
            {
                Size = new Size(1180, 56),
                Margin = new Padding(0, 0, 0, 12),
                BackColor = Color.Transparent
            };

            Label lblTitle = new Label
            {
                Location = new Point(0, 0),
                Size = new Size(1160, 30),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Text = heading
            };

            Label lblSub = new Label
            {
                Location = new Point(0, 30),
                Size = new Size(1160, 22),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = colorTextMuted,
                Text = subtext
            };

            p.Controls.AddRange(new Control[] { lblTitle, lblSub });
            return p;
        }

        private Panel CreateMatchCard(MatchRecord match)
        {
            Panel card = new Panel
            {
                Size = new Size(280, 315),
                Margin = new Padding(0, 0, 16, 16),
                BackColor = colorNavyCard
            };
            card.Paint += DrawCardBorder;

            PictureBox picMatch = new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(260, 122),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = CreateFallbackSportsGraphic(match.Sport, match.City, match.Symbol, colorNavyElevated)
            };
            _ = LoadImageAsync(picMatch, match.ImageUrl);

            PictureBox picPlayer = new PictureBox
            {
                Location = new Point(210, 6),
                Size = new Size(44, 44),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = CreateFallbackSportsGraphic(match.HighlightPlayer, match.Symbol, "★", colorOrange)
            };
            _ = LoadImageAsync(picPlayer, match.PlayerImageUrl);
            picMatch.Controls.Add(picPlayer);

            Label lblSportTag = new Label
            {
                Location = new Point(12, 138),
                Size = new Size(256, 20),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = colorGold,
                Text = $"{match.Symbol} {match.Sport.ToUpperInvariant()} • {match.Date} {match.Time}"
            };

            Label lblTitle = new Label
            {
                Location = new Point(12, 158),
                Size = new Size(256, 24),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Text = match.Title
            };

            Label lblTeamsVenue = new Label
            {
                Location = new Point(12, 182),
                Size = new Size(256, 44),
                Font = new Font("Segoe UI", 8.8F),
                ForeColor = colorTextMuted,
                Text = $"{match.Teams}\n{match.Venue}, {match.City}\nStar: {match.HighlightPlayer}"
            };

            decimal displayPrice = blackFridayActive ? Math.Round(match.Price * 0.75m, 2) : match.Price;
            Label lblPrice = new Label
            {
                Location = new Point(12, 232),
                Size = new Size(256, 24),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(74, 222, 128),
                Text = blackFridayActive
                    ? $"${displayPrice:0.00} NZD (was ${match.Price:0.00})"
                    : $"${match.Price:0.00} NZD"
            };

            Button btnAddCart = new Button
            {
                Location = new Point(12, 262),
                Size = new Size(256, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = colorOrange,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Text = "Add to Cart"
            };
            btnAddCart.FlatAppearance.BorderSize = 0;
            btnAddCart.Click += (s, e) =>
            {
                CartItem existing = cartItems.FirstOrDefault(c => c.Match.Id == match.Id);
                if (existing != null)
                {
                    existing.Quantity++;
                }
                else
                {
                    cartItems.Add(new CartItem
                    {
                        Match = match,
                        Quantity = 1,
                        UnitPrice = match.Price
                    });
                }

                UpdateNavBadges();
                AddNotification($"Added '{match.Title}' ({match.Teams}) to My Cart.");
                MessageBox.Show($"Added '{match.Title}' to My Cart!", "KIWIARENA", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            card.Controls.AddRange(new Control[] { picMatch, lblSportTag, lblTitle, lblTeamsVenue, lblPrice, btnAddCart });
            return card;
        }

        private Panel CreatePlayerCard(MatchRecord match)
        {
            Panel card = new Panel
            {
                Size = new Size(222, 205),
                Margin = new Padding(0, 0, 14, 14),
                BackColor = colorNavyCard
            };
            card.Paint += DrawCardBorder;

            PictureBox pic = new PictureBox
            {
                Location = new Point(12, 12),
                Size = new Size(198, 118),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = CreateFallbackSportsGraphic(match.HighlightPlayer, match.Sport, match.Symbol, colorNavyElevated)
            };
            _ = LoadImageAsync(pic, match.PlayerImageUrl);

            Label lblPlayer = new Label
            {
                Location = new Point(12, 136),
                Size = new Size(198, 24),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Text = match.HighlightPlayer
            };

            Label lblSport = new Label
            {
                Location = new Point(12, 162),
                Size = new Size(198, 26),
                Font = new Font("Segoe UI", 9F),
                ForeColor = colorGold,
                Text = $"{match.Symbol} {match.Sport} • {match.City}"
            };

            card.Controls.AddRange(new Control[] { pic, lblPlayer, lblSport });
            return card;
        }

        private async Task LoadImageAsync(PictureBox targetBox, string url)
        {
            if (string.IsNullOrWhiteSpace(url) || targetBox == null || targetBox.IsDisposed)
            {
                return;
            }

            try
            {
                if (ImageCache.TryGetValue(url, out Image cached) && cached != null)
                {
                    if (!targetBox.IsDisposed)
                    {
                        targetBox.Image = cached;
                    }
                    return;
                }

                byte[] bytes = await HttpClientInstance.GetByteArrayAsync(url);
                using (MemoryStream ms = new MemoryStream(bytes))
                using (Image loaded = Image.FromStream(ms))
                {
                    Bitmap copy = new Bitmap(loaded);
                    ImageCache[url] = copy;
                    if (!targetBox.IsDisposed)
                    {
                        targetBox.Image = copy;
                    }
                }
            }
            catch
            {
                // Keep drawn sports/player artwork if remote host blocks or times out
            }
        }

        private Bitmap CreateFallbackSportsGraphic(string primaryText, string secondaryText, string symbol, Color accentColor)
        {
            Bitmap bmp = new Bitmap(260, 140);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new Rectangle(0, 0, 260, 140),
                    colorNavyDark,
                    accentColor,
                    LinearGradientMode.ForwardDiagonal))
                {
                    g.FillRectangle(brush, 0, 0, 260, 140);
                }

                using (Pen ringPen = new Pen(Color.FromArgb(90, 255, 255, 255), 2))
                {
                    g.DrawEllipse(ringPen, 145, 10, 100, 100);
                    g.DrawLine(ringPen, 0, 118, 260, 118);
                }

                using (Pen borderPen = new Pen(colorOrange, 2))
                {
                    g.DrawRectangle(borderPen, 1, 1, 258, 138);
                }

                using (Font symFont = new Font("Segoe UI Emoji", 22F))
                using (Font titleFont = new Font("Segoe UI", 10.5F, FontStyle.Bold))
                using (Font subFont = new Font("Segoe UI", 8.5F, FontStyle.Bold))
                using (SolidBrush whiteBrush = new SolidBrush(Color.White))
                using (SolidBrush goldBrush = new SolidBrush(colorGold))
                {
                    g.DrawString(symbol, symFont, whiteBrush, new PointF(14, 12));
                    g.DrawString(primaryText, titleFont, whiteBrush, new PointF(16, 66));
                    g.DrawString(secondaryText, subFont, goldBrush, new PointF(16, 92));
                }
            }
            return bmp;
        }

        private void DrawCardBorder(object sender, PaintEventArgs e)
        {
            if (sender is Control ctrl)
            {
                using (Pen pen = new Pen(colorBorder, 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, ctrl.Width - 1, ctrl.Height - 1);
                }
            }
        }

        private sealed class MatchRecord
        {
            public int Id { get; set; }
            public string Sport { get; set; }
            public string Symbol { get; set; }
            public string Title { get; set; }
            public string Teams { get; set; }
            public string Venue { get; set; }
            public string City { get; set; }
            public string Date { get; set; }
            public string Time { get; set; }
            public decimal Price { get; set; }
            public bool IsFeatured { get; set; }
            public string HighlightPlayer { get; set; }
            public string ImageUrl { get; set; }
            public string PlayerImageUrl { get; set; }
        }

        private sealed class CartItem
        {
            public MatchRecord Match { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }

        private sealed class TicketRecord
        {
            public string TicketCode { get; set; }
            public MatchRecord Match { get; set; }
            public int Quantity { get; set; }
            public decimal TotalPaid { get; set; }
            public string PurchasedAt { get; set; }
        }
    }
}