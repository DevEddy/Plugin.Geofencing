using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Geofencing.Sample.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AddEditGeofencePage : ContentPage
	{
		public AddEditGeofencePage()
		{
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex}");
            }
		}
    }
}