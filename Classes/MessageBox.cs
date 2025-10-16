using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Input;

namespace AppLauncher.Classes
{
    /// <summary>
    /// Custom MessageBox implementation with neon-themed styling and smooth animations
    /// </summary>
    public class MessageBox
    {
        #region Enums

        /// <summary>
        /// Specifies the type of message box icon to display
        /// </summary>
        public enum MessageBoxIcon
        {
            None,
            Information,
            Warning,
            Error,
            Question,
            Success
        }

        /// <summary>
        /// Specifies the button configuration for the message box
        /// </summary>
        public enum MessageBoxButton
        {
            OK,
            OKCancel,
            YesNo,
            YesNoCancel
        }

        /// <summary>
        /// Specifies the result returned by the message box
        /// </summary>
        public enum MessageBoxResult
        {
            None,
            OK,
            Cancel,
            Yes,
            No
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Displays a message box with the specified text
        /// </summary>
        public static MessageBoxResult Show(string messageBoxText)
        {
            return Show(messageBoxText, string.Empty, MessageBox.MessageBoxButton.OK, MessageBoxIcon.None);
        }

        /// <summary>
        /// Displays a message box with the specified text and title
        /// </summary>
        public static MessageBoxResult Show(string messageBoxText, string caption)
        {
            return Show(messageBoxText, caption, MessageBox.MessageBoxButton.OK, MessageBoxIcon.None);
        }

        /// <summary>
        /// Displays a message box with the specified text, title, and buttons
        /// </summary>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button)
        {
            return Show(messageBoxText, caption, button, MessageBoxIcon.None);
        }

        /// <summary>
        /// Displays a message box with the specified text, title, buttons, and icon
        /// </summary>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxIcon icon)
        {
            var window = new MessageBoxWindow(messageBoxText, caption, button, icon);
            window.ShowDialog();
            return window.Result;
        }

        #endregion

        #region Private MessageBoxWindow Class

        /// <summary>
        /// Internal window implementation for the custom message box
        /// </summary>
        private class MessageBoxWindow : Window
        {
            private MessageBoxResult _result = MessageBoxResult.None;
            private ApplyThemes _theme;
            private Grid _mainGrid;
            private Border _contentBorder;
            private StackPanel _buttonPanel;

            public MessageBoxResult Result => _result;

            public MessageBoxWindow(string message, string caption, MessageBoxButton buttons, MessageBoxIcon icon)
            {
                // Load theme
                _theme = new ApplyThemes();
                _theme.ApplyThm();

                // Configure window
                InitializeWindow(caption);

                // Build UI
                BuildInterface(message, icon, buttons);

                // Apply animations
                ApplyEntryAnimation();
            }

            /// <summary>
            /// Initializes the window properties and styling
            /// </summary>
            private void InitializeWindow(string caption)
            {
                Title = caption;
                Width = 480;
                MinWidth = 400;
                MaxWidth = 600;
                SizeToContent = SizeToContent.Height;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                AllowsTransparency = true;
                Background = Brushes.Transparent;
                ShowInTaskbar = false;
                Topmost = true;
            }

            /// <summary>
            /// Constructs the complete UI hierarchy for the message box
            /// </summary>
            private void BuildInterface(string message, MessageBoxIcon icon, MessageBoxButton buttons)
            {
                // Main container with shadow effect
                _mainGrid = new Grid
                {
                    Margin = new Thickness(10),
                    Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        Color = (Color)ColorConverter.ConvertFromString(_theme.ForegroundPrimary),
                        BlurRadius = 20,
                        ShadowDepth = 0,
                        Opacity = 0.5
                    }
                };

