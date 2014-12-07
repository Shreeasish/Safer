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
using Windows.UI.Core;
using Windows.Storage;
using System.Threading.Tasks;
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

        bool Registered = false;

        bool BackGroundTaskIsActive = false;

        private DeviceUseTrigger accelerometerTrigger;

        private string Error;

        public bool BackgroundRequestGranted= false;

        private BackgroundTaskRegistration saferAccelerometerRegistration;
        #endregion


#warning remove timer;

        DispatcherTimer Timer;


        /// <summary>
        /// Constructor for MainPage
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            _acclerometer = Accelerometer.GetDefault();
            
            Timer = new DispatcherTimer();
            Timer.Interval = System.TimeSpan.FromMilliseconds(50);
            Timer.Start();

            Timer.Tick += Timer_Tick;

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void Timer_Tick(object sender, object e)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("SampleCount"))
                count.Text = ApplicationData.Current.LocalSettings.Values["SampleCount"].ToString();
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

                    Error = "Background tasks may be disabled for this app";
                    return;
                }

            }
            else
            {
#warning shift to UI
                
                Error = "Accelerometer unavailable";
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

            //Trigger set
            accelerometerTaskBuilder.SetTrigger(accelerometerTrigger);

            //Task registered
            saferAccelerometerRegistration = accelerometerTaskBuilder.Register();

            //background task completion event handler
            saferAccelerometerRegistration.Completed += new BackgroundTaskCompletedEventHandler(OnBackgroundTaskCompleted);

            return Registered = true;
        }





        private async void OnBackgroundTaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        args.CheckResult();
                        if (ApplicationData.Current.LocalSettings.Values.ContainsKey("TaskCancelationReason"))
                        {
                            string cancelationReason = (string)ApplicationData.Current.LocalSettings.Values["TaskCancelationReason"];
                            Error = cancelationReason;
                        }
                    }
                    catch (Exception ex)
                    {
                        Error = "Exception" + ex.Message;
                    }

                    if (null != saferAccelerometerRegistration)
                    {
                        saferAccelerometerRegistration.Unregister(false);
                        saferAccelerometerRegistration = null;
                    }

                });
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

        private async void AppBarToggleButton_Click(object sender, RoutedEventArgs e)
        {
            
          
        }

        private async Task<bool> StartAccelerometerBackGroundTask(String deviceId)
        {
            bool started = false;

            try
            {
                // Request a DeviceUse task to use the accelerometer.
                DeviceTriggerResult accelerometerTriggerResult = await accelerometerTrigger.RequestAsync(deviceId);

#warning background task seems to be run from here

                switch (accelerometerTriggerResult)
                {
                    case DeviceTriggerResult.Allowed:
                        Status = "Background task started";
                        started = true;
                        break;

                    case DeviceTriggerResult.LowBattery:
                        Error = "Insufficient battery to run the background task";
                        break;

                    case DeviceTriggerResult.DeniedBySystem:
                        // The system can deny a task request if the system-wide DeviceUse task limit is reached.
                        Error = "The system has denied the background task request";
                        break;

                    default:
                        Error = "Could not start the background task: " + accelerometerTriggerResult;
                        break;
                }
            }
            catch (InvalidOperationException)
            {
                // If toggling quickly between 'Disable' and 'Enable', the previous task
                // could still be in the process of cleaning up.
                Status = "A previous background task is still running, please wait for it to exit";
                FindAndCancelExistingBackgroundTask();
            }

            return started;
        }






        public string Status { get; set; }


        private void Register_Click(object sender, RoutedEventArgs e)
        {
            RegisterBackgroundTask();
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            BackGroundTaskIsActive = await StartAccelerometerBackGroundTask(_acclerometer.DeviceId);

        }



    }//Class End
}
