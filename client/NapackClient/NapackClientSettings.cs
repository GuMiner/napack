using System;

namespace NapackClient
{
    /// <summary>
    /// Defines the napack client settings.
    /// </summary>
    public class NapackClientSettings
    {
        /// <summary>
        /// The server hosting Napacks.
        /// </summary>
        public Uri NapackFrameworkServer { get; set; }

        /// <summary>
        /// Allows the inclusion of commercially-licensed napacks.
        /// </summary>
        public bool AllowCommercial { get; set; }
        
        /// <summary>
        /// Allows the inclusion of copy-left napacks.
        /// </summary>
        public bool AllowCopyLeft { get; set; }
        
        /// <summary>
        /// Allows the inclusion of custom-licensed napacks.
        /// </summary>
        public bool AllowCustomLicenses { get; set; }
        
        /// <summary>
        /// If true, auto-updates Napacks with newer patch versions.
        /// </summary>
        public bool AutoUpdatePatch { get; set; }
        
        /// <summary>
        /// If true, auto-updates Napacks with newer minor versions. Implies <see cref="AutoUpdatePatch"/>
        /// </summary>
        public bool AutoUpdateMinor { get; set; }
        
        /// <summary>
        /// If true, emits a warning message for each Napack with a newer major version.
        /// </summary>
        public bool WarnMajorUpdates { get; set; }
        
        /// <summary>
        /// The last time an auto-update was performed.
        /// </summary>
        public DateTime LastUpdateTime { get; set; }
        
        /// <summary>
        /// The interval between auto-updates.
        /// </summary>
        public TimeSpan AutoUpdateInterval { get; set; }
    }
}