using Apps.Jira.DataSourceHandlers;
using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Apps.Jira.Models.Responses;
using Apps.Jira.Utils;
using Apps.Jira.Webhooks.Payload;
using Apps.JiraDataCenter.Models.Requests;
using Apps.JiraDataCenter.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RestSharp;
using System.Globalization;
using System.Text.RegularExpressions;
using Method = RestSharp.Method;
namespace Apps.Jira.Actions;

[ActionList]
public class IssueActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : JiraInvocable(invocationContext)
{
    #region GET

    [Action("Get issue", Description = "Get the specified issue.")]
    public async Task<IssueDto> GetIssueByKey([ActionParameter] IssueIdentifier input)
    {
        if (input == null || string.IsNullOrEmpty(input.IssueKey))
        {
            throw new PluginMisconfigurationException("IssueKey can not be null or empty.");
        }

        var request = new JiraRequest($"/issue/{input.IssueKey}", Method.Get);
        var issue = await Client.ExecuteWithHandling<IssueWrapper>(request);
        return new IssueDto(issue);
    }

    [Action("Search issues", Description =
        "Returns issues that meet the provided criteria.")]
    public async Task<IssuesResponse> ListRecentlyCreatedIssues(
        [ActionParameter] ProjectIdentifier project,
        [ActionParameter] ListRecentlyCreatedIssuesRequest listRequest,
        [ActionParameter][Display("Custom JQL conditions")] string? customJql)
    {
        var jqlConditions = new List<string> { $"project={project.ProjectKey}" };

        if (listRequest.Hours.HasValue)
            jqlConditions.Add($"created >= -{listRequest.Hours}h");

        if (listRequest.Labels?.Any() == true)
        {
            var labels = '"' + string.Join("\", \"", listRequest.Labels.Where(l => !string.IsNullOrWhiteSpace(l))) + '"';
            jqlConditions.Add($"labels in ({labels})");
        }

        if (listRequest.Versions?.Any() == true)
        {
            var versions = '"' + string.Join("\", \"", listRequest.Versions.Where(v => !string.IsNullOrWhiteSpace(v))) + '"';
            jqlConditions.Add($"fixVersion in ({versions})");
        }

        if (!string.IsNullOrWhiteSpace(listRequest.ParentIssue))
            jqlConditions.Add($"parent = {listRequest.ParentIssue.Trim()}");

        if (!string.IsNullOrWhiteSpace(customJql))
            jqlConditions.Add($"({customJql})");

        var jql = string.Join(" AND ", jqlConditions);

        const int pageSize = 50;
        var startAt = 0;
        var allIssues = new List<IssueWrapper>();

        while (true)
        {
            var req = new JiraRequest("/search", Method.Get);
            req.AddQueryParameter("jql", jql);
            req.AddQueryParameter("fields", "id,key,summary,status,priority,assignee,reporter,project,description,labels,subtasks,duedate,parent");
            req.AddQueryParameter("validateQuery", "false");
            req.AddQueryParameter("maxResults", pageSize.ToString());
            req.AddQueryParameter("startAt", startAt.ToString());

            var page = await Client.ExecuteWithHandling<JiraSearchPage<IssueWrapper>>(req);

            if (page?.Issues?.Count > 0)
                allIssues.AddRange(page.Issues);

            if (page == null || page.Issues.Count == 0 || startAt + page.Issues.Count >= page.Total)
                break;

            startAt += page.Issues.Count;
        }

        return new IssuesResponse
        {
            Issues = allIssues.Select(i => new IssueDto(i)),
            Count = allIssues.Count
        };
    }

