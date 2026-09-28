using System.Drawing;
using System.Windows.Forms;

namespace NullEx
{
    public static class Theme
    {
        public static Color BgOuter = Color.FromArgb(10, 14, 22);
        public static Color BgPanel = Color.FromArgb(16, 20, 30);
        public static Color BgInset = Color.FromArgb(20, 26, 38);
        public static Color BgRow = Color.FromArgb(24, 30, 44);
        public static Color BgRowHv = Color.FromArgb(34, 44, 62);

        public static Color Accent = Color.FromArgb(0, 160, 255);
        public static Color AccentDim = Color.FromArgb(0, 110, 180);
        public static Color AccentSoft = Color.FromArgb(20, 60, 100);
        public static Color Accent2 = Color.FromArgb(120, 80, 255);
        public static Color Accent2Dim = Color.FromArgb(80, 50, 180);

        public static Color TextMain = Color.FromArgb(220, 230, 245);
        public static Color TextDim = Color.FromArgb(140, 155, 175);
        public static Color TextMuted = Color.FromArgb(90, 105, 125);

        public static Color BorderCol = Color.FromArgb(34, 46, 66);
        public static Color BorderBright = Color.FromArgb(60, 90, 130);

        public static Color CloseRed = Color.FromArgb(220, 70, 70);
        public static Color Success = Color.FromArgb(80, 200, 120);
        public static Color Warning = Color.FromArgb(240, 180, 60);
        public static Color Info = Color.FromArgb(80, 170, 240);

        public static event System.Action PaletteChanged;

        public static string Current { get; private set; } = "Blue";

        public static string[] Presets => new[]
        {
            "Default", "Blue", "Dark", "Pink",

            "Cyberpunk", "Mint", "Sunset", "Ocean",
            "Amber", "Matrix", "Nord", "Blood",
            "Lavender", "Monochrome", "Forest", "Solarized",

            "Dracula", "Gruvbox", "Tokyo Night", "Catppuccin",
            "Rose Pine", "Everforest", "Kanagawa", "Ayu",
            "One Dark", "One Light", "Material", "VSCode",
            "Atom", "Sublime", "GitHub Dark", "GitHub Light",
            "Discord", "Slack", "Telegram", "Reddit",
            "Spotify", "YouTube", "Twitch", "Steam",
            "Neon", "Vaporwave", "Retrowave", "Holographic",
            "Pastel", "Peach", "Cherry", "Coral",
            "Ice", "Frost", "Glacier", "Arctic",
            "Gold", "Silver", "Bronze", "Platinum",
            "Halloween", "Christmas", "Valentine", "Easter",
            "Sepia", "Coffee", "Chocolate", "Caramel",
            "Toxic", "Radioactive", "Plasma", "Inferno"
        };

        public static void Apply(string name)
        {
            switch (name)
            {
                case "Default": ApplyDefault(); break;
                case "Dark": ApplyDark(); break;
                case "Pink": ApplyPink(); break;
                case "Cyberpunk": ApplyCyberpunk(); break;
                case "Mint": ApplyMint(); break;
                case "Sunset": ApplySunset(); break;
                case "Ocean": ApplyOcean(); break;
                case "Amber": ApplyAmber(); break;
                case "Matrix": ApplyMatrix(); break;
                case "Nord": ApplyNord(); break;
                case "Blood": ApplyBlood(); break;
                case "Lavender": ApplyLavender(); break;
                case "Monochrome": ApplyMonochrome(); break;
                case "Forest": ApplyForest(); break;
                case "Solarized": ApplySolarized(); break;

                case "Dracula": ApplyDracula(); break;
                case "Gruvbox": ApplyGruvbox(); break;
                case "Tokyo Night": ApplyTokyoNight(); break;
                case "Catppuccin": ApplyCatppuccin(); break;
                case "Rose Pine": ApplyRosePine(); break;
                case "Everforest": ApplyEverforest(); break;
                case "Kanagawa": ApplyKanagawa(); break;
                case "Ayu": ApplyAyu(); break;
                case "One Dark": ApplyOneDark(); break;
                case "One Light": ApplyOneLight(); break;
                case "Material": ApplyMaterial(); break;
                case "VSCode": ApplyVSCode(); break;
                case "Atom": ApplyAtom(); break;
                case "Sublime": ApplySublime(); break;
                case "GitHub Dark": ApplyGitHubDark(); break;
                case "GitHub Light": ApplyGitHubLight(); break;

                case "Discord": ApplyDiscord(); break;
                case "Slack": ApplySlack(); break;
                case "Telegram": ApplyTelegram(); break;
                case "Reddit": ApplyReddit(); break;
                case "Spotify": ApplySpotify(); break;
                case "YouTube": ApplyYouTube(); break;
                case "Twitch": ApplyTwitch(); break;
                case "Steam": ApplySteam(); break;

                case "Neon": ApplyNeon(); break;
                case "Vaporwave": ApplyVaporwave(); break;
                case "Retrowave": ApplyRetrowave(); break;
                case "Holographic": ApplyHolographic(); break;
                case "Pastel": ApplyPastel(); break;
                case "Peach": ApplyPeach(); break;
                case "Cherry": ApplyCherry(); break;
                case "Coral": ApplyCoral(); break;

                case "Ice": ApplyIce(); break;
                case "Frost": ApplyFrost(); break;
                case "Glacier": ApplyGlacier(); break;
                case "Arctic": ApplyArctic(); break;

                case "Gold": ApplyGold(); break;
                case "Silver": ApplySilver(); break;
                case "Bronze": ApplyBronze(); break;
                case "Platinum": ApplyPlatinum(); break;

                case "Halloween": ApplyHalloween(); break;
                case "Christmas": ApplyChristmas(); break;
                case "Valentine": ApplyValentine(); break;
                case "Easter": ApplyEaster(); break;

                case "Sepia": ApplySepia(); break;
                case "Coffee": ApplyCoffee(); break;
                case "Chocolate": ApplyChocolate(); break;
                case "Caramel": ApplyCaramel(); break;

                case "Toxic": ApplyToxic(); break;
                case "Radioactive": ApplyRadioactive(); break;
                case "Plasma": ApplyPlasma(); break;
                case "Inferno": ApplyInferno(); break;

                case "Blue":
                default: ApplyBlue(); break;
            }
            Current = name;
            PaletteChanged?.Invoke();
        }
        public static void Reskin(Control root, bool isMainForm)
        {
            if (root == null) return;

            if (isMainForm)
            {
                root.BackColor = BgOuter;
                root.ForeColor = TextMain;
            }

            foreach (Control c in root.Controls)
            {
                switch (c)
                {
                    case Label lbl:
                        lbl.ForeColor = IsAccent(lbl.ForeColor) ? Accent : TextDim;
                        lbl.BackColor = Color.Transparent;
                        break;

                    case TextBox tb:
                        tb.BackColor = BgInset;
                        tb.ForeColor = TextMain;
                        break;

                    case ComboBox cb:
                        cb.BackColor = BgInset;
                        cb.ForeColor = TextMain;
                        break;

                    case Button btn:
                        if (btn.BackColor == Color.Transparent)
                        {
                            btn.ForeColor = TextMuted;
                        }
                        else
                        {
                            btn.BackColor = BgRow;
                            btn.ForeColor = TextMain;
                            btn.FlatAppearance.BorderColor = BorderCol;
                            btn.FlatAppearance.MouseOverBackColor = BgRowHv;
                            btn.FlatAppearance.MouseDownBackColor = AccentDim;
                        }
                        break;

                    case Panel p:
                        if (IsCustomPanel(p))
                        {
                        }
                        else if (p.BackColor == BgPanel || p.Tag as string == "BgPanel")
                        {
                            p.BackColor = BgPanel;
                            p.Tag = "BgPanel";
                        }
                        break;
                }

                Reskin(c, false);
            }
        }

        static bool IsCustomPanel(Control c)
        {
            string n = c.GetType().Name;
            return n == "SubToggle" || n == "SubSlider" || n == "RowColor"
                || n == "ScrollIndicator" || n == "ScrollContainer"
                || n == "KeyBindRow";   
        }

        static bool IsAccent(Color c)
        {
            return c.B > c.R + 40 && c.B > c.G + 40;
        }

