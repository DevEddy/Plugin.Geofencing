using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Prism;
using Prism.Ioc;
using Xamarin.Forms;

namespace Geofencing.Sample.Droid
{
    [Activity(Label = "Geofences", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static bool Initialized { get; private set; } = false;

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App(new AndroidInitializer()));

            if (!Initialized)
            {
                MessagingCenter.Subscribe<string, string>(string.Empty, "StartGeofencingService", StartGeofencingService);
                MessagingCenter.Subscribe<string, string>(string.Empty, "StopGeofencingService", StopGeofencingService);
            }
            Initialized = true;
        }

        public void StartGeofencingService(object sender, string whatEver)
        {
            Services.GeofencingHelper.StartGeofencingService();
        }

        public void StopGeofencingService(object sender, string whatEver)
        {
            Services.GeofencingHelper.StopGeofencingService();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
            => Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);

    }

    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry container)
        {
            // Register any platform specific implementations
        }
    }
}

