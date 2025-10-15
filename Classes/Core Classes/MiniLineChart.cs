using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace AppLauncher.Classes.Core_Classes
{
    public class MiniLineChart : Canvas
    {
        private Polyline _polyline;
        private Rectangle _bg;

        public MiniLineChart()
        {
            Height = 60;
            _bg = new Rectangle { RadiusX = 6, RadiusY = 6, Fill = new SolidColorBrush(Color.FromRgb(10, 26, 30)) };
            Children.Add(_bg);
            _polyline = new Polyline
            {
                StrokeThickness = 2,
                Stroke = new SolidColorBrush(Color.FromRgb(79, 209, 197)),
                StrokeLineJoin = PenLineJoin.Round
            };
            Children.Add(_polyline);
            SizeChanged += (s, e) => { _bg.Width = ActualWidth; _bg.Height = ActualHeight; };
        }

        public void Draw(List<double> points)
        {
            _polyline.Points.Clear();
            if (points == null || points.Count == 0) return;
            double w = ActualWidth <= 0 ? 200 : ActualWidth;
            double h = ActualHeight <= 0 ? 60 : ActualHeight;
            int n = points.Count;
            double max = System.Math.Max(10, points.Max());
            double min = System.Math.Min(0, points.Min());
            double range = System.Math.Max(1, max - min);

            for (int i = 0; i < n; i++)
            {
                double x = (double)i / System.Math.Max(1, n - 1) * w;
                double normalized = (points[i] - min) / range;
                double y = h - (normalized * h);
                _polyline.Points.Add(new Point(x, y));
            }
        }
    }
}
    