        static void Set(
            Color bgOuter, Color bgPanel, Color bgInset, Color bgRow, Color bgRowHv,
            Color accent, Color accentDim, Color accentSoft,
            Color accent2, Color accent2Dim,
            Color textMain, Color textDim, Color textMuted,
            Color borderCol, Color borderBright,
            Color closeRed, Color success, Color warning, Color info)
        {
            BgOuter = bgOuter; BgPanel = bgPanel; BgInset = bgInset;
            BgRow = bgRow; BgRowHv = bgRowHv;
            Accent = accent; AccentDim = accentDim; AccentSoft = accentSoft;
            Accent2 = accent2; Accent2Dim = accent2Dim;
            TextMain = textMain; TextDim = textDim; TextMuted = textMuted;
            BorderCol = borderCol; BorderBright = borderBright;
            CloseRed = closeRed; Success = success; Warning = warning; Info = info;
        }


        static void ApplyBlue() => Set(
            Color.FromArgb(10, 14, 22), Color.FromArgb(16, 20, 30), Color.FromArgb(20, 26, 38),
            Color.FromArgb(24, 30, 44), Color.FromArgb(34, 44, 62),
            Color.FromArgb(0, 160, 255), Color.FromArgb(0, 110, 180), Color.FromArgb(20, 60, 100),
            Color.FromArgb(120, 80, 255), Color.FromArgb(80, 50, 180),
            Color.FromArgb(220, 230, 245), Color.FromArgb(140, 155, 175), Color.FromArgb(90, 105, 125),
            Color.FromArgb(34, 46, 66), Color.FromArgb(60, 90, 130),
            Color.FromArgb(220, 70, 70), Color.FromArgb(80, 200, 120), Color.FromArgb(240, 180, 60), Color.FromArgb(80, 170, 240));

        static void ApplyDefault() => Set(
            Color.FromArgb(28, 28, 28), Color.FromArgb(38, 38, 38), Color.FromArgb(46, 46, 46),
            Color.FromArgb(52, 52, 52), Color.FromArgb(70, 70, 70),
            Color.FromArgb(200, 200, 200), Color.FromArgb(150, 150, 150), Color.FromArgb(70, 70, 70),
            Color.FromArgb(160, 160, 160), Color.FromArgb(120, 120, 120),
            Color.FromArgb(235, 235, 235), Color.FromArgb(180, 180, 180), Color.FromArgb(120, 120, 120),
            Color.FromArgb(70, 70, 70), Color.FromArgb(120, 120, 120),
            Color.FromArgb(220, 70, 70), Color.FromArgb(100, 200, 130), Color.FromArgb(230, 180, 80), Color.FromArgb(150, 180, 220));

        static void ApplyDark() => Set(
            Color.FromArgb(6, 6, 8), Color.FromArgb(12, 12, 14), Color.FromArgb(18, 18, 20),
            Color.FromArgb(22, 22, 26), Color.FromArgb(34, 34, 40),
            Color.FromArgb(140, 100, 255), Color.FromArgb(90, 60, 180), Color.FromArgb(40, 30, 70),
            Color.FromArgb(255, 90, 160), Color.FromArgb(180, 60, 110),
            Color.FromArgb(230, 225, 245), Color.FromArgb(160, 155, 180), Color.FromArgb(105, 100, 125),
            Color.FromArgb(40, 38, 55), Color.FromArgb(80, 70, 120),
            Color.FromArgb(220, 70, 70), Color.FromArgb(110, 200, 140), Color.FromArgb(230, 180, 90), Color.FromArgb(120, 160, 220));

        static void ApplyPink() => Set(
            Color.FromArgb(24, 12, 18), Color.FromArgb(34, 18, 26), Color.FromArgb(44, 24, 34),
            Color.FromArgb(52, 28, 40), Color.FromArgb(72, 38, 54),
            Color.FromArgb(255, 90, 160), Color.FromArgb(190, 60, 120), Color.FromArgb(80, 30, 55),
            Color.FromArgb(255, 180, 90), Color.FromArgb(200, 130, 60),
            Color.FromArgb(250, 230, 240), Color.FromArgb(200, 155, 175), Color.FromArgb(140, 100, 120),
            Color.FromArgb(70, 40, 55), Color.FromArgb(150, 80, 110),
            Color.FromArgb(230, 80, 100), Color.FromArgb(140, 220, 170), Color.FromArgb(255, 200, 90), Color.FromArgb(220, 160, 220));

        static void ApplyCyberpunk() => Set(
            Color.FromArgb(8, 4, 16), Color.FromArgb(16, 8, 28), Color.FromArgb(24, 12, 40),
            Color.FromArgb(30, 16, 48), Color.FromArgb(46, 24, 70),
            Color.FromArgb(0, 255, 220), Color.FromArgb(0, 180, 160), Color.FromArgb(0, 60, 55),
            Color.FromArgb(255, 0, 180), Color.FromArgb(180, 0, 130),
            Color.FromArgb(230, 255, 250), Color.FromArgb(150, 190, 200), Color.FromArgb(90, 130, 150),
            Color.FromArgb(40, 60, 90), Color.FromArgb(0, 200, 200),
            Color.FromArgb(255, 60, 100), Color.FromArgb(0, 255, 180), Color.FromArgb(255, 200, 0), Color.FromArgb(0, 200, 255));

        static void ApplyMint() => Set(
            Color.FromArgb(8, 20, 18), Color.FromArgb(14, 30, 26), Color.FromArgb(20, 40, 34),
            Color.FromArgb(26, 50, 42), Color.FromArgb(38, 70, 58),
            Color.FromArgb(0, 220, 150), Color.FromArgb(0, 160, 110), Color.FromArgb(0, 60, 45),
            Color.FromArgb(120, 240, 200), Color.FromArgb(80, 180, 150),
            Color.FromArgb(220, 250, 240), Color.FromArgb(150, 195, 180), Color.FromArgb(90, 130, 115),
            Color.FromArgb(34, 60, 52), Color.FromArgb(60, 130, 100),
            Color.FromArgb(230, 90, 90), Color.FromArgb(120, 230, 130), Color.FromArgb(240, 200, 100), Color.FromArgb(120, 200, 220));

        static void ApplySunset() => Set(
            Color.FromArgb(26, 12, 20), Color.FromArgb(38, 18, 26), Color.FromArgb(48, 24, 32),
            Color.FromArgb(58, 28, 38), Color.FromArgb(78, 40, 52),
            Color.FromArgb(255, 120, 80), Color.FromArgb(200, 80, 50), Color.FromArgb(90, 40, 30),
            Color.FromArgb(255, 200, 90), Color.FromArgb(190, 140, 50),
            Color.FromArgb(255, 235, 220), Color.FromArgb(210, 170, 150), Color.FromArgb(150, 110, 95),
            Color.FromArgb(70, 40, 45), Color.FromArgb(180, 90, 70),
            Color.FromArgb(240, 80, 80), Color.FromArgb(160, 220, 140), Color.FromArgb(255, 190, 70), Color.FromArgb(255, 160, 130));

        static void ApplyOcean() => Set(
            Color.FromArgb(6, 16, 30), Color.FromArgb(12, 24, 42), Color.FromArgb(18, 34, 54),
            Color.FromArgb(22, 42, 66), Color.FromArgb(34, 60, 92),
            Color.FromArgb(80, 190, 255), Color.FromArgb(50, 130, 190), Color.FromArgb(20, 60, 95),
            Color.FromArgb(80, 240, 200), Color.FromArgb(50, 170, 140),
            Color.FromArgb(220, 240, 255), Color.FromArgb(150, 180, 205), Color.FromArgb(95, 125, 155),
            Color.FromArgb(30, 55, 85), Color.FromArgb(60, 130, 190),
            Color.FromArgb(230, 90, 100), Color.FromArgb(100, 220, 180), Color.FromArgb(240, 200, 100), Color.FromArgb(120, 200, 250));

        static void ApplyAmber() => Set(
            Color.FromArgb(20, 14, 6), Color.FromArgb(32, 22, 10), Color.FromArgb(44, 30, 14),
            Color.FromArgb(54, 38, 18), Color.FromArgb(76, 54, 26),
            Color.FromArgb(255, 170, 40), Color.FromArgb(200, 120, 20), Color.FromArgb(90, 55, 15),
            Color.FromArgb(255, 220, 120), Color.FromArgb(200, 170, 80),
            Color.FromArgb(255, 240, 215), Color.FromArgb(210, 180, 140), Color.FromArgb(150, 120, 90),
            Color.FromArgb(70, 50, 25), Color.FromArgb(180, 130, 50),
            Color.FromArgb(230, 90, 70), Color.FromArgb(180, 210, 100), Color.FromArgb(255, 200, 60), Color.FromArgb(240, 190, 120));

