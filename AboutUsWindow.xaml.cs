using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AppLauncher
{
    public partial class AboutUsWindow : Window
    {
        #region Variables
        private MainWindow main = new MainWindow();
        private string userName = Environment.UserName;
        #endregion
        #region Methods
        public AboutUsWindow()
        {
            InitializeComponent();
        }

        private async void btnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            bool isConnection = await IsInternetAvailable();
            var win = new UpdateWindow { Owner = this };
            if (isConnection)
                win.ShowDialog();
        }

        private async void btnTelegram_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://t.me/UprixApplication",
                    UseShellExecute = true
                });
                await main.SendMessageAsync($"{userName} Opened Uprix Telegram Channel", "", "normal");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open Telegram link:\n" + ex.Message);
            }
        }

        public static async Task<bool> IsInternetAvailable()
        {
            try
            {
                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };
                var response = await client.GetAsync("https://www.google.com");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private async void btnGithub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/MehranQadirian/Uprix-Application",
                    UseShellExecute = true
                });
                await main.SendMessageAsync($"{userName} Opened Uprix Launcher Github", "", "high");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open GitHub link:\n" + ex.Message);
            }
        }

        private async void btnTelephone_Click(object sender, RoutedEventArgs e)
        {
            GmailWindow gmail = new GmailWindow();
            Hide();
            gmail.ShowDialog();
            Show();
        }
        #endregion
    }
}
