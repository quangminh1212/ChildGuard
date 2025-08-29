using System.Drawing;

namespace ChildGuard.UI.FluentUI
{
    /// <summary>
    /// Fluent Design System Colors - Windows 11 inspired
    /// </summary>
    public static class FluentColors
    {
        // Primary Brand Colors
        public static readonly Color Primary = Color.FromArgb(0, 120, 212);
        public static readonly Color PrimaryLight = Color.FromArgb(64, 158, 255);
        public static readonly Color PrimaryDark = Color.FromArgb(0, 90, 158);
        
        // Neutral Colors (Windows 11 style)
        public static readonly Color Gray10 = Color.FromArgb(250, 249, 248);
        public static readonly Color Gray20 = Color.FromArgb(243, 242, 241);
        public static readonly Color Gray30 = Color.FromArgb(237, 235, 233);
        public static readonly Color Gray40 = Color.FromArgb(225, 223, 221);
        public static readonly Color Gray50 = Color.FromArgb(210, 208, 206);
        public static readonly Color Gray60 = Color.FromArgb(200, 198, 196);
        public static readonly Color Gray70 = Color.FromArgb(190, 185, 184);
        public static readonly Color Gray80 = Color.FromArgb(179, 176, 173);
        public static readonly Color Gray90 = Color.FromArgb(161, 159, 157);
        public static readonly Color Gray100 = Color.FromArgb(151, 149, 146);
        public static readonly Color Gray110 = Color.FromArgb(138, 136, 134);
        public static readonly Color Gray120 = Color.FromArgb(121, 119, 117);
        public static readonly Color Gray130 = Color.FromArgb(96, 94, 92);
        public static readonly Color Gray140 = Color.FromArgb(72, 70, 68);
        public static readonly Color Gray150 = Color.FromArgb(59, 58, 57);
        public static readonly Color Gray160 = Color.FromArgb(50, 49, 48);
        
        // Semantic Colors
        public static readonly Color Success = Color.FromArgb(16, 124, 16);
        public static readonly Color Warning = Color.FromArgb(255, 140, 0);
        public static readonly Color Error = Color.FromArgb(196, 43, 28);
        public static readonly Color Info = Color.FromArgb(0, 120, 212);
        
        // Background Colors
        public static readonly Color Background = Color.FromArgb(255, 255, 255);
        public static readonly Color BackgroundSecondary = Color.FromArgb(250, 249, 248);
        public static readonly Color BackgroundTertiary = Color.FromArgb(243, 242, 241);
        
        // Surface Colors
        public static readonly Color Surface = Color.FromArgb(255, 255, 255);
        public static readonly Color SurfaceSecondary = Color.FromArgb(246, 246, 246);
        public static readonly Color SurfaceTertiary = Color.FromArgb(240, 240, 240);
        
        // Text Colors
        public static readonly Color TextPrimary = Color.FromArgb(50, 49, 48);
        public static readonly Color TextSecondary = Color.FromArgb(96, 94, 92);
        public static readonly Color TextTertiary = Color.FromArgb(161, 159, 157);
        public static readonly Color TextDisabled = Color.FromArgb(200, 198, 196);
        public static readonly Color TextOnPrimary = Color.White;
        
        // Border Colors
        public static readonly Color Border = Color.FromArgb(225, 223, 221);
        public static readonly Color BorderSecondary = Color.FromArgb(237, 235, 233);
        public static readonly Color BorderFocus = Color.FromArgb(0, 120, 212);
        
        // Shadow Colors
        public static readonly Color Shadow2 = Color.FromArgb(14, 0, 0, 0);
        public static readonly Color Shadow4 = Color.FromArgb(24, 0, 0, 0);
        public static readonly Color Shadow8 = Color.FromArgb(33, 0, 0, 0);
        public static readonly Color Shadow16 = Color.FromArgb(42, 0, 0, 0);
        public static readonly Color Shadow64 = Color.FromArgb(72, 0, 0, 0);
        
        // Interactive States
        public static readonly Color Hover = Color.FromArgb(243, 242, 241);
        public static readonly Color Pressed = Color.FromArgb(237, 235, 233);
        public static readonly Color Selected = Color.FromArgb(237, 235, 233);
        public static readonly Color Focus = Color.FromArgb(0, 120, 212);
        
        // Accent Colors (Windows 11 system colors)
        public static readonly Color AccentBlue = Color.FromArgb(0, 120, 212);
        public static readonly Color AccentPurple = Color.FromArgb(136, 23, 152);
        public static readonly Color AccentTeal = Color.FromArgb(0, 183, 195);
        public static readonly Color AccentGreen = Color.FromArgb(16, 124, 16);
        public static readonly Color AccentOrange = Color.FromArgb(255, 140, 0);
        public static readonly Color AccentRed = Color.FromArgb(196, 43, 28);
        
        // Fluent Design specific
        public static readonly Color AcrylicBase = Color.FromArgb(243, 243, 243);
        public static readonly Color AcrylicTint = Color.FromArgb(252, 252, 252);
        public static readonly Color Mica = Color.FromArgb(243, 243, 243);
        
        // Helper methods
        public static Color WithOpacity(Color color, double opacity)
        {
            return Color.FromArgb((int)(255 * opacity), color.R, color.G, color.B);
        }
        
        public static Color Lighten(Color color, double amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(255, (int)(color.R + (255 - color.R) * amount)),
                Math.Min(255, (int)(color.G + (255 - color.G) * amount)),
                Math.Min(255, (int)(color.B + (255 - color.B) * amount))
            );
        }
        
        public static Color Darken(Color color, double amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Max(0, (int)(color.R * (1 - amount))),
                Math.Max(0, (int)(color.G * (1 - amount))),
                Math.Max(0, (int)(color.B * (1 - amount)))
            );
        }
    }
}