        static void ApplyMatrix() => Set(
            Color.FromArgb(2, 8, 2), Color.FromArgb(4, 14, 4), Color.FromArgb(8, 22, 8),
            Color.FromArgb(12, 30, 12), Color.FromArgb(20, 46, 20),
            Color.FromArgb(0, 255, 80), Color.FromArgb(0, 180, 60), Color.FromArgb(0, 60, 20),
            Color.FromArgb(180, 255, 200), Color.FromArgb(120, 180, 140),
            Color.FromArgb(200, 255, 210), Color.FromArgb(120, 200, 130), Color.FromArgb(70, 140, 80),
            Color.FromArgb(20, 60, 25), Color.FromArgb(0, 200, 60),
            Color.FromArgb(255, 60, 60), Color.FromArgb(0, 255, 120), Color.FromArgb(200, 255, 0), Color.FromArgb(100, 255, 180));

        static void ApplyNord() => Set(
            Color.FromArgb(46, 52, 64), Color.FromArgb(59, 66, 82), Color.FromArgb(67, 76, 94),
            Color.FromArgb(76, 86, 106), Color.FromArgb(94, 106, 128),
            Color.FromArgb(136, 192, 208), Color.FromArgb(94, 129, 172), Color.FromArgb(76, 96, 128),
            Color.FromArgb(180, 142, 173), Color.FromArgb(140, 100, 130),
            Color.FromArgb(236, 239, 244), Color.FromArgb(216, 222, 233), Color.FromArgb(180, 190, 205),
            Color.FromArgb(76, 86, 106), Color.FromArgb(143, 188, 187),
            Color.FromArgb(191, 97, 106), Color.FromArgb(163, 190, 140), Color.FromArgb(235, 203, 139), Color.FromArgb(129, 161, 193));

        static void ApplyBlood() => Set(
            Color.FromArgb(14, 4, 4), Color.FromArgb(24, 8, 8), Color.FromArgb(34, 12, 12),
            Color.FromArgb(44, 16, 16), Color.FromArgb(64, 22, 22),
            Color.FromArgb(220, 40, 40), Color.FromArgb(150, 25, 25), Color.FromArgb(70, 15, 15),
            Color.FromArgb(255, 120, 80), Color.FromArgb(190, 80, 50),
            Color.FromArgb(240, 220, 220), Color.FromArgb(190, 150, 150), Color.FromArgb(130, 95, 95),
            Color.FromArgb(60, 20, 20), Color.FromArgb(160, 40, 40),
            Color.FromArgb(255, 60, 60), Color.FromArgb(180, 100, 100), Color.FromArgb(240, 140, 60), Color.FromArgb(220, 100, 120));

        static void ApplyLavender() => Set(
            Color.FromArgb(20, 18, 32), Color.FromArgb(30, 26, 48), Color.FromArgb(40, 34, 62),
            Color.FromArgb(50, 42, 76), Color.FromArgb(66, 56, 98),
            Color.FromArgb(190, 150, 255), Color.FromArgb(140, 100, 200), Color.FromArgb(60, 40, 95),
            Color.FromArgb(255, 160, 220), Color.FromArgb(190, 110, 160),
            Color.FromArgb(240, 235, 255), Color.FromArgb(190, 180, 220), Color.FromArgb(130, 120, 165),
            Color.FromArgb(55, 45, 85), Color.FromArgb(140, 110, 200),
            Color.FromArgb(230, 90, 120), Color.FromArgb(150, 220, 190), Color.FromArgb(250, 200, 130), Color.FromArgb(170, 190, 255));

        static void ApplyMonochrome() => Set(
            Color.FromArgb(14, 14, 14), Color.FromArgb(22, 22, 22), Color.FromArgb(30, 30, 30),
            Color.FromArgb(38, 38, 38), Color.FromArgb(54, 54, 54),
            Color.FromArgb(240, 240, 240), Color.FromArgb(160, 160, 160), Color.FromArgb(60, 60, 60),
            Color.FromArgb(180, 180, 180), Color.FromArgb(130, 130, 130),
            Color.FromArgb(245, 245, 245), Color.FromArgb(180, 180, 180), Color.FromArgb(120, 120, 120),
            Color.FromArgb(60, 60, 60), Color.FromArgb(140, 140, 140),
            Color.FromArgb(230, 90, 90), Color.FromArgb(190, 230, 190), Color.FromArgb(240, 220, 160), Color.FromArgb(190, 210, 230));

        static void ApplyForest() => Set(
            Color.FromArgb(12, 18, 12), Color.FromArgb(18, 28, 18), Color.FromArgb(26, 38, 26),
            Color.FromArgb(34, 48, 34), Color.FromArgb(48, 66, 48),
            Color.FromArgb(120, 200, 90), Color.FromArgb(80, 140, 60), Color.FromArgb(40, 70, 30),
            Color.FromArgb(220, 200, 120), Color.FromArgb(160, 140, 80),
            Color.FromArgb(230, 245, 220), Color.FromArgb(170, 200, 155), Color.FromArgb(110, 140, 95),
            Color.FromArgb(40, 60, 35), Color.FromArgb(90, 140, 70),
            Color.FromArgb(220, 80, 60), Color.FromArgb(150, 230, 120), Color.FromArgb(240, 210, 100), Color.FromArgb(140, 200, 170));

        static void ApplySolarized() => Set(
            Color.FromArgb(0, 43, 54), Color.FromArgb(7, 54, 66), Color.FromArgb(10, 66, 80),
            Color.FromArgb(14, 78, 94), Color.FromArgb(20, 100, 120),
            Color.FromArgb(38, 139, 210), Color.FromArgb(28, 100, 160), Color.FromArgb(15, 70, 110),
            Color.FromArgb(203, 75, 22), Color.FromArgb(150, 55, 15),
            Color.FromArgb(253, 246, 227), Color.FromArgb(147, 161, 161), Color.FromArgb(100, 115, 115),
            Color.FromArgb(20, 80, 95), Color.FromArgb(38, 139, 210),
            Color.FromArgb(220, 50, 47), Color.FromArgb(133, 153, 0), Color.FromArgb(181, 137, 0), Color.FromArgb(42, 161, 152));

        // ============================================================
        //  EDITOR THEMES
        // ============================================================

        static void ApplyDracula() => Set(
            Color.FromArgb(30, 31, 41), Color.FromArgb(40, 42, 54), Color.FromArgb(52, 55, 70),
            Color.FromArgb(60, 63, 80), Color.FromArgb(80, 84, 105),
            Color.FromArgb(189, 147, 249), Color.FromArgb(140, 100, 200), Color.FromArgb(60, 45, 90),
            Color.FromArgb(255, 121, 198), Color.FromArgb(200, 80, 150),
            Color.FromArgb(248, 248, 242), Color.FromArgb(200, 200, 215), Color.FromArgb(140, 140, 160),
            Color.FromArgb(68, 71, 90), Color.FromArgb(120, 100, 180),
            Color.FromArgb(255, 85, 85), Color.FromArgb(80, 250, 123), Color.FromArgb(241, 250, 140), Color.FromArgb(139, 233, 253));

        static void ApplyGruvbox() => Set(
            Color.FromArgb(29, 32, 33), Color.FromArgb(40, 40, 40), Color.FromArgb(50, 48, 47),
            Color.FromArgb(60, 56, 54), Color.FromArgb(80, 73, 69),
            Color.FromArgb(250, 189, 47), Color.FromArgb(200, 140, 30), Color.FromArgb(80, 60, 20),
            Color.FromArgb(254, 128, 25), Color.FromArgb(200, 90, 15),
            Color.FromArgb(235, 219, 178), Color.FromArgb(213, 196, 161), Color.FromArgb(168, 153, 132),
            Color.FromArgb(80, 73, 69), Color.FromArgb(200, 160, 60),
            Color.FromArgb(251, 73, 52), Color.FromArgb(184, 187, 38), Color.FromArgb(250, 189, 47), Color.FromArgb(131, 165, 152));

        static void ApplyTokyoNight() => Set(
            Color.FromArgb(17, 18, 28), Color.FromArgb(26, 27, 38), Color.FromArgb(36, 40, 59),
            Color.FromArgb(45, 50, 75), Color.FromArgb(60, 66, 95),
            Color.FromArgb(122, 162, 247), Color.FromArgb(90, 120, 200), Color.FromArgb(30, 45, 90),
            Color.FromArgb(187, 154, 247), Color.FromArgb(140, 100, 200),
            Color.FromArgb(192, 202, 245), Color.FromArgb(160, 170, 210), Color.FromArgb(110, 120, 160),
            Color.FromArgb(45, 50, 75), Color.FromArgb(120, 150, 220),
            Color.FromArgb(247, 118, 142), Color.FromArgb(158, 206, 106), Color.FromArgb(224, 175, 104), Color.FromArgb(125, 207, 255));

