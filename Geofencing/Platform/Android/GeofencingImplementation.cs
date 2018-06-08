using Android.App;
using Android.Gms.Location;
using Com.Pathsense.Android.Sdk.Location;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plugin.Geofencing
{
    /// <summary>
    /// Interface for $safeprojectgroupname$
    /// </summary>
    public class GeofencingImplementation : Java.Lang.Object, IGeofencing
    {
        public event EventHandler<GeofenceStatusChangedEventArgs> RegionStatusChanged;
        public Distance DesiredAccuracy { get; set; } = Distance.FromMeters(200);
        public IReadOnlyList<GeofenceRegion> MonitoredRegions => _states.Values.Select(x => x.Region).ToList();

        readonly IDictionary<string, GeofenceState> _states;
        readonly PathsenseLocationProviderApi _pathSenseApi;

        public GeofencingImplementation()
        {
            _states = new Dictionary<string, GeofenceState>();
            _pathSenseApi = PathsenseLocationProviderApi.GetInstance(Application.Context);

            var dbRegions = MarcelloDatabase.Current.GetAll<DbGeofenceRegion>();
            foreach (var dbRegion in dbRegions)
            {
                var radius = Distance.FromMeters(dbRegion.CenterRadiusMeters);
                var center = new Position(dbRegion.CenterLatitude, dbRegion.CenterLongitude);
                var region = new GeofenceRegion(dbRegion.Identifier, center, radius);
                var state = new GeofenceState(region);
                _states.Add(dbRegion.Identifier, state);
            }
        }

        public void StartMonitoring(GeofenceRegion region)
        {
            MarcelloDatabase.Current.Save(new DbGeofenceRegion
            {
                Identifier = region.Identifier,
                CenterLatitude = region.Center.Latitude,
                CenterLongitude = region.Center.Longitude,
                CenterRadiusMeters = region.Radius.TotalMeters
            });
            var state = new GeofenceState(region);
            _states.Add(region.Identifier, state);
            AddGeofence(region.Identifier, region.Center.Latitude, region.Center.Longitude, Convert.ToInt32(region.Radius.TotalMeters));
        }

        void AddGeofence(string id, double latitude, double longtitude, int radiusInMeter)
        {
            _pathSenseApi?.AddGeofence(id, latitude, longtitude, radiusInMeter, Java.Lang.Class.FromType(typeof(PathsenseGeofenceDemoGeofenceEventReceiver)));
        }
        void AddGeofences()
        {
            foreach (var item in _states)
                AddGeofence(item.Key,
                            item.Value.Region.Center.Latitude,
                            item.Value.Region.Center.Longitude,
                            Convert.ToInt32(item.Value.Region.Radius.TotalMeters));
        }

        public void StopMonitoring(GeofenceRegion region)
        {
            if (!_states.ContainsKey(region.Identifier))
                return;
            
            MarcelloDatabase.Current.Delete<DbGeofenceRegion>(region.Identifier);
            _pathSenseApi.RemoveGeofence(region.Identifier);
            _states.Remove(region.Identifier);
        }

        public void StopAllMonitoring()
        {
            MarcelloDatabase.Current.DeleteAll<DbGeofenceRegion>();
            _pathSenseApi.RemoveGeofences();
            _states.Clear();
        }

        public void DoBroadcast(string triggeredGeofenceId, GeofenceStatus transitionType)
        {
            var reqId = triggeredGeofenceId;
            if (!_states.ContainsKey(reqId))
            {
                System.Diagnostics.Debug.WriteLine($"Triggered geofence {reqId} is not in the local app database. Skipping. Remove this geofence maybe?");
                return;
            }
            _states[reqId].Status = transitionType;
            RegionStatusChanged?.Invoke(this, new GeofenceStatusChangedEventArgs(_states[reqId].Region, transitionType));
        }

        public void DoBroadcast(IList<IGeofence> triggeredGeofences, GeofenceStatus transitionType)
        {
            foreach (var triggeredGeofence in triggeredGeofences)
                DoBroadcast(triggeredGeofence.RequestId, transitionType);
        }

        public void DoBroadcast(IList<IGeofence> triggeredGeofences, int transitionType)
        {
            DoBroadcast(triggeredGeofences, FromNative(transitionType));
        }

        protected virtual GeofenceStatus FromNative(int transitionType)
        {
            switch (transitionType)
            {
                case Geofence.GeofenceTransitionEnter:
                    return GeofenceStatus.Entered;
                case Geofence.GeofenceTransitionExit:
                    return GeofenceStatus.Exited;
                default:
                    return GeofenceStatus.Unknown;
            }
        }

        protected virtual IGeofence ToNative(GeofenceRegion region)
        {
            return new GeofenceBuilder()
                .SetRequestId(region.Identifier)
                .SetCircularRegion(
                    region.Center.Latitude,
                    region.Center.Longitude,
                    (float)region.Radius.TotalMeters
                )
                .SetExpirationDuration(Geofence.NeverExpire)
                .SetTransitionTypes(Geofence.GeofenceTransitionEnter | Geofence.GeofenceTransitionExit)
                .Build();
        }
    }
}
