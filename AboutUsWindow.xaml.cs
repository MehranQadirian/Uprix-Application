using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    /// <summary>
    /// Interaction logic for AboutUsWindow.xaml
    /// </summary>
    public partial class AboutUsWindow : Window
    {
        public AboutUsWindow()
        {
            InitializeComponent();
        }
        private void btnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            var win = new UpdateWindow { Owner = this };
            win.ShowDialog();
        }

        private void btnTelegram_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://t.me/UprixApplication",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open Telegram link:\n" + ex.Message);
            }
        }

        private void btnGithub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/MehranQadirian/Uprix-Application",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open GitHub link:\n" + ex.Message);
            }
        }

        private void btnTelephone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string machineName = Environment.MachineName;
                string userName = Environment.UserName;
                string osVersion = Environment.OSVersion.ToString();
                string dotnetVersion = Environment.Version.ToString();

                string subject = Uri.EscapeDataString("Uprix Application - Support Request");
                string body = Uri.EscapeDataString(
                    $"Hello Uprix Team" +
                    $"\nI need support regarding the Uprix Application" +
                    $"\nMachine Name: {machineName}" +
                    $"\nUser Name: {userName}" +
                    $"\nOS Version: {osVersion}" +
                    $"\n.NET Version: {dotnetVersion}" +
                    $"\n\n\n[Please describe your issue here... <Persian or English> ]:\n\n\t"
                );

                string gmailUrl = $"https://mail.google.com/mail/?view=cm&fs=1" +
                                  $"&to=mehranghadirian01@gmail.com" +
                                  $"&su={subject}" +
                                  $"&body={body}";

                Process.Start(new ProcessStartInfo
                {
                    FileName = gmailUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open Gmail:\n" + ex.Message);
            }
        }
    }
}