        static void ApplyCatppuccin() => Set(
            Color.FromArgb(30, 30, 46), Color.FromArgb(49, 50, 68), Color.FromArgb(69, 71, 90),
            Color.FromArgb(88, 91, 112), Color.FromArgb(108, 112, 134),
            Color.FromArgb(203, 166, 247), Color.FromArgb(150, 120, 200), Color.FromArgb(60, 45, 90),
            Color.FromArgb(245, 194, 231), Color.FromArgb(190, 140, 180),
            Color.FromArgb(205, 214, 244), Color.FromArgb(166, 173, 200), Color.FromArgb(127, 132, 156),
            Color.FromArgb(88, 91, 112), Color.FromArgb(180, 190, 220),
            Color.FromArgb(243, 139, 168), Color.FromArgb(166, 227, 161), Color.FromArgb(249, 226, 175), Color.FromArgb(137, 220, 235));

        static void ApplyRosePine() => Set(
            Color.FromArgb(25, 23, 36), Color.FromArgb(31, 29, 46), Color.FromArgb(38, 35, 58),
            Color.FromArgb(45, 42, 70), Color.FromArgb(60, 55, 90),
            Color.FromArgb(196, 167, 231), Color.FromArgb(150, 120, 190), Color.FromArgb(60, 45, 85),
            Color.FromArgb(235, 111, 146), Color.FromArgb(180, 70, 105),
            Color.FromArgb(224, 222, 244), Color.FromArgb(180, 175, 210), Color.FromArgb(130, 125, 160),
            Color.FromArgb(60, 55, 90), Color.FromArgb(180, 150, 220),
            Color.FromArgb(235, 111, 146), Color.FromArgb(156, 207, 216), Color.FromArgb(246, 193, 119), Color.FromArgb(49, 116, 143));

        static void ApplyEverforest() => Set(
            Color.FromArgb(45, 53, 59), Color.FromArgb(52, 63, 68), Color.FromArgb(63, 74, 78),
            Color.FromArgb(72, 84, 88), Color.FromArgb(90, 104, 108),
            Color.FromArgb(167, 192, 128), Color.FromArgb(120, 145, 90), Color.FromArgb(50, 65, 40),
            Color.FromArgb(230, 126, 128), Color.FromArgb(180, 90, 90),
            Color.FromArgb(211, 198, 170), Color.FromArgb(180, 170, 145), Color.FromArgb(140, 132, 112),
            Color.FromArgb(72, 84, 88), Color.FromArgb(167, 192, 128),
            Color.FromArgb(230, 126, 128), Color.FromArgb(167, 192, 128), Color.FromArgb(219, 188, 127), Color.FromArgb(127, 187, 179));

        static void ApplyKanagawa() => Set(
            Color.FromArgb(20, 20, 28), Color.FromArgb(31, 31, 40), Color.FromArgb(42, 42, 55),
            Color.FromArgb(54, 54, 70), Color.FromArgb(72, 72, 92),
            Color.FromArgb(149, 127, 184), Color.FromArgb(110, 90, 145), Color.FromArgb(45, 35, 65),
            Color.FromArgb(210, 126, 153), Color.FromArgb(160, 90, 115),
            Color.FromArgb(220, 215, 186), Color.FromArgb(190, 180, 155), Color.FromArgb(145, 138, 120),
            Color.FromArgb(54, 54, 70), Color.FromArgb(149, 127, 184),
            Color.FromArgb(255, 93, 98), Color.FromArgb(152, 187, 108), Color.FromArgb(230, 195, 132), Color.FromArgb(126, 156, 216));

        static void ApplyAyu() => Set(
            Color.FromArgb(10, 14, 20), Color.FromArgb(13, 20, 28), Color.FromArgb(20, 28, 38),
            Color.FromArgb(27, 38, 50), Color.FromArgb(40, 55, 72),
            Color.FromArgb(255, 180, 84), Color.FromArgb(200, 140, 60), Color.FromArgb(70, 50, 20),
            Color.FromArgb(54, 163, 217), Color.FromArgb(35, 120, 165),
            Color.FromArgb(203, 214, 224), Color.FromArgb(170, 180, 195), Color.FromArgb(120, 130, 145),
            Color.FromArgb(40, 55, 72), Color.FromArgb(255, 180, 84),
            Color.FromArgb(240, 113, 120), Color.FromArgb(186, 230, 126), Color.FromArgb(255, 238, 153), Color.FromArgb(54, 163, 217));

        static void ApplyOneDark() => Set(
            Color.FromArgb(33, 37, 43), Color.FromArgb(40, 44, 52), Color.FromArgb(50, 54, 62),
            Color.FromArgb(58, 62, 72), Color.FromArgb(75, 80, 92),
            Color.FromArgb(97, 175, 239), Color.FromArgb(70, 135, 195), Color.FromArgb(30, 55, 85),
            Color.FromArgb(198, 120, 221), Color.FromArgb(150, 85, 175),
            Color.FromArgb(171, 178, 191), Color.FromArgb(140, 148, 165), Color.FromArgb(100, 108, 125),
            Color.FromArgb(58, 62, 72), Color.FromArgb(97, 175, 239),
            Color.FromArgb(224, 108, 117), Color.FromArgb(152, 195, 121), Color.FromArgb(229, 192, 123), Color.FromArgb(86, 182, 194));

        static void ApplyOneLight() => Set(
            Color.FromArgb(250, 250, 250), Color.FromArgb(240, 240, 240), Color.FromArgb(225, 228, 232),
            Color.FromArgb(212, 216, 220), Color.FromArgb(195, 200, 210),
            Color.FromArgb(64, 120, 200), Color.FromArgb(40, 90, 160), Color.FromArgb(200, 215, 235),
            Color.FromArgb(160, 60, 180), Color.FromArgb(120, 40, 140),
            Color.FromArgb(40, 44, 52), Color.FromArgb(80, 88, 100), Color.FromArgb(130, 140, 155),
            Color.FromArgb(200, 205, 215), Color.FromArgb(64, 120, 200),
            Color.FromArgb(200, 60, 60), Color.FromArgb(80, 160, 80), Color.FromArgb(190, 150, 50), Color.FromArgb(50, 130, 150));

        static void ApplyMaterial() => Set(
            Color.FromArgb(38, 50, 56), Color.FromArgb(55, 71, 79), Color.FromArgb(69, 90, 100),
            Color.FromArgb(84, 110, 122), Color.FromArgb(96, 125, 139),
            Color.FromArgb(0, 188, 212), Color.FromArgb(0, 140, 160), Color.FromArgb(0, 55, 65),
            Color.FromArgb(255, 64, 129), Color.FromArgb(200, 40, 95),
            Color.FromArgb(236, 239, 241), Color.FromArgb(176, 190, 197), Color.FromArgb(120, 144, 156),
            Color.FromArgb(69, 90, 100), Color.FromArgb(0, 188, 212),
            Color.FromArgb(244, 67, 54), Color.FromArgb(76, 175, 80), Color.FromArgb(255, 193, 7), Color.FromArgb(3, 169, 244));

        static void ApplyVSCode() => Set(
            Color.FromArgb(30, 30, 30), Color.FromArgb(37, 37, 38), Color.FromArgb(45, 45, 48),
            Color.FromArgb(52, 52, 56), Color.FromArgb(70, 70, 76),
            Color.FromArgb(0, 122, 204), Color.FromArgb(0, 90, 160), Color.FromArgb(20, 45, 75),
            Color.FromArgb(197, 134, 192), Color.FromArgb(150, 100, 145),
            Color.FromArgb(212, 212, 212), Color.FromArgb(160, 160, 160), Color.FromArgb(110, 110, 110),
            Color.FromArgb(60, 60, 66), Color.FromArgb(0, 122, 204),
            Color.FromArgb(244, 71, 71), Color.FromArgb(80, 200, 120), Color.FromArgb(220, 180, 90), Color.FromArgb(0, 160, 220));

        static void ApplyAtom() => Set(
            Color.FromArgb(40, 44, 52), Color.FromArgb(33, 37, 43), Color.FromArgb(45, 50, 58),
            Color.FromArgb(55, 60, 70), Color.FromArgb(72, 78, 90),
            Color.FromArgb(86, 182, 194), Color.FromArgb(60, 140, 150), Color.FromArgb(25, 55, 62),
            Color.FromArgb(198, 120, 221), Color.FromArgb(150, 85, 170),
            Color.FromArgb(200, 205, 215), Color.FromArgb(160, 165, 180), Color.FromArgb(115, 120, 135),
            Color.FromArgb(60, 66, 78), Color.FromArgb(86, 182, 194),
            Color.FromArgb(224, 108, 117), Color.FromArgb(152, 195, 121), Color.FromArgb(229, 192, 123), Color.FromArgb(86, 182, 194));

