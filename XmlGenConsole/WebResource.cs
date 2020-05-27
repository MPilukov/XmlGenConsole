using System;

namespace XmlGenConsole
{
    [Serializable]
    public class WebResource
    {
        public string WebResourceId { get; set; }
        public string Name { get; set; }
        
        public string DisplayName { get; set; }
        public string DependencyXml { get; set; }
        public string WebResourceType { get; set; }
        public string IntroducedVersion { get; set; }
        public byte IsEnabledForMobileClient { get; set; }
        public byte IsAvailableForMobileOffline { get; set; }

        public byte CanBeDeleted { get; set; }
        public byte IsHidden { get; set; }
        public byte IsCustomizable { get; set; }
    }
}