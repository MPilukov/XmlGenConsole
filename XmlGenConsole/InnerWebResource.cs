namespace XmlGenConsole
{
    public class InnerODataResponse
    {
        public InnerWebResource[] Value { get; set; }
    }
    
    public class InnerWebResource
    {
        public InnerWebResource()
        {
            CanBeDeleted = new BoolDataObj();
            IsHidden = new BoolDataObj();
            IsCustomizable = new BoolDataObj();
        }
        
        public string WebResourceId { get; set; }
        public string Name { get; set; }
        
        public string DisplayName { get; set; }
        public string DependencyXml { get; set; }
        public string WebResourceType { get; set; }
        public string IntroducedVersion { get; set; }
        public bool IsEnabledForMobileClient { get; set; }
        public bool IsAvailableForMobileOffline { get; set; }

        public BoolDataObj CanBeDeleted { get; set; }
        public BoolDataObj IsHidden { get; set; }
        public BoolDataObj IsCustomizable { get; set; }
    }
    
    public class BoolDataObj
    {
        public BoolDataObj()
        {
            
        }
        
        public bool Value { get; set; }
    }
}