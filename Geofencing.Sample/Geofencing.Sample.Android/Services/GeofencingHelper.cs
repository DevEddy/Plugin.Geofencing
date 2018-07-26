using Android.Content;
using System;
using System.Threading.Tasks;

namespace Geofencing.Sample.Droid.Services
{
    public class GeofencingHelper
    {
        protected static GeofencingServiceConnection GeofencingServiceConnection;
        public static GeofencingHelper Current { get; }
        static bool _isRunning;
        public bool IsRunning => _isRunning;

        public GeofencingService GeofencingService
        {
            get
            {
                if (GeofencingServiceConnection.Binder == null)
                    throw new Exception("Service not bound yet");

                // note that we use the ServiceConnection to get the Binder, and the Binder to get the Service here
                return GeofencingServiceConnection.Binder.Service;
            }
        }
        
        public event EventHandler<ServiceConnectedEventArgs> LocationServiceConnected = delegate { };

        #region Application context

        static GeofencingHelper()
        {
            Current = new GeofencingHelper();
        }

        protected GeofencingHelper()
        {
            // create a new service connection so we can get a binder to the service
            GeofencingServiceConnection = new GeofencingServiceConnection(null);

            // this event will fire when the Service connectin in the OnServiceConnected call 
            GeofencingServiceConnection.ServiceConnected += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("Service Connected");
                // we will use this event to notify MainActivity when to start updating the UI
                LocationServiceConnected(this, e);
            };
        }

        public static Task StartGeofencingService()
        {
            if (_isRunning)
                return Task.FromResult(true);

            _isRunning = true;
            // Starting a service like this is blocking, so we want to do it on a background thread
            return Task.Run(() =>
            {
                // Start our main service
                System.Diagnostics.Debug.WriteLine("Calling StartService");
                Android.App.Application.Context.StartService(new Intent(Android.App.Application.Context, typeof(GeofencingService)));

                // bind our service (Android goes and finds the running service by type, and puts a reference
                // on the binder to that service)
                // The Intent tells the OS where to find our Service (the Context) and the Type of Service
                // we're looking for (LocationService)
                var locationServiceIntent = new Intent(Android.App.Application.Context, typeof(GeofencingService));
                System.Diagnostics.Debug.WriteLine("Calling service binding");

                // Finally, we can bind to the Service using our Intent and the ServiceConnection we
                // created in a previous step.
                Android.App.Application.Context.BindService(locationServiceIntent, GeofencingServiceConnection, Bind.AutoCreate);
            });
        }

        public static void StopGeofencingService()
        {
            try
            {
                if (!_isRunning)
                    return;

                _isRunning = false;
                // Check for nulls in case StartLocationService task has not yet completed.
                System.Diagnostics.Debug.WriteLine("Stop GeofencingService");

                // Unbind from the LocationService; otherwise, StopSelf (below) will not work:
                if (GeofencingServiceConnection != null)
                {
                    System.Diagnostics.Debug.WriteLine("Unbinding from GeofencingService");
                    Android.App.Application.Context.UnbindService(GeofencingServiceConnection);
                }

                // Stop the LocationService:
                if (Current.GeofencingService != null)
                {
                    System.Diagnostics.Debug.WriteLine("Stopping the GeofencingService");
                    Current.GeofencingService.StopSelf();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping GeofencingService: {ex.Message}");
            }
        }
        #endregion
    }
}