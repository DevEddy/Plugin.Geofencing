using Geofencing.Sample.Model;
using Plugin.Geofencing;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Geofencing.Sample.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public ObservableCollection<GeofencePlace> GeofencePlaces { get; } = new ObservableCollection<GeofencePlace>();

        bool _showListView;
        public bool ShowListView
        {
            get { return _showListView; }
            set { SetProperty(ref _showListView, value); }
        }

        public MainPageViewModel(INavigationService navigationService) 
            : base (navigationService)
        {
            Title = "Geofencing";
            LoadCommand = new DelegateCommand(OnLoadCommandExecuted);
            AddCommand = new DelegateCommand(OnAddCommandExecuted);
            EditCommand = new DelegateCommand<GeofencePlace>(OnEditCommandExecuted);
            DeleteCommand = new DelegateCommand<GeofencePlace>(OnDeleteCommandExecuted);
            GeofencePlaces.CollectionChanged += GeofencePlaces_CollectionChanged;
        }

        private void GeofencePlaces_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ShowListView = GeofencePlaces.Any();
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            LoadCommand.Execute();
            base.OnNavigatedTo(parameters);
        }

        public DelegateCommand LoadCommand { get; }
        void OnLoadCommandExecuted()
        {
            var monitoredRegions = CrossGeofencing.Current.MonitoredRegions;
            GeofencePlaces.Clear();
            if (monitoredRegions != null && monitoredRegions.Any())
            {
                foreach (var monitoredRegion in monitoredRegions)
                {
                    GeofencePlaces.Add(new GeofencePlace {
                        ID = monitoredRegion.Identifier,
                        Latitude = monitoredRegion.Center.Latitude,
                        Longitude = monitoredRegion.Center.Longitude,
                        Radius = monitoredRegion.Radius.TotalMeters
                    });
                }
            }
            // ios local notifications
            MessagingCenter.Send(string.Empty, "RequestNotificationPermission", "");
        }

        public DelegateCommand AddCommand { get; }
        async void OnAddCommandExecuted()
        {
            var permissionsGranted = await CheckLocationPermissions();
            if (!permissionsGranted)
                return;

            await NavigationService.NavigateAsync($"AddEditGeofencePage", useModalNavigation: false);
        }

        public DelegateCommand<GeofencePlace> DeleteCommand { get; }
        void OnDeleteCommandExecuted(GeofencePlace place)
        {
            try
            {
                CrossGeofencing.Current.StopMonitoring(
                    new GeofenceRegion(place.ID,
                                       new Position(place.Latitude, place.Longitude),
                                       Distance.FromMeters(place.Radius)));
                
                if(!CrossGeofencing.Current.MonitoredRegions.Any())
                    MessagingCenter.Send("", "StopGeofencingService", "");

                LoadCommand.Execute();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting geofence place: {ex.Message}");
            }
        }

        public DelegateCommand<GeofencePlace> EditCommand { get; }
        async void OnEditCommandExecuted(GeofencePlace place)
        {
            await NavigationService.NavigateAsync($"AddEditGeofencePage", new NavigationParameters {
                { "place", place }
            }, false);
        }

        async Task<bool> CheckLocationPermissions()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                if (status != PermissionStatus.Granted)
                {
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
                    if (results.ContainsKey(Permission.Location))
                        status = results[Permission.Location];
                }
                if (status == PermissionStatus.Granted)
                    return true;                
                else if (status != PermissionStatus.Unknown)                
                    return false;
                
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking permissions : {ex.Message}");
                return false;
            }
        }
    }
}
