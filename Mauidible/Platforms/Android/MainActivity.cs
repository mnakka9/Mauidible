using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace Mauidible
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate (Bundle savedInstanceState)
        {
            if (!Android.OS.Environment.IsExternalStorageManager)
            {
                Intent intent = new Intent();
                intent.SetAction(Android.Provider.Settings.ActionManageAppAllFilesAccessPermission);
                Android.Net.Uri uri = Android.Net.Uri.FromParts("package", PackageName, null);
                intent.SetData(uri);
                StartActivity(intent);
            }
            base.OnCreate(savedInstanceState);
        }
    }
}