        static void ApplySublime() => Set(
            Color.FromArgb(39, 40, 34), Color.FromArgb(48, 49, 42), Color.FromArgb(58, 60, 50),
            Color.FromArgb(68, 70, 60), Color.FromArgb(85, 88, 75),
            Color.FromArgb(255, 150, 50), Color.FromArgb(200, 110, 30), Color.FromArgb(75, 45, 20),
            Color.FromArgb(150, 200, 230), Color.FromArgb(110, 150, 180),
            Color.FromArgb(248, 248, 242), Color.FromArgb(190, 195, 180), Color.FromArgb(140, 145, 130),
            Color.FromArgb(70, 72, 62), Color.FromArgb(255, 150, 50),
            Color.FromArgb(249, 38, 114), Color.FromArgb(166, 226, 46), Color.FromArgb(253, 151, 31), Color.FromArgb(102, 217, 239));

        static void ApplyGitHubDark() => Set(
            Color.FromArgb(13, 17, 23), Color.FromArgb(22, 27, 34), Color.FromArgb(33, 38, 45),
            Color.FromArgb(48, 54, 61), Color.FromArgb(68, 76, 86),
            Color.FromArgb(88, 166, 255), Color.FromArgb(60, 130, 210), Color.FromArgb(25, 55, 90),
            Color.FromArgb(219, 97, 219), Color.FromArgb(165, 70, 165),
            Color.FromArgb(201, 209, 217), Color.FromArgb(160, 170, 180), Color.FromArgb(120, 130, 140),
            Color.FromArgb(48, 54, 61), Color.FromArgb(88, 166, 255),
            Color.FromArgb(248, 81, 73), Color.FromArgb(63, 185, 80), Color.FromArgb(210, 153, 34), Color.FromArgb(88, 166, 255));

        static void ApplyGitHubLight() => Set(
            Color.FromArgb(255, 255, 255), Color.FromArgb(246, 248, 250), Color.FromArgb(234, 238, 242),
            Color.FromArgb(225, 228, 232), Color.FromArgb(208, 215, 222),
            Color.FromArgb(9, 105, 218), Color.FromArgb(5, 75, 165), Color.FromArgb(200, 220, 245),
            Color.FromArgb(191, 61, 191), Color.FromArgb(145, 45, 145),
            Color.FromArgb(31, 35, 40), Color.FromArgb(80, 88, 100), Color.FromArgb(130, 140, 155),
            Color.FromArgb(208, 215, 222), Color.FromArgb(9, 105, 218),
            Color.FromArgb(207, 34, 46), Color.FromArgb(26, 127, 55), Color.FromArgb(154, 103, 0), Color.FromArgb(9, 105, 218));

        static void ApplyDiscord() => Set(
            Color.FromArgb(30, 31, 34), Color.FromArgb(47, 49, 54), Color.FromArgb(54, 57, 63),
            Color.FromArgb(64, 68, 75), Color.FromArgb(79, 84, 92),
            Color.FromArgb(88, 101, 242), Color.FromArgb(65, 75, 190), Color.FromArgb(30, 40, 100),
            Color.FromArgb(235, 69, 158), Color.FromArgb(180, 50, 120),
            Color.FromArgb(220, 221, 222), Color.FromArgb(185, 187, 190), Color.FromArgb(140, 143, 148),
            Color.FromArgb(64, 68, 75), Color.FromArgb(88, 101, 242),
            Color.FromArgb(240, 71, 71), Color.FromArgb(59, 165, 93), Color.FromArgb(250, 166, 26), Color.FromArgb(88, 101, 242));

        static void ApplySlack() => Set(
            Color.FromArgb(26, 29, 33), Color.FromArgb(34, 37, 41), Color.FromArgb(44, 47, 51),
            Color.FromArgb(54, 57, 61), Color.FromArgb(70, 74, 78),
            Color.FromArgb(54, 197, 240), Color.FromArgb(40, 150, 185), Color.FromArgb(20, 60, 75),
            Color.FromArgb(232, 145, 45), Color.FromArgb(180, 110, 30),
            Color.FromArgb(220, 220, 220), Color.FromArgb(180, 180, 180), Color.FromArgb(130, 130, 130),
            Color.FromArgb(54, 57, 61), Color.FromArgb(54, 197, 240),
            Color.FromArgb(224, 30, 90), Color.FromArgb(45, 180, 130), Color.FromArgb(236, 178, 46), Color.FromArgb(54, 197, 240));

        static void ApplyTelegram() => Set(
            Color.FromArgb(23, 33, 43), Color.FromArgb(28, 39, 51), Color.FromArgb(34, 47, 62),
            Color.FromArgb(42, 58, 75), Color.FromArgb(58, 78, 100),
            Color.FromArgb(82, 172, 229), Color.FromArgb(60, 135, 185), Color.FromArgb(25, 60, 85),
            Color.FromArgb(100, 200, 180), Color.FromArgb(70, 150, 135),
            Color.FromArgb(240, 245, 250), Color.FromArgb(190, 205, 220), Color.FromArgb(140, 155, 175),
            Color.FromArgb(42, 58, 75), Color.FromArgb(82, 172, 229),
            Color.FromArgb(240, 90, 90), Color.FromArgb(120, 220, 160), Color.FromArgb(250, 190, 90), Color.FromArgb(82, 172, 229));

        static void ApplyReddit() => Set(
            Color.FromArgb(26, 26, 27), Color.FromArgb(33, 33, 34), Color.FromArgb(39, 39, 41),
            Color.FromArgb(48, 48, 50), Color.FromArgb(64, 64, 68),
            Color.FromArgb(255, 69, 0), Color.FromArgb(200, 55, 0), Color.FromArgb(80, 25, 0),
            Color.FromArgb(0, 180, 220), Color.FromArgb(0, 130, 165),
            Color.FromArgb(215, 218, 220), Color.FromArgb(175, 180, 185), Color.FromArgb(130, 135, 140),
            Color.FromArgb(48, 48, 50), Color.FromArgb(255, 69, 0),
            Color.FromArgb(255, 88, 88), Color.FromArgb(80, 200, 120), Color.FromArgb(255, 180, 60), Color.FromArgb(0, 180, 220));

        static void ApplySpotify() => Set(
            Color.FromArgb(18, 18, 18), Color.FromArgb(24, 24, 24), Color.FromArgb(32, 32, 32),
            Color.FromArgb(40, 40, 40), Color.FromArgb(56, 56, 56),
            Color.FromArgb(30, 215, 96), Color.FromArgb(20, 165, 70), Color.FromArgb(10, 60, 30),
            Color.FromArgb(80, 200, 120), Color.FromArgb(50, 150, 90),
            Color.FromArgb(240, 240, 240), Color.FromArgb(180, 180, 180), Color.FromArgb(120, 120, 120),
            Color.FromArgb(48, 48, 48), Color.FromArgb(30, 215, 96),
            Color.FromArgb(230, 60, 60), Color.FromArgb(30, 215, 96), Color.FromArgb(240, 180, 60), Color.FromArgb(120, 200, 240));

        static void ApplyYouTube() => Set(
            Color.FromArgb(15, 15, 15), Color.FromArgb(22, 22, 22), Color.FromArgb(30, 30, 30),
            Color.FromArgb(38, 38, 38), Color.FromArgb(56, 56, 56),
            Color.FromArgb(255, 0, 0), Color.FromArgb(200, 0, 0), Color.FromArgb(80, 10, 10),
            Color.FromArgb(255, 120, 120), Color.FromArgb(200, 80, 80),
            Color.FromArgb(240, 240, 240), Color.FromArgb(180, 180, 180), Color.FromArgb(120, 120, 120),
            Color.FromArgb(48, 48, 48), Color.FromArgb(255, 0, 0),
            Color.FromArgb(255, 60, 60), Color.FromArgb(120, 220, 120), Color.FromArgb(255, 190, 60), Color.FromArgb(80, 180, 255));

        static void ApplyTwitch() => Set(
            Color.FromArgb(14, 14, 16), Color.FromArgb(24, 24, 27), Color.FromArgb(32, 32, 36),
            Color.FromArgb(41, 41, 46), Color.FromArgb(58, 58, 64),
            Color.FromArgb(145, 70, 255), Color.FromArgb(105, 45, 200), Color.FromArgb(45, 20, 90),
            Color.FromArgb(0, 200, 255), Color.FromArgb(0, 150, 195),
            Color.FromArgb(239, 239, 241), Color.FromArgb(180, 180, 190), Color.FromArgb(120, 120, 130),
            Color.FromArgb(48, 48, 54), Color.FromArgb(145, 70, 255),
            Color.FromArgb(235, 60, 90), Color.FromArgb(120, 220, 160), Color.FromArgb(250, 190, 60), Color.FromArgb(0, 200, 255));

