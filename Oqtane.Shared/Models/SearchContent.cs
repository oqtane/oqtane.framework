using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
namespace Oqtane.Models
{
    public class SearchContent
    {
        public int SearchContentId { get; set; }

        public int SiteId { get; set; }

        public string EntityName { get; set; }

        public string EntityId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Body { get; set; }

        public string Url { get; set; }

        public string Permissions { get; set; } // comma delimited EntityName:EntityId,EntityName:EntityId

        public string ContentModifiedBy { get; set; }

        public DateTime ContentModifiedOn { get; set; }

        public string AdditionalContent { get; set; }

        public DateTime CreatedOn { get; set; }

        public List<SearchContentProperty> SearchContentProperties { get; set; }

        public List<SearchContentWord> SearchContentWords { get; set; }

        [NotMapped]
        public string UniqueKey => $"{TenantId}:{SiteId}:{EntityName}:{EntityId}";

        [NotMapped]
        public int TenantId { get; set; }

        // constructors
        public SearchContent() { }

        public SearchContent(int siteId, string entityName, string entityId, string title, string description, string body, string url, string permissions, string contentModifiedBy, DateTime contentModifiedOn)
        {
            SiteId = siteId;
            EntityName = entityName;
            EntityId = entityId;
            Title = title;
            Description = description;
            Body = body;
            Url = url;
            Permissions = permissions;
            ContentModifiedBy = contentModifiedBy;
            ContentModifiedOn = contentModifiedOn;
            AdditionalContent = "";
            CreatedOn = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
