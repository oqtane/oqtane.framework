using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
namespace Oqtane.Models
{
    public class SearchDocument : ModelBase
    {
        public int SearchDocumentId { get; set; }

        [NotMapped]
        public string UniqueKey => $"{IndexerName}:{EntryId}";

        public int EntryId { get; set; }

        public string IndexerName { get; set; }

        public int SiteId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Body { get; set; }

        public string Url { get; set; }

        public DateTime ModifiedTime { get; set; }

        public bool IsActive { get; set; }

        public string AdditionalContent { get; set; }

        public string LanguageCode { get; set; }

        public IList<SearchDocumentTag> Tags { get; set; }

        public IList<SearchDocumentProperty> Properties { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
