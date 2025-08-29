using System;
using System.Drawing;

namespace ChildGuard.UI.Theming
{
    /// <summary>
    /// Định nghĩa bảng màu theo phong cách Windows 11 và Facebook
    /// </summary>
    public static class ColorScheme
    {
        // Facebook Brand Colors
        public static class Facebook
        {
            public static readonly Color Primary = ColorTranslator.FromHtml("#1877F2");        // Facebook Blue
            public static readonly Color Secondary = ColorTranslator.FromHtml("#42B883");      // Success Green
            public static readonly Color Tertiary = ColorTranslator.FromHtml("#E4E6EB");       // Light Gray
            public static readonly Color Background = ColorTranslator.FromHtml("#F0F2F5");     // Background Gray
            public static readonly Color Surface = ColorTranslator.FromHtml("#FFFFFF");        // Card White
            public static readonly Color TextPrimary = ColorTranslator.FromHtml("#050505");    // Primary Text
            public static readonly Color TextSecondary = ColorTranslator.FromHtml("#65676B");  // Secondary Text
            public static readonly Color Error = ColorTranslator.FromHtml("#F02849");          // Error Red
            public static readonly Color Warning = ColorTranslator.FromHtml("#F7B928");        // Warning Yellow
            public static readonly Color Success = ColorTranslator.FromHtml("#31A24C");        // Success Green
        }

        // Windows 11 Light Theme
        public static class Windows11Light
        {
            public static readonly Color AccentDefault = ColorTranslator.FromHtml("#0078D4");   // Windows Blue
            public static readonly Color AccentLight1 = ColorTranslator.FromHtml("#40E0D0");    
            public static readonly Color AccentLight2 = ColorTranslator.FromHtml("#99EBFF");    
            public static readonly Color AccentDark1 = ColorTranslator.FromHtml("#005A9E");     
            public static readonly Color AccentDark2 = ColorTranslator.FromHtml("#004578");     

            public static readonly Color ChromeHigh = ColorTranslator.FromHtml("#CCCCCC");
            public static readonly Color ChromeMedium = ColorTranslator.FromHtml("#E1E1E1");
            public static readonly Color ChromeLow = ColorTranslator.FromHtml("#F2F2F2");
            public static readonly Color ChromeWhite = ColorTranslator.FromHtml("#FFFFFF");
            public static readonly Color ChromeBlack = ColorTranslator.FromHtml("#000000");

            public static readonly Color BaseHigh = ColorTranslator.FromHtml("#000000");
            public static readonly Color BaseMediumHigh = ColorTranslator.FromHtml("#333333");
            public static readonly Color BaseMedium = ColorTranslator.FromHtml("#666666");
            public static readonly Color BaseMediumLow = ColorTranslator.FromHtml("#999999");
            public static readonly Color BaseLow = ColorTranslator.FromHtml("#CCCCCC");

            public static readonly Color AltHigh = ColorTranslator.FromHtml("#FFFFFF");
            public static readonly Color AltMediumHigh = ColorTranslator.FromHtml("#F2F2F2");
            public static readonly Color AltMedium = ColorTranslator.FromHtml("#E1E1E1");
            public static readonly Color AltMediumLow = ColorTranslator.FromHtml("#CCCCCC");
            public static readonly Color AltLow = ColorTranslator.FromHtml("#999999");
        }

        // Windows 11 Dark Theme
        public static class Windows11Dark
        {
            public static readonly Color AccentDefault = ColorTranslator.FromHtml("#60CDFF");   
            public static readonly Color AccentLight1 = ColorTranslator.FromHtml("#99E7FF");    
            public static readonly Color AccentLight2 = ColorTranslator.FromHtml("#CCFAFF");    
            public static readonly Color AccentDark1 = ColorTranslator.FromHtml("#0091F8");     
            public static readonly Color AccentDark2 = ColorTranslator.FromHtml("#0078D4");     

