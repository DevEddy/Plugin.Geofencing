using Prism.Mvvm;
using System;

namespace Geofencing.Sample.Model
{
    public class GeofencePlace : BindableBase
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();

        double _latitude;
        public double Latitude
        {
            get { return _latitude; }
            set { SetProperty(ref _latitude, value); }
        }

        double _longitude;
        public double Longitude
        {
            get { return _longitude; }
            set { SetProperty(ref _longitude, value); }
        }

        double _radius;
        public double Radius
        {
            get { return _radius; }
            set { SetProperty(ref _radius, value); }
        }        

        public string RadiusDescription => $"Radius {Radius} m";
    }
}
