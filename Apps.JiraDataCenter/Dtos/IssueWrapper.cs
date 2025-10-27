﻿
using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.Jira.Dtos;

public class IssuesWrapper
{
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

    public Description? Description { get; set; }
        
    public IEnumerable<AttachmentDto>? Attachment { get; set; }

    [JsonProperty("labels")]
    public List<string> Labels { get; set; } = new ();

    [JsonProperty("subtasks")]
    public List<SubTaskWrapper> SubTasks { get; set; } = new();

    [JsonProperty("duedate")]
    public string? DueDate { get; set; }
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