        static void ApplySteam() => Set(
            Color.FromArgb(23, 26, 33), Color.FromArgb(27, 40, 56), Color.FromArgb(33, 49, 68),
            Color.FromArgb(42, 58, 79), Color.FromArgb(58, 78, 102),
            Color.FromArgb(102, 192, 244), Color.FromArgb(70, 145, 190), Color.FromArgb(25, 55, 80),
            Color.FromArgb(26, 159, 255), Color.FromArgb(20, 110, 190),
            Color.FromArgb(198, 212, 224), Color.FromArgb(160, 175, 190), Color.FromArgb(115, 130, 145),
            Color.FromArgb(42, 58, 79), Color.FromArgb(102, 192, 244),
            Color.FromArgb(240, 90, 90), Color.FromArgb(120, 200, 130), Color.FromArgb(240, 200, 90), Color.FromArgb(102, 192, 244));


        static void ApplyNeon() => Set(
            Color.FromArgb(6, 6, 10), Color.FromArgb(12, 12, 18), Color.FromArgb(18, 18, 26),
            Color.FromArgb(24, 24, 34), Color.FromArgb(38, 38, 52),
            Color.FromArgb(0, 255, 200), Color.FromArgb(0, 180, 145), Color.FromArgb(0, 60, 50),
            Color.FromArgb(255, 60, 200), Color.FromArgb(200, 40, 150),
            Color.FromArgb(240, 240, 250), Color.FromArgb(180, 190, 210), Color.FromArgb(130, 140, 165),
            Color.FromArgb(40, 50, 80), Color.FromArgb(0, 240, 200),
            Color.FromArgb(255, 60, 100), Color.FromArgb(0, 255, 140), Color.FromArgb(255, 240, 0), Color.FromArgb(0, 220, 255));

        static void ApplyVaporwave() => Set(
            Color.FromArgb(20, 12, 40), Color.FromArgb(30, 18, 60), Color.FromArgb(42, 24, 80),
            Color.FromArgb(56, 32, 100), Color.FromArgb(78, 46, 130),
            Color.FromArgb(255, 110, 199), Color.FromArgb(200, 75, 155), Color.FromArgb(85, 30, 65),
            Color.FromArgb(90, 220, 255), Color.FromArgb(50, 160, 195),
            Color.FromArgb(255, 230, 250), Color.FromArgb(215, 175, 220), Color.FromArgb(165, 125, 175),
            Color.FromArgb(80, 50, 120), Color.FromArgb(255, 110, 199),
            Color.FromArgb(255, 90, 160), Color.FromArgb(140, 240, 220), Color.FromArgb(255, 220, 120), Color.FromArgb(90, 220, 255));

        static void ApplyRetrowave() => Set(
            Color.FromArgb(10, 6, 24), Color.FromArgb(18, 10, 40), Color.FromArgb(26, 14, 58),
            Color.FromArgb(36, 20, 76), Color.FromArgb(56, 32, 106),
            Color.FromArgb(255, 60, 130), Color.FromArgb(200, 40, 95), Color.FromArgb(85, 15, 45),
            Color.FromArgb(0, 200, 255), Color.FromArgb(0, 140, 185),
            Color.FromArgb(255, 220, 240), Color.FromArgb(210, 160, 200), Color.FromArgb(160, 115, 155),
            Color.FromArgb(60, 35, 95), Color.FromArgb(255, 60, 130),
            Color.FromArgb(255, 60, 130), Color.FromArgb(80, 220, 200), Color.FromArgb(255, 200, 60), Color.FromArgb(0, 200, 255));

        static void ApplyHolographic() => Set(
            Color.FromArgb(20, 20, 30), Color.FromArgb(28, 28, 42), Color.FromArgb(38, 38, 58),
            Color.FromArgb(50, 50, 76), Color.FromArgb(70, 70, 100),
            Color.FromArgb(120, 200, 255), Color.FromArgb(85, 150, 210), Color.FromArgb(35, 70, 110),
            Color.FromArgb(255, 150, 220), Color.FromArgb(200, 100, 165),
            Color.FromArgb(240, 240, 255), Color.FromArgb(190, 195, 225), Color.FromArgb(140, 145, 180),
            Color.FromArgb(60, 65, 95), Color.FromArgb(180, 190, 255),
            Color.FromArgb(255, 100, 160), Color.FromArgb(120, 240, 200), Color.FromArgb(255, 220, 130), Color.FromArgb(120, 200, 255));

        static void ApplyPastel() => Set(
            Color.FromArgb(248, 245, 250), Color.FromArgb(240, 236, 245), Color.FromArgb(230, 225, 238),
            Color.FromArgb(220, 213, 230), Color.FromArgb(205, 195, 220),
            Color.FromArgb(255, 150, 180), Color.FromArgb(210, 110, 140), Color.FromArgb(255, 225, 235),
            Color.FromArgb(160, 200, 255), Color.FromArgb(120, 160, 210),
            Color.FromArgb(60, 50, 80), Color.FromArgb(100, 90, 120), Color.FromArgb(140, 130, 160),
            Color.FromArgb(210, 200, 220), Color.FromArgb(255, 150, 180),
            Color.FromArgb(240, 100, 120), Color.FromArgb(130, 210, 150), Color.FromArgb(250, 200, 120), Color.FromArgb(140, 190, 240));

        static void ApplyPeach() => Set(
            Color.FromArgb(32, 22, 18), Color.FromArgb(45, 30, 24), Color.FromArgb(58, 40, 32),
            Color.FromArgb(72, 50, 40), Color.FromArgb(94, 66, 54),
            Color.FromArgb(255, 175, 120), Color.FromArgb(200, 130, 80), Color.FromArgb(85, 50, 25),
            Color.FromArgb(255, 210, 170), Color.FromArgb(200, 160, 120),
            Color.FromArgb(255, 240, 225), Color.FromArgb(215, 185, 165), Color.FromArgb(165, 135, 115),
            Color.FromArgb(85, 58, 45), Color.FromArgb(255, 175, 120),
            Color.FromArgb(240, 90, 90), Color.FromArgb(180, 210, 130), Color.FromArgb(255, 200, 100), Color.FromArgb(255, 175, 120));

        static void ApplyCherry() => Set(
            Color.FromArgb(20, 6, 10), Color.FromArgb(32, 10, 16), Color.FromArgb(46, 14, 22),
            Color.FromArgb(58, 20, 30), Color.FromArgb(80, 28, 42),
            Color.FromArgb(255, 60, 90), Color.FromArgb(200, 40, 65), Color.FromArgb(85, 15, 30),
            Color.FromArgb(255, 160, 200), Color.FromArgb(200, 110, 150),
            Color.FromArgb(255, 230, 235), Color.FromArgb(215, 165, 180), Color.FromArgb(165, 115, 130),
            Color.FromArgb(85, 25, 40), Color.FromArgb(255, 60, 90),
            Color.FromArgb(255, 60, 90), Color.FromArgb(140, 220, 150), Color.FromArgb(255, 190, 90), Color.FromArgb(255, 130, 170));

        static void ApplyCoral() => Set(
            Color.FromArgb(24, 18, 20), Color.FromArgb(36, 26, 28), Color.FromArgb(50, 36, 40),
            Color.FromArgb(64, 46, 52), Color.FromArgb(86, 62, 70),
            Color.FromArgb(255, 127, 110), Color.FromArgb(200, 90, 78), Color.FromArgb(85, 35, 30),
            Color.FromArgb(255, 190, 140), Color.FromArgb(200, 145, 100),
            Color.FromArgb(255, 240, 235), Color.FromArgb(215, 185, 180), Color.FromArgb(165, 135, 130),
            Color.FromArgb(85, 55, 62), Color.FromArgb(255, 127, 110),
            Color.FromArgb(255, 100, 90), Color.FromArgb(150, 220, 160), Color.FromArgb(255, 200, 120), Color.FromArgb(255, 160, 140));



