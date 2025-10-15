namespace AppLauncher.Classes
{
    public class GmailViewTheme
    {
        public string Background { get; set; }
        public string BackgroundSecundary { get; set; }
        public string BackgroundTitle { get; set; }
        public string ForegroundTitel { get; set; }
        public string ButtonHover { get; set; }
        public string ButtonNormal { get; set; }
        public string ButtonForegroundNormal { get; set; }
        public string ButtonForegroundHover { get; set; }
        public string LabelNormal { get; set; }
        public string LabelFocus { get; set; }
        private ApplyThemes apply = new ApplyThemes();
        public void SetThemes()
        {
            apply.ApplyThm();
            Background = apply.BackgroundLightPart;
            BackgroundSecundary = apply.BackgroundPrimary;

            BackgroundTitle = apply.BackgroundSecundary;
            ForegroundTitel = apply.ForegroundSecundary;

            ButtonNormal = apply.BackgroundPrimary;
            ButtonForegroundNormal = apply.MenuIconColor;

            ButtonHover = apply.BackgroundSecundary;
            ButtonForegroundNormal = apply.MenuIconColor;

            LabelNormal = apply.BackgroundSecundary;
            LabelFocus = apply.BackgroundPrimary;
        }
    }
}
