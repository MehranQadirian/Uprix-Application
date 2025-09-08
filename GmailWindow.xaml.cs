using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AppLauncher
{
    public partial class GmailWindow : Window
    {
        #region Variables
        private bool isConnection;
        private bool isExist;
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private string selectedImageBase64 = null;
        private string userName = Environment.UserName;
        #endregion
        #region Methods
        public GmailWindow()
        {
            InitializeComponent();
            isExist = false;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += async (s, ev) => await CheckInternetStatus();
            timer.Start();
            await CheckInternetStatus();
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AnimateLabel(TextBlock label, bool toTop)
        {
            var trans = (TranslateTransform)label.RenderTransform;
            var brush = (SolidColorBrush)label.Foreground;

            var moveAnim = new DoubleAnimation
            {
                To = toTop ? -25 : 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new BackEase
                {
                    Amplitude = 0.7,
                    EasingMode = EasingMode.EaseOut
                }
            };

            var colorAnim = new ColorAnimation
            {
                To = toTop ? Colors.DodgerBlue : Colors.Gray,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            var fontAnim = new DoubleAnimation
            {
                To = toTop ? 16 : 12,
                Duration = TimeSpan.FromMilliseconds(280),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            label.BeginAnimation(TextBlock.FontSizeProperty, fontAnim);
            trans.BeginAnimation(TranslateTransform.YProperty, moveAnim);
            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
        }

        private async void GmailBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string email = GmailBox.Text;
            if (IsValidEmail(email))
            {
                bool exists = await DoesEmailExist(email);
                if (exists)
                {
                    GmailValidationIcon.Data = Geometry.Parse("M61.44,0c33.932,0,61.44,27.508,61.44,61.44c0,33.932-27.508,61.439-61.44,61.439 C27.507,122.88,0,95.372,0,61.44C0,27.508,27.507,0,61.44,0L61.44,0z M34.258,63.075c0.824-4.78,6.28-7.44,10.584-4.851 c0.39,0.233,0.763,0.51,1.11,0.827l0.034,0.032c1.932,1.852,4.096,3.778,6.242,5.688l1.841,1.652l21.84-22.91 c1.304-1.366,2.259-2.25,4.216-2.689c6.701-1.478,11.412,6.712,6.663,11.719L59.565,81.108c-2.564,2.735-7.147,2.985-9.901,0.373 c-1.581-1.466-3.297-2.958-5.034-4.467c-3.007-2.613-6.077-5.28-8.577-7.919C34.551,67.595,33.903,65.139,34.258,63.075 L34.258,63.075z"); // تیک
                    GmailValidationIcon.Fill = Brushes.Green;
                    GmailValidationText.Text = "Gmail exists!";
                    GmailValidationText.Foreground = Brushes.Green;
                    isExist = true;
                    if(lbCheckInternet.Content == "Please enter a valid and existing Gmail address")
                    {
                        lbCheckInternet.Content = "Check you internet!";
                        lbCheckInternet.Visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    GmailValidationIcon.Data = Geometry.Parse("M256 0c70.686 0 134.69 28.658 181.016 74.984C483.342 121.31 512 185.314 512 256c0 70.686-28.658 134.69-74.984 181.016C390.69 483.342 326.686 512 256 512c-70.686 0-134.69-28.658-181.016-74.984C28.658 390.69 0 326.686 0 256c0-70.686 28.658-134.69 74.984-181.016C121.31 28.658 185.314 0 256 0zm19.546 302.281c-.88 22.063-38.246 22.092-39.099-.007-3.779-37.804-13.444-127.553-13.136-163.074.312-10.946 9.383-17.426 20.99-19.898 3.578-.765 7.512-1.136 11.476-1.132 3.987.007 7.932.4 11.514 1.165 11.989 2.554 21.402 9.301 21.398 20.444l-.044 1.117-13.099 161.385zm-19.55 39.211c14.453 0 26.168 11.717 26.168 26.171 0 14.453-11.715 26.167-26.168 26.167s-26.171-11.714-26.171-26.167c0-14.454 11.718-26.171 26.171-26.171z"); // ضربدر
                    GmailValidationIcon.Fill = Brushes.OrangeRed;
                    GmailValidationText.Text = "Format valid, but may not exist";
                    GmailValidationText.Foreground = Brushes.OrangeRed;
                    isExist = false;
                }
            }
            else
            {
                if(!string.IsNullOrEmpty(GmailBox.Text))
                {
                    GmailValidationIcon.Data = Geometry.Parse("M61.44,0c33.933,0,61.441,27.507,61.441,61.439 c0,33.933-27.508,61.44-61.441,61.44C27.508,122.88,0,95.372,0,61.439C0,27.507,27.508,0,61.44,0L61.44,0z M81.719,36.226 c1.363-1.363,3.572-1.363,4.936,0c1.363,1.363,1.363,3.573,0,4.936L66.375,61.439l20.279,20.278c1.363,1.363,1.363,3.573,0,4.937 c-1.363,1.362-3.572,1.362-4.936,0L61.44,66.376L41.162,86.654c-1.362,1.362-3.573,1.362-4.936,0c-1.363-1.363-1.363-3.573,0-4.937 l20.278-20.278L36.226,41.162c-1.363-1.363-1.363-3.573,0-4.936c1.363-1.363,3.573-1.363,4.936,0L61.44,56.504L81.719,36.226 L81.719,36.226z"); // دایره اخطار
                    GmailValidationIcon.Fill = Brushes.Red;
                    GmailValidationText.Text = "Invalid Gmail format.";
                    GmailValidationText.Foreground = Brushes.Red;
                    isExist = false;
                }
                else if(string.IsNullOrEmpty(GmailBox.Text))
                {
                    GmailValidationIcon.Data = Geometry.Parse("M57.85,3.61a3.61,3.61,0,1,1,7.21,0V27.45a3.61,3.61,0,0,1-7.21,0V3.61ZM29.42,13.15a3.6,3.6,0,0,1,6.23-3.61L47.57,30.19a3.6,3.6,0,1,1-6.22,3.61L29.42,13.15ZM9.57,35.62a3.59,3.59,0,0,1,3.58-6.22L33.8,41.32a3.59,3.59,0,1,1-3.58,6.22L9.57,35.62ZM3.61,65a3.61,3.61,0,1,1,0-7.21H27.45a3.61,3.61,0,0,1,0,7.21Zm9.54,28.43a3.6,3.6,0,1,1-3.61-6.23L30.19,75.31a3.6,3.6,0,1,1,3.61,6.22L13.15,93.46Zm22.47,19.85a3.59,3.59,0,0,1-6.22-3.58L41.32,89.08a3.59,3.59,0,1,1,6.22,3.58L35.62,113.31Zm29.41,6a3.61,3.61,0,1,1-7.21,0V95.43a3.61,3.61,0,0,1,7.21,0v23.84Zm28.43-9.54a3.6,3.6,0,0,1-6.23,3.61L75.31,92.69a3.6,3.6,0,0,1,6.22-3.61l11.93,20.65Zm19.85-22.47a3.59,3.59,0,0,1-3.58,6.22L89.08,81.56a3.59,3.59,0,1,1,3.58-6.22l20.65,11.92Zm6-29.41a3.61,3.61,0,1,1,0,7.21H95.43a3.61,3.61,0,0,1,0-7.21Zm-9.54-28.43a3.6,3.6,0,0,1,3.61,6.23L92.69,47.57a3.6,3.6,0,0,1-3.61-6.22l20.65-11.93ZM87.26,9.57a3.59,3.59,0,1,1,6.22,3.58L81.56,33.8a3.59,3.59,0,1,1-6.22-3.58L87.26,9.57Z"); // دایره اخطار
                    GmailValidationIcon.Fill = Brushes.Gray;
                    GmailValidationText.Text = "Status...";
                    GmailValidationText.Foreground = Brushes.Gray;
                    isExist = false;
                }
            }
        }

        private void SetValidationMessage(string text, string svgPathData, Brush color)
        {
            GmailValidationMessage.Children.Clear();

            // آیکون SVG به صورت Path
            var path = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse(svgPathData), // داده‌های SVG path
                Fill = color,
                Width = 10,
                Height = 10,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5, 0, 5, 0)
            };

            // متن
            var tb = new TextBlock
            {
                Text = text,
                Foreground = color,
                VerticalAlignment = VerticalAlignment.Center
            };

            GmailValidationMessage.Children.Add(path);
            GmailValidationMessage.Children.Add(tb);
        }

        private async Task<bool> DoesEmailExist(string email)
        {
            using (var client = new HttpClient())
            {
                string apiKey = "<YOUR-API-KEY>"; 
                string url = $"https://emailvalidation.abstractapi.com/v1/?api_key={apiKey}&email={email}";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode) return false;

                var result = await response.Content.ReadAsStringAsync();

                dynamic json = JsonConvert.DeserializeObject(result);
                return json.deliverability == "DELIVERABLE";
            }
        }

        private bool IsValidEmail(string email)
        {
            // بررسی ساده با regex برای gmail
            string pattern = @"^[a-zA-Z0-9._%+-]+@gmail\.com$";
            return Regex.IsMatch(email, pattern);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == SubjectBox)
                AnimateLabel(SubjectLabel, true);
            else if (tb == MessageBox)
                AnimateLabel(MessageLabel, true);
            else if (tb == GmailBox)
                AnimateLabel(GmailLabel, true);
        }

        private void btnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                string filePath = dlg.FileName;

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                SelectedImagePreview.Source = bitmap;
                SelectedImagePreview.Visibility = Visibility.Visible;

                byte[] imageBytes = File.ReadAllBytes(filePath);
                string mimeType = "image/png";
                string extension = Path.GetExtension(filePath).ToLower();
                if (extension == ".jpg" || extension == ".jpeg") mimeType = "image/jpeg";
                else if (extension == ".bmp") mimeType = "image/bmp";
                else if (extension == ".gif") mimeType = "image/gif";

                selectedImageBase64 = $"data:{mimeType};base64,{Convert.ToBase64String(imageBytes)}";
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == SubjectBox && string.IsNullOrWhiteSpace(tb.Text))
                AnimateLabel(SubjectLabel, false);
            if (tb == MessageBox && string.IsNullOrWhiteSpace(tb.Text))
                AnimateLabel(MessageLabel, false);
            if (tb == GmailBox && string.IsNullOrWhiteSpace(tb.Text))
                AnimateLabel(GmailLabel, false);
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            string userEmail = GmailBox.Text;
            string subject = SubjectBox.Text;
            string message = MessageBox.Text;

            var payload = new
            {
                userEmail = userEmail,
                subject = subject,
                message = message,
                file = selectedImageBase64
            };

            try
            {
                if (isExist)
                {
                    OverlayPanel.Visibility = Visibility.Visible;

                    using HttpClient client = new HttpClient();
                    string json = System.Text.Json.JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("https://ancient-night-0780.mehranghadirian01.workers.dev/", content);
                    MainWindow main = new MainWindow();
                    await main.SendMessageAsync($"{userName} sent a message in @uprixfbbot");
                    OverlayPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    lbCheckInternet.Content = "Please enter a valid and existing Gmail address";
                    lbCheckInternet.Visibility = Visibility.Visible;
                }
            }
            catch
            {
                OverlayPanel.Visibility = Visibility.Collapsed;
            }
        }

        public async Task CheckInternetStatus()
        {
            isConnection = await IsInternetAvailable();
            if (isConnection)
            {
                Connection.Data = Geometry.Parse("M109.465,89.503c0.182,0,0.359,0.019,0.533,0.053c1.146-1.998,2.191-4.095,3.135-6.286 c0.018-0.044,0.037-0.086,0.059-0.128c1.418-3.345,2.488-6.819,3.209-10.419c0.559-2.793,0.904-5.657,1.035-8.591h-16.893 c-0.307,8.574-2.867,17.03-7.639,25.371H109.465L109.465,89.503z M106.52,94.889H89.506c-5.164,7.481-12.121,14.87-20.838,22.167 c1.367-0.17,2.719-0.388,4.055-0.655c3.646-0.729,7.164-1.817,10.549-3.264l-0.002-0.004c3.441-1.48,6.646-3.212,9.609-5.199 c2.969-1.992,5.721-4.255,8.25-6.795l0.01-0.01l0,0C103.096,99.18,104.889,97.099,106.52,94.889L106.52,94.889z M54.21,117.055 c-8.716-7.296-15.673-14.685-20.838-22.166H16.361c1.631,2.21,3.423,4.291,5.379,6.24l0.01,0.011v-0.001 c2.53,2.54,5.282,4.803,8.25,6.795c2.962,1.987,6.167,3.719,9.609,5.199c0.043,0.019,0.086,0.038,0.128,0.059 c3.345,1.419,6.819,2.488,10.42,3.209C51.493,116.668,52.843,116.886,54.21,117.055L54.21,117.055z M12.852,89.503h17.122 c-4.771-8.341-7.332-16.797-7.637-25.371H5.445c0.13,2.934,0.475,5.797,1.034,8.59c0.729,3.646,1.818,7.164,3.264,10.549 l0.004-0.001C10.682,85.442,11.716,87.521,12.852,89.503L12.852,89.503z M5.445,58.747h16.997c0.625-8.4,3.412-16.857,8.407-25.371 H12.852c-1.136,1.982-2.17,4.061-3.105,6.234c-0.019,0.043-0.039,0.086-0.059,0.127C8.269,43.083,7.2,46.557,6.479,50.157 C5.92,52.95,5.575,55.814,5.445,58.747L5.445,58.747z M16.361,27.991h17.938c5.108-7.361,11.862-14.765,20.29-22.212 c-1.496,0.175-2.973,0.408-4.431,0.7c-3.647,0.729-7.164,1.818-10.549,3.264l0.001,0.003c-3.442,1.481-6.647,3.212-9.609,5.2 c-2.968,1.992-5.72,4.255-8.25,6.794l-0.011,0.01h0C19.784,23.7,17.992,25.78,16.361,27.991L16.361,27.991z M68.289,5.778 c8.428,7.447,15.182,14.851,20.291,22.212h17.939c-1.631-2.21-3.424-4.291-5.381-6.24l-0.01-0.01l0,0 c-2.529-2.54-5.281-4.802-8.25-6.794c-2.963-1.988-6.168-3.719-9.609-5.2c-0.043-0.018-0.086-0.038-0.127-0.059 c-3.346-1.418-6.82-2.488-10.42-3.208C71.264,6.187,69.785,5.954,68.289,5.778L68.289,5.778z M110.027,33.376H92.029 c4.996,8.514,7.783,16.971,8.408,25.371h16.998c-0.131-2.934-0.477-5.797-1.035-8.59c-0.73-3.646-1.818-7.164-3.264-10.549 l-0.004,0.002C112.197,37.437,111.164,35.358,110.027,33.376L110.027,33.376z M49.106,1.198C53.098,0.399,57.21,0,61.44,0 c4.23,0,8.341,0.399,12.333,1.198c3.934,0.788,7.758,1.97,11.473,3.547c0.051,0.018,0.1,0.037,0.148,0.058 c3.703,1.594,7.197,3.485,10.471,5.684c3.268,2.192,6.291,4.677,9.066,7.462c2.785,2.775,5.27,5.799,7.461,9.065 c2.197,3.275,4.09,6.768,5.684,10.473l-0.004,0.001l0.004,0.009c1.607,3.758,2.809,7.628,3.605,11.609 c0.799,3.992,1.197,8.104,1.197,12.334c0,4.23-0.398,8.343-1.197,12.335c-0.787,3.932-1.971,7.758-3.547,11.472 c-0.018,0.05-0.037,0.099-0.059,0.147c-1.594,3.705-3.486,7.197-5.684,10.472c-2.191,3.267-4.676,6.29-7.461,9.065 c-2.775,2.785-5.799,5.271-9.066,7.462c-3.273,2.198-6.768,4.091-10.471,5.684l-0.002-0.004l-0.01,0.004 c-3.758,1.606-7.629,2.808-11.609,3.604c-3.992,0.799-8.104,1.198-12.333,1.198c-4.229,0-8.342-0.399-12.334-1.198 c-3.933-0.787-7.758-1.97-11.474-3.546c-0.049-0.019-0.098-0.037-0.147-0.059c-3.705-1.593-7.197-3.485-10.472-5.684 c-3.266-2.191-6.29-4.677-9.065-7.462c-2.785-2.775-5.27-5.799-7.461-9.065c-2.198-3.274-4.09-6.767-5.684-10.472l0.004-0.002 l-0.004-0.009c-1.606-3.758-2.808-7.628-3.604-11.609C0.4,69.782,0,65.67,0,61.439c0-4.229,0.4-8.342,1.199-12.334 c0.787-3.933,1.97-7.758,3.546-11.473c0.018-0.049,0.037-0.099,0.058-0.147c1.594-3.705,3.485-7.198,5.684-10.473 c2.192-3.266,4.677-6.29,7.461-9.065c2.775-2.785,5.799-5.27,9.065-7.462c3.275-2.198,6.768-4.09,10.472-5.684l0.001,0.004 l0.009-0.004C41.254,3.197,45.125,1.995,49.106,1.198L49.106,1.198z M64.133,9.268v18.723h17.826 C77.275,21.815,71.34,15.575,64.133,9.268L64.133,9.268z M64.133,33.376v25.371h30.922c-0.699-8.332-3.789-16.788-9.318-25.371 H64.133L64.133,33.376z M64.133,64.132v25.371h22.51c5.328-8.396,8.189-16.854,8.531-25.371H64.133L64.133,64.132z M64.133,94.889 v18.952c7.645-6.283,13.902-12.601,18.746-18.952H64.133L64.133,94.889z M58.747,113.843V94.889H40 C44.843,101.24,51.1,107.559,58.747,113.843L58.747,113.843z M58.747,89.503V64.132H27.706c0.341,8.518,3.201,16.975,8.531,25.371 H58.747L58.747,89.503z M58.747,58.747V33.376H37.143c-5.529,8.583-8.619,17.04-9.319,25.371H58.747L58.747,58.747z M58.747,27.991 V9.266C51.54,15.573,45.604,21.815,40.92,27.991H58.747L58.747,27.991z");
                Connection.Fill = new SolidColorBrush(Color.FromRgb(80, 178, 240));
                Connection.Visibility = Visibility.Visible;
                Connection.Opacity = 1;
                ReloadButton.Visibility = Visibility.Hidden;
                if(lbCheckInternet.Content != "Please enter a valid and existing Gmail address")
                {
                    lbCheckInternet.Visibility = Visibility.Hidden;
                    lbCheckInternet.Content = "Check you internet!";
                }
            }
            else
            {
                Connection.Data = Geometry.Parse("M113.875,65.43c0.105-1.039,0.18-2.088,0.221-3.154h-5.076c-5.268-2.862-11.303-4.487-17.719-4.487 c-6.441,0-12.5,1.639-17.783,4.524H62.313v9.387c-2.119,2.637-3.887,5.568-5.229,8.725V62.313H26.93 c0.322,8.271,3.116,16.473,8.272,24.637h19.799c-0.373,1.703-0.632,3.449-0.764,5.229H38.855 c4.691,6.16,10.779,12.32,18.228,18.408v-1.1c1.529,3.596,3.608,6.902,6.133,9.811c-1.164,0.066-2.338,0.098-3.519,0.098 c-4.118,0-8.094-0.393-11.997-1.182c-3.832-0.752-7.52-1.896-11.137-3.438c-0.036,0-0.107-0.035-0.143-0.072 c-3.581-1.539-6.983-3.4-10.171-5.514c-3.187-2.113-6.124-4.549-8.81-7.234c-2.722-2.686-5.121-5.623-7.234-8.811 c-2.148-3.186-3.975-6.588-5.515-10.17c-1.576-3.652-2.722-7.412-3.51-11.281C0.394,67.826,0,63.816,0,59.698 c0-4.118,0.394-8.094,1.182-11.997c0.752-3.832,1.898-7.52,3.438-11.137c0-0.036,0.036-0.107,0.072-0.143 c1.54-3.617,3.402-6.983,5.515-10.171c2.113-3.187,4.548-6.124,7.234-8.81c2.686-2.722,5.623-5.121,8.81-7.234 c3.188-2.149,6.59-3.975,10.171-5.515c3.653-1.576,7.413-2.722,11.28-3.51C51.568,0.394,55.58,0,59.698,0 c4.119,0,8.093,0.394,11.998,1.182c3.83,0.752,7.52,1.898,11.137,3.438c0.035,0,0.107,0.036,0.143,0.072 c3.582,1.54,6.984,3.402,10.172,5.515c3.186,2.113,6.123,4.548,8.809,7.234c2.723,2.686,5.121,5.623,7.234,8.81 c2.148,3.188,3.975,6.589,5.514,10.171c1.576,3.652,2.723,7.413,3.51,11.28c0.789,3.868,1.182,7.879,1.182,11.997 c0,3.423-0.271,6.75-0.816,10.017C117.137,68.156,115.563,66.723,113.875,65.43L113.875,65.43z M91.301,67.029 c15.42,0,27.926,12.504,27.926,27.926c0,15.42-12.506,27.924-27.926,27.924s-27.926-12.504-27.926-27.924 C63.375,79.533,75.881,67.029,91.301,67.029L91.301,67.029z M108.146,83.867l-27.932,27.934c3.182,2.098,6.99,3.32,11.086,3.32 c11.135,0,20.166-9.031,20.166-20.166c0-4.096-1.223-7.904-3.32-11.086V83.867L108.146,83.867z M74.455,106.039l27.932-27.932 c-3.18-2.098-6.99-3.32-11.086-3.32c-11.135,0-20.166,9.031-20.166,20.166C71.135,99.049,72.357,102.857,74.455,106.039 L74.455,106.039L74.455,106.039z M52.678,113.701c-8.451-7.09-15.22-14.252-20.233-21.523H15.936 c1.576,2.15,3.331,4.154,5.229,6.053c2.471,2.471,5.121,4.656,8.022,6.59c2.865,1.934,5.98,3.617,9.347,5.049 c0.036,0.037,0.072,0.037,0.107,0.072c3.258,1.361,6.625,2.436,10.134,3.115c1.29,0.25,2.614,0.465,3.939,0.645H52.678 L52.678,113.701z M12.499,86.949h16.616c-4.62-8.092-7.127-16.33-7.413-24.637H5.3c0.144,2.863,0.465,5.621,1.003,8.344 c0.716,3.545,1.755,6.947,3.187,10.242C10.385,83.012,11.388,85.016,12.499,86.949L12.499,86.949z M5.3,57.083h16.509 c0.608-8.165,3.331-16.366,8.165-24.638H12.499c-1.11,1.934-2.113,3.939-3.009,6.052c-0.036,0.036-0.036,0.072-0.071,0.107 c-1.361,3.259-2.436,6.625-3.116,10.135C5.765,51.461,5.407,54.218,5.3,57.083L5.3,57.083z M15.9,27.216h17.44 c4.978-7.162,11.531-14.36,19.696-21.594c-1.468,0.179-2.9,0.394-4.297,0.68c-3.545,0.716-6.948,1.754-10.242,3.187 c-3.331,1.433-6.446,3.116-9.347,5.05c-2.901,1.934-5.551,4.118-8.022,6.589c-1.898,1.898-3.652,3.903-5.229,6.052V27.216 L15.9,27.216z M66.359,5.623c8.199,7.233,14.754,14.432,19.695,21.594h17.441c-1.576-2.148-3.332-4.154-5.229-6.052 c-2.473-2.471-5.121-4.656-8.023-6.589c-2.865-1.934-5.98-3.617-9.346-5.049c-0.037-0.036-0.072-0.036-0.107-0.072 c-3.26-1.361-6.625-2.435-10.135-3.115c-1.434-0.287-2.865-0.502-4.297-0.681V5.623L66.359,5.623z M106.896,32.445H89.422 c4.834,8.272,7.555,16.473,8.164,24.638h16.51c-0.145-2.865-0.467-5.623-1.004-8.344c-0.715-3.545-1.754-6.947-3.186-10.242 C109.01,36.384,108.008,34.379,106.896,32.445L106.896,32.445L106.896,32.445z M62.313,9.025v18.191h17.332 C75.096,21.2,69.33,15.148,62.313,9.025L62.313,9.025z M62.313,32.445v24.638h30.045c-0.68-8.094-3.688-16.294-9.061-24.638H62.313 L62.313,32.445z M57.083,57.083V32.445H36.098c-5.372,8.344-8.379,16.544-9.06,24.638H57.083L57.083,57.083z M57.083,27.216V9.025 C50.064,15.148,44.299,21.2,39.75,27.216H57.083L57.083,27.216z");
                Connection.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                Connection.Visibility = Visibility.Visible;
                Connection.Opacity = 0.7;
                ReloadButton.Visibility = Visibility.Visible;
                lbCheckInternet.Visibility = Visibility.Visible;
                lbCheckInternet.Content = "Check you internet!";
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

        private void HeaderPanel_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private async void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            Connection.Visibility = Visibility.Hidden;
            ReloadButton.Visibility = Visibility.Hidden;
            await CheckInternetStatus();
        }
        #endregion
    }
}
