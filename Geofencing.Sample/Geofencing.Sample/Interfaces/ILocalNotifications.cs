using System;
namespace Geofencing.Sample.Interfaces
{
    public interface ILocalNotifications
    {
        void Show(string title, string body, int id = 0);
        void Show(string title, string body, DateTime notifyTime, int id = 0);
        void Cancel(int id);
    }
}