    [Action("Find issue", Description = "Find the first issue that matches given conditions. Allows appending custom JQL conditions.")]
    public async Task<IssueDto> FindIssue(
    [ActionParameter] [Display("Parent issue")][DataSource(typeof(IssueDataSourceHandler))]string? parentIssue,
    [ActionParameter] [Display("Summary")] string? issueName,
    [ActionParameter] ProjectIdentifier project,
    [ActionParameter] [Display("Custom JQL conditions")] string? customJql = null)
    {
        var jqlConditions = new List<string>();

        if (project != null && !string.IsNullOrWhiteSpace(project.ProjectKey))
            jqlConditions.Add($"project={project.ProjectKey}");

        if (!string.IsNullOrWhiteSpace(parentIssue))
            jqlConditions.Add($"(parent = {parentIssue.Trim()} OR \"Epic Link\" = {parentIssue.Trim()})");

        if (!string.IsNullOrWhiteSpace(issueName))
            jqlConditions.Add($"summary ~ \"{issueName.Trim()}\"");

        if (!string.IsNullOrWhiteSpace(customJql))
            jqlConditions.Add($"({customJql})");

        var jql = string.Join(" AND ", jqlConditions);

        var request = new JiraRequest("/search", Method.Get);
        request.AddQueryParameter("jql", jql);
        request.AddQueryParameter("fields", "id,key,summary,status,priority,assignee,reporter,project,description,labels,subtasks,duedate,parent");
        request.AddQueryParameter("maxResults", "1");
        request.AddQueryParameter("validateQuery", "false");

        var issues = await Client.ExecuteWithHandling<IssuesWrapper>(request);

        var firstIssue = issues.Issues.FirstOrDefault();
        return firstIssue != null ? new IssueDto(firstIssue) : null;
    }

    [Action("List attachments", Description = "List files attached to an issue.")]
    public async Task<AttachmentsResponse> ListAttachments([ActionParameter] IssueIdentifier issue)
    {
        var request = new JiraRequest($"/issue/{issue.IssueKey}", Method.Get);
        var result = await Client.ExecuteWithHandling<IssueWrapper>(request);
        var attachments = result.Fields.Attachment ?? new AttachmentDto[] { };
        return new AttachmentsResponse { Attachments = attachments };
    }

    [Action("Download attachment", Description = "Download an attachment.")]
    public async Task<DownloadAttachmentResponse> DownloadAttachment([ActionParameter] AttachmentIdentifier attachment)
    {
        var request = new JiraRequest($"/attachment/content/{attachment.AttachmentId}", Method.Get);
        var response = await Client.ExecuteWithHandling(request);
        var filename = response.ContentHeaders.First(h => h.Name == "Content-Disposition").Value.ToString()
            .Split('"')[1];
        var contentType = response.ContentHeaders.First(h => h.Name == "Content-Type").Value.ToString();

        using var stream = new MemoryStream(response.RawBytes);
        var file = await fileManagementClient.UploadAsync(stream, contentType, filename);
        return new DownloadAttachmentResponse { Attachment = file };
    }

