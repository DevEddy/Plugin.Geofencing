using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.Geofencing
{
    public interface IGeofencing
    {
        /// <summary>
        /// Current set of regions being monitored
        /// </summary>
        IReadOnlyList<GeofenceRegion> MonitoredRegions { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="region"></param>
        void StartMonitoring(GeofenceRegion region);
        void StopMonitoring(GeofenceRegion region);
        void StopAllMonitoring();
        Distance DesiredAccuracy { get; set; }

        event EventHandler<GeofenceStatusChangedEventArgs> RegionStatusChanged;
    }
}
