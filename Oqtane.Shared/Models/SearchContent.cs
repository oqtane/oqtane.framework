using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
namespace Oqtane.Models
{
    public class SearchContent : ModelBase
    {
        public int SearchContentId { get; set; }

        [NotMapped]
        public string UniqueKey => $"{EntityName}:{EntityId}";

        public string EntityName { get; set; }

        public int EntityId { get; set; }

        public int SiteId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Body { get; set; }

        public string Url { get; set; }

        public DateTime ModifiedTime { get; set; }

        public bool IsActive { get; set; } = true;

        public string AdditionalContent { get; set; }

        public IList<SearchContentProperty> Properties { get; set; }

        public IList<SearchContentWords> Words { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
