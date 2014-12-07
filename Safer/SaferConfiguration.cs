using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shreeasish.Safer
{
    class SaferConfiguration
    {
        private static string AccelerometerTaskName = "AccelerometerTask";
       
        private static string AccelerometerTaskEntryPoint = "SaferBackGroundTasks.AccelerometerTask";
        /// <summary>
        ///Returns the Accelerometer Task Name
        /// </summary>
        /// <returns>(String)</returns>
        public static string GetTaskName()
        {
            return AccelerometerTaskName;
        }
        /// <summary>
        /// Returns the Accelerometer Task Entry Point
        /// </summary>
        /// <returns></returns>
        public static string GetTaskEntryPoint()
        {
            return AccelerometerTaskEntryPoint;
        }
	



    }
}
