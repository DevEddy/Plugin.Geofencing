using Prism;
using Prism.Ioc;
using Geofencing.Sample.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Prism.DryIoc;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Geofencing.Sample
{
    public partial class App : PrismApplication
    {
        /* 
         * The Xamarin Forms XAML Previewer in Visual Studio uses System.Activator.CreateInstance.
         * This imposes a limitation in which the App class must have a default constructor. 
         * App(IPlatformInitializer initializer = null) cannot be handled by the Activator.
         */
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
        }
    }
}
