using System;

namespace Oqtane.Models
{
    /// <summary>
    /// An instance of a Task which is executed by the TaskJob
    /// </summary>
    public class JobTask : ModelBase
    {
        /// <summary>
        /// Internal ID
        /// </summary>
        public int JobTaskId { get; set; }

        /// <summary>
        /// Site where the Task should execute
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// Name for simple identification
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Fully qualified type name of the Task
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Any parameters related to the Task
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// Indicates if the Task is completed 
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Any status information provided by the Task
        /// </summary>
        public string Status { get; set; }

        // constructors
        public JobTask() { }

        public JobTask(int siteId, string name, string type, string parameters)
        {
            SiteId = siteId;
            Name = name;
            Type = type;
            Parameters = parameters;
        }
    }
}
