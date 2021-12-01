using System.Collections.Generic;

namespace Mara.Common.Models
{
    public sealed class MaraConfig
    {
        public static readonly MaraConfig Default = new()
        {
            DiscordToken = "",
            ConnectionStrings = new() {["DbConnection"] = ""},
            PrivacyPolicyUrl = ""
        };

        public string DiscordToken { get; init; } = "";
        public Dictionary<string, string> ConnectionStrings { get; init; } = new();
        public string PrivacyPolicyUrl { get; init; } = "";
    }
}
