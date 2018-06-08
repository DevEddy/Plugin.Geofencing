using Android.Content;
using Android.Support.V4.Content;
using Com.Pathsense.Android.Sdk.Location;

namespace Plugin.Geofencing
{
    [BroadcastReceiver]
    public class PathsenseGeofenceDemoGeofenceEventReceiver : PathsenseGeofenceEventReceiver
    {
        protected override void OnGeofenceEvent(Context p0, PathsenseGeofenceEvent p1)
        {
            System.Diagnostics.Debug.WriteLine($"geofence = {p1.GeofenceId}, {p1.Latitude}, {p1.Longitude}, {p1.Radius}");

            if (!p1.IsEgress && !p1.IsIngress)
                return;

            if (CrossGeofencing.Current is GeofencingImplementation managerImpl)
                managerImpl.DoBroadcast(p1.GeofenceId, p1.IsEgress ? GeofenceStatus.Exited : GeofenceStatus.Entered);

            var location = p1.Location;
            System.Diagnostics.Debug.WriteLine($"geofenceEgress = {location.Time}, {location.Provider}, {location.Latitude}, {location.Longitude}, {location.Altitude}, {location.Speed}, {location.Bearing}, {location.Accuracy}");

            // maybe sound later 
            // https://github.com/pathsense/pathsense-samples-android/blob/master/pathsense-geofencedemo-app/src/main/java/com/pathsense/geofencedemo/app/SoundManager.java
            //SoundManager.getInstance(context).playDing();

            // broadcast event
            Intent geofenceEventIntent = new Intent("geofenceEvent");
            geofenceEventIntent.PutExtra("geofenceEvent", p1);
            LocalBroadcastManager.GetInstance(p0).SendBroadcast(geofenceEventIntent);
        }
    }
}
