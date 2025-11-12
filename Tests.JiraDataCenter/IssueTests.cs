using Apps.Jira.Actions;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Apps.JiraDataCenter.DataSourceHandlers;
using Apps.JiraDataCenter.Models.Requests;
using Tests.Appname.Base;

namespace Tests.Appname;

[TestClass]
public class IssueTests : TestBase
{
    [TestMethod]
    public async Task CreateIssue_ReturnsSuccess()
    {
        var action = new IssueActions(InvocationContext, FileManager);

        var project = new ProjectIdentifier
        {
            ProjectKey = "GLS"
        };
        var request = new CreateIssueRequest
        {
            Summary = "Test issue blackbird",
            IssueTypeId = "10800",
            Description = "Test description",
        };
        var response = await action.CreateIssue(project, request);

        Console.WriteLine(response.Key);

        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task UpdateIssue_ReturnsSuccess()
    {
        // Arrange
        var action = new IssueActions(InvocationContext, FileManager);
        var project = new ProjectIdentifier { ProjectKey = "GLS" };
        var issue = new IssueIdentifier { IssueKey = "GLS-18918" };
        var request = new UpdateIssueRequest
        {
            StatusId = "171",
        };

        // Act
        var response = await action.UpdateIssue(project, issue, request);
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(true);
    }

    [TestMethod]
    public async Task GetIssue_ReturnsSuccess()
    {
        var action = new IssueActions(InvocationContext, FileManager);

        var project = new IssueIdentifier
        {
            IssueKey = "GLS-18889"
        };

        var response = await action.GetIssueByKey(project);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task IssueLabelsDataHandler_ReturnsSuccess()
    {
        var action = new IssueLabelDataHandler(InvocationContext);

        var response = await action.GetDataAsync(new Blackbird.Applications.Sdk.Common.Dynamic.DataSourceContext { }, CancellationToken.None);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task RemoveLabelsFromIssue_ReturnsSuccess()
    {
        var action = new IssueActions(InvocationContext, FileManager);

        var project = new IssueIdentifier
        {
            IssueKey = "GLS-18889"
        };

        var response = await action.RemoveLabelsFromIssue(project, new RemoveLabelsRequest { Labels = new List<string> { "BlackbirdTest1", "BlackbirdTest2", "BlackbirdTest3", "BlackbirdTest4" } });

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task AddIssueLabels_ReturnsSuccess()
    {
        var action = new IssueActions(InvocationContext, FileManager);

        var project = new ProjectIdentifier
        {
            ProjectKey = "GLS"
        };

        var issue = new IssueIdentifier
        {
            IssueKey = "GLS-18889"
        };
        var request = new AddLabelsRequest
        {
            Labels = new List<string> { "BlackbirdTest1", "BlackbirdTest2", "BlackbirdTest3", "BlackbirdTest4" }
        };
        await action.AddLabelsToIssue(issue, request);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task FindIssue_ReturnsSuccess()
    {
        var action = new IssueActions(InvocationContext, FileManager);

        var p1roject = new IssueIdentifier
        {
            IssueKey = "GLS-16713"
        };
        var project = new ProjectIdentifier { ProjectKey = "GLS" };
        var response = await action.FindIssue("", "Multiple Care Jan 25", project, "");

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task ListRecentlyCreatedIssues_ReturnsSuccess()
    {
        var action = new IssueActions(InvocationContext, FileManager);

        var project = new ProjectIdentifier { ProjectKey = "GLS" };
        var listRequest = new ListRecentlyCreatedIssuesRequest
        {
            //Hours = 500,
            //Labels = ["form", "non-existent-label"],
            //Versions = ["v1.0", "v1.1"]
            //ParentIssue = "AC-8"
        };

        var response = await action.ListRecentlyCreatedIssues(project, listRequest, null);
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);

        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task AddIssueComment_ReturnsSuccess()
    {
        var action = new IssueCommentActions(InvocationContext);

        var project = new ProjectIdentifier
        {
            ProjectKey = "AC"
        };

        var issue = new IssueIdentifier
        {
            IssueKey = "AC-1"
        };
        var request = new AddIssueCommentRequest
        {
            Text = "Test comment Test",
        };
        await action.AddIssueComment(issue, request);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task CloneIssue_ReturnsSuccess()
    {
        var action = new IssueActions(InvocationContext, FileManager);

        var project = new IssueIdentifier
        {
            IssueKey = "GLS-18902"
        };
        var clone = new CloneIssueRequest
        {
            NewDueDate = DateTime.Now.AddDays(7),
            NewDescription = "Cloned issue description",
            LinkTypeName = "Cloners",
        };

        var response = await action.CloneIssue(project, clone);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(response);
    }
}
