using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Metadata;

namespace Apps.Jira;

internal class JiraApplication : IApplication, ICategoryProvider
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

    public T GetInstance<T>()
    {
        throw new NotImplementedException();
    }
}