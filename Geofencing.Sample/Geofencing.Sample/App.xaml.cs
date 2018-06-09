using Prism;
using Prism.Ioc;
using Geofencing.Sample.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Prism.DryIoc;
using Geofencing.Sample.Interfaces;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Geofencing.Sample
{
    public partial class App : PrismApplication
    {
        public static ILocalNotifications LocalNotifications { get; } = DependencyService.Get<ILocalNotifications>();

        public App() : this(null) { }

        public App(IPlatformInitializer initializer) : base(initializer) { }

        protected override async void OnInitialized()
        {
            InitializeComponent();

            await NavigationService.NavigateAsync("NavigationPage/MainPage");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<MainPage>();
            containerRegistry.RegisterForNavigation<AddEditGeofencePage>();
        }

        protected override void OnStart()
        {
            Plugin.Geofencing.CrossGeofencing.Current.RegionStatusChanged += Current_RegionStatusChanged;
            base.OnStart();
        }

        private void Current_RegionStatusChanged(object sender, Plugin.Geofencing.GeofenceStatusChangedEventArgs e)
        {
            var geofencePlaceId = e.Region.Identifier;
            var entered = e.Status == Plugin.Geofencing.GeofenceStatus.Entered;
            var text = entered ? "entered" : "exited";
            LocalNotifications.Show("Geofence update", $"{geofencePlaceId} {text}");
        }
    }
}
