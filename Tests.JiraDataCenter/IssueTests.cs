using Apps.Jira.Actions;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
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
        var project = new ProjectIdentifier { ProjectKey = "TL" };
        var issue = new IssueIdentifier { IssueKey = "TL-11" };
        var request = new UpdateIssueRequest
        {
            StatusId = "3",
        };

        // Act
        await action.UpdateIssue(project, issue, request);
    }

    [TestMethod]
    public async Task GetIssue_ReturnsSuccess()
    {
        var action = new IssueActions(InvocationContext, FileManager);

        var project = new IssueIdentifier
        {
            IssueKey = "GLS-16713"
        };

        var response = await action.GetIssueByKey(project);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(response);
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
            IssueKey = "GLS-16713"
        };
        var clone = new CloneIssueRequest
        {
            ReporterName= "marenb"
        };

        var response = await action.CloneIssue(project, clone);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(response);
    }
}
