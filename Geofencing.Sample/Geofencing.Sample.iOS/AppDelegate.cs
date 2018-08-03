using Foundation;
using Prism;
using Prism.Ioc;
using UIKit;
using UserNotifications;
using Xamarin.Forms;

namespace Geofencing.Sample.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
                UNUserNotificationCenter.Current.Delegate = new UserNotificationCenterDelegate();

            Xamarin.Forms.Forms.Init();
            Syncfusion.ListView.XForms.iOS.SfListViewRenderer.Init();
            new Syncfusion.SfRangeSlider.XForms.iOS.SfRangeSliderRenderer();
            LoadApplication(new App(new iOSInitializer()));
            MessagingCenter.Subscribe<string, string>(string.Empty, "RequestNotificationPermission", RequestNotificationPermission);

            return base.FinishedLaunching(app, options);
        }

        public void RequestNotificationPermission(object sender, string hexColor)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                // Ask the user for permission to get notifications on iOS 10.0+
                UNUserNotificationCenter.Current.RequestAuthorization(
                        UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
                        (approved, error) => { });
            }
            else if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                // Ask the user for permission to get notifications on iOS 8.0+
                var settings = UIUserNotificationSettings.GetSettingsForTypes(
                        UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                        new NSSet());

                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            }
        }
    }

    public class iOSInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry container)
        {

        }
    }
}
