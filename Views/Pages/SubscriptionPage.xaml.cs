using System.Windows.Controls;
using System.Windows.Media;
using AppLauncher.Classes;
using AppLauncher.Classes.Core_Classes;

namespace AppLauncher.Views.Pages
{
    public partial class SubscriptionPage : Page
    {
        private ApplyThemes apply;

        public SubscriptionPage(ApplyThemes themes)
        {
            InitializeComponent();
            apply = themes;
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundPrimary));
            SoonText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
        }
    }
}