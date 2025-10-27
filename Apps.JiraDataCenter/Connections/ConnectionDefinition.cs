using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.Jira.Connections
{
    public class ConnectionDefinition : IConnectionDefinition
    {
        public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups =>
        [
        new()
        {
            Name = "Access Token",
            DisplayName = "Personal Access Token auth",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionProperties =
            [
                new("Jira URL")
                {
                    DisplayName = "Jira URL",
                    Description = "For example: https://jira.company.com",
                    Sensitive = false
                },
                new("PAT")
                {
                    DisplayName = "Personal Access Token",
                    Sensitive = true
                }
            ]
        }
    ];

        public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
            Dictionary<string, string> values)
        {
            var pat = values.First(v => v.Key == "PAT").Value;
            yield return new AuthenticationCredentialsProvider("Authorization", $"Bearer {pat}");

            var jiraUrl = new Uri(values.First(v => v.Key == "Jira URL").Value).GetLeftPart(UriPartial.Authority);
            yield return new AuthenticationCredentialsProvider("JiraUrl", jiraUrl);
        }
    }
}
