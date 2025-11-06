using Newtonsoft.Json;

namespace Apps.Jira.Dtos;

public class TransitionDto
{
    [JsonProperty("id")]
    public string Id { get; set; } = default!;

    [JsonProperty("name")]
    public string Name { get; set; } = default!;

    [JsonProperty("to")]
    public StatusDto? To { get; set; }
}