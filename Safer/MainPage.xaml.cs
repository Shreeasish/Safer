using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Background;
using Windows.Devices.Sensors;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace Shreeasish.Safer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
    #region registration objects created
        private Accelerometer _acclerometer;
       

        private DeviceUseTrigger accelerometerTrigger;

        private string error;

        public bool BackgroundRequestGranted= false;

        private BackgroundTaskRegistration saferAccelerometerRegistration;
        #endregion

        /// <summary>
        /// Constructor for MainPage
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            _acclerometer = Accelerometer.GetDefault();
            

            #region Accelerometer Registration
            
            #endregion
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }//Constructor End


        /// <summary>
        /// Function to Request Background Access, Sets BackgroundRequestGranted to 'true' if granted
        /// Also checks if the accelerometer exists
        /// </summary>
        async void RequestBackgroundAccessAsync()
        {
            if (_acclerometer != null)
            {

                accelerometerTrigger = new DeviceUseTrigger();

                BackgroundAccessStatus accessStatus = await BackgroundExecutionManager.RequestAccessAsync();

                if ((BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity == accessStatus) ||
                    (BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity == accessStatus))
                {
                    BackgroundRequestGranted = true;
                    return;
                }
                else
                {
#warning shift to UI

                    error = "Background tasks may be disabled for this app";
                    return;
                }

            }
            else
            {
#warning shift to UI
                
                error = "Accelerometer unavailable";
                return;
            }
        }//RequestBackgroundAccess end


        /// <summary>
        /// Finds a previously registered background task for this scenario and cancels it (if present)
        /// </summary>
        private void FindAndCancelExistingBackgroundTask()
        {
            foreach (var backgroundTask in BackgroundTaskRegistration.AllTasks.Values)
            {
                
                if (SaferConfiguration.GetTaskName() == backgroundTask.Name)
                {
                    ((BackgroundTaskRegistration)backgroundTask).Unregister(true);
                    break;
                }
            }
        }

        

        public bool RegisterBackgroundTask()
        {
            FindAndCancelExistingBackgroundTask();
            BackgroundTaskBuilder accelerometerTaskBuilder = new BackgroundTaskBuilder()
            {
                Name = SaferConfiguration.GetTaskName(),
                TaskEntryPoint = SaferConfiguration.GetTaskEntryPoint(),
            };
            
            accelerometerTaskBuilder

        }
        
        
        
        
        
        
        
        
        
        
        
        
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            RequestBackgroundAccessAsync();
            

            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.




        }

        private void AppBarToggleButton_Click(object sender, RoutedEventArgs e)
        {

        }



        


    }//Class End
}
