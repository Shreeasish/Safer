// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.ApplicationModel.Background;
using Windows.Devices.Background;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Storage;
using Windows.Devices.Geolocation.Geofencing;


namespace SaferBackGroundTasks
{
    public sealed class AccelerometerTask : IBackgroundTask, IDisposable
    {
        private Accelerometer _accelerometer;
        private BackgroundTaskDeferral _deferral;
        private GeofenceStateChangeReport _lastReport;
        private bool _manualTrigger=false;


        
    #warning "Remove sample count";

        private ulong _sampleCount;

        //private IReadOnlyList<GeofenceStateChangeReport> ReportList;

        /// <summary> 
        /// Background task entry point.
        /// </summary> 
        /// <param name="taskInstance"></param>
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _accelerometer = Accelerometer.GetDefault();

#region Check Manual Trigger
            try
            {
                _manualTrigger = (bool) ApplicationData.Current.LocalSettings.Values["ManualTrigger"];
            }
            catch (Exception)
            {

                _manualTrigger = false;
            }
#endregion


            if (null != _accelerometer)
            {
                _sampleCount = 0;

                // Select a report interval that is both suitable for the purposes of the app and supported by the sensor.
                uint minReportIntervalMsecs = _accelerometer.MinimumReportInterval;
                _accelerometer.ReportInterval = minReportIntervalMsecs > 16 ? minReportIntervalMsecs : 16;

                // Subscribe to accelerometer ReadingChanged events.
                _accelerometer.ReadingChanged += new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);

                // Take a deferral that is released when the task is completed.
                _deferral = taskInstance.GetDeferral();

                // Get notified when the task is canceled.
                taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);

                // Store a setting so that the app knows that the task is running.
                ApplicationData.Current.LocalSettings.Values["IsBackgroundTaskActive"] = true;

            }

            GeofenceMonitor.Current.GeofenceStateChanged += OnGeoFenceStateChanged;

        }
        /// <summary>
        /// Event handler for GeoFence entry and exits
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void OnGeoFenceStateChanged(GeofenceMonitor sender, object args)
        {

            var reports = sender.ReadReports();

            foreach (GeofenceStateChangeReport report in reports)
            {
                GeofenceState state = report.NewState;

                Geofence geofence = report.Geofence;

                if (state == GeofenceState.Removed)
                {
                    report.Geofence.Id.ToString();
                    // remove the geofence from the geofences collection
                    GeofenceMonitor.Current.Geofences.Remove(geofence);

                }
                else if (state == GeofenceState.Entered)
                {
                    if(_manualTrigger==false)
                        _deferral.Complete();
                }
                else if (state == GeofenceState.Exited)
                {
                    

                }

            }
        }

        /// <summary> 
        /// Called when the background task is canceled by the app or by the system.
        /// </summary> 
        /// <param name="sender"></param>
        /// <param name="reason"></param>
        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            ApplicationData.Current.LocalSettings.Values["TaskCancelationReason"] = reason.ToString();
            ApplicationData.Current.LocalSettings.Values["SampleCount"] = _sampleCount;
            ApplicationData.Current.LocalSettings.Values["IsBackgroundTaskActive"] = false;

            // Complete the background task (this raises the OnCompleted event on the corresponding BackgroundTaskRegistration).
            _deferral.Complete();
        }

        /// <summary>
        /// Frees resources held by this background task.
        /// </summary>
        public void Dispose()
        {
            if (null != _accelerometer)
            {
                _accelerometer.ReadingChanged -= new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);
                _accelerometer.ReportInterval = 0;
            }
        }

        /// <summary>
        /// This is the event handler for acceleroemter ReadingChanged events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            _sampleCount++;
     
            // Save the sample count if the foreground app is visible.
            //bool appVisible = (bool)ApplicationData.Current.LocalSettings.Values["IsAppVisible"];
            //if (appVisible)
            //{
                ApplicationData.Current.LocalSettings.Values["SampleCount"] = _sampleCount;
            //}
        }
    }
}
