using System;
using System.Collections.Generic;
using System.Linq;

namespace Infusion.Trading.MarketData
{
    public sealed class SecurityIdCollection : HashSet<string>
    {
        public SecurityIdCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public static IEnumerable<string> TryParse(params string[] securityIds)
        {
            return from id in securityIds
                   where id != null
                   let trimmedId = id.Trim()
                   where 0 < trimmedId.Length && trimmedId.Length <= 5
                      && trimmedId.Cast<char>().All(char.IsLetter)
                   select trimmedId;
        }

        public IEnumerable<string> TryAddRange(string[] securityIds)
        {
            foreach (var id in TryParse(securityIds))
            {
                Add(id);

                yield return id;
            }
        }
    }
}
