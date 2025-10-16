using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AppLauncher.Classes;
using AppLauncher.Classes.Core_Classes;
using AppLauncher.Configuration;
using AppLauncher.Models;
using MessageBox = AppLauncher.Classes.MessageBox;

namespace AppLauncher.Views.Pages
{
    public partial class ExplorePage : Page
    {
        private ApplyThemes apply;
        private MainWindow mainWindow;

        public ExplorePage(MainWindow window, ApplyThemes themes)
        {
            InitializeComponent();
            mainWindow = window;
            apply = themes;
            ApplyTheme();
            LoadFeatures();
        }

        private void ApplyTheme()
        {
            this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundPrimary));
            PageTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            PageSubtitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
        }

        /// <summary>
        /// Loads all features from configuration and creates cards
        /// </summary>
        private void LoadFeatures()
        {
            ExploreCardsPanel.Children.Clear();

            // Get all enabled features from configuration
            var features = ExploreFeatures.GetEnabledFeatures();

            // Option 1: Display all features in one list
            foreach (var feature in features)
            {
                CreateFeatureCard(feature);
            }

            // Option 2: Display features grouped by category (uncomment to use)
            // LoadFeaturesGroupedByCategory();
        }

        private void CreateFeatureCard(ExploreFeature feature)
        {
            var card = new Border
            {
                Width = 280,
                Height = 140,
                Margin = new Thickness(15),
                CornerRadius = new CornerRadius(12),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSecundary)),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor)) { Opacity = 0.3 },
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                Tag = feature,
                Opacity = feature.IsEnabled ? 1.0 : 0.5
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Icon
            var iconPath = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse(feature.IconPath),
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor)),
                Stretch = Stretch.Uniform,
                Width = 50,
                Height = 50,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 15, 0, 0)
            };
            Grid.SetRow(iconPath, 0);

            // Badge (if present)
            if (!string.IsNullOrEmpty(feature.Badge))
            {
                var badge = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(255, 152, 0)),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(8, 2, 8, 2),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 10, 10, 0)
                };
                Grid.SetRow(badge, 0);

                var badgeText = new TextBlock
                {
                    Text = feature.Badge,
                    Foreground = Brushes.White,
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    FontFamily = new FontFamily("Nunito")
                };
                badge.Child = badgeText;
                grid.Children.Add(badge);
            }

            // Text content
            var textStack = new StackPanel
            {
                Margin = new Thickness(15, 0, 15, 15),
                VerticalAlignment = VerticalAlignment.Bottom
            };
            Grid.SetRow(textStack, 1);

            var titleText = new TextBlock
            {
                Text = feature.Title,
                FontFamily = new FontFamily("Nunito"),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor)),
                TextAlignment = TextAlignment.Center
            };

            var descText = new TextBlock
            {
                Text = feature.Description,
                FontFamily = new FontFamily("Nunito"),
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor)) { Opacity = 0.7 },
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5, 0, 0)
            };

            textStack.Children.Add(titleText);
            textStack.Children.Add(descText);

            grid.Children.Add(iconPath);
            grid.Children.Add(textStack);
            card.Child = grid;

            // Event handlers (only if enabled)
            if (feature.IsEnabled)
            {
                card.MouseEnter += Card_MouseEnter;
                card.MouseLeave += Card_MouseLeave;
                card.MouseLeftButtonDown += Card_MouseLeftButtonDown;
            }

            ExploreCardsPanel.Children.Add(card);
        }

        private void Card_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border card && card.Tag is ExploreFeature feature && feature.IsEnabled)
            {
                var scaleTransform = new ScaleTransform(1, 1);
                card.RenderTransform = scaleTransform;
                card.RenderTransformOrigin = new Point(0.5, 0.5);

                var animation = new DoubleAnimation(1, 1.05, TimeSpan.FromMilliseconds(200))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);

                card.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor)) { Opacity = 0.8 };
            }
        }

        private void Card_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border card)
            {
                var scaleTransform = card.RenderTransform as ScaleTransform;
                if (scaleTransform != null)
                {
                    var animation = new DoubleAnimation(1.05, 1, TimeSpan.FromMilliseconds(200))
                    {
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };

                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
                }

                card.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor)) { Opacity = 0.3 };
            }
        }

        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is Border card && card.Tag is ExploreFeature feature)
            {
                HandleFeatureNavigation(feature);
            }
        }

        /// <summary>
        /// Handles navigation based on feature configuration
        /// </summary>
        private void HandleFeatureNavigation(ExploreFeature feature)
        {
            if (!feature.IsEnabled)
            {
                MessageBox.Show(
                    $"{feature.Title} is not available yet.",
                    "Feature Unavailable",
                    MessageBox.MessageBoxButton.OK,
                    MessageBox.MessageBoxIcon.Information
                );
                return;
            }

            switch (feature.NavigationType)
            {
                case FeatureNavigationType.Page:
                    // Navigate to a page in the main content area
                    mainWindow.NavigateToView(feature.Id);
                    break;

                case FeatureNavigationType.LauncherView:
                    // Navigate to launcher view (apps display)
                    mainWindow.NavigateToView(feature.Id);
                    break;

                case FeatureNavigationType.ExternalUrl:
                    // Open external URL (implement based on your needs)
                    MessageBox.Show(
                        $"Would open external URL for: {feature.Title}",
                        "External Link",
                        MessageBox.MessageBoxButton.OK,
                        MessageBox.MessageBoxIcon.Information
                    );
                    // Example: System.Diagnostics.Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    break;

                case FeatureNavigationType.CustomAction:
                    // Handle custom actions
                    HandleCustomAction(feature);
                    break;

                default:
                    mainWindow.NavigateToView(feature.Id);
                    break;
            }
        }

        /// <summary>
        /// Handles custom actions for special features
        /// Extend this method to add custom behaviors
        /// </summary>
        private void HandleCustomAction(ExploreFeature feature)
        {
            // Add custom action handlers here
            switch (feature.Id)
            {
                // Example custom actions:
                /*
                case "QuickLaunch":
                    // Launch most used app
                    break;
                
                case "SystemInfo":
                    // Show system information dialog
                    break;
                
                case "ClearCache":
                    // Clear application cache
                    break;
                */

                default:
                    MessageBox.Show(
                        $"Custom action not implemented for: {feature.Title}",
                        "Custom Action",
                        MessageBox.MessageBoxButton.OK,
                        MessageBox.MessageBoxIcon.Information
                    );
                    break;
            }
        }
    }
}