        static void ApplyIce() => Set(
            Color.FromArgb(10, 20, 30), Color.FromArgb(16, 30, 45), Color.FromArgb(24, 42, 60),
            Color.FromArgb(32, 55, 78), Color.FromArgb(46, 74, 100),
            Color.FromArgb(160, 220, 255), Color.FromArgb(115, 170, 210), Color.FromArgb(45, 90, 130),
            Color.FromArgb(120, 240, 240), Color.FromArgb(80, 180, 180),
            Color.FromArgb(230, 245, 255), Color.FromArgb(180, 205, 225), Color.FromArgb(130, 155, 180),
            Color.FromArgb(40, 65, 90), Color.FromArgb(160, 220, 255),
            Color.FromArgb(230, 100, 110), Color.FromArgb(120, 240, 210), Color.FromArgb(255, 220, 140), Color.FromArgb(160, 220, 255));

        static void ApplyFrost() => Set(
            Color.FromArgb(14, 22, 34), Color.FromArgb(22, 34, 50), Color.FromArgb(32, 46, 68),
            Color.FromArgb(42, 60, 86), Color.FromArgb(58, 80, 112),
            Color.FromArgb(200, 230, 255), Color.FromArgb(150, 180, 220), Color.FromArgb(60, 90, 130),
            Color.FromArgb(180, 200, 240), Color.FromArgb(130, 155, 200),
            Color.FromArgb(235, 245, 255), Color.FromArgb(180, 200, 220), Color.FromArgb(130, 150, 175),
            Color.FromArgb(50, 70, 100), Color.FromArgb(200, 230, 255),
            Color.FromArgb(240, 110, 130), Color.FromArgb(140, 220, 200), Color.FromArgb(255, 220, 160), Color.FromArgb(200, 230, 255));

        static void ApplyGlacier() => Set(
            Color.FromArgb(20, 32, 48), Color.FromArgb(30, 46, 66), Color.FromArgb(42, 62, 88),
            Color.FromArgb(54, 78, 108), Color.FromArgb(72, 100, 136),
            Color.FromArgb(180, 230, 255), Color.FromArgb(130, 180, 220), Color.FromArgb(55, 90, 130),
            Color.FromArgb(120, 200, 255), Color.FromArgb(80, 145, 200),
            Color.FromArgb(230, 245, 255), Color.FromArgb(180, 200, 225), Color.FromArgb(130, 155, 180),
            Color.FromArgb(62, 88, 118), Color.FromArgb(180, 230, 255),
            Color.FromArgb(240, 100, 120), Color.FromArgb(150, 230, 200), Color.FromArgb(255, 215, 150), Color.FromArgb(180, 230, 255));

        static void ApplyArctic() => Set(
            Color.FromArgb(240, 246, 252), Color.FromArgb(226, 236, 246), Color.FromArgb(212, 226, 240),
            Color.FromArgb(198, 216, 234), Color.FromArgb(178, 200, 222),
            Color.FromArgb(30, 90, 160), Color.FromArgb(20, 65, 120), Color.FromArgb(200, 220, 240),
            Color.FromArgb(80, 140, 200), Color.FromArgb(60, 105, 155),
            Color.FromArgb(20, 35, 55), Color.FromArgb(60, 80, 105), Color.FromArgb(105, 125, 150),
            Color.FromArgb(180, 200, 220), Color.FromArgb(30, 90, 160),
            Color.FromArgb(200, 60, 60), Color.FromArgb(60, 160, 100), Color.FromArgb(200, 150, 40), Color.FromArgb(30, 90, 160));


        static void ApplyGold() => Set(
            Color.FromArgb(20, 16, 8), Color.FromArgb(32, 26, 12), Color.FromArgb(46, 36, 18),
            Color.FromArgb(58, 46, 24), Color.FromArgb(80, 64, 34),
            Color.FromArgb(255, 200, 60), Color.FromArgb(200, 155, 40), Color.FromArgb(85, 65, 20),
            Color.FromArgb(255, 230, 140), Color.FromArgb(200, 180, 100),
            Color.FromArgb(255, 245, 215), Color.FromArgb(210, 190, 145), Color.FromArgb(160, 140, 100),
            Color.FromArgb(80, 62, 30), Color.FromArgb(255, 200, 60),
            Color.FromArgb(230, 80, 60), Color.FromArgb(180, 210, 110), Color.FromArgb(255, 200, 60), Color.FromArgb(240, 200, 130));

        static void ApplySilver() => Set(
            Color.FromArgb(18, 18, 20), Color.FromArgb(28, 28, 32), Color.FromArgb(40, 40, 46),
            Color.FromArgb(52, 52, 58), Color.FromArgb(72, 72, 80),
            Color.FromArgb(210, 215, 225), Color.FromArgb(160, 165, 175), Color.FromArgb(70, 72, 80),
            Color.FromArgb(180, 185, 200), Color.FromArgb(135, 140, 155),
            Color.FromArgb(240, 242, 248), Color.FromArgb(180, 185, 195), Color.FromArgb(130, 135, 145),
            Color.FromArgb(70, 72, 82), Color.FromArgb(210, 215, 225),
            Color.FromArgb(230, 100, 100), Color.FromArgb(150, 210, 160), Color.FromArgb(230, 200, 130), Color.FromArgb(180, 200, 230));

        static void ApplyBronze() => Set(
            Color.FromArgb(20, 14, 8), Color.FromArgb(34, 22, 12), Color.FromArgb(48, 32, 18),
            Color.FromArgb(62, 42, 24), Color.FromArgb(86, 58, 34),
            Color.FromArgb(200, 130, 70), Color.FromArgb(155, 100, 50), Color.FromArgb(70, 45, 22),
            Color.FromArgb(230, 170, 100), Color.FromArgb(180, 130, 75),
            Color.FromArgb(240, 220, 195), Color.FromArgb(200, 175, 145), Color.FromArgb(150, 125, 100),
            Color.FromArgb(80, 55, 32), Color.FromArgb(200, 130, 70),
            Color.FromArgb(230, 80, 60), Color.FromArgb(180, 200, 110), Color.FromArgb(255, 190, 100), Color.FromArgb(220, 160, 110));

        static void ApplyPlatinum() => Set(
            Color.FromArgb(240, 242, 246), Color.FromArgb(228, 232, 238), Color.FromArgb(214, 218, 226),
            Color.FromArgb(200, 205, 214), Color.FromArgb(182, 188, 198),
            Color.FromArgb(90, 100, 120), Color.FromArgb(60, 70, 90), Color.FromArgb(210, 218, 230),
            Color.FromArgb(150, 130, 190), Color.FromArgb(110, 95, 150),
            Color.FromArgb(40, 44, 55), Color.FromArgb(85, 92, 108), Color.FromArgb(130, 138, 155),
            Color.FromArgb(182, 190, 205), Color.FromArgb(90, 100, 120),
            Color.FromArgb(200, 60, 60), Color.FromArgb(60, 160, 90), Color.FromArgb(200, 150, 40), Color.FromArgb(70, 130, 200));

        static void ApplyHalloween() => Set(
            Color.FromArgb(14, 8, 4), Color.FromArgb(24, 12, 6), Color.FromArgb(36, 18, 8),
            Color.FromArgb(48, 24, 12), Color.FromArgb(70, 36, 18),
            Color.FromArgb(255, 120, 0), Color.FromArgb(200, 90, 0), Color.FromArgb(85, 35, 0),
            Color.FromArgb(140, 40, 200), Color.FromArgb(100, 25, 150),
            Color.FromArgb(255, 235, 200), Color.FromArgb(210, 175, 130), Color.FromArgb(165, 130, 90),
            Color.FromArgb(80, 40, 20), Color.FromArgb(255, 120, 0),
            Color.FromArgb(230, 60, 60), Color.FromArgb(140, 200, 90), Color.FromArgb(255, 170, 30), Color.FromArgb(180, 100, 220));

        static void ApplyChristmas() => Set(
            Color.FromArgb(8, 20, 12), Color.FromArgb(14, 32, 20), Color.FromArgb(20, 46, 28),
            Color.FromArgb(28, 60, 38), Color.FromArgb(40, 80, 52),
            Color.FromArgb(220, 40, 40), Color.FromArgb(165, 30, 30), Color.FromArgb(75, 15, 15),
            Color.FromArgb(60, 200, 100), Color.FromArgb(40, 150, 75),
            Color.FromArgb(250, 250, 245), Color.FromArgb(200, 210, 200), Color.FromArgb(150, 165, 155),
            Color.FromArgb(40, 65, 45), Color.FromArgb(220, 40, 40),
            Color.FromArgb(220, 40, 40), Color.FromArgb(60, 200, 100), Color.FromArgb(255, 220, 80), Color.FromArgb(90, 160, 230));

