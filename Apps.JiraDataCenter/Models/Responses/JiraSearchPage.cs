using Newtonsoft.Json;

namespace Apps.JiraDataCenter.Models.Responses
{
    public class JiraSearchPage<T>
    {
        [JsonProperty("startAt")]
        public int StartAt { get; set; }
        [JsonProperty("maxResults")]
        public int MaxResults { get; set; }
        [JsonProperty("total")]
        public int Total { get; set; }
        [JsonProperty("issues")]
        public List<T> Issues { get; set; } = new();
    }
}
