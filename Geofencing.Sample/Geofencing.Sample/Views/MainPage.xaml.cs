using Geofencing.Sample.Model;
using Geofencing.Sample.ViewModels;
using Xamarin.Forms;

namespace Geofencing.Sample.Views
{
    public partial class MainPage : ContentPage
	{
		public MainPage ()
		{
			InitializeComponent ();
		}

        private void ItemTapped(object sender, Syncfusion.ListView.XForms.ItemTappedEventArgs e)
        {
            if (!(e.ItemData is GeofencePlace place) ||
            !(BindingContext is MainPageViewModel viewModel))
                return;

            viewModel.EditCommand.Execute(place);
        }

        private void ItemHolding(object sender, Syncfusion.ListView.XForms.ItemHoldingEventArgs e)
        {
            if (!(e.ItemData is GeofencePlace place) ||
                !(BindingContext is MainPageViewModel viewModel))
                return;

            viewModel.EditCommand.Execute(place);
        }

        GeofencePlace swipedItem;

        private void SwipeStarted(object sender, Syncfusion.ListView.XForms.SwipeStartedEventArgs e)
        {
            swipedItem = null;
        }

        private void SwipeEnded(object sender, Syncfusion.ListView.XForms.SwipeEndedEventArgs e)
        {
            swipedItem = e.ItemData as GeofencePlace;
        }

        private void ItemDeleteTapped(object sender, System.EventArgs e)
        {
            if (swipedItem == null || !(BindingContext is MainPageViewModel viewModel))
                return;

            viewModel.DeleteCommand.Execute(swipedItem);
            list.ResetSwipe();
        }
        private void ItemEditTapped(object sender, System.EventArgs e)
        {
            if (swipedItem == null || !(BindingContext is MainPageViewModel viewModel))
                return;

            viewModel.EditCommand.Execute(swipedItem);
            list.ResetSwipe();
        }        
    }
}