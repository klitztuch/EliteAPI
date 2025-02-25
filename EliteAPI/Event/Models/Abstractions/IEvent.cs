using System;
using EliteAPI.Abstractions;
using Newtonsoft.Json;

namespace EliteAPI.Event.Models.Abstractions
{
    /// <summary>
    /// An in-game event
    /// </summary>
    public interface IEvent : IJsonObject
    {
        /// <summary>
        /// The timestamp of the event
        /// </summary>
        [JsonProperty("timestamp", Order = -3)]
        DateTime Timestamp { get; }

        /// <summary>
        /// The name of the event
        /// </summary>
        [JsonProperty("event", Order = -2)]
        string Event { get; }

        /// <summary>
        /// Generates this event's Json
        /// </summary>
        new string ToJson();
    }
}