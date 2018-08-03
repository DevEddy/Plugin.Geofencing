using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;

namespace Geofencing.Sample.Droid.Services
{
    [BroadcastReceiver(Exported = false, DirectBootAware = true)]
    [IntentFilter(new[] { "android.intent.action.BOOT_COMPLETED", "android.intent.action.LOCKED_BOOT_COMPLETED" })]
    public class BootBroadcastReceiver : BroadcastReceiver
    {
        static readonly string TAG = "BootBroadcastReceiver";

        public override void OnReceive(Context context, Intent intent)
        {
            bool bootCompleted;
            string action = intent.Action;
            Log.Info(TAG, $"Recieved action {action}, user unlocked: "); 

            if (Build.VERSION.SdkInt > BuildVersionCodes.M)
                bootCompleted = Intent.ActionLockedBootCompleted == action;
            else
                bootCompleted = Intent.ActionBootCompleted == action;

            if (!bootCompleted)
                return;

            GeofencingHelper.StartGeofencingService();
        }
    }
}