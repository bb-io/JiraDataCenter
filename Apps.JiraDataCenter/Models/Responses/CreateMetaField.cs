using Newtonsoft.Json.Linq;

namespace Apps.JiraDataCenter.Models.Responses
{
    public class CreateMetaField
    {
        public string? Name { get; set; }
        public string? Key { get; set; }
        public string? SchemaType { get; set; }
        public string? SchemaItems { get; set; }
        public string? Custom { get; set; }
        public bool Required { get; set; }
        public JArray? AllowedValues { get; set; }
        public List<string>? Operations { get; set; }
    }
}