    [Action("Get issue type details", Description = "Get issue type details by name")]
    public async Task<IssueTypeDto> GetIssueTypeDetails([ActionParameter] ProjectIdentifier projectIdentifier,
        [Display("Type name")] [ActionParameter] string TypeName)
    {
        var getProjectRequest = new JiraRequest($"/project/{projectIdentifier.ProjectKey}", Method.Get);
        var project = await Client.ExecuteWithHandling<ProjectDto>(getProjectRequest);

        var getIssueTypesRequest = new JiraRequest("/issuetype", Method.Get);
        var issueTypes = await Client.ExecuteWithHandling<IEnumerable<IssueTypeDto>>(getIssueTypesRequest);
        try
        {
            return issueTypes.Where(type =>
                    type.Scope is null || type.Scope?.Type == "PROJECT" && type.Scope.Project!.Id == project.Id)
                .First(x => x.Name.ToLower() == TypeName.ToLower());
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region POST

    [Action("Create issue", Description = "Create a new issue.")]
    public async Task<CreatedIssueDto> CreateIssue([ActionParameter] ProjectIdentifier project,
        [ActionParameter] CreateIssueRequest input)
    {
        var projectRequest = new JiraRequest($"/project/{project.ProjectKey}", Method.Get);
        var projectResponse = await Client.ExecuteWithHandling<DetailedProjectDto>(projectRequest);
        var validIssueTypes = projectResponse.IssueTypes.Select(t => t.Id).ToList();
        if (!validIssueTypes.Contains(input.IssueTypeId))
        {
            throw new PluginMisconfigurationException($"Invalid issue type ID: {input.IssueTypeId}. Valid issue type IDs for project {project.ProjectKey} are: {string.Join(", ", validIssueTypes)}");
        }

        var fields = new Dictionary<string, object>
        {
            { "project",   new { key = project.ProjectKey } },
            { "summary",   input.Summary },
            { "description", string.IsNullOrEmpty(input.Description) ? null : input.Description },
            { "issuetype", new { id = input.IssueTypeId } }
        };

        var accountId = input.AccountId;
        if (!string.IsNullOrEmpty(accountId))
        {
            fields.Add("assignee", new { id = accountId });
        }

        if (!string.IsNullOrEmpty(input.OriginalEstimate))
        {
            fields.Add("timetracking", new { originalEstimate = input.OriginalEstimate});
        }

        if (input.DueDate.HasValue)
        {
            fields.Add("duedate", input.DueDate.Value.ToString("yyyy-MM-dd"));
        }

        if (!string.IsNullOrEmpty(input.Reporter))
        {
            fields.Add("reporter", new { id = input.Reporter });
        }

        if (!string.IsNullOrEmpty(input.ParentIssueKey))
            fields.Add("parent", new { key = input.ParentIssueKey });

        var request = new JiraRequest("/issue", Method.Post).AddJsonBody(new
        {
            fields = fields
        });

        var createdIssue = await Client.ExecuteWithHandling<CreatedIssueDto>(request);
        return createdIssue;
    }

    [Action("Clone issue", Description = "Jira Data Center/Server: clone an issue copying summary, description, labels, assignee, reporter, due date, parent, and all creatable custom fields. Links new issue to source as 'Cloners'. Optionally copy status.")]
    public async Task<IssueDto> CloneIssue([ActionParameter] IssueIdentifier sourceIssue,
    [ActionParameter] CloneIssueRequest input)
    {
        var getWrapReq = new JiraRequest($"/issue/{sourceIssue.IssueKey}", Method.Get);
        var original = await Client.ExecuteWithHandling<IssueWrapper>(getWrapReq);

        var getRawReq = new JiraRequest($"/issue/{sourceIssue.IssueKey}", Method.Get);
        var originalRaw = await Client.ExecuteWithHandling<JObject>(getRawReq);
        var originalFields = (JObject?)originalRaw["fields"] ?? new JObject();

        var projectKey = original.Fields.Project.Key;
        var issueTypeId = original.Fields.IssueType.Id;

        var meta = await GetCreateMetaValuesDc(projectKey, issueTypeId);

        var baseSystem = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "project", "issuetype" };

        var fields = new JObject
        {
            ["project"] = new JObject { ["key"] = projectKey },
            ["issuetype"] = new JObject { ["id"] = issueTypeId }
        };

        var summaryMeta = meta.FirstOrDefault(m => m.FieldId.Equals("summary", StringComparison.OrdinalIgnoreCase));
        if (summaryMeta?.Operations.Contains("set", StringComparer.OrdinalIgnoreCase) == true)
        {
            fields["summary"] = string.IsNullOrWhiteSpace(input.NewSummary)
                ? original.Fields.Summary
                : input.NewSummary;
        }

        var descrMeta = meta.FirstOrDefault(m => m.FieldId.Equals("description", StringComparison.OrdinalIgnoreCase));
        if (descrMeta?.Operations.Contains("set", StringComparer.OrdinalIgnoreCase) == true)
        {
            if (!string.IsNullOrWhiteSpace(input.NewDescription))
                fields["description"] = input.NewDescription;
            else if (originalFields.TryGetValue("description", out var od) && od is not null && od.Type != JTokenType.Null)
                fields["description"] = od.DeepClone();
        }

        var assigneeMeta = meta.FirstOrDefault(m => m.FieldId.Equals("assignee", StringComparison.OrdinalIgnoreCase));
        if (assigneeMeta?.Operations.Contains("set", StringComparer.OrdinalIgnoreCase) == true
            && !string.IsNullOrWhiteSpace(input.AssigneeName))
        {
            fields["assignee"] = new JObject { ["name"] = input.AssigneeName };
        }

        var reporterMeta = meta.FirstOrDefault(m => m.FieldId.Equals("reporter", StringComparison.OrdinalIgnoreCase));
        if (reporterMeta?.Operations.Contains("set", StringComparer.OrdinalIgnoreCase) == true)
        {
            if (!string.IsNullOrWhiteSpace(input.ReporterName))
                fields["reporter"] = new JObject { ["name"] = input.ReporterName };
            else if (original.Fields.Reporter?.Name is string rep && !string.IsNullOrWhiteSpace(rep))
                fields["reporter"] = new JObject { ["name"] = rep };
        }

        var dueMeta = meta.FirstOrDefault(m => m.FieldId.Equals("duedate", StringComparison.OrdinalIgnoreCase));
        if (dueMeta?.Operations.Contains("set", StringComparer.OrdinalIgnoreCase) == true)
        {
            var due = (input.NewDueDate ?? DateTime.UtcNow.Date.AddDays(7)).ToString("yyyy-MM-dd");
            fields["duedate"] = due;
        }

        var skipNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "rank","rankBeforeIssue","rankAfterIssue",
            "attachment","votes","watches","worklog","comment",
            "timeoriginalestimate","timeestimate","timespent",
            "aggregatetimeoriginalestimate","aggregatetimeestimate","aggregatetimespent",
            "timetracking","lastViewed","created","updated","resolution","resolutiondate",
            "statuscategorychangedate","creator","progress","aggregateprogress"
        };

        foreach (var m in meta)
        {
            var key = m.FieldId;
            if (baseSystem.Contains(key)) continue;
            if (fields.ContainsKey(key)) continue;
            if (m.Operations == null || !m.Operations.Contains("set", StringComparer.OrdinalIgnoreCase)) continue;
            if (skipNames.Contains(key) || key.IndexOf("rank", StringComparison.OrdinalIgnoreCase) >= 0) continue;

            if (!originalFields.TryGetValue(key, out var srcTok) || srcTok is null || srcTok.Type == JTokenType.Null)
                continue;

            var normalized = NormalizeForCreate(m, srcTok);
            if (normalized is null) continue;

            fields[key] = JToken.FromObject(normalized);
        }

        var labelsMeta = meta.FirstOrDefault(m => m.FieldId.Equals("labels", StringComparison.OrdinalIgnoreCase));
        if (labelsMeta?.Operations.Contains("set", StringComparer.OrdinalIgnoreCase) == true
            && original.Fields.Labels?.Any() == true
            && !fields.ContainsKey("labels"))
        {
            fields["labels"] = new JArray(original.Fields.Labels);
        }

        foreach (var m in meta.Where(x => x.Required))
        {
            if (fields.ContainsKey(m.FieldId)) continue;

            if (m.DefaultValue is JObject defObj)
            {
                var fill = NormalizeDefaultOrSingleAllowed(m, defObj);
                if (fill != null) { fields[m.FieldId] = JToken.FromObject(fill); continue; }
            }

            if (m.AllowedValues?.Count == 1 && m.AllowedValues[0] is JObject only)
            {
                var fill = NormalizeDefaultOrSingleAllowed(m, only);
                if (fill != null) { fields[m.FieldId] = JToken.FromObject(fill); continue; }
            }
        }

        var body = new JObject { ["fields"] = fields };
        StripRankFields(body);

        var createReq = new JiraRequest("/issue", Method.Post).WithJsonBody(body);
        var created = await Client.ExecuteWithHandling<CreatedIssueDto>(createReq);

        await LinkIssuesDc(
            created.Key,
            sourceIssue.IssueKey,
            string.IsNullOrWhiteSpace(input.LinkTypeName) ? "Cloners" : input.LinkTypeName,
            input.Comment
        );

        if (input.CopyStatus == true)
            await TransitionIssueToStatusDc(created.Key, original.Fields.Status.Id);

        return await GetIssueByKey(new IssueIdentifier { IssueKey = created.Key });
    }

