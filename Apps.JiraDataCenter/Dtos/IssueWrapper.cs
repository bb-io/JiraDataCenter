
using Apps.JiraDataCenter.Utils;
using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.Jira.Dtos;

public class IssuesWrapper
{
    [JsonProperty("startAt")]
    [DefinitionIgnore]
    public int StartAt { get; set; }

    [JsonProperty("maxResults")]
    [DefinitionIgnore]
    public int MaxResults { get; set; }

    [JsonProperty("total")]
    [DefinitionIgnore]
    public int Total { get; set; }
    public IEnumerable<IssueWrapper> Issues { get; set; }
}

public class IssueWrapper
{
    public string Key { get; set; }
    public IssueFields Fields { get; set; }
}

public class IssueFields
{
    public string Summary { get; set; }

    [JsonProperty("issuetype")]
    public IssueTypeSimpleDto IssueType { get; set; }

    public ProjectDto Project { get; set; }

    public PriorityDto Priority { get; set; }

    public StatusDto Status { get; set; }

    public UserDto? Assignee { get; set; }

    public UserDto? Reporter { get; set; }

    [JsonProperty("description")]
    public JToken? DescriptionRaw { get; set; }

    [JsonProperty("timespent"), Display("Time spent (in seconds)"), JsonConverter(typeof(NullableDoubleConverter))]
    public double TimeSpent { get; set; }

    [JsonProperty("aggregatetimespent"), Display("Aggregate time spent (in seconds)"), JsonConverter(typeof(NullableDoubleConverter))]
    public double AggregateTimeSpent { get; set; }

    [JsonProperty("timeoriginalestimate"), Display("Original estimate (in seconds)"), JsonConverter(typeof(NullableDoubleConverter))]
    public double OriginalEstimate { get; set; }

    [Display("Worklog")]
    public WorklogWrapper Worklog { get; set; } = new();

    public IEnumerable<AttachmentDto>? Attachment { get; set; }

    [JsonProperty("labels")]
    public List<string> Labels { get; set; } = new();

    [JsonProperty("subtasks")]
    public List<SubTaskWrapper> SubTasks { get; set; } = new();

    [JsonProperty("duedate")]
    public string? DueDate { get; set; }

    [JsonProperty("parent")]
    public IssueIdWrapper? Parent { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JToken> CustomFields { get; set; } = new();
}

public class IssueIdWrapper
{
    [JsonProperty("id")]
    public string Id { get; set; } = default!;

    [JsonProperty("key")]
    public string Key { get; set; } = default!;
}


public class Description
{
    public string Type { get; set; } = default!;

    public int Version { get; set; }

    public List<ContentElement> Content { get; set; } = new();
}

public class ContentElement
{
    public string Type { get; set; } = default!;

    public List<ContentElement>? Content { get; set; } = default!;

    public string Text { get; set; } = default!;

    public List<Mark> Marks { get; set; } = default!;

    public JObject? Attrs { get; set; } = default!;
}

public class Mark
{
    public string Type { get; set; } = default!;

    public JObject Attrs { get; set; } = default!;
}

public class ContentData
{
    public string Type { get; set; } = default!;
    public string Text { get; set; } = default!;
}

public class SubTaskWrapper
{
    [Display("Subtask ID")]
    public string Id { get; set; } = default!;

    [Display("Subtask key")]
    public string Key { get; set; } = default!;

    public SubTaskFields Fields { get; set; } = default!;
}

public class SubTaskFields
{
    [Display("Summary of subtask")]
    public string Summary { get; set; } = default!;
}

public class WorklogWrapper
{
    [JsonProperty("startAt"), DefinitionIgnore]
    [Display("Start at")]
    public int StartAt { get; set; }

    [JsonProperty("maxResults"), DefinitionIgnore]
    [Display("Max results")]
    public int MaxResults { get; set; }

    [JsonProperty("total")]
    [Display("Total")]
    public int Total { get; set; }

    [JsonProperty("worklogs")]
    [Display("Worklogs")]
    public List<WorklogDto> Worklogs { get; set; } = new();
}

public class WorklogDto
{
    [JsonProperty("id")]
    [Display("Worklog ID")]
    public string Id { get; set; } = default!;

    [JsonProperty("issueId")]
    [Display("Issue ID")]
    public string IssueId { get; set; } = default!;

    [JsonProperty("created")]
    [Display("Created")]
    public DateTime Created { get; set; }

    [JsonProperty("updated")]
    [Display("Updated")]
    public DateTime Updated { get; set; }

    [JsonProperty("started")]
    [Display("Started")]
    public DateTime Started { get; set; }

    [JsonProperty("timeSpent")]
    [Display("Time spent")]
    public string TimeSpent { get; set; } = default!;

    [JsonProperty("timeSpentSeconds")]
    [Display("Time spent (in seconds)")]
    public int TimeSpentSeconds { get; set; }

    [JsonProperty("author")]
    [Display("Worklog author")]
    public UserDto? Author { get; set; }
}