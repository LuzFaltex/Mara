using System.Collections.Generic;

namespace Mara.Common.Models
{
    public class MaraConfig
    {
        public string DiscordToken { get; init; }
        public Dictionary<string, string> ConnectionStrings { get; init; }
        public string PrivacyPolicyUrl { get; init; }
    }
}
