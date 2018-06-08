using System;
namespace Plugin.Geofencing
{
    public class DbGeofenceRegion : IObjectIdentifier
    {
        public string Identifier { get; set; }
        public double CenterLatitude { get; set; }
        public double CenterLongitude { get; set; }
        public double CenterRadiusMeters { get; set; }
    }
}
