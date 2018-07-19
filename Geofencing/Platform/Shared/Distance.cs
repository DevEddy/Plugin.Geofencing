using System;

namespace Plugin.Geofencing
{
    public sealed class Distance
    {
        public const double MILES_TO_KM = 1.60934;
        public const double KM_TO_MILES = 0.621371;
        public const int KM_TO_METERS = 1000;

        public double TotalMiles => TotalKilometers * KM_TO_MILES;
        public double TotalMeters => TotalKilometers * 1000;
        public double TotalKilometers { get; set; }

        public override string ToString() => $"{TotalKilometers} km";
        public override int GetHashCode() => TotalKilometers.GetHashCode();
        public override bool Equals(object obj)
        {
            if(!(obj is Distance other))
                return false;

            if (TotalKilometers.Equals(other.TotalKilometers))
                return false;

            return false;
        }
        
        public static Distance FromMiles(int miles) => new Distance { TotalKilometers = miles * MILES_TO_KM };
        public static Distance FromMeters(double meters) => new Distance { TotalKilometers = meters / KM_TO_METERS };
        public static Distance FromKilometers(double km) => new Distance { TotalKilometers = km };

        public static bool operator ==(Distance x, Distance y) => x.TotalKilometers == y.TotalKilometers;
        public static bool operator !=(Distance x, Distance y) => x.TotalKilometers != y.TotalKilometers;
        public static bool operator >(Distance x, Distance y) => x.TotalKilometers > y.TotalKilometers;
        public static bool operator <(Distance x, Distance y) => x.TotalKilometers < y.TotalKilometers;
    }
}
