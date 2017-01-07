using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Napack.Client
{
    /// <summary>
    /// Defines the napack client settings.
    /// </summary>
    public class NapackClientSettings
    {
        /// <summary>
        /// The server hosting Napacks.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Uri NapackFrameworkServer { get; set; }

        /// <summary>
        /// Allows the inclusion of commercially-licensed napacks.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public bool AllowCommercial { get; set; }

        /// <summary>
        /// Allows the inclusion of copy-left napacks.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public bool AllowCopyLeft { get; set; }

        /// <summary>
        /// Allows the inclusion of custom-licensed napacks.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public bool AllowCustomLicenses { get; set; }

        /// <summary>
        /// If true, auto-updates Napacks with newer patch versions.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public bool AutoUpdatePatch { get; set; }

        /// <summary>
        /// If true, auto-updates Napacks with newer minor versions. Implies <see cref="AutoUpdatePatch"/>
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public bool AutoUpdateMinor { get; set; }

        /// <summary>
        /// If true, emits a warning message for each Napack with a newer major version.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public bool WarnMajorUpdates { get; set; }

        /// <summary>
        /// The last time an auto-update was performed.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public DateTime LastUpdateTime { get; set; }
        
        /// <summary>
        /// The interval between auto-updates.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public TimeSpan AutoUpdateInterval { get; set; }
    }
}