namespace Plugin.Geofencing
{
    public class GeofenceRegion : IObjectIdentifier
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

        public override bool Equals(object obj) => Identifier.Equals(obj);
        public override int GetHashCode() => Identifier.GetHashCode();
    }
}
