using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
namespace Oqtane.Models
{
    public class SearchContent
    {
        public int SearchContentId { get; set; }

        public string UniqueKey { get; set; }

        public string EntityName { get; set; }

        public int EntityId { get; set; }

        public int SiteId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Body { get; set; }

        public string Url { get; set; }

        public string ContentAuthoredBy { get; set; }

        public DateTime ContentAuthoredOn { get; set; }

        public bool IsActive { get; set; } = true;

        public string AdditionalContent { get; set; }

        public List<SearchContentProperty> SearchContentProperties { get; set; }

        public List<SearchContentWord> SearchContentWords { get; set; }

        public DateTime CreatedOn { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
