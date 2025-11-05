using Newtonsoft.Json.Linq;

namespace Apps.JiraDataCenter.Models.Requests
{
    public class CreateMetaValue
    {
        public string FieldId { get; init; } = default!;
        public bool Required { get; init; }
        public string? SchemaType { get; init; } 
        public string? SchemaItems { get; init; }
        public List<string> Operations { get; init; } = new();
        public JArray? AllowedValues { get; init; }
        public JObject? DefaultValue { get; init; }
    }
}
