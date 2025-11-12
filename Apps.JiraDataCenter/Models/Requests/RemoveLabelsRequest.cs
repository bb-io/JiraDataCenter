using Apps.JiraDataCenter.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.JiraDataCenter.Models.Requests
{
    public class RemoveLabelsRequest
    {
        [Display("Labels")]
        [DataSource(typeof(IssueLabelDataHandler))]
        public IEnumerable<string>? Labels { get; set; }

        [Display("Clear all labels")]
        public bool? ClearAll { get; set; }
    }
}
