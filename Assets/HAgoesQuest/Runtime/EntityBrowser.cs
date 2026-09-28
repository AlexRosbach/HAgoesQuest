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
                case "light": return "Lampen";
                case "switch": return "Schalter";
                case "sensor": return "Sensoren";
                case "binary_sensor": return "Binärsensoren";
                default: return domain;
            }
        }
    }
}
