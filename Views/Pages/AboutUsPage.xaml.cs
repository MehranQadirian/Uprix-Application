using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using AppLauncher.Classes.MainClasses;
using AppLauncher.Classes.Core_Classes;
using AppLauncher.Views.Windows;
using AppLauncher.Classes;

namespace AppLauncher.Views.Pages
{
    /// <summary>
    /// Modern, optimized About Us page with clean architecture
    /// </summary>
    public partial class AboutUsPage : Page
    {
        #region Fields
        private readonly MainWindow _mainWindow;
        private readonly UpdaterService _updaterService;
        private readonly ApplyThemes _themeManager;
        private readonly string _currentVersion = "v3.0.0.0";
        private readonly string _userName = Environment.UserName;

        private string _selectedImageBase64;
        private CancellationTokenSource _emailValidationCts;
        private bool _isAnimating;

        public bool IsUpdateAvailable { get; set; }

        // Cached brushes for performance
        private SolidColorBrush _primaryBrush;
        private SolidColorBrush _secondaryBrush;
        private SolidColorBrush _textBrush;
        #endregion

        #region Constructor
        public AboutUsPage(MainWindow mainWindow, ApplyThemes themeManager)
        {
            InitializeComponent();

            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
            _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
            _updaterService = new UpdaterService(_currentVersion, _mainWindow);

            Loaded += OnPageLoaded;
            Unloaded += OnPageUnloaded;
        }
        #endregion

