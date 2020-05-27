namespace XmlGenConsole
{
    public class WebResource
    {
        public string WebResourceId { get; set; }
        public string Name { get; set; }
        
        public string DisplayName { get; set; }
        public string DependencyXml { get; set; }
        public string WebResourceType { get; set; }
        public string IntroducedVersion { get; set; }
        public bool IsEnabledForMobileClient { get; set; }
        public bool IsAvailableForMobileOffline { get; set; }

        public bool CanBeDeleted { get; set; }
        public bool IsHidden { get; set; }
        public bool IsCustomizable { get; set; }
    }
}