            public static readonly Color ChromeHigh = ColorTranslator.FromHtml("#767676");
            public static readonly Color ChromeMedium = ColorTranslator.FromHtml("#404040");
            public static readonly Color ChromeLow = ColorTranslator.FromHtml("#171717");
            public static readonly Color ChromeWhite = ColorTranslator.FromHtml("#FFFFFF");
            public static readonly Color ChromeBlack = ColorTranslator.FromHtml("#000000");

            public static readonly Color BaseHigh = ColorTranslator.FromHtml("#FFFFFF");
            public static readonly Color BaseMediumHigh = ColorTranslator.FromHtml("#CCCCCC");
            public static readonly Color BaseMedium = ColorTranslator.FromHtml("#999999");
            public static readonly Color BaseMediumLow = ColorTranslator.FromHtml("#666666");
            public static readonly Color BaseLow = ColorTranslator.FromHtml("#333333");

            public static readonly Color AltHigh = ColorTranslator.FromHtml("#000000");
            public static readonly Color AltMediumHigh = ColorTranslator.FromHtml("#0F0F0F");
            public static readonly Color AltMedium = ColorTranslator.FromHtml("#1C1C1C");
            public static readonly Color AltMediumLow = ColorTranslator.FromHtml("#2B2B2B");
            public static readonly Color AltLow = ColorTranslator.FromHtml("#404040");

            public static readonly Color Background = ColorTranslator.FromHtml("#202020");
            public static readonly Color Surface = ColorTranslator.FromHtml("#2D2D2D");
            public static readonly Color SurfaceStroke = ColorTranslator.FromHtml("#3D3D3D");
        }

        // Combined Modern Theme (Facebook + Windows 11)
        public static class Modern
        {
            // Primary colors
            public static readonly Color Primary = Facebook.Primary;
            public static readonly Color PrimaryLight = ColorTranslator.FromHtml("#E7F3FF");
            public static readonly Color PrimaryDark = ColorTranslator.FromHtml("#1665CC");

            // Surface colors
            public static readonly Color Background = ColorTranslator.FromHtml("#F8F9FA");
            public static readonly Color Surface = Color.White;
            public static readonly Color SurfaceVariant = ColorTranslator.FromHtml("#F5F6F7");
            
            // Text colors
            public static readonly Color TextPrimary = ColorTranslator.FromHtml("#1C1E21");
            public static readonly Color TextSecondary = ColorTranslator.FromHtml("#606770");
            public static readonly Color TextDisabled = ColorTranslator.FromHtml("#8A8D91");

            // Border & Divider
            public static readonly Color Border = ColorTranslator.FromHtml("#E4E6E9");
            public static readonly Color BorderHover = ColorTranslator.FromHtml("#D0D2D5");
            public static readonly Color Divider = ColorTranslator.FromHtml("#DADDE1");

            // State colors
            public static readonly Color Success = ColorTranslator.FromHtml("#00A400");
            public static readonly Color Warning = ColorTranslator.FromHtml("#FFA500");
            public static readonly Color Error = ColorTranslator.FromHtml("#F02849");
            public static readonly Color Info = ColorTranslator.FromHtml("#1BA1E2");

            // Hover & Focus
            public static readonly Color HoverOverlay = Color.FromArgb(8, 0, 0, 0);
            public static readonly Color FocusOverlay = Color.FromArgb(12, 24, 119, 242);
            public static readonly Color PressedOverlay = Color.FromArgb(16, 0, 0, 0);

            // Shadow colors
            public static readonly Color ShadowLight = Color.FromArgb(10, 0, 0, 0);
            public static readonly Color ShadowMedium = Color.FromArgb(25, 0, 0, 0);
            public static readonly Color ShadowDark = Color.FromArgb(40, 0, 0, 0);
        }

        // Google Material + Windows Fluent inspired theme
        public static class MaterialFluent
        {
            // Primary colors (Google Blue with Windows refinement)
            public static readonly Color Primary = ColorTranslator.FromHtml("#1A73E8");
            public static readonly Color PrimaryHover = ColorTranslator.FromHtml("#1967D2");
            public static readonly Color PrimaryPressed = ColorTranslator.FromHtml("#174EA6");
            public static readonly Color PrimaryLight = ColorTranslator.FromHtml("#E8F0FE");
            public static readonly Color PrimaryDark = ColorTranslator.FromHtml("#1557B0");

