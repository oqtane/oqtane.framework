using Oqtane.Shared;

namespace Oqtane.Models
{
    /// <summary>
    /// Stylesheet inherits from Resource and offers constructors with parameters specific to Stylesheets
    /// </summary>
    public class Stylesheet : Resource
    {
        private void SetDefaults()
        {
            this.ResourceType = ResourceType.Stylesheet;
            this.Location = ResourceLocation.Head;
        }

        public Stylesheet(string Href)
        {
            SetDefaults();
            this.Url = Href;
        }

        public Stylesheet(string Href, string Integrity, string CrossOrigin)
        {
            SetDefaults();
            this.Url = Href;
            this.Integrity = Integrity;
            this.CrossOrigin = CrossOrigin;
        }
    }
}