    private async Task<List<CreateMetaValue>> GetCreateMetaValuesDc(string projectKey, string issueTypeId)
    {
        var req = new JiraRequest($"/issue/createmeta/{projectKey}/issuetypes/{issueTypeId}", Method.Get);
        var resp = await Client.ExecuteWithHandling<JObject>(req);

        var values = resp["values"] as JArray ?? new JArray();
        var list = new List<CreateMetaValue>(values.Count);

        foreach (var v in values.OfType<JObject>())
        {
            var cm = new CreateMetaValue
            {
                FieldId = v["fieldId"]?.ToString() ?? v["key"]?.ToString() ?? "",
                Required = v["required"]?.Value<bool>() ?? false,
                SchemaType = v.SelectToken("schema.type")?.ToString(),
                SchemaItems = v.SelectToken("schema.items")?.ToString(),
                AllowedValues = v["allowedValues"] as JArray,
                DefaultValue = v["defaultValue"] as JObject,
                Operations = v["operations"]?.Values<string>().ToList() ?? new List<string>()
            };
            if (!string.IsNullOrWhiteSpace(cm.FieldId))
                list.Add(cm);
        }
        return list;
    }

    private static object? NormalizeForCreate(CreateMetaValue meta, JToken src)
    {
        if (string.Equals(meta.SchemaType, "date", StringComparison.OrdinalIgnoreCase))
        {
            return NextWeekDate();
        }

