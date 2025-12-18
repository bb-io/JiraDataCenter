using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Apps.JiraDataCenter.DataSourceHandlers.CustomFields;

namespace Apps.JiraDataCenter.Models.Identifiers;

public class CustomLinkFieldIdentifier
{
    [Display("Custom link field ID")]
    [DataSource(typeof(CustomLinkFieldDataSourceHandler))]
    public string CustomLinkFieldId { get; set; }
}