        static void ApplyValentine() => Set(
            Color.FromArgb(30, 10, 18), Color.FromArgb(44, 16, 26), Color.FromArgb(60, 22, 36),
            Color.FromArgb(78, 30, 48), Color.FromArgb(102, 42, 62),
            Color.FromArgb(255, 100, 140), Color.FromArgb(200, 70, 105), Color.FromArgb(85, 25, 45),
            Color.FromArgb(255, 180, 200), Color.FromArgb(200, 130, 155),
            Color.FromArgb(255, 235, 240), Color.FromArgb(215, 175, 190), Color.FromArgb(165, 125, 140),
            Color.FromArgb(95, 40, 58), Color.FromArgb(255, 100, 140),
            Color.FromArgb(255, 80, 100), Color.FromArgb(160, 210, 160), Color.FromArgb(255, 190, 130), Color.FromArgb(255, 140, 180));

        static void ApplyEaster() => Set(
            Color.FromArgb(245, 240, 250), Color.FromArgb(235, 228, 245), Color.FromArgb(222, 214, 238),
            Color.FromArgb(208, 198, 230), Color.FromArgb(188, 178, 218),
            Color.FromArgb(255, 150, 200), Color.FromArgb(210, 110, 160), Color.FromArgb(255, 220, 240),
            Color.FromArgb(140, 200, 255), Color.FromArgb(100, 160, 210),
            Color.FromArgb(60, 50, 85), Color.FromArgb(100, 90, 125), Color.FromArgb(140, 130, 165),
            Color.FromArgb(200, 190, 220), Color.FromArgb(255, 150, 200),
            Color.FromArgb(240, 100, 140), Color.FromArgb(130, 210, 160), Color.FromArgb(255, 210, 130), Color.FromArgb(150, 190, 240));

        static void ApplySepia() => Set(
            Color.FromArgb(30, 24, 16), Color.FromArgb(44, 36, 24), Color.FromArgb(60, 48, 32),
            Color.FromArgb(76, 62, 42), Color.FromArgb(100, 82, 56),
            Color.FromArgb(210, 170, 110), Color.FromArgb(160, 130, 80), Color.FromArgb(75, 60, 35),
            Color.FromArgb(200, 150, 90), Color.FromArgb(155, 115, 65),
            Color.FromArgb(240, 225, 200), Color.FromArgb(200, 180, 150), Color.FromArgb(150, 130, 105),
            Color.FromArgb(95, 76, 52), Color.FromArgb(210, 170, 110),
            Color.FromArgb(210, 100, 80), Color.FromArgb(170, 190, 120), Color.FromArgb(230, 190, 100), Color.FromArgb(210, 170, 110));

        static void ApplyCoffee() => Set(
            Color.FromArgb(26, 18, 12), Color.FromArgb(38, 26, 18), Color.FromArgb(52, 36, 24),
            Color.FromArgb(66, 46, 32), Color.FromArgb(90, 64, 44),
            Color.FromArgb(200, 155, 110), Color.FromArgb(155, 115, 80), Color.FromArgb(70, 50, 35),
            Color.FromArgb(230, 190, 150), Color.FromArgb(180, 145, 110),
            Color.FromArgb(245, 230, 210), Color.FromArgb(210, 185, 160), Color.FromArgb(160, 135, 110),
            Color.FromArgb(85, 60, 42), Color.FromArgb(200, 155, 110),
            Color.FromArgb(220, 90, 70), Color.FromArgb(170, 190, 120), Color.FromArgb(230, 190, 110), Color.FromArgb(200, 170, 140));

        static void ApplyChocolate() => Set(
            Color.FromArgb(28, 16, 10), Color.FromArgb(42, 24, 14), Color.FromArgb(58, 34, 20),
            Color.FromArgb(74, 44, 26), Color.FromArgb(98, 60, 36),
            Color.FromArgb(190, 120, 75), Color.FromArgb(145, 90, 55), Color.FromArgb(65, 40, 25),
            Color.FromArgb(230, 170, 120), Color.FromArgb(180, 130, 90),
            Color.FromArgb(250, 230, 210), Color.FromArgb(215, 185, 160), Color.FromArgb(165, 135, 110),
            Color.FromArgb(92, 56, 34), Color.FromArgb(190, 120, 75),
            Color.FromArgb(220, 80, 60), Color.FromArgb(170, 190, 110), Color.FromArgb(240, 190, 110), Color.FromArgb(200, 160, 120));

        static void ApplyCaramel() => Set(
            Color.FromArgb(32, 22, 12), Color.FromArgb(48, 32, 18), Color.FromArgb(66, 44, 26),
            Color.FromArgb(84, 56, 34), Color.FromArgb(112, 76, 46),
            Color.FromArgb(255, 180, 90), Color.FromArgb(200, 140, 70), Color.FromArgb(85, 55, 25),
            Color.FromArgb(255, 215, 140), Color.FromArgb(200, 165, 105),
            Color.FromArgb(255, 240, 220), Color.FromArgb(215, 190, 160), Color.FromArgb(165, 140, 115),
            Color.FromArgb(105, 72, 44), Color.FromArgb(255, 180, 90),
            Color.FromArgb(240, 100, 70), Color.FromArgb(180, 210, 120), Color.FromArgb(255, 200, 90), Color.FromArgb(255, 190, 140));

        static void ApplyToxic() => Set(
            Color.FromArgb(6, 12, 4), Color.FromArgb(10, 20, 6), Color.FromArgb(16, 30, 10),
            Color.FromArgb(22, 40, 14), Color.FromArgb(36, 60, 22),
            Color.FromArgb(180, 255, 0), Color.FromArgb(135, 200, 0), Color.FromArgb(55, 85, 0),
            Color.FromArgb(0, 255, 120), Color.FromArgb(0, 200, 90),
            Color.FromArgb(220, 255, 200), Color.FromArgb(170, 220, 145), Color.FromArgb(120, 170, 95),
            Color.FromArgb(40, 65, 25), Color.FromArgb(180, 255, 0),
            Color.FromArgb(255, 60, 60), Color.FromArgb(180, 255, 0), Color.FromArgb(255, 240, 0), Color.FromArgb(120, 255, 180));

        static void ApplyRadioactive() => Set(
            Color.FromArgb(4, 14, 6), Color.FromArgb(6, 22, 10), Color.FromArgb(10, 32, 14),
            Color.FromArgb(16, 44, 20), Color.FromArgb(28, 64, 30),
            Color.FromArgb(120, 255, 60), Color.FromArgb(85, 200, 40), Color.FromArgb(35, 85, 15),
            Color.FromArgb(255, 220, 0), Color.FromArgb(200, 170, 0),
            Color.FromArgb(210, 255, 210), Color.FromArgb(160, 220, 160), Color.FromArgb(110, 170, 110),
            Color.FromArgb(34, 70, 36), Color.FromArgb(120, 255, 60),
            Color.FromArgb(255, 100, 60), Color.FromArgb(120, 255, 60), Color.FromArgb(255, 220, 0), Color.FromArgb(120, 255, 200));

        static void ApplyPlasma() => Set(
            Color.FromArgb(16, 4, 20), Color.FromArgb(26, 8, 32), Color.FromArgb(38, 14, 46),
            Color.FromArgb(50, 20, 60), Color.FromArgb(72, 32, 84),
            Color.FromArgb(255, 80, 240), Color.FromArgb(200, 55, 190), Color.FromArgb(85, 20, 80),
            Color.FromArgb(120, 180, 255), Color.FromArgb(80, 135, 200),
            Color.FromArgb(250, 230, 255), Color.FromArgb(205, 175, 225), Color.FromArgb(155, 125, 175),
            Color.FromArgb(75, 40, 92), Color.FromArgb(255, 80, 240),
            Color.FromArgb(255, 70, 130), Color.FromArgb(140, 240, 200), Color.FromArgb(255, 220, 120), Color.FromArgb(255, 80, 240));

        static void ApplyInferno() => Set(
            Color.FromArgb(14, 4, 2), Color.FromArgb(26, 8, 4), Color.FromArgb(40, 14, 6),
            Color.FromArgb(54, 20, 10), Color.FromArgb(78, 32, 16),
            Color.FromArgb(255, 80, 0), Color.FromArgb(200, 60, 0), Color.FromArgb(85, 25, 0),
            Color.FromArgb(255, 200, 0), Color.FromArgb(200, 155, 0),
            Color.FromArgb(255, 235, 210), Color.FromArgb(220, 180, 145), Color.FromArgb(170, 130, 95),
            Color.FromArgb(85, 34, 18), Color.FromArgb(255, 80, 0),
            Color.FromArgb(255, 60, 40), Color.FromArgb(220, 200, 60), Color.FromArgb(255, 160, 40), Color.FromArgb(255, 200, 60));
    }
}