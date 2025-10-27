using Blackbird.Applications.Sdk.Common.Metadata;

namespace Apps.Jira;

internal class JiraApplication : ICategoryProvider
{
    public IEnumerable<ApplicationCategory> Categories
    {
        get => [ApplicationCategory.ProjectManagementAndProductivity, ApplicationCategory.TaskManagement];
        set { }
    }


    public string Name
    {
        get => "Jira Data Center";
        set { }
    }
}