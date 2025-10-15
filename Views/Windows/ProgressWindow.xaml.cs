using System.Windows;
using System.Windows.Input;

namespace AppLauncher.Views.Windows
{
    public partial class ProgressWindow : Window
    {
        #region Methods
        public ProgressWindow()
        {
            InitializeComponent();
        }

        private void DragThumb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        public void UpdateStatus(string message)
        {
            StatusText.Text = message;
        }
        #endregion
    }
}
