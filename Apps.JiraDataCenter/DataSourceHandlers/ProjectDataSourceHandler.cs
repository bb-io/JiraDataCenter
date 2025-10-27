using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers;

public class ProjectDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
{
    public ProjectDataSourceHandler(InvocationContext invocationContext): base(invocationContext) { }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var request = new JiraRequest("/project", Method.Get);
        request.AddQueryParameter("maxResults", "50");
        var all = await Client.ExecuteWithHandling<List<ProjectDto>>(request) ?? new List<ProjectDto>();

        IEnumerable<ProjectDto> filtered = all;

        var query = (context.SearchString ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(query))
        {
            var tokens = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            filtered = all
                .Select(p =>
                {
                    var name = p.Name ?? string.Empty;
                    var key = p.Key ?? string.Empty;

                    var lname = name.ToLowerInvariant();
                    var lkey = key.ToLowerInvariant();
                    int score = 0;
                    foreach (var t in tokens)
                    {
                        var lt = t.ToLowerInvariant();

                        if (lkey.StartsWith(lt)) score += 4;
                        else if (lname.StartsWith(lt)) score += 3;
                        else
                        {
                            if (lkey.Contains(lt)) score += 2;
                            if (lname.Contains(lt)) score += 1;
                        }
                    }

                    return new { Project = p, Score = score };
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => (x.Project.Key ?? "").Length)
                .ThenBy(x => x.Project.Name)
                .Select(x => x.Project);
        }
        var list = filtered
            .Where(p => !string.IsNullOrWhiteSpace(p.Key))
            .DistinctBy(p => p.Key)
            .Take(20)
            .ToList();

        return list.ToDictionary(p => p.Key, p => p.Name);
    }
}