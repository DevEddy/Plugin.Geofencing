using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.Support.V4.App;

namespace Plugin.Geofencing
{
    [Service(Exported = true, Permission = "android.permission.BIND_JOB_SERVICE")]
    public class GeofenceTransitionsJobIntentService : JobIntentService
    {
        const int JOB_ID = 573;

        public static void EnqueueWork(Context context, Intent intent)
        {
            EnqueueWork(context, Java.Lang.Class.FromType(typeof(GeofenceTransitionsJobIntentService)), JOB_ID, intent);
        }

        protected override void OnHandleWork(Intent intent)
        {
            if (CrossGeofencing.Current is GeofencingImplementation managerImpl)
                managerImpl.TryFireEvent(GeofencingEvent.FromIntent(intent));
        }
    }
}