        if (string.Equals(meta.SchemaType, "option", StringComparison.OrdinalIgnoreCase))
        {
            var id = TryMapOptionId(src, meta.AllowedValues);
            return id != null ? new JObject { ["id"] = id } : null;
        }

        if (string.Equals(meta.SchemaType, "array", StringComparison.OrdinalIgnoreCase)
            && string.Equals(meta.SchemaItems, "option", StringComparison.OrdinalIgnoreCase))
        {
            if (src is not JArray arr) return null;
            var ids = new JArray();
            foreach (var it in arr)
            {
                var id = TryMapOptionId(it, meta.AllowedValues);
                if (id != null) ids.Add(new JObject { ["id"] = id });
            }
            return ids.Count > 0 ? ids : null;
        }

        if (src is JValue v) return v.Value;

        return src.ToObject<object>();
    }

    private static string? TryMapOptionId(JToken valueToken, JArray? allowed)
    {
        if (valueToken is JObject vo)
        {
            var id = vo["id"]?.ToString();
            if (!string.IsNullOrEmpty(id)) return id;

            var byVal = vo["value"]?.ToString();
            if (!string.IsNullOrEmpty(byVal))
                return FindAllowedIdByValue(byVal, allowed);
        }

        if (valueToken is JValue jv && jv.Type == JTokenType.String)
            return FindAllowedIdByValue(jv.ToString(), allowed);

        return null;

        static string? FindAllowedIdByValue(string value, JArray? allowed)
        {
            if (allowed == null) return null;
            foreach (var a in allowed.OfType<JObject>())
            {
                var val = a["value"]?.ToString();
                if (string.Equals(val, value, StringComparison.OrdinalIgnoreCase))
                    return a["id"]?.ToString();
            }
            return null;
        }
    }

    private static string NextWeekDate() =>  DateTime.UtcNow.Date.AddDays(7).ToString("yyyy-MM-dd");
    private static object? NormalizeDefaultOrSingleAllowed(CreateMetaValue meta, JObject src)
    {

        if (string.Equals(meta.SchemaType, "date", StringComparison.OrdinalIgnoreCase))
        {
            return NextWeekDate();
        }

        if (string.Equals(meta.SchemaType, "option", StringComparison.OrdinalIgnoreCase))
        {
            var id = src["id"]?.ToString();
            if (!string.IsNullOrEmpty(id)) return new JObject { ["id"] = id };

            var val = src["value"]?.ToString();
            if (!string.IsNullOrEmpty(val) && meta.AllowedValues != null)
            {
                foreach (var a in meta.AllowedValues.OfType<JObject>())
                    if (string.Equals(a["value"]?.ToString(), val, StringComparison.OrdinalIgnoreCase))
                        return new JObject { ["id"] = a["id"]?.ToString() };
            }
            return null;
        }

        if (string.Equals(meta.SchemaType, "array", StringComparison.OrdinalIgnoreCase)
            && string.Equals(meta.SchemaItems, "option", StringComparison.OrdinalIgnoreCase))
        {
            if (src["id"] != null)
                return new JArray(new JObject { ["id"] = src["id"]!.ToString() });

            return null;
        }

        return src["value"] is JValue v ? v.Value : null;
    }

    private static void StripRankFields(JObject obj)
    {
        if (obj is null) return;

        var toRemove = obj.Properties()
            .Where(p => p.Name.IndexOf("rank", StringComparison.OrdinalIgnoreCase) >= 0
                        || p.Name.Equals("rankBeforeIssue", StringComparison.OrdinalIgnoreCase)
                        || p.Name.Equals("rankAfterIssue", StringComparison.OrdinalIgnoreCase))
            .ToList();
        foreach (var p in toRemove) p.Remove();

        if (obj["fields"] is JObject f) StripRankFields(f);
        if (obj["update"] is JObject u) StripRankFields(u);

        foreach (var child in obj.Properties().Select(p => p.Value).OfType<JObject>())
            StripRankFields(child);
    }

    private async Task LinkIssuesDc(string newIssueKey, string sourceKey, string linkTypeName, string? comment)
    {
        var payload = new
        {
            type = new { name = linkTypeName },
            outwardIssue = new { key = sourceKey },
            inwardIssue = new { key = newIssueKey },
            comment = string.IsNullOrWhiteSpace(comment) ? null : new { body = comment }
        };

        var req = new JiraRequest("/issueLink", Method.Post).AddJsonBody(payload);
        await Client.ExecuteWithHandling(req);
    }

