﻿using Apps.Jira.Actions;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Jira.DataSourceHandlers;

public class IssueCommentDataHandler : JiraInvocable, IAsyncDataSourceHandler
{
    private readonly string _issueKey;

    public IssueCommentDataHandler(InvocationContext invocationContext,
        [ActionParameter] IssueCommentIdentifier identifier) : base(invocationContext)
    {
        _issueKey = identifier.IssueKey;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var issueCommentActions = new IssueCommentActions(InvocationContext);
        
        var comments = await issueCommentActions.GetIssueComments(new GetIssueCommentsRequest { IssueKey = _issueKey });
        return comments
            .ToDictionary(comment => comment.Id, comment => comment.Id);
    }
}