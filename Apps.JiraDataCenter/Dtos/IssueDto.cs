using Apps.Jira.Utils;
using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json.Linq;

namespace Apps.Jira.Dtos;

public class IssueDto
{
    public IssueDto() { }

    public IssueDto(IssueWrapper issueWrapper)
    {
        IssueKey = issueWrapper.Key;
        Summary = issueWrapper?.Fields?.Summary ?? string.Empty;
        Status = issueWrapper.Fields.Status ?? null;
        Priority = issueWrapper.Fields.Priority ?? new PriorityDto();
        Assignee = issueWrapper.Fields.Assignee;
        Reporter = issueWrapper.Fields.Reporter;
        Project = issueWrapper.Fields.Project ?? new ProjectDto();
        Description = ConvertDescriptionToString(issueWrapper?.Fields?.DescriptionRaw);
        Labels = issueWrapper.Fields.Labels;
        SubTasks = issueWrapper.Fields.SubTasks?
            .Select(subTask => new SubTaskDto
            {
                Id = subTask.Id,
                Key = subTask.Key,
                Summary = subTask.Fields.Summary
            }).ToList() ?? new List<SubTaskDto>();
        if (!string.IsNullOrEmpty(issueWrapper.Fields.DueDate) && DateTime.TryParse(issueWrapper.Fields.DueDate, out var dueDate))
        {
            DueDate = dueDate;
        }
        else
        {
            DueDate = DateTime.MinValue;
        }
    }
        
    [Display("Issue key")]
    public string IssueKey { get; set; }
        
    public string Summary { get; set; }
        
    public string? Description { get; set; }

    public StatusDto? Status { get; set; }
        
    public PriorityDto? Priority { get; set; }
        
    public ProjectDto? Project { get; set; }

    public UserDto? Assignee { get; set; }

    public UserDto? Reporter { get; set; }

    public List<string>? Labels { get; set; } = new();
    
    [Display("Subtasks info")]
    public List<SubTaskDto>? SubTasks { get; set; } = new();

    [Display("Due date")]
    public DateTime? DueDate { get; set; }

    private static string? ConvertDescriptionToString(JToken? raw)
    {
        if (raw is null || raw.Type == JTokenType.Null)
            return null;

        if (raw.Type == JTokenType.String)
            return raw.Value<string>();

        try
        {
            var adf = raw.ToObject<Apps.Jira.Dtos.Description>();
            return adf is null ? null : JiraDocToMarkdownConverter.ConvertToMarkdown(adf);
        }
        catch
        {
            return raw.ToString(Newtonsoft.Json.Formatting.None);
        }
    }
}

public class SubTaskDto
{
    [Display("Subtask ID")]
    public string Id { get; set; } = default!;

    [Display("Subtask key")]
    public string Key { get; set; } = default!;

    [Display("Subtask summary")]
    public string Summary { get; set; } = default!;
}

