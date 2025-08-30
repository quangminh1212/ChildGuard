using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ChildGuard.UI.GlassUI
{
    /// <summary>
    /// Glass Morphism + CyberPunk Gaming Color System
    /// Modern, beautiful, and professional
    /// </summary>
    public static class GlassColors
    {
        // === BACKGROUND SYSTEM ===
        public static readonly Color DarkBase = Color.FromArgb(15, 15, 23);           // Deep dark blue
        public static readonly Color DarkSecondary = Color.FromArgb(22, 22, 35);      // Slightly lighter
        public static readonly Color DarkTertiary = Color.FromArgb(30, 30, 46);       // Card backgrounds
        
        // === GLASS MORPHISM ===
        public static readonly Color GlassBase = Color.FromArgb(40, 255, 255, 255);   // 15% white
        public static readonly Color GlassSecondary = Color.FromArgb(25, 255, 255, 255); // 10% white
        public static readonly Color GlassBorder = Color.FromArgb(60, 255, 255, 255);  // 25% white
        public static readonly Color GlassHighlight = Color.FromArgb(80, 255, 255, 255); // 30% white
        
        // === NEON ACCENT SYSTEM ===
        public static readonly Color NeonCyan = Color.FromArgb(0, 255, 255);          // Electric cyan
        public static readonly Color NeonPink = Color.FromArgb(255, 20, 147);         // Hot pink
        public static readonly Color NeonPurple = Color.FromArgb(138, 43, 226);       // Blue violet
        public static readonly Color NeonGreen = Color.FromArgb(57, 255, 20);         // Electric green
        public static readonly Color NeonOrange = Color.FromArgb(255, 165, 0);        // Electric orange
        public static readonly Color NeonBlue = Color.FromArgb(30, 144, 255);         // Dodger blue
        
        // === PRIMARY BRAND ===
        public static readonly Color Primary = Color.FromArgb(100, 149, 237);         // Cornflower blue
        public static readonly Color PrimaryGlow = Color.FromArgb(60, 100, 149, 237); // With glow
        public static readonly Color PrimaryDark = Color.FromArgb(72, 61, 139);       // Dark slate blue
        
        // === SEMANTIC COLORS ===
        public static readonly Color Success = Color.FromArgb(34, 197, 94);           // Emerald
        public static readonly Color SuccessGlow = Color.FromArgb(40, 34, 197, 94);
        public static readonly Color Warning = Color.FromArgb(251, 191, 36);          // Amber
        public static readonly Color WarningGlow = Color.FromArgb(40, 251, 191, 36);
        public static readonly Color Error = Color.FromArgb(239, 68, 68);             // Red
        public static readonly Color ErrorGlow = Color.FromArgb(40, 239, 68, 68);
        public static readonly Color Info = Color.FromArgb(59, 130, 246);             // Blue
        public static readonly Color InfoGlow = Color.FromArgb(40, 59, 130, 246);
        
        // === TEXT SYSTEM ===
        public static readonly Color TextPrimary = Color.FromArgb(248, 250, 252);     // Almost white
        public static readonly Color TextSecondary = Color.FromArgb(203, 213, 225);   // Light gray
        public static readonly Color TextTertiary = Color.FromArgb(148, 163, 184);    // Medium gray
        public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);       // Dark gray
        public static readonly Color TextDisabled = Color.FromArgb(71, 85, 105);      // Very dark gray
        
        // === SURFACE SYSTEM ===
        public static readonly Color Surface = Color.FromArgb(30, 41, 59);            // Slate 800
        public static readonly Color SurfaceSecondary = Color.FromArgb(51, 65, 85);   // Slate 700
        public static readonly Color SurfaceTertiary = Color.FromArgb(71, 85, 105);   // Slate 600
        public static readonly Color SurfaceHover = Color.FromArgb(100, 116, 139);    // Slate 500
        
        // === BORDER SYSTEM ===
        public static readonly Color Border = Color.FromArgb(51, 65, 85);             // Subtle border
        public static readonly Color BorderSecondary = Color.FromArgb(71, 85, 105);   // More visible
        public static readonly Color BorderFocus = Color.FromArgb(59, 130, 246);      // Blue focus
        public static readonly Color BorderGlow = Color.FromArgb(80, 59, 130, 246);   // Glowing focus
        
        // === SHADOW SYSTEM ===
        public static readonly Color Shadow = Color.FromArgb(30, 0, 0, 0);            // 12% black
        public static readonly Color ShadowMedium = Color.FromArgb(50, 0, 0, 0);      // 20% black
        public static readonly Color ShadowHeavy = Color.FromArgb(80, 0, 0, 0);       // 30% black
        public static readonly Color ShadowGlow = Color.FromArgb(40, 100, 149, 237);  // Colored glow
        
        // === GRADIENT CREATORS ===
        public static LinearGradientBrush CreatePrimaryGradient(Rectangle rect)
        {
            return new LinearGradientBrush(
                rect,
                Color.FromArgb(139, 69, 19),   // Saddle brown
                Color.FromArgb(100, 149, 237), // Cornflower blue
                LinearGradientMode.Vertical
            );
        }
        
        public static LinearGradientBrush CreateGlassGradient(Rectangle rect)
        {
            return new LinearGradientBrush(
                rect,
                Color.FromArgb(60, 255, 255, 255),  // Top highlight
                Color.FromArgb(20, 255, 255, 255),  // Bottom subtle
                LinearGradientMode.Vertical
            );
        }
        
        public static LinearGradientBrush CreateNeonGradient(Rectangle rect, Color neonColor)
        {
            var startColor = Color.FromArgb(80, neonColor.R, neonColor.G, neonColor.B);
            var endColor = Color.FromArgb(20, neonColor.R, neonColor.G, neonColor.B);
            
            return new LinearGradientBrush(rect, startColor, endColor, LinearGradientMode.Vertical);
        }
        
        public static LinearGradientBrush CreateBackgroundGradient(Rectangle rect)
        {
            return new LinearGradientBrush(
                rect,
                DarkBase,
                DarkSecondary,
                45f // Diagonal gradient
            );
        }
        
        // === UTILITY METHODS ===
        public static Color WithOpacity(Color color, double opacity)
        {
            opacity = Math.Max(0, Math.Min(1, opacity));
            return Color.FromArgb((int)(255 * opacity), color.R, color.G, color.B);
        }
        
        public static Color AddGlow(Color color, double glowIntensity = 0.3)
        {
            glowIntensity = Math.Max(0, Math.Min(1, glowIntensity));
            return Color.FromArgb(
                (int)(255 * glowIntensity),
                color.R,
                color.G,
                color.B
            );
        }
        
        public static Color Lighten(Color color, double amount)
        {
            amount = Math.Max(0, Math.Min(1, amount));
            return Color.FromArgb(
                color.A,
                Math.Min(255, (int)(color.R + (255 - color.R) * amount)),
                Math.Min(255, (int)(color.G + (255 - color.G) * amount)),
                Math.Min(255, (int)(color.B + (255 - color.B) * amount))
            );
        }
        
        public static Color Darken(Color color, double amount)
        {
            amount = Math.Max(0, Math.Min(1, amount));
            return Color.FromArgb(
                color.A,
                Math.Max(0, (int)(color.R * (1 - amount))),
                Math.Max(0, (int)(color.G * (1 - amount))),
                Math.Max(0, (int)(color.B * (1 - amount)))
            );
        }
        
        public static Color BlendColors(Color color1, Color color2, double ratio)
        {
            ratio = Math.Max(0, Math.Min(1, ratio));
            
            return Color.FromArgb(
                (int)(color1.A + (color2.A - color1.A) * ratio),
                (int)(color1.R + (color2.R - color1.R) * ratio),
                (int)(color1.G + (color2.G - color1.G) * ratio),
                (int)(color1.B + (color2.B - color1.B) * ratio)
            );
        }
        
        // === THEME VARIANTS ===
        public static class Themes
        {
            public static class CyberPunk
            {
                public static readonly Color Primary = NeonCyan;
                public static readonly Color Secondary = NeonPink;
                public static readonly Color Accent = NeonPurple;
            }
            
            public static class Gaming
            {
                public static readonly Color Primary = NeonGreen;
                public static readonly Color Secondary = NeonOrange;
                public static readonly Color Accent = NeonBlue;
            }
            
            public static class Professional
            {
                public static readonly Color Primary = Color.FromArgb(59, 130, 246);
                public static readonly Color Secondary = Color.FromArgb(139, 92, 246);
                public static readonly Color Accent = Color.FromArgb(34, 197, 94);
            }
        }
    }
}
