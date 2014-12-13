using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Shreeasish.Safer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GeoFencePage : Page
    {
        private CancellationToken token;

        public Geolocator geolocator;

        public CancellationTokenSource cts ;
        

        public GeoFencePage()
        {
            this.InitializeComponent();

              try 
        {
            // Get a geolocator object 
            geolocator = new Geolocator();

            // Get cancellation token
            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

          
                                                                
        } 





        }




        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {



        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await geolocator.GetGeopositionAsync().AsTask(token);
        }





        
    }
}
