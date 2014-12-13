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
using Windows.Devices.Geolocation.Geofencing;
using Windows.Devices.Geolocation;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace Shreeasish.Safer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
    #region objects created
        private Accelerometer _acclerometer;

        bool Registered = false;

        public string Status { get; set; }

        bool BackGroundTaskIsActive = false;

        private DeviceUseTrigger manualTrigger;

        private LocationTrigger geofenceTrigger;

        private string Error;

        public bool BackgroundRequestGranted= false;

        private BackgroundTaskRegistration saferAccelerometerRegistrationManual;

        private BackgroundTaskRegistration saferAccelerometerRegistrationGeofence;
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
            else
                count.Text = "Task is inactive";
        }//Constructor End


        /// <summary>
        /// Function to Request Background Access, Sets BackgroundRequestGranted to 'true' if granted
        /// Also checks if the accelerometer exists
        /// </summary>
        async void RequestBackgroundAccessAsync()
        {
            if (_acclerometer != null)
            {

                manualTrigger = new DeviceUseTrigger();

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
            accelerometerTaskBuilder.SetTrigger(manualTrigger);

            //Task registered
            saferAccelerometerRegistrationManual = accelerometerTaskBuilder.Register();

            //background task completion event handler
            saferAccelerometerRegistrationManual.Completed += new BackgroundTaskCompletedEventHandler(OnBackgroundTaskCompleted);

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

#warning unregisters the background task

                    if (null != saferAccelerometerRegistrationManual)
                    {
                        saferAccelerometerRegistrationManual.Unregister(false);
                        saferAccelerometerRegistrationManual = null;
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

        #region Start Accelerometer Function

        private async Task<bool> StartAccelerometerBackGroundTask(String deviceId)
        {
            bool started = false;

            try
            {
                // Request a DeviceUse task to use the accelerometer.
                DeviceTriggerResult accelerometerTriggerResult = await manualTrigger.RequestAsync(deviceId);

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

        #endregion




        


        private void Register_Click(object sender, RoutedEventArgs e)
        {
            RegisterBackgroundTask();
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            BackGroundTaskIsActive = await StartAccelerometerBackGroundTask(_acclerometer.DeviceId);

        }







        private async void GRegister_Click(object sender, RoutedEventArgs e)
        {
            
            

         // Get permission for a background task from the user. If the user has already answered once,
         //this does nothing and the user must manually update their preference via PC Settings.
         BackgroundAccessStatus backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
        
         // Regardless of the answer, register the background task. If the user later adds this application
         // to the lock screen, the background task will be ready to run.
         // Create a new background task builder
         BackgroundTaskBuilder geofenceTaskBuilder = new BackgroundTaskBuilder();
    
          geofenceTaskBuilder.Name = SaferConfiguration.GetTaskName();
          geofenceTaskBuilder.TaskEntryPoint = SaferConfiguration.GetTaskEntryPoint();

          // Create a new location trigger
           var trigger = new LocationTrigger(LocationTriggerType.Geofence);

           // Associate the locationi trigger with the background task builder
           geofenceTaskBuilder.SetTrigger(trigger);

        // If it is important that there is user presence and/or
        // internet connection when OnCompleted is called
        // the following could be called before calling Register()
        // SystemCondition condition = new SystemCondition(SystemConditionType.UserPresent | SystemConditionType.InternetAvailable);
        // geofenceTaskBuilder.AddCondition(condition);

        // Register the background task
           saferAccelerometerRegistrationGeofence = geofenceTaskBuilder.Register();

        // Associate an event handler with the new background task
           saferAccelerometerRegistrationGeofence.Completed += new BackgroundTaskCompletedEventHandler(OnBackgroundTaskCompleted);

        switch (backgroundAccessStatus)
        {
        case BackgroundAccessStatus.Unspecified:
        case BackgroundAccessStatus.Denied:
            Error = "This application must be added to the lock screen before the background task will run.";
            break;

        }


        }


        

 

           
        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CreateGeofence();
        }


        
        public async void CreateGeofence()
        {
            Geofence geofence = null;

            string fenceKey = "My Geofence";

            Geolocator MyGeolocator = new Geolocator();
            
            Geoposition MyGeoposition = await MyGeolocator.GetGeopositionAsync();



            BasicGeoposition position;
            position.Latitude = Double.Parse(MyGeoposition.Coordinate.Latitude.ToString());
            position.Longitude = Double.Parse(MyGeoposition.Coordinate.Longitude.ToString());
            position.Altitude = 0.0;
            double radius = 100;

            // the geofence is a circular region
            Geocircle geocircle = new Geocircle(position, radius);

            bool singleUse=false;

            // want to listen for enter geofence, exit geofence and remove geofence events
            // you can select a subset of these event states
            MonitoredGeofenceStates mask = 0;

            mask |= MonitoredGeofenceStates.Entered;
            mask |= MonitoredGeofenceStates.Exited;

            // setting up how long you need to be in geofence for enter event to fire
            TimeSpan dwellTime;


            dwellTime = new TimeSpan(0, 0, 30);//(ParseTimeSpan("0", defaultDwellTimeSeconds));
  
            // setting up how long the geofence should be active
            TimeSpan duration;

            duration = new TimeSpan(2,0,0,0);


            // setting up the start time of the geofence
            DateTimeOffset startTime;

            startTime = DateTime.Today;


            geofence = new Geofence(fenceKey, geocircle, mask, singleUse, dwellTime, startTime, duration);
           
                GeofenceMonitor.Current.Geofences.Add(geofence);
           
           

        }







    }//Class End
}
