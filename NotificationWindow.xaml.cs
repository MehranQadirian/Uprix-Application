using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AppLauncher
{
    public partial class NotificationWindow : Window
    {
        public bool IsNotificationVisible { get; private set; } = false;

        public NotificationWindow(string caption, string text, MessageBoxImage type)
        {
            InitializeComponent();

            CaptionBlock.Text = caption;
            TextBlock.Text = text;

            Icon sysIcon;

            switch (type)
            {
                case MessageBoxImage.Error: sysIcon = SystemIcons.Error; break;
                case MessageBoxImage.Information: sysIcon = SystemIcons.Information; break;
                case MessageBoxImage.Warning: sysIcon = SystemIcons.Warning; break;
                case MessageBoxImage.Question: sysIcon = SystemIcons.Question; break;
                default: sysIcon = SystemIcons.Information; break;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                sysIcon.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();

                IconImage.Source = bitmap;
            }

            this.Opacity = 0;
        }

        public void ShowNotification()
        {
            if (IsNotificationVisible) return;
            IsNotificationVisible = true;

            this.Show();

            var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            var slide = new ThicknessAnimation(
                new Thickness(0, -20, 0, 0),
                new Thickness(0),
                TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            this.BeginAnimation(Window.OpacityProperty, fade);
            this.BeginAnimation(Window.MarginProperty, slide);
        }

        public void CloseNotification()
        {
            var fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250));
            fade.Completed += (s, e) =>
            {
                this.Close();
                IsNotificationVisible = false;
            };
            this.BeginAnimation(Window.OpacityProperty, fade);

        }
    }
}
