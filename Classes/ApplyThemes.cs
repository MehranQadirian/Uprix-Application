using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppLauncher.Classes
{
    public class ApplyThemes
    {
        public string BackgroundPrimary { get; set; } //Deep Blue [Default]
        public string BackgroundSecundary { get; set; } //Light Blue [Default]
        public string BackgroundSelectedTile { get; set; } //C8A2A4 [Default]
        public string BackgroundLightPart { get; set; } //White Border [Default]
        public string ForegroundPrimary { get; set; } //White [Default]
        public string ForegroundSecundary { get; set; } //Semi White [Default]
        public string ForegroundStatus { get; set; } //Gray [Default]
        public string IconColorOnBackSec { get; set; }
        public string MenuIconColor { get; set; } //White [Default]


        public string ForegroundOnPrimary { get; set; } //White [Default]
        public string ForegroundOnSecundary { get; set; } //Semi White [Default]
        public string ForegroundOnLightPart { get; set; } //Gray [Default]

        public void ApplyThm()
        {
            BackgroundPrimary = "#000000";
            BackgroundSelectedTile = "#0f0f0f";
            BackgroundSecundary = "#FF121212";
            BackgroundLightPart = "#FF1B1B1B";
            switch (Properties.Settings.Default.Theme)
            {
                case "NeonCyan":
                    ForegroundPrimary = "#00f7f3";
                    ForegroundSecundary = "#00f7f3";
                    ForegroundStatus = "#00f7f3";

                    ForegroundOnPrimary = "#00f7f3";
                    ForegroundOnSecundary = "#00f7f3";
                    ForegroundOnLightPart = "#00f7f3";

                    MenuIconColor = "#00f7f3";
                    break;
                case "NeonGreen":
                    ForegroundPrimary = "#39ff14";
                    ForegroundSecundary = "#39ff14";
                    ForegroundStatus = "#39ff14";

                    ForegroundOnPrimary = "#39ff14";
                    ForegroundOnSecundary = "#39ff14";
                    ForegroundOnLightPart = "#39ff14";

                    MenuIconColor = "#39ff14";
                    break;
                case "NeonYellow":
                    ForegroundPrimary = "#f9ff00";
                    ForegroundSecundary = "#f9ff00";
                    ForegroundStatus = "#f9ff00";

                    ForegroundOnPrimary = "#f9ff00";
                    ForegroundOnSecundary = "#f9ff00";
                    ForegroundOnLightPart = "#f9ff00";

                    MenuIconColor = "#f9ff00";
                    break;
                case "NeonOrange":
                    ForegroundPrimary = "#ff6b00";
                    ForegroundSecundary = "#ff6b00";
                    ForegroundStatus = "#ff6b00";

                    ForegroundOnPrimary = "#ff6b00";
                    ForegroundOnSecundary = "#ff6b00";
                    ForegroundOnLightPart = "#ff6b00";

                    MenuIconColor = "#ff6b00";
                    break;
                case "NeonRed":
                    ForegroundPrimary = "#ff1744";
                    ForegroundSecundary = "#ff1744";
                    ForegroundStatus = "#ff1744";

                    ForegroundOnPrimary = "#ff1744";
                    ForegroundOnSecundary = "#ff1744";
                    ForegroundOnLightPart = "#ff1744";

                    MenuIconColor = "#ff1744";
                    break;
                case "NeonPurple":
                    ForegroundPrimary = "#a100ff";
                    ForegroundSecundary = "#a100ff";
                    ForegroundStatus = "#a100ff";

                    ForegroundOnPrimary = "#a100ff";
                    ForegroundOnSecundary = "#a100ff";
                    ForegroundOnLightPart = "#a100ff";

                    MenuIconColor = "#a100ff";
                    break;
                case "NeonPink":
                    ForegroundPrimary = "#ff00c8";
                    ForegroundSecundary = "#ff00c8";
                    ForegroundStatus = "#ff00c8";

                    ForegroundOnPrimary = "#ff00c8";
                    ForegroundOnSecundary = "#ff00c8";
                    ForegroundOnLightPart = "#ff00c8";

                    MenuIconColor = "#ff00c8";
                    break;
                case "NeonCobaltBlue":
                    ForegroundPrimary = "#007bff";
                    ForegroundSecundary = "#007bff";
                    ForegroundStatus = "#007bff";

                    ForegroundOnPrimary = "#007bff";
                    ForegroundOnSecundary = "#007bff";
                    ForegroundOnLightPart = "#007bff";

                    MenuIconColor = "#007bff";
                    break;
                case "NeonTeal":
                    ForegroundPrimary = "#00ffd1";
                    ForegroundSecundary = "#00ffd1";
                    ForegroundStatus = "#00ffd1";

                    ForegroundOnPrimary = "#00ffd1";
                    ForegroundOnSecundary = "#00ffd1";
                    ForegroundOnLightPart = "#00ffd1";

                    MenuIconColor = "#00ffd1";
                    break;
                case "NeonGold":
                    ForegroundPrimary = "#ffc300";
                    ForegroundSecundary = "#ffc300";
                    ForegroundStatus = "#ffc300";

                    ForegroundOnPrimary = "#ffc300";
                    ForegroundOnSecundary = "#ffc300";
                    ForegroundOnLightPart = "#ffc300";

                    MenuIconColor = "#ffc300";
                    break;
                default:
                    ForegroundPrimary = "#00f7f3";
                    ForegroundSecundary = "#00f7f3";
                    ForegroundStatus = "#00f7f3";

                    ForegroundOnPrimary = "#00f7f3";
                    ForegroundOnSecundary = "#00f7f3";
                    ForegroundOnLightPart = "#00f7f3";

                    MenuIconColor = "#00f7f3";
                    break;
            }

        }
    }
}
