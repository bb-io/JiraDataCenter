using RestSharp;
using Apps.Jira;
using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.JiraDataCenter.DataSourceHandlers.CustomFields;

public class CustomLinkFieldDataSourceHandler(InvocationContext context) 
    : JiraInvocable(context), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken ct)
    {
        var request = new JiraRequest("/field", Method.Get);
        var fields = await Client.ExecuteWithHandling<IEnumerable<FieldDto>>(request);

        var linkFields = fields
            .Where(field => field.Custom)
            .Where(field =>
            {
                if (field.Schema == null) 
                    return false;

                var customType = field.Schema.Custom ?? "";
                return
                    customType.Contains("gh-epic-link") ||
                    customType.Contains("jpo-custom-field-parent");
            })
            .Where(field => 
                context.SearchString == null ||
                field.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase)
            );

        return linkFields.Select(x => new DataSourceItem(x.Id, x.Name));
    }
}
