using AppLauncher.Classes;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace AppLauncher.Views.Controls
{
    public partial class LazyLoadingOverlay : UserControl
    {
        private Color primaryColor;
        private Color secondaryColor;
        private Color accentColor;
        private Color textColors;
        private int totalSteps = 0;
        private int currentStep = 0;
        private ApplyThemes apply;
        public LazyLoadingOverlay(ApplyThemes aply,
                                  string backgroundColor = "#F5F5F5",
                                  string skeletonColor = "#E0E0E0",
                                  string shimmerColor = "#F0F0F0",
                                  string textColor = "#FFFFFF")
        {
            InitializeComponent();
            apply = aply;
            primaryColor = (Color)ColorConverter.ConvertFromString(backgroundColor);
            secondaryColor = (Color)ColorConverter.ConvertFromString(skeletonColor);
            accentColor = (Color)ColorConverter.ConvertFromString(shimmerColor);
            textColors = (Color)ColorConverter.ConvertFromString(textColor);
            Loaded += LazyLoadingOverlay_Loaded;
        }

        private void LazyLoadingOverlay_Loaded(object sender, RoutedEventArgs e)
        {
            SetTheme();
            ApplyThemeColors();
            StartShimmerAnimation();
            StartDotAnimation();
        }

        private void ApplyThemeColors()
        {
            if (FindName("Backdrop") is Rectangle backdrop)
            {
                backdrop.Fill = new SolidColorBrush(primaryColor);
            }
            //StatusText.Foreground = new SolidColorBrush(textColors);
            //Dot1.Fill = new SolidColorBrush(textColors);
            //Dot2.Fill = new SolidColorBrush(textColors);
            //Dot3.Fill = new SolidColorBrush(textColors);
            UpdateAllGradients();
        }
        private void SetTheme()
        {
            rectangle1.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            rectangle2.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            rectangle3.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            rectangle4.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            rectangle5.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            rectangle6.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            rectangle7.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            rectangle8.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            rectangle9.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            rectangle10.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            rectangle11.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            rectangle12.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            rectangle13.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            rectangle14.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            rectangle15.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            rectangle16.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            rectangle17.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));

            ProgressBar.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));

            ellips1.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            ellips2.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            ellips3.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            ellips4.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            ellips5.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            ellips6.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            ellips7.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            ellips8.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));

            Dot1.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            Dot2.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
            Dot3.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));

            brCard1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSecundary));
            brCard2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSecundary));
            brCard3.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSecundary));
            brCard4.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSecundary));
            brCard5.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSecundary));
            brCard6.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSecundary));
            brCard7.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSecundary));
            brCard8.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSecundary));

            brSearch.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSecundary));

            brProgressBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.BackgroundSecundary));

            StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(apply.MenuIconColor));
        }
        private void UpdateAllGradients()
        {
            var brushes = FindVisualChildren<LinearGradientBrush>(this);

            foreach (var brush in brushes)
            {
                if (brush.GradientStops.Count >= 5)
                {
                    brush.GradientStops[0].Color = secondaryColor;
                    brush.GradientStops[1].Color = accentColor;
                    brush.GradientStops[2].Color = Color.FromRgb(
                        (byte)((accentColor.R + 255) / 2),
                        (byte)((accentColor.G + 255) / 2),
                        (byte)((accentColor.B + 255) / 2)
                    );
                    brush.GradientStops[3].Color = accentColor;
                    brush.GradientStops[4].Color = secondaryColor;
                }
            }
        }

        private void StartShimmerAnimation()
        {
            var brushes = FindVisualChildren<LinearGradientBrush>(this).ToList();

            for (int i = 0; i < brushes.Count; i++)
            {
                var brush = brushes[i];
                if (brush.Transform is TranslateTransform transform)
                {
                    var animation = new DoubleAnimation
                    {
                        From = -400,
                        To = 1200,
                        Duration = TimeSpan.FromSeconds(1.8),
                        RepeatBehavior = RepeatBehavior.Forever,
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut },
                        BeginTime = TimeSpan.FromMilliseconds(i * 35)
                    };

                    transform.BeginAnimation(TranslateTransform.XProperty, animation);
                }
            }
        }

        private void StartDotAnimation()
        {
            // Animate loading dots in sequence
            if (FindName("Dot1") is Ellipse dot1 &&
                FindName("Dot2") is Ellipse dot2 &&
                FindName("Dot3") is Ellipse dot3)
            {
                AnimateDot(dot1, TimeSpan.FromMilliseconds(0));
                AnimateDot(dot2, TimeSpan.FromMilliseconds(200));
                AnimateDot(dot3, TimeSpan.FromMilliseconds(400));
            }
        }

        private void AnimateDot(Ellipse dot, TimeSpan delay)
        {
            var animation = new DoubleAnimation
            {
                From = 0.3,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.6),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = delay,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            dot.BeginAnimation(OpacityProperty, animation);

            var scaleAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 1.3,
                Duration = TimeSpan.FromSeconds(0.6),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = delay,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            var scaleTransform = new ScaleTransform(1, 1, 4, 4);
            dot.RenderTransform = scaleTransform;
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }

        public void SetTotalSteps(int total)
        {
            totalSteps = total;
            currentStep = 0;
        }

        public void UpdateStatus(string status)
        {
            Dispatcher.Invoke(() =>
            {
                if (FindName("StatusText") is TextBlock statusText)
                {
                    statusText.Text = status;

                    // Pulse animation on status change
                    var pulse = new DoubleAnimation
                    {
                        From = 0.6,
                        To = 1.0,
                        Duration = TimeSpan.FromSeconds(0.3),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };
                    statusText.BeginAnimation(OpacityProperty, pulse);
                }

                // Update progress bar
                currentStep++;
                UpdateProgressBar();
            });
        }

        private void UpdateProgressBar()
        {
            if (FindName("ProgressBar") is Rectangle progressBar && totalSteps > 0)
            {
                double targetWidth = (currentStep / (double)totalSteps) * 200;

                var widthAnimation = new DoubleAnimation
                {
                    To = targetWidth,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                progressBar.BeginAnimation(WidthProperty, widthAnimation);
            }
        }

        private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                    if (child is T t)
                    {
                        yield return t;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public async System.Threading.Tasks.Task HideWithAnimation()
        {
            // Complete progress bar
            if (FindName("ProgressBar") is Rectangle progressBar)
            {
                var completeAnimation = new DoubleAnimation
                {
                    To = 200,
                    Duration = TimeSpan.FromSeconds(0.2)
                };
                progressBar.BeginAnimation(WidthProperty, completeAnimation);
            }

            await System.Threading.Tasks.Task.Delay(150);

            // Stop all animations
            StopAllAnimations();

            // Create fade and scale out effect
            var scaleTransform = new ScaleTransform(1, 1, ActualWidth / 2, ActualHeight / 2);
            var translateTransform = new TranslateTransform(0, 0);
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(translateTransform);
            RenderTransform = transformGroup;

            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            var scaleOut = new DoubleAnimation
            {
                From = 1,
                To = 0.92,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            var slideUp = new DoubleAnimation
            {
                From = 0,
                To = -30,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            fadeOut.Completed += (s, e) => tcs.SetResult(true);

            BeginAnimation(OpacityProperty, fadeOut);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleOut);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleOut);
            translateTransform.BeginAnimation(TranslateTransform.YProperty, slideUp);

            await tcs.Task;

            Visibility = Visibility.Collapsed;
        }

        private void StopAllAnimations()
        {
            // Stop shimmer animations
            var brushes = FindVisualChildren<LinearGradientBrush>(this);
            foreach (var brush in brushes)
            {
                if (brush.Transform is TranslateTransform transform)
                {
                    transform.BeginAnimation(TranslateTransform.XProperty, null);
                }
            }

            // Stop dot animations
            if (FindName("Dot1") is Ellipse dot1) StopDotAnimation(dot1);
            if (FindName("Dot2") is Ellipse dot2) StopDotAnimation(dot2);
            if (FindName("Dot3") is Ellipse dot3) StopDotAnimation(dot3);
        }

        private void StopDotAnimation(Ellipse dot)
        {
            dot.BeginAnimation(OpacityProperty, null);
            if (dot.RenderTransform is ScaleTransform st)
            {
                st.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                st.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            }
        }
    }
}