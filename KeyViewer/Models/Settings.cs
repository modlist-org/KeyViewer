using JSON;
using KeyViewer.Core.Interfaces;
using KeyViewer.Utils;
using System.Collections.Generic;
using System.Linq;

namespace KeyViewer.Models
{
    public class Settings : IModel, ICopyable<Settings>
    {
        public string Lang = "English";
        public bool useLegacyTheme = false;
        public List<ActiveProfile> ActiveProfiles = new List<ActiveProfile>();
        public JsonNode Serialize()
        {
            var node = JsonNode.Empty;
            node[nameof(Lang)] = Lang;
            node[nameof(ActiveProfiles)] = ModelUtils.WrapCollection(ActiveProfiles);

            return node;
        }
        public void Deserialize(JsonNode node)
        {
            Lang = node[nameof(Lang)];
            ActiveProfiles = ModelUtils.UnwrapList<ActiveProfile>(node[nameof(ActiveProfiles)].AsArray);
        }
        public Settings Copy()
        {
            var newSettings = new Settings();
            newSettings.Lang = Lang;
            newSettings.ActiveProfiles = ActiveProfiles.Select(p => p.Copy()).ToList();
            return newSettings;
        }
    }
}
