using Geofencing.Sample.Model;
using Plugin.Geofencing;
using Plugin.Geolocator;
using Prism.Commands;
using Prism.Navigation;
using System;
using Xamarin.Forms;

namespace Geofencing.Sample.ViewModels
{
    public class AddEditGeofencePageViewModel : ViewModelBase
    {
        public GeofencePlace Place { get; private set; } = new GeofencePlace() ;
        public bool Edit { get; private set; }

        Plugin.Geolocator.Abstractions.Position _lastPosition;

        public AddEditGeofencePageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            LoadCommand = new DelegateCommand(OnLoadCommandExecuted);
            SaveCommand = new DelegateCommand(OnSaveCommandExecuted);
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            if (parameters.GetNavigationMode() == NavigationMode.Back)
                return;

            if (parameters["place"] is GeofencePlace place)
            {
                Place.ID = place.ID;
                Place.Radius = place.Radius;
                Place.Latitude = place.Latitude;
                Place.Longitude = place.Longitude;
                Edit = true;
            }
            else
                Place.Radius = 300;

            Title = Edit ? $"Edit {Place.ID}" : $"Add";
            LoadCommand.Execute();
        }

        public DelegateCommand LoadCommand { get; }
        async void OnLoadCommandExecuted()
        {
            try
            {
                if (!Edit)
                {
                    Place.Radius = 1000;

                    if (_lastPosition == null)
                    {
                        _lastPosition = await CrossGeolocator.Current.GetLastKnownLocationAsync();
                        if (_lastPosition == null)
                            _lastPosition = await CrossGeolocator.Current.GetPositionAsync(TimeSpan.FromSeconds(8), null, true);
                    }

                    if (_lastPosition == null)
                        return;
                    
                    Place.Latitude = Convert.ToDouble(_lastPosition?.Latitude);
                    Place.Longitude = Convert.ToDouble(_lastPosition?.Longitude);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting current position: {ex.Message}");
            }
        }

        public DelegateCommand SaveCommand { get; }
        async void OnSaveCommandExecuted()
        {
            try
            {
                if (Edit)
                {
                    CrossGeofencing.Current.StopMonitoring(new GeofenceRegion(Place.ID,
                                           new Position(Place.Latitude, Place.Longitude),
                                           Distance.FromMeters(Place.Radius)));

                }
                CrossGeofencing.Current.StartMonitoring(new GeofenceRegion(
                    Place.ID,
                    new Position(Place.Latitude, Place.Longitude),
                    Distance.FromMeters(Place.Radius)
                ));

                MessagingCenter.Send("", "StartGeofencingService", "");

                await NavigationService.GoBackAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in saving geofence: {ex.Message}");
            }
        }
    }
}
