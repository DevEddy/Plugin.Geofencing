using Android.Content;
using Android.Util;

namespace Plugin.Geofencing
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    public class GeofenceBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Log.Info("GeofenceBroadcastReceiver", "Geofencing OnReceive");
            // Enqueues a JobIntentService passing the context and intent as parameters
            GeofenceTransitionsJobIntentService.EnqueueWork(context, intent);
        }
    }
}
