using System.Collections.Generic;
using System.Xml.Serialization;

namespace KeyViewer.Migration.V3
{
    [XmlRoot("Settings")]
    public class V3Settings
    {
        public int ProfileIndex = 0;
        [XmlArrayItem("Profile")]
        public List<V3Profile> Profiles = new List<V3Profile>();
        public int BackupInterval = 10;
    }
}
