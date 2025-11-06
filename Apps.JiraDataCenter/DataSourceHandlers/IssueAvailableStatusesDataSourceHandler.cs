using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Responses;
using Apps.Jira;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.JiraDataCenter.DataSourceHandlers
{
    public class IssueAvailableStatusesDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
    {
        private readonly IssueIdentifier _issue;

        public IssueAvailableStatusesDataSourceHandler(
            InvocationContext invocationContext,
            [ActionParameter] IssueIdentifier issue) : base(invocationContext)
        {
            _issue = issue;
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_issue.IssueKey))
                throw new Exception("Please specify issue key first.");

            var req = new JiraRequest($"/issue/{_issue.IssueKey}/transitions", Method.Get);
            var resp = await Client.ExecuteWithHandling<TransitionsResponse>(req);

            var items = resp.Transitions
                .GroupBy(t => t.To.Id)
                .Select(g =>
                {
                    var first = g.First();
                    var label = first.To.Name;
                    return new { Id = first.To.Id, Label = label };
                });

            if (!string.IsNullOrWhiteSpace(context.SearchString))
            {
                var s = context.SearchString.Trim();
                items = items.Where(x =>
                    x.Id.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    x.Label.Contains(s, StringComparison.OrdinalIgnoreCase));
            }

            return items
                .OrderBy(x => x.Label, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Id, x => x.Label);
        }
    }
}
