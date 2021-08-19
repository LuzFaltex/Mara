using System.Collections.Generic;

namespace Mara.Common.Models
{
    public class MaraConfig
    {
        public static readonly MaraConfig Default = new()
        {
            DiscordToken = "",
            ConnectionStrings = new() {["DbConnection"] = ""},
            PrivacyPolicyUrl = ""
        };

        public string DiscordToken { get; init; }
        public Dictionary<string, string> ConnectionStrings { get; init; }
        public string PrivacyPolicyUrl { get; init; }
    }
}
