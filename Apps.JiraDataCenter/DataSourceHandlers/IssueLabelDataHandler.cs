using Apps.Jira;
using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.JiraDataCenter.DataSourceHandlers
{
    public class IssueLabelDataHandler(InvocationContext invocationContext) : JiraInvocable(invocationContext), IAsyncDataSourceHandler
    {
        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
        {
            var request = new JiraRequest("/jql/autocompletedata/suggestions", Method.Get);
            request.AddQueryParameter("fieldName", "labels");

            var q = context.SearchString?.Trim();
            if (!string.IsNullOrEmpty(q))
                request.AddQueryParameter("fieldValue", q);

            var resp = await Client.ExecuteWithHandling<AutoCompleteResultWrapper>(request);

            var labels = (resp?.Results ?? new())
                .Select(r => r.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!string.IsNullOrEmpty(q))
                labels = labels
                    .Where(x => x.Contains(q, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            return labels.ToDictionary(l => l, l => l, StringComparer.OrdinalIgnoreCase);
        }

        private class AutoCompleteResultWrapper
        {
            [JsonProperty("results")]
            public List<AutoCompleteResult> Results { get; set; } = new();
        }

        private class AutoCompleteResult
        {
            [JsonProperty("value")]
            public string? Value { get; set; }

            [JsonProperty("displayName")]
            public string? DisplayName { get; set; }
        }
    }

}