    private async Task TransitionIssueToStatusDc(string issueKey, string statusId)
    {
        var get = new JiraRequest($"/issue/{issueKey}/transitions", Method.Get);
        var transitions = await Client.ExecuteWithHandling<TransitionsResponse>(get);

        var target = transitions.Transitions.FirstOrDefault(t => t.To.Id == statusId);
        if (target != null)
        {
            var post = new JiraRequest($"/issue/{issueKey}/transitions", Method.Post)
                .WithJsonBody(new { transition = new { id = target.Id } });
            await Client.ExecuteWithHandling(post);
        }
    }

    [Action("Add attachment", Description = "Add attachment to an issue.")]
    public async Task<AttachmentDto> AddAttachment([ActionParameter] IssueIdentifier issue,
        [ActionParameter] AddAttachmentRequest input)
    {
        var request = new JiraRequest($"/issue/{issue.IssueKey}/attachments", Method.Post);
        var attachmentStream = await fileManagementClient.DownloadAsync(input.Attachment);
        var attachmentBytes = await attachmentStream.GetByteData();
        request.AddHeader("X-Atlassian-Token", "no-check");
        request.AddFile("file", attachmentBytes, input.Attachment.Name);
        var response = await Client.ExecuteWithHandling<IEnumerable<AttachmentDto>>(request);
        return response.First();
    }

    [Action("Add labels to issue", Description = "Add labels to a specific issue.")]
    public async Task<IssueDto> AddLabelsToIssue([ActionParameter] IssueIdentifier issue,
        [ActionParameter] AddLabelsRequest input)
    {
        var request = new JiraRequest($"/issue/{issue.IssueKey}", Method.Put)
            .WithJsonBody(new { update = new { labels = input.Labels.Select(label => new { add = label }) } });
        await Client.ExecuteWithHandling(request);

        return await GetIssueByKey(issue);
    }


