using System;

namespace Plugin.Geofencing
{
    public class GeofenceRegion : IObjectIdentifier, IEquatable<GeofenceRegion>
    {
        public GeofenceRegion(string identifier, Position center, Distance radius)
        {
            Identifier = identifier;
            Center = center;
            Radius = radius;
        }

        public string Identifier { get; set; }
        public Position Center { get; set; }
        public Distance Radius { get; set; }
        public GeofenceStatus LastKnownGeofenceStatus { get; set; }

        public bool Equals(GeofenceRegion other)
        {
            if (other == null) return false;
            return other.Identifier.Equals(other.Identifier);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is GeofenceRegion region))
                return false;

            return Equals(region);
        }

        public override int GetHashCode() => Identifier.GetHashCode();
    }
}