        #region Lifecycle Events
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            InitializeTheme();
            AnimatePageEntrance();
            var year = DateTime.Now.Year;
            txtCRight.Text = $"© {year} All rights reserved";
            if (IsUpdateAvailable)
            {
                UpdateBadge.Visibility = Visibility.Visible;
                AnimatePulse(UpdateBadge);
            }
        }

        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            // Cleanup
            _emailValidationCts?.Cancel();
            _emailValidationCts?.Dispose();
            _primaryBrush = null;
            _secondaryBrush = null;
            _textBrush = null;
        }
        #endregion

        #region Theme Management
        private void InitializeTheme()
        {
            _themeManager.ApplyThm();

            // Create and cache brushes
            _primaryBrush = CreateBrush(_themeManager.BackgroundPrimary);
            _secondaryBrush = CreateBrush(_themeManager.BackgroundSecundary);
            _textBrush = CreateBrush(_themeManager.MenuIconColor);

            // Set dynamic resources
            Resources["BackgroundPrimary"] = _primaryBrush;
            Resources["BackgroundSecondary"] = _secondaryBrush;
            Resources["MenuIconColor"] = _textBrush;
        }

        private SolidColorBrush CreateBrush(string colorHex)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
            brush.Freeze(); // Freeze for better performance
            return brush;
        }
        #endregion

        #region Animations
        private void AnimatePageEntrance()
        {
            if (_isAnimating) return;
            _isAnimating = true;

            var elements = new FrameworkElement[]
            {
                HeroCard, AboutCard, FeaturesGrid, DeveloperCard, SocialCard
            };

            for (int i = 0; i < elements.Length; i++)
            {
                AnimateFadeIn(elements[i], TimeSpan.FromMilliseconds(i * 80));
            }

            // Reset flag after animations complete
            Dispatcher.InvokeAsync(async () =>
            {
                await Task.Delay(elements.Length * 80 + 400);
                _isAnimating = false;
            });
        }

        private void AnimateFadeIn(FrameworkElement element, TimeSpan delay)
        {
            element.Opacity = 0;
            element.RenderTransform = new TranslateTransform(0, 20);

            var storyboard = new Storyboard { BeginTime = delay };

            // Fade animation
            var fadeAnim = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(fadeAnim, element);
            Storyboard.SetTargetProperty(fadeAnim, new PropertyPath(OpacityProperty));

            // Slide animation
            var slideAnim = new DoubleAnimation
            {
                From = 20,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(slideAnim, element);
            Storyboard.SetTargetProperty(slideAnim, new PropertyPath("RenderTransform.Y"));

            storyboard.Children.Add(fadeAnim);
            storyboard.Children.Add(slideAnim);
            storyboard.Begin();
        }

        private void AnimatePulse(FrameworkElement element)
        {
            var animation = new DoubleAnimation
            {
                From = 1,
                To = 0.7,
                Duration = TimeSpan.FromSeconds(1),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            element.BeginAnimation(OpacityProperty, animation);
        }

        private void AnimateScale(FrameworkElement element, double from, double to)
        {
            if (element.RenderTransform == null || !(element.RenderTransform is ScaleTransform))
            {
                element.RenderTransform = new ScaleTransform(1, 1);
                element.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            var scaleX = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var scaleY = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            element.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
            element.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);
        }
        #endregion

        #region Social Icon Interactions
        private void SocialIcon_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                AnimateScale(border, 1, 1.1);
            }
        }

        private void SocialIcon_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                AnimateScale(border, 1.1, 1);
            }
        }

        private async void Telegram_Click(object sender, MouseButtonEventArgs e)
        {
            await OpenUrlAsync("https://t.me/UprixApplication", "Telegram");
        }

        private async void GitHub_Click(object sender, MouseButtonEventArgs e)
        {
            await OpenUrlAsync("https://github.com/MehranQadirian/Uprix-Application", "GitHub");
        }

        private void Email_Click(object sender, MouseButtonEventArgs e)
        {
            ShowFeedbackModal();
        }

        private async void Update_Click(object sender, MouseButtonEventArgs e)
        {
            if (await IsInternetAvailableAsync())
            {
                var updateWindow = new UpdateWindow(_mainWindow, _themeManager);
                updateWindow.ShowDialog();
            }
            else
            {
                ShowMessage("No internet connection available.", "Connection Error", MessageBoxImage.Warning);
            }
        }

        private void LumoraFlowButton_Click(object sender, RoutedEventArgs e)
        {
            _ = OpenUrlAsync("https://lumora-flow.pages.dev", "Lumora Flow");
        }
        #endregion

        #region Feedback Modal
        private void ShowFeedbackModal()
        {
            FeedbackOverlay.Visibility = Visibility.Visible;
            FeedbackModal.Opacity = 0;
            FeedbackModal.RenderTransform = new ScaleTransform(0.9, 0.9);
            FeedbackModal.RenderTransformOrigin = new Point(0.5, 0.5);

            var storyboard = new Storyboard();

            var fadeAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            Storyboard.SetTarget(fadeAnim, FeedbackModal);
            Storyboard.SetTargetProperty(fadeAnim, new PropertyPath(OpacityProperty));

            var scaleAnim = new DoubleAnimation(0.9, 1, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new BackEase { Amplitude = 0.3, EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(scaleAnim, FeedbackModal);
            Storyboard.SetTargetProperty(scaleAnim, new PropertyPath("RenderTransform.ScaleX"));

            var scaleAnimY = new DoubleAnimation(0.9, 1, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new BackEase { Amplitude = 0.3, EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(scaleAnimY, FeedbackModal);
            Storyboard.SetTargetProperty(scaleAnimY, new PropertyPath("RenderTransform.ScaleY"));

            storyboard.Children.Add(fadeAnim);
            storyboard.Children.Add(scaleAnim);
            storyboard.Children.Add(scaleAnimY);
            storyboard.Begin();
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            var fadeAnim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
            fadeAnim.Completed += (s, ev) =>
            {
                FeedbackOverlay.Visibility = Visibility.Collapsed;
                ClearFeedbackForm();
            };
            FeedbackOverlay.BeginAnimation(OpacityProperty, fadeAnim);
        }

        private void Modal_StopPropagation(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true; // Prevent closing when clicking inside modal
        }

        private void ClearFeedbackForm()
        {
            EmailInput.Clear();
            SubjectInput.Clear();
            MessageInput.Clear();
            EmailError.Visibility = Visibility.Collapsed;
            AttachmentName.Visibility = Visibility.Collapsed;
            _selectedImageBase64 = null;
        }
        #endregion

        #region Feedback Submission
        private async void SendFeedback_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailInput.Text.Trim();
            var subject = SubjectInput.Text.Trim();
            var message = MessageInput.Text.Trim();

            // Validation
            if (!ValidateInput(email, subject, message)) return;

            // Show loading
            LoadingPanel.Visibility = Visibility.Visible;
            StartLoadingAnimation();

            try
            {
                var payload = new
                {
                    userEmail = email,
                    subject = subject,
                    message = message,
                    file = _selectedImageBase64
                };

                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    "<YOUR-URL(WORKER)>", content);

                LoadingPanel.Visibility = Visibility.Collapsed;

                if (response.IsSuccessStatusCode)
                {
                    ShowMessage("Thank you! We'll get back to you soon.", "Message Sent", MessageBoxImage.Information);
                    CloseModal_Click(sender, e);
                }
                else
                {
                    ShowMessage("Failed to send. Please try again.", "Error", MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
                ShowMessage($"Error: {ex.Message}", "Error", MessageBoxImage.Error);
            }
        }

        private bool ValidateInput(string email, string subject, string message)
        {
            if (string.IsNullOrEmpty(email))
            {
                ShowValidationError("Please enter your email");
                EmailInput.Focus();
                return false;
            }

            if (!IsValidEmail(email))
            {
                ShowValidationError("Invalid email format");
                EmailInput.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(subject))
            {
                ShowMessage("Please enter a subject", "Validation", MessageBoxImage.Warning);
                SubjectInput.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(message))
            {
                ShowMessage("Please enter your message", "Validation", MessageBoxImage.Warning);
                MessageInput.Focus();
                return false;
            }

            return true;
        }

        private void ShowValidationError(string message)
        {
            EmailError.Text = message;
            EmailError.Visibility = Visibility.Visible;
        }

        private void AttachImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                Title = "Select Image"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var fileInfo = new System.IO.FileInfo(dialog.FileName);
                    if (fileInfo.Length > 5 * 1024 * 1024)
                    {
                        ShowMessage("Image must be less than 5MB", "File Too Large", MessageBoxImage.Warning);
                        return;
                    }

                    var bytes = System.IO.File.ReadAllBytes(dialog.FileName);
                    var ext = fileInfo.Extension.ToLower();
                    var mimeType = ext == ".png" ? "image/png" : "image/jpeg";

                    _selectedImageBase64 = $"data:{mimeType};base64,{Convert.ToBase64String(bytes)}";

                    AttachmentName.Text = $"📎 {fileInfo.Name}";
                    AttachmentName.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    ShowMessage($"Failed to load image: {ex.Message}", "Error", MessageBoxImage.Error);
                }
            }
        }

        private void StartLoadingAnimation()
        {
            var rotation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(1.5),
                RepeatBehavior = RepeatBehavior.Forever
            };
            LoadingSpinner.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, rotation);
        }
        #endregion

        #region Utilities
        private async Task OpenUrlAsync(string url, string serviceName)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to open {serviceName}: {ex.Message}", "Error", MessageBoxImage.Error);
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            try
            {
                var regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> IsInternetAvailableAsync()
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var response = await client.GetAsync("https://www.google.com");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private void ShowMessage(string message, string title, MessageBoxImage icon)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }
        #endregion
    }
}