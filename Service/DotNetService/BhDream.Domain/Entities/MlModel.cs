using BhDream.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace BhDream.Domain.Entities
{
    public class MlModel
    {
        // Unique identifier for the record
        public Guid Id { get; set; }

        public string ModelName { get; set; }

        // Timestamps for the execution window
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        // Using JsonNode allows you to store dynamic JSON structures 
        // without defining a strict C# class for 'Features' upfront.
        public string? Features { get; set; }
        public string? Parameters { get; set; }

        // Representing status as an integer (e.g., 0 = Pending, 1 = Running, 2 = Completed)
        public MlTrainingStatus Status { get; set; }

        public DateTime LastUpdatedAt { get; set; }

        // Reference string for the model (marked as nullable if it can be empty)
        public string? ModelReference { get; set; }

        // For metrics, a generic string or a dynamic JSON object works best 
        // depending on how you plan to store it in your database.
        public string? ModelMetrics { get; set; }

        public string? FailureReason { get; set; }
    }
}