            // Secondary colors
            public static readonly Color Secondary = ColorTranslator.FromHtml("#5F6368");
            public static readonly Color SecondaryHover = ColorTranslator.FromHtml("#505459");
            public static readonly Color SecondaryLight = ColorTranslator.FromHtml("#F8F9FA");

            // Background colors (clean and minimal)
            public static readonly Color Background = ColorTranslator.FromHtml("#FAFBFC");
            public static readonly Color Surface = ColorTranslator.FromHtml("#FFFFFF");
            public static readonly Color SurfaceVariant = ColorTranslator.FromHtml("#F8F9FA");
            public static readonly Color SurfaceElevated = ColorTranslator.FromHtml("#FFFFFF");
            public static readonly Color SurfaceContainer = ColorTranslator.FromHtml("#F1F3F4");

            // Text colors (Google Material hierarchy)
            public static readonly Color TextPrimary = ColorTranslator.FromHtml("#202124");
            public static readonly Color TextSecondary = ColorTranslator.FromHtml("#5F6368");
            public static readonly Color TextDisabled = ColorTranslator.FromHtml("#9AA0A6");
            public static readonly Color TextOnPrimary = ColorTranslator.FromHtml("#FFFFFF");
            public static readonly Color TextOnSurface = ColorTranslator.FromHtml("#202124");

            // Border and divider (subtle and clean)
            public static readonly Color Border = ColorTranslator.FromHtml("#DADCE0");
            public static readonly Color BorderLight = ColorTranslator.FromHtml("#E8EAED");
            public static readonly Color Divider = ColorTranslator.FromHtml("#F1F3F4");
            public static readonly Color Outline = ColorTranslator.FromHtml("#DADCE0");

            // Status colors (Material Design 3)
            public static readonly Color Success = ColorTranslator.FromHtml("#34A853");
            public static readonly Color Warning = ColorTranslator.FromHtml("#FBBC04");
            public static readonly Color Error = ColorTranslator.FromHtml("#EA4335");
            public static readonly Color Info = ColorTranslator.FromHtml("#4285F4");

            // Elevation and shadows
            public static readonly Color Shadow1 = Color.FromArgb(8, 0, 0, 0);
            public static readonly Color Shadow2 = Color.FromArgb(16, 0, 0, 0);
            public static readonly Color Shadow3 = Color.FromArgb(24, 0, 0, 0);
            public static readonly Color Shadow4 = Color.FromArgb(32, 0, 0, 0);

            // Interactive states
            public static readonly Color Hover = Color.FromArgb(8, 26, 115, 232);
            public static readonly Color Focus = Color.FromArgb(12, 26, 115, 232);
            public static readonly Color Pressed = Color.FromArgb(16, 26, 115, 232);
            public static readonly Color Selected = Color.FromArgb(12, 26, 115, 232);

            // Special colors
            public static readonly Color Accent = ColorTranslator.FromHtml("#1A73E8");
            public static readonly Color AccentLight = ColorTranslator.FromHtml("#E8F0FE");
            public static readonly Color AccentDark = ColorTranslator.FromHtml("#1557B0");
        }

        /// <summary>
        /// Lấy màu với độ trong suốt
        /// </summary>
        public static Color WithAlpha(Color color, int alpha)
        {
            return Color.FromArgb(alpha, color);
        }

        /// <summary>
        /// Làm sáng màu
        /// </summary>
        public static Color Lighten(Color color, float percent)
        {
            int r = (int)(color.R + (255 - color.R) * percent);
            int g = (int)(color.G + (255 - color.G) * percent);
            int b = (int)(color.B + (255 - color.B) * percent);
            return Color.FromArgb(color.A, r, g, b);
        }

        /// <summary>
        /// Làm tối màu
        /// </summary>
        public static Color Darken(Color color, float percent)
        {
            int r = (int)(color.R * (1 - percent));
            int g = (int)(color.G * (1 - percent));
            int b = (int)(color.B * (1 - percent));
            return Color.FromArgb(color.A, r, g, b);
        }
    }
}