    [Action("Move issues to sprint", Description = "Moves issues to a specific sprint")]
    public async Task<MoveIssuesToSprintResponse> MoveIssuesToSprint(
    [ActionParameter] MoveIssuesToSprintRequest input)
    {
        var authenticationProviders = InvocationContext.AuthenticationCredentialsProviders;
        var agileClient = new JiraClient(authenticationProviders, "agile");

        var request = new JiraRequest($"/sprint/{input.SprintId}/issue", Method.Post)
         .WithJsonBody(new
         {
             issues = input.Issues,
             rankAfterIssue = input.RankAfterIssue,
             rankBeforeIssue = input.RankBeforeIssue,
             rankCustomFieldId = input.RankCustomFieldId
         });

        try
        {
            var response = await agileClient.ExecuteWithHandling(request);

            return new MoveIssuesToSprintResponse
            {
                Success = true,
                Message = "Issues moved successfully."
            };
        }
        catch (Exception ex)
        {
            return new MoveIssuesToSprintResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }


    #endregion

    #region PUT

    [Action("Update issue", Description = "Update issue, specifying only the fields that require updating.")]
    public async Task<IssueDto> UpdateIssue([ActionParameter] ProjectIdentifier projectIdentifier,
        [ActionParameter] IssueIdentifier issue,
        [ActionParameter] UpdateIssueRequest input)
    {
        if (!string.IsNullOrWhiteSpace(input.AssigneeAccountId))
        {
            var assigneeRequest = new JiraRequest($"/issue/{issue.IssueKey}/assignee", Method.Put)
                .WithJsonBody(new { name = input.AssigneeAccountId });
            await Client.ExecuteWithHandling(assigneeRequest);
        }

        var fields = new Dictionary<string, object>();
        var update = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(input.Summary))
            fields["summary"] = input.Summary;

        if (!string.IsNullOrWhiteSpace(input.Description))
        {
            var wiki = input.Description;
            fields["description"] = wiki;
        }

        if (!string.IsNullOrWhiteSpace(input.IssueTypeId))
            fields["issuetype"] = new { id = input.IssueTypeId };

        if (!string.IsNullOrWhiteSpace(input.OriginalEstimate))
        {
            update["timetracking"] = new[]
            {
            new Dictionary<string, object>
            {
                ["set"] = new { originalEstimate = input.OriginalEstimate }
            }
        };
        }

        if (input.DueDate.HasValue)
            fields["duedate"] = input.DueDate.Value.ToString("yyyy-MM-dd");

        if (!string.IsNullOrWhiteSpace(input.Reporter))
            fields["reporter"] = new { name = input.Reporter }; 

        if (fields.Count > 0 || update.Count > 0)
        {
            var endpoint = $"/issue/{issue.IssueKey}";

            var qs = new List<string>();
            if (input.OverrideScreenSecurity == true) qs.Add("overrideScreenSecurity=true");
            if (input.NotifyUsers.HasValue) qs.Add($"notifyUsers={input.NotifyUsers.Value.ToString().ToLower()}");
            if (qs.Count > 0) endpoint += "?" + string.Join("&", qs);

            var payload = new Dictionary<string, object>();
            if (fields.Count > 0) payload["fields"] = fields;
            if (update.Count > 0) payload["update"] = update;

            var updateRequest = new JiraRequest(endpoint, Method.Put)
                .WithJsonBody(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            await Client.ExecuteWithHandling(updateRequest);
        }

        if (!string.IsNullOrWhiteSpace(input.StatusId))
        {
            var getTransitionsRequest = new JiraRequest($"/issue/{issue.IssueKey}/transitions", Method.Get);
            var transitions = await Client.ExecuteWithHandling<TransitionsResponse>(getTransitionsRequest);

            var target = transitions.Transitions.FirstOrDefault(t => t.To?.Id == input.StatusId);
            if (target != null)
            {
                var transitionRequest = new JiraRequest($"/issue/{issue.IssueKey}/transitions", Method.Post)
                    .WithJsonBody(new { transition = new { id = target.Id } });
                await Client.ExecuteWithHandling(transitionRequest);
            }
        }
        var issueResponse = await GetIssueByKey(issue);

        return issueResponse;

    }

    //[Action("Append to issue description", Description = "Appends additional text with optional formatting to an issue's description.")]
    //public async Task AppendIssueDescription([ActionParameter] IssueIdentifier issueIdentifier,
    //    [ActionParameter] AppendDescriptionRequest input)
    //{
    //    var request = new JiraRequest($"/issue/{issueIdentifier.IssueKey}", Method.Get);
    //    var issue = await Client.ExecuteWithHandling<IssueWrapper>(request);

    //    var contentElement = new ContentElement() 
    //    {
    //        Type = "text",
    //        Text = input.Text
    //    };
        
    //    if(input.Formatting != null && input.Formatting != "none")
    //    {
    //        contentElement.Marks =
    //        [
    //            new Mark { Type = input.Formatting }
    //        ];
    //    }

    //    if (issue.Fields.Description is null) 
    //    {
    //        issue.Fields.Description = new Description { Type = "doc", Version = 1 ,Content = new List<ContentElement> { new ContentElement 
    //        {
    //            Type = "paragraph", 
    //            Content = new List<ContentElement> { contentElement }

    //        } } };
    //    }
    //    else 
    //    {
    //        issue.Fields.Description?.Content.Add(new ContentElement
    //        { Type = "paragraph", Content = new List<ContentElement> { contentElement } });
    //    }

    //    var body = new { fields = new { description = issue.Fields.Description } };
    //    var updateIssueRequest = new JiraRequest($"/issue/{issueIdentifier.IssueKey}", Method.Put)
    //        .WithJsonBody(body, new JsonSerializerSettings
    //        {
    //            ContractResolver = new DefaultContractResolver()
    //            {
    //                NamingStrategy = new SnakeCaseNamingStrategy()
    //            },
    //            NullValueHandling = NullValueHandling.Ignore
    //        });

    //    await Client.ExecuteWithHandling(updateIssueRequest);
    //}

    #endregion

    #region DELETE

    [Action("Delete issue", Description = "Delete an issue. To delete the issue along with its subtasks, " +
                                          "set the optional input parameter 'Delete subtasks' to 'True'.")]
    public async Task DeleteIssue([ActionParameter] IssueIdentifier issue,
        [ActionParameter] [Display("Delete subtasks")]
        bool? deleteSubtasks)
    {
        var request =
            new JiraRequest($"/issue/{issue.IssueKey}?deleteSubtasks={(deleteSubtasks ?? false).ToString()}",
                Method.Delete);

        await Client.ExecuteWithHandling(request);
    }

    #endregion
}