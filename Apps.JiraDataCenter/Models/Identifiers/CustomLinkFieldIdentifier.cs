using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.JiraDataCenter.Models.Identifiers;

public class CustomLinkFieldIdentifier
{
    [Display("Custom link field ID")]
    [DataSource(typeof(CustomLinkFieldIdentifier))]
    public string CustomLinkFieldId { get; set; }
}
