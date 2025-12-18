using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.JiraDataCenter.Models.Identifiers;

public class TargetIssueIdentifier
{
    [Display("Target issue key")]
    [DataSource(typeof(IssueDataSourceHandler))]
    public string TargetIssueKey { get; set; }
}
