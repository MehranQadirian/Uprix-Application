using System.Windows.Media;

namespace AppLauncher.Classes.MainClasses
{
    public class AppModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ImageSource Icon { get; set; }
        public bool IsFavorite { get; set; }
        public long Rate { get; set; }
        public bool Favorite { get; set; }
    }
}

