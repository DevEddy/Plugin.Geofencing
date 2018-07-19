using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Geofencing
{
    public enum GeofenceManagerStatus
    {
        Unknown,
        Ready,
        Disabled,
        NotSupported,
        PermissionDenied
    }
}