                // Background border with rounded corners
                _contentBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_theme.BackgroundPrimary)),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_theme.ForegroundPrimary)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(0)
                };

                // Content stack panel
                var contentStack = new StackPanel();

                // Title bar
                var titleBar = CreateTitleBar(Title);
                contentStack.Children.Add(titleBar);

                // Message content area
                var messageArea = CreateMessageArea(message, icon);
                contentStack.Children.Add(messageArea);

                // Button panel
                _buttonPanel = CreateButtonPanel(buttons);
                contentStack.Children.Add(_buttonPanel);

                _contentBorder.Child = contentStack;
                _mainGrid.Children.Add(_contentBorder);
                Content = _mainGrid;
            }

            /// <summary>
            /// Creates the title bar with close button
            /// </summary>
            private Border CreateTitleBar(string title)
            {
                var titleBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_theme.BackgroundSecundary)),
                    Padding = new Thickness(20, 15, 15, 15),
                    CornerRadius = new CornerRadius(8, 8, 0, 0)
                };

                var titleGrid = new Grid();
                titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // Title text
                var titleText = new TextBlock
                {
                    Text = string.IsNullOrEmpty(title) ? "Message" : title,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_theme.ForegroundPrimary)),
                    FontSize = 16,
                    FontWeight = FontWeights.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(titleText, 0);

                // Close button
                var closeButton = CreateCloseButton();
                Grid.SetColumn(closeButton, 1);

                titleGrid.Children.Add(titleText);
                titleGrid.Children.Add(closeButton);

                // Make title bar draggable
                titleBorder.MouseLeftButtonDown += (s, e) => DragMove();

                titleBorder.Child = titleGrid;
                return titleBorder;
            }

            /// <summary>
            /// Creates the close button for the title bar
            /// </summary>
            private Button CreateCloseButton()
            {
                var closeButton = new Button
                {
                    Width = 32,
                    Height = 32,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Content = CreateCloseSvgIcon()
                };

                closeButton.Click += (s, e) =>
                {
                    _result = MessageBoxResult.Cancel;
                    ApplyExitAnimation();
                };

                // Hover effect
                closeButton.MouseEnter += (s, e) =>
                {
                    closeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_theme.BackgroundPrimary));
                };
                closeButton.MouseLeave += (s, e) =>
                {
                    closeButton.Background = Brushes.Transparent;
                };

                return closeButton;
            }

            /// <summary>
            /// Creates SVG icon for close button
            /// </summary>
            private Viewbox CreateCloseSvgIcon()
            {
                var path = new Path
                {
                    Data = Geometry.Parse("M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_theme.ForegroundPrimary)),
                    Stretch = Stretch.Uniform
                };

                return new Viewbox
                {
                    Width = 14,
                    Height = 14,
                    Child = path
                };
            }

            /// <summary>
            /// Creates the message content area with icon and text
            /// </summary>
            private Grid CreateMessageArea(string message, MessageBoxIcon icon)
            {
                var messageGrid = new Grid
                {
                    Margin = new Thickness(20, 25, 20, 25)
                };

                messageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                messageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                // Icon
                if (icon != MessageBoxIcon.None)
                {
                    var iconViewbox = CreateMessageIcon(icon);
                    iconViewbox.Margin = new Thickness(0, 0, 20, 0);
                    Grid.SetColumn(iconViewbox, 0);
                    messageGrid.Children.Add(iconViewbox);

                    // Apply appropriate animation based on icon type
                    var iconPath = (iconViewbox.Child as Path);
                    if (icon == MessageBoxIcon.Warning)
                    {
                        ApplyWarningFlashAnimation(iconViewbox, iconPath);
                    }
                    else if (icon == MessageBoxIcon.Error)
                    {
                        ApplyErrorFlashAnimation(iconViewbox, iconPath);
                    }
                    else
                    {
                        ApplySmoothPulseAnimation(iconViewbox);
                    }
                }

                // Message text
                var messageText = new TextBlock
                {
                    Text = message,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_theme.ForegroundSecundary)),
                    FontSize = 14,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Center,
                    LineHeight = 22
                };
                Grid.SetColumn(messageText, icon == MessageBoxIcon.None ? 0 : 1);
                Grid.SetColumnSpan(messageText, icon == MessageBoxIcon.None ? 2 : 1);
                messageGrid.Children.Add(messageText);

                return messageGrid;
            }

            /// <summary>
            /// Creates SVG icon based on message type
            /// </summary>
            private Viewbox CreateMessageIcon(MessageBoxIcon icon)
            {
                Path iconPath = new Path
                {
                    Stretch = Stretch.Uniform,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_theme.ForegroundPrimary))
                };

                switch (icon)
                {
                    case MessageBoxIcon.Information:
                        iconPath.Data = Geometry.Parse("M256 0c70.69 0 134.69 28.66 181.02 74.98C483.34 121.3 512 185.31 512 256c0 70.69-28.66 134.7-74.98 181.02C390.69 483.34 326.69 512 256 512c-70.69 0-134.69-28.66-181.02-74.98C28.66 390.69 0 326.69 0 256c0-70.69 28.66-134.69 74.98-181.02C121.31 28.66 185.31 0 256 0zm-9.96 161.03c0-4.28.76-8.26 2.27-11.91 1.5-3.63 3.77-6.94 6.79-9.91 3-2.95 6.29-5.2 9.84-6.7 3.57-1.5 7.41-2.28 11.52-2.28 4.12 0 7.96.78 11.49 2.27 3.54 1.51 6.78 3.76 9.75 6.73 2.95 2.97 5.16 6.26 6.64 9.91 1.49 3.63 2.22 7.61 2.22 11.89 0 4.17-.73 8.08-2.21 11.69-1.48 3.6-3.68 6.94-6.65 9.97-2.94 3.03-6.18 5.32-9.72 6.84-3.54 1.51-7.38 2.29-11.52 2.29-4.22 0-8.14-.76-11.75-2.26-3.58-1.51-6.86-3.79-9.83-6.79-2.94-3.02-5.16-6.34-6.63-9.97-1.48-3.62-2.21-7.54-2.21-11.77zm13.4 178.16c-1.11 3.97-3.35 11.76 3.3 11.76 1.44 0 3.27-.81 5.46-2.4 2.37-1.71 5.09-4.31 8.13-7.75 3.09-3.5 6.32-7.65 9.67-12.42 3.33-4.76 6.84-10.22 10.49-16.31.37-.65 1.23-.87 1.89-.48l12.36 9.18c.6.43.73 1.25.35 1.86-5.69 9.88-11.44 18.51-17.26 25.88-5.85 7.41-11.79 13.57-17.8 18.43l-.1.06c-6.02 4.88-12.19 8.55-18.51 11.01-17.58 6.81-45.36 5.7-53.32-14.83-5.02-12.96-.9-27.69 3.06-40.37l19.96-60.44c1.28-4.58 2.89-9.62 3.47-14.33.97-7.87-2.49-12.96-11.06-12.96h-17.45c-.76 0-1.38-.62-1.38-1.38l.08-.48 4.58-16.68c.16-.62.73-1.04 1.35-1.02l89.12-2.79c.76-.03 1.41.57 1.44 1.33l-.07.43-37.76 124.7zm158.3-244.93c-41.39-41.39-98.58-67-161.74-67-63.16 0-120.35 25.61-161.74 67-41.39 41.39-67 98.58-67 161.74 0 63.16 25.61 120.35 67 161.74 41.39 41.39 98.58 67 161.74 67 63.16 0 120.35-25.61 161.74-67 41.39-41.39 67-98.58 67-161.74 0-63.16-25.61-120.35-67-161.74z");
                        break;

                    case MessageBoxIcon.Warning:
                        iconPath.Data = Geometry.Parse("M289.639 9.137c12.411 7.25 23.763 18.883 33.037 34.913l.97 1.813 1.118 1.941 174.174 302.48c33.712 56.407-1.203 113.774-66.174 112.973v.12H73.485c-.895 0-1.78-.04-2.657-.112-59.104-.799-86.277-54.995-61.909-106.852.842-1.805 1.816-3.475 2.816-5.201L189.482 43.959l-.053-.032c9.22-15.786 20.717-27.457 33.411-34.805C243.788-3 268.711-3.086 289.639 9.137zM255.7 339.203c13.04 0 23.612 10.571 23.612 23.612 0 13.041-10.572 23.613-23.612 23.613-13.041 0-23.613-10.572-23.613-23.613s10.572-23.612 23.613-23.612zm17.639-35.379c-.794 19.906-34.506 19.931-35.278-.006-3.41-34.108-12.129-111.541-11.853-143.591.284-9.874 8.469-15.724 18.939-17.955 3.231-.686 6.781-1.024 10.357-1.019 3.595.008 7.153.362 10.387 1.051 10.818 2.303 19.309 8.392 19.309 18.446l-.043 1.005-11.818 142.069zM37.596 369.821L216.864 59.942c21.738-37.211 56.225-38.289 78.376 0l176.298 306.166c17.177 28.285 10.04 66.236-38.774 65.488H73.485c-33.017.756-52.841-25.695-35.889-61.775z");
                        break;

                    case MessageBoxIcon.Error:
                        iconPath.Data = Geometry.Parse("M61.44,0c16.966,0,32.326,6.877,43.445,17.996c11.119,11.118,17.996,26.479,17.996,43.444 c0,16.967-6.877,32.326-17.996,43.444C93.766,116.003,78.406,122.88,61.44,122.88c-16.966,0-32.326-6.877-43.444-17.996 C6.877,93.766,0,78.406,0,61.439c0-16.965,6.877-32.326,17.996-43.444C29.114,6.877,44.474,0,61.44,0L61.44,0z M80.16,37.369 c1.301-1.302,3.412-1.302,4.713,0c1.301,1.301,1.301,3.411,0,4.713L65.512,61.444l19.361,19.362c1.301,1.301,1.301,3.411,0,4.713 c-1.301,1.301-3.412,1.301-4.713,0L60.798,66.157L41.436,85.52c-1.301,1.301-3.412,1.301-4.713,0c-1.301-1.302-1.301-3.412,0-4.713 l19.363-19.362L36.723,42.082c-1.301-1.302-1.301-3.412,0-4.713c1.301-1.302,3.412-1.302,4.713,0l19.363,19.362L80.16,37.369 L80.16,37.369z M100.172,22.708C90.26,12.796,76.566,6.666,61.44,6.666c-15.126,0-28.819,6.13-38.731,16.042 C12.797,32.62,6.666,46.314,6.666,61.439c0,15.126,6.131,28.82,16.042,38.732c9.912,9.911,23.605,16.042,38.731,16.042 c15.126,0,28.82-6.131,38.732-16.042c9.912-9.912,16.043-23.606,16.043-38.732C116.215,46.314,110.084,32.62,100.172,22.708 L100.172,22.708z");
                        break;

                    case MessageBoxIcon.Question:
                        iconPath.Data = Geometry.Parse("M256 0c70.69 0 134.7 28.66 181.02 74.98C483.34 121.31 512 185.31 512 256c0 70.69-28.66 134.7-74.98 181.02C390.7 483.34 326.69 512 256 512c-70.69 0-134.69-28.66-181.02-74.98C28.66 390.7 0 326.69 0 256c0-70.69 28.66-134.69 74.98-181.02C121.31 28.66 185.31 0 256 0zm-21.49 301.51v-2.03c.16-13.46 1.48-24.12 4.07-32.05 2.54-7.92 6.19-14.37 10.97-19.25 4.77-4.92 10.51-9.39 17.22-13.46 4.31-2.74 8.22-5.78 11.68-9.18 3.45-3.36 6.19-7.27 8.23-11.69 2.02-4.37 3.04-9.24 3.04-14.62 0-6.4-1.52-11.94-4.57-16.66-3-4.68-7.06-8.28-12.04-10.87-5.03-2.54-10.61-3.81-16.76-3.81-5.53 0-10.81 1.11-15.89 3.45-5.03 2.29-9.25 5.89-12.55 10.77-3.3 4.87-5.23 11.12-5.74 18.74h-32.91c.51-12.95 3.81-23.92 9.85-32.91 6.1-8.99 14.13-15.8 24.08-20.42 10.01-4.62 21.08-6.9 33.16-6.9 13.31 0 24.89 2.43 34.84 7.41 9.96 4.93 17.73 11.83 23.27 20.67 5.48 8.84 8.28 19.1 8.28 30.88 0 8.08-1.27 15.34-3.81 21.79-2.54 6.45-6.1 12.24-10.77 17.27-4.68 5.08-10.21 9.54-16.71 13.41-6.15 3.86-11.12 7.82-14.88 11.93-3.81 4.11-6.56 8.99-8.28 14.58-1.73 5.63-2.69 12.59-2.84 20.92v2.03h-30.94zm16.36 65.82c-5.94-.04-11.02-2.13-15.29-6.35-4.26-4.21-6.35-9.34-6.35-15.33 0-5.89 2.09-10.97 6.35-15.19 4.27-4.21 9.35-6.35 15.29-6.35 5.84 0 10.92 2.14 15.18 6.35 4.32 4.22 6.45 9.3 6.45 15.19 0 3.96-1.01 7.62-2.99 10.87-1.98 3.3-4.57 5.94-7.82 7.87-3.25 1.93-6.86 2.9-10.82 2.94zM417.71 94.29C376.33 52.92 319.15 27.32 256 27.32c-63.15 0-120.32 25.6-161.71 66.97C52.92 135.68 27.32 192.85 27.32 256c0 63.15 25.6 120.33 66.97 161.71 41.39 41.37 98.56 66.97 161.71 66.97 63.15 0 120.33-25.6 161.71-66.97 41.37-41.38 66.97-98.56 66.97-161.71 0-63.15-25.6-120.32-66.97-161.71z");
                        break;

                    case MessageBoxIcon.Success:
                        iconPath.Data = Geometry.Parse("M61.44,0c16.966,0,32.326,6.877,43.445,17.995s17.996,26.479,17.996,43.444c0,16.967-6.877,32.327-17.996,43.445 S78.406,122.88,61.44,122.88c-16.966,0-32.326-6.877-43.444-17.995S0,78.406,0,61.439c0-16.965,6.877-32.326,17.996-43.444 S44.474,0,61.44,0L61.44,0z M34.556,67.179c-1.313-1.188-1.415-3.216-0.226-4.529c1.188-1.313,3.216-1.415,4.529-0.227L52.3,74.611 l31.543-33.036c1.223-1.286,3.258-1.336,4.543-0.114c1.285,1.223,1.336,3.257,0.113,4.542L54.793,81.305l-0.004-0.004 c-1.195,1.257-3.182,1.338-4.475,0.168L34.556,67.179L34.556,67.179z M100.33,22.55C90.377,12.598,76.627,6.441,61.44,6.441 c-15.188,0-28.938,6.156-38.89,16.108c-9.953,9.953-16.108,23.702-16.108,38.89c0,15.188,6.156,28.938,16.108,38.891 c9.952,9.952,23.702,16.108,38.89,16.108c15.187,0,28.937-6.156,38.89-16.108c9.953-9.953,16.107-23.702,16.107-38.891 C116.438,46.252,110.283,32.502,100.33,22.55L100.33,22.55z");
                        break;
                }

                return new Viewbox
                {
                    Width = 48,
                    Height = 48,
                    Child = iconPath
                };
            }

            /// <summary>
            /// Creates the button panel with appropriate buttons
            /// </summary>
            private StackPanel CreateButtonPanel(MessageBoxButton buttons)
            {
                var panel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(20, 0, 20, 20)
                };

                switch (buttons)
                {
                    case MessageBox.MessageBoxButton.OK:
                        panel.Children.Add(CreateButton("OK", MessageBoxResult.OK, true));
                        break;

                    case MessageBox.MessageBoxButton.OKCancel:
                        panel.Children.Add(CreateButton("Cancel", MessageBoxResult.Cancel, false));
                        panel.Children.Add(CreateButton("OK", MessageBoxResult.OK, true));
                        break;

                    case MessageBoxButton.YesNo:
                        panel.Children.Add(CreateButton("No", MessageBoxResult.No, false));
                        panel.Children.Add(CreateButton("Yes", MessageBoxResult.Yes, true));
                        break;

                    case MessageBoxButton.YesNoCancel:
                        panel.Children.Add(CreateButton("Cancel", MessageBoxResult.Cancel, false));
                        panel.Children.Add(CreateButton("No", MessageBoxResult.No, false));
                        panel.Children.Add(CreateButton("Yes", MessageBoxResult.Yes, true));
                        break;
                }

                return panel;
            }

            /// <summary>
            /// Creates a styled button with hover effects
            /// </summary>
            private Button CreateButton(string text, MessageBoxResult result, bool isPrimary)
            {
                var button = new Button
                {
                    Content = text,
                    Width = 90,
                    Height = 36,
                    Margin = new Thickness(8, 0, 0, 0),
                    FontSize = 13,
                    FontWeight = FontWeights.Medium,
                    Cursor = Cursors.Hand,
                    BorderThickness = new Thickness(1)
                };

                if (isPrimary)
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_theme.BackgroundSecundary));
                    button.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_theme.ForegroundPrimary));
                    button.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_theme.ForegroundPrimary));
                }
                else
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_theme.BackgroundLightPart));
                    button.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_theme.ForegroundSecundary));
                    button.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_theme.ForegroundStatus));
                }

                // Click handler
                button.Click += (s, e) =>
                {
                    _result = result;
                    ApplyExitAnimation();
                };

                // Hover effects
                var originalBackground = button.Background.Clone();
                var hoverColor = (Color)ColorConverter.ConvertFromString(_theme.BackgroundPrimary);
                hoverColor.A = 30;

                button.MouseEnter += (s, e) =>
                {
                var animation = new ColorAnimation
                {
                    To = hoverColor,
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                    button.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);

                    // Scale effect
                    var scaleTransform = new ScaleTransform(1, 1);
                    button.RenderTransform = scaleTransform;
                    button.RenderTransformOrigin = new Point(0.5, 0.5);

                    var scaleAnimation = new DoubleAnimation
                    {
                        To = 1.05,
                        Duration = TimeSpan.FromMilliseconds(200),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
                };

                button.MouseLeave += (s, e) =>
                {
                    button.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                    button.Background = originalBackground.Clone();

                    if (button.RenderTransform is ScaleTransform scaleTransform)
                    {
                        var scaleAnimation = new DoubleAnimation
                        {
                            To = 1,
                            Duration = TimeSpan.FromMilliseconds(200),
                            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                        };
                        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
                    }
                };

                return button;
            }

            /// <summary>
            /// Applies entrance animation to the message box
            /// </summary>
            private void ApplyEntryAnimation()
            {
                // Opacity animation
                Opacity = 0;
                var opacityAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                // Scale animation
                var scaleTransform = new ScaleTransform(0.8, 0.8);
                _mainGrid.RenderTransform = scaleTransform;
                _mainGrid.RenderTransformOrigin = new Point(0.5, 0.5);

                var scaleAnimation = new DoubleAnimation
                {
                    From = 0.8,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(400),
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
                };

                // Apply animations
                BeginAnimation(OpacityProperty, opacityAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            }

            /// <summary>
            /// Applies exit animation before closing
            /// </summary>
            private void ApplyExitAnimation()
            {
                // Opacity animation
                var opacityAnimation = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };

                // Scale animation
                var scaleTransform = _mainGrid.RenderTransform as ScaleTransform ?? new ScaleTransform(1, 1);
                _mainGrid.RenderTransform = scaleTransform;
                _mainGrid.RenderTransformOrigin = new Point(0.5, 0.5);

                var scaleAnimation = new DoubleAnimation
                {
                    From = 1,
                    To = 0.8,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };

                opacityAnimation.Completed += (s, e) => Close();

                // Apply animations
                BeginAnimation(OpacityProperty, opacityAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            }

            /// <summary>
            /// Applies smooth continuous pulse animation to an element with high-quality rendering
            /// </summary>
            private void ApplySmoothPulseAnimation(UIElement element)
            {
                // Enable high-quality rendering for smooth animations
                RenderOptions.SetEdgeMode(element, EdgeMode.Aliased);
                RenderOptions.SetBitmapScalingMode(element, BitmapScalingMode.HighQuality);

                var scaleTransform = new ScaleTransform(1, 1);
                element.RenderTransform = scaleTransform;
                element.RenderTransformOrigin = new Point(0.5, 0.5);

                var pulseAnimation = new DoubleAnimation
                {
                    From = 1,
                    To = 1.15,
                    Duration = TimeSpan.FromMilliseconds(1200),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };

                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, pulseAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, pulseAnimation);
            }

            /// <summary>
            /// Applies warning flash animation simulating an electrical short circuit with blinking light effect
            /// </summary>
            private void ApplyWarningFlashAnimation(UIElement element, Path iconPath)
            {
                // Enable high-quality rendering
                RenderOptions.SetEdgeMode(element, EdgeMode.Aliased);
                RenderOptions.SetBitmapScalingMode(element, BitmapScalingMode.HighQuality);

                // Scale animation - simulating electrical pulse
                var scaleTransform = new ScaleTransform(1, 1);
                element.RenderTransform = scaleTransform;
                element.RenderTransformOrigin = new Point(0.5, 0.5);

                var scaleAnimation = new DoubleAnimation
                {
                    From = 1,
                    To = 1.15,
                    Duration = TimeSpan.FromMilliseconds(600),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };

                // Flashing opacity animation - simulating short circuit light blinking
                var opacityAnimation = new DoubleAnimation
                {
                    From = 1,
                    To = 0.25,
                    Duration = TimeSpan.FromMilliseconds(600),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };

                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
                iconPath.BeginAnimation(OpacityProperty, opacityAnimation);
            }

            /// <summary>
            /// Applies error flash animation with rapid blinking simulating critical electrical fault
            /// </summary>
            private void ApplyErrorFlashAnimation(UIElement element, Path iconPath)
            {
                // Enable high-quality rendering
                RenderOptions.SetEdgeMode(element, EdgeMode.Aliased);
                RenderOptions.SetBitmapScalingMode(element, BitmapScalingMode.HighQuality);

                // Scale animation - more aggressive for critical errors
                var scaleTransform = new ScaleTransform(1, 1);
                element.RenderTransform = scaleTransform;
                element.RenderTransformOrigin = new Point(0.5, 0.5);

                var scaleAnimation = new DoubleAnimation
                {
                    From = 1,
                    To = 1.2,
                    Duration = TimeSpan.FromMilliseconds(450),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };

                // Rapid flashing opacity - simulating critical alert light
                var opacityAnimation = new DoubleAnimation
                {
                    From = 1,
                    To = 0.15,
                    Duration = TimeSpan.FromMilliseconds(450),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };

                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
                iconPath.BeginAnimation(OpacityProperty, opacityAnimation);
            }

            /// <summary>
            /// Handles ESC key to close dialog
            /// </summary>
            protected override void OnKeyDown(KeyEventArgs e)
            {
                base.OnKeyDown(e);
                if (e.Key == Key.Escape)
                {
                    _result = MessageBoxResult.Cancel;
                    ApplyExitAnimation();
                }
            }
        }

        #endregion
    }
}