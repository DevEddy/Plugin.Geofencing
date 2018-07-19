using Android.App;
using Android.Content;
using Android.Gms.Location;

namespace Plugin.Geofencing.Platform.Android
{
    public class GeofenceIntentService : IntentService
    {
        protected override void OnHandleIntent(Intent intent)
        {
            if (CrossGeofencing.Current is GeofencingImplementation managerImpl)
                managerImpl.TryFireEvent(GeofencingEvent.FromIntent(intent));
        }
    }
}
