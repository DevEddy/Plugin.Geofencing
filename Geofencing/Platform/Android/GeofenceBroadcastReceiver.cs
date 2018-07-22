using Android.Content;

namespace Plugin.Geofencing
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    public class GeofenceBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            // Enqueues a JobIntentService passing the context and intent as parameters
            GeofenceTransitionsJobIntentService.EnqueueWork(context, intent);
        }
    }
}
