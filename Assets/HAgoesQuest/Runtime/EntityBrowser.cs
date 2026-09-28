using System;
using System.Collections.Generic;
using System.Linq;
using HAgoesQuest.Core;

namespace HAgoesQuest
{
    public sealed class EntityBrowser
    {
        public string Search = "", Domain = "", Area = "";
        public IEnumerable<HaEntity> Apply(IEnumerable<HaEntity> entities) => entities.Where(e =>
            (Domain.Length == 0 || EntityPolicy.Domain(e.Id) == Domain) &&
            (Area.Length == 0 || (Area == "__none" ? string.IsNullOrEmpty(e.AreaId) : e.AreaId == Area)) &&
            (Search.Length == 0 || (e.Name ?? "").IndexOf(Search.Trim(), StringComparison.OrdinalIgnoreCase) >= 0 ||
                e.Id.IndexOf(Search.Trim(), StringComparison.OrdinalIgnoreCase) >= 0))
            .OrderBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase).ThenBy(e => e.Id);

        public static string DomainName(string domain)
        {
            switch (domain)
            {
                case "light": return ExperienceSettings.English?"Lights":"Lampen";
                case "camera": return ExperienceSettings.English?"Cameras":"Kameras";
                case "image": return ExperienceSettings.English?"Images":"Bilder";
                case "media_player": return ExperienceSettings.English?"Media players":"Mediengeräte";
                case "climate": return ExperienceSettings.English?"Climate":"Heizung / Klima";
                case "cover": return ExperienceSettings.English?"Covers":"Rollläden";
                case "fan": return ExperienceSettings.English?"Fans":"Ventilatoren";
                case "vacuum": return ExperienceSettings.English?"Vacuums":"Saugroboter";
                case "lock": return ExperienceSettings.English?"Locks":"Schlösser";
                case "person": return ExperienceSettings.English?"People":"Personen";
                case "weather": return ExperienceSettings.English?"Weather":"Wetter";
                case "automation": return ExperienceSettings.English?"Automations":"Automationen";
                case "scene": return ExperienceSettings.English?"Scenes":"Szenen";
                case "switch": return L10n.T("Schalter");
                case "sensor": return L10n.T("Sensoren");
                case "binary_sensor": return L10n.T("Binärsensoren");
                default: return domain;
            }
        }
    }
}
