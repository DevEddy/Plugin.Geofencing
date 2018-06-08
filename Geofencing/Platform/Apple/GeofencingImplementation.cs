using CoreLocation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UIKit;

namespace Plugin.Geofencing
{
    /// <summary>
    /// Interface for $safeprojectgroupname$
    /// </summary>
    public class GeofencingImplementation : IGeofencing
    {
        readonly CLLocationManager _locationManager;

        public GeofencingImplementation()
        {
            _locationManager = new CLLocationManager();
            _locationManager.RegionEntered += (sender, args) => DoBroadcast(args, GeofenceStatus.Entered);
            _locationManager.RegionLeft += (sender, args) => DoBroadcast(args, GeofenceStatus.Exited);
        }

        public Distance DesiredAccuracy { get; set; } = Distance.FromKilometers(1);
        public event EventHandler<GeofenceStatusChangedEventArgs> RegionStatusChanged;

        public IReadOnlyList<GeofenceRegion> MonitoredRegions
        {
            get
            {
                var list =
                    _locationManager
                    .MonitoredRegions
                    .Select(x => x as CLCircularRegion)
                    .Where(x => x != null)
                    .Select(FromNative)
                    .ToList();
                return new ReadOnlyCollection<GeofenceRegion>(list);
            }
        }

        public void StartMonitoring(GeofenceRegion region)
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                var native = ToNative(region);
                _locationManager.StartMonitoring(native);
            });
        }

        public void StopMonitoring(GeofenceRegion region)
        {
            var native = ToNative(region);
            _locationManager.StopMonitoring(native);
        }

        public void StopAllMonitoring()
        {
            var natives =
                _locationManager
                .MonitoredRegions
                .Select(x => x as CLCircularRegion)
                .Where(x => x != null)
                .ToList();

            foreach (var native in natives)
                _locationManager.StopMonitoring(native);
        }

        protected virtual void DoBroadcast(CLRegionEventArgs args, GeofenceStatus status)
        {
            if (!(args.Region is CLCircularRegion native))
                return;

            var region = FromNative(native);
            RegionStatusChanged?.Invoke(this, new GeofenceStatusChangedEventArgs(region, status));
        }

        protected virtual GeofenceRegion FromNative(CLCircularRegion native)
        {
            var radius = Distance.FromMeters(native.Radius);
            var center = FromNative(native.Center);
            return new GeofenceRegion(native.Identifier, center, radius);
        }

        protected virtual GeofenceStatus FromNative(CLRegionState state)
        {
            switch (state)
            {
                case CLRegionState.Inside:
                    return GeofenceStatus.Entered;

                case CLRegionState.Outside:
                    return GeofenceStatus.Exited;

                case CLRegionState.Unknown:
                default:
                    return GeofenceStatus.Unknown;
            }
        }

        protected virtual Position FromNative(CLLocationCoordinate2D native)
            => new Position(native.Latitude, native.Longitude);

        protected virtual CLLocationCoordinate2D ToNative(Position position)
            => new CLLocationCoordinate2D(position.Latitude, position.Longitude);

        protected virtual CLCircularRegion ToNative(GeofenceRegion region)
        {
            return new CLCircularRegion(
                ToNative(region.Center),
                region.Radius.TotalMeters,
                region.Identifier
            )
            {
                NotifyOnExit = true,
                NotifyOnEntry = true
            };
        }
    }
}
