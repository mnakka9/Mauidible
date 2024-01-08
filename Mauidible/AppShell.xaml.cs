namespace Mauidible
{
    public partial class AppShell : Shell
    {
        public AppShell ()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(Player), typeof(Player));
        }
    }
}
