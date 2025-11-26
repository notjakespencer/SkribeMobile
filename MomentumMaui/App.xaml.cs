namespace MomentumMaui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            
            // Set default theme to Dark
            Application.Current?.UserAppTheme = AppTheme.Dark;
            
            MainPage = new AppShell();
        }

        public void SetTheme(AppTheme theme)
        {
            Application.Current?.UserAppTheme = theme;
        }

        public AppTheme GetCurrentTheme()
        {
            return Application.Current?.UserAppTheme ?? AppTheme.Unspecified;
        }

        public void ToggleTheme()
        {
            var currentTheme = GetCurrentTheme();
            if (currentTheme == AppTheme.Dark)
            {
                SetTheme(AppTheme.Light);
            }
            else
            {
                SetTheme(AppTheme.Dark);
            }
        }
    }
}