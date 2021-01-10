namespace TagFinder.VersionManager
{
    /// <summary>
    /// Contains information about app version.
    /// </summary>
    public class VersionInfo
    {
        public string Version { get; set; }
        public string Changelog { get; set; }
        public bool ShouldUpdateManually { get; set; }
    }
}
