using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Oqtane.Models
{
    /// <summary>
    /// Describes a Site in a <see cref="Tenant"/> in an Oqtane installation.
    /// Sites can have multiple <see cref="Alias"/>es.
    /// </summary>
    public class Site : ModelBase, IDeletable
    {
        /// <summary>
        /// The ID of the Site
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Tenant"/> the Site is in
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// The site Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Reference to a <see cref="File"/> which has the Logo for this site.
        /// Should be an image.
        /// The theme can then use this where needed. 
        /// </summary>
        public int? LogoFileId { get; set; }

        /// <summary>
        /// Reference to a <see cref="File"/> which has the FavIcon for this site.
        /// Should be an image. 
        /// The theme can then use this where needed.
        /// </summary>
        public int? FaviconFileId { get; set; }

        /// <summary>
        /// Default theme for the site
        /// </summary>
        public string DefaultThemeType { get; set; }

        /// <summary>
        /// Default container for the site
        /// </summary>
        public string DefaultContainerType { get; set; }

        /// <summary>
        /// Default admin container
        /// </summary>
        public string AdminContainerType { get; set; }

        /// <summary>
        /// Indicates if the site is a progressive web application (PWA)
        /// </summary>
        public bool PwaIsEnabled { get; set; }

        /// <summary>
        /// The app icon for the progressive web application (PWA)
        /// </summary>
        public int? PwaAppIconFileId { get; set; }

        /// <summary>
        /// The splash icon for the progressive web application (PWA)
        /// </summary>
        public int? PwaSplashIconFileId { get; set; }

        /// <summary>
        /// Determines if visitors may register / create user accounts
        /// </summary>
        public bool AllowRegistration { get; set; }

        /// <summary>
        /// Determines if site visitors will be recorded
        /// </summary>
        public bool VisitorTracking { get; set; }

        /// <summary>
        /// Determines if broken urls (404s) will be captured automatically
        /// </summary>
        public bool CaptureBrokenUrls { get; set; }

        /// <summary>
        /// Unique GUID to identify the Site.
        /// </summary>
        public string SiteGuid { get; set; }

        /// <summary>
        /// The default render mode for the site ie. Static,Interactive,Headless
        /// </summary>
        public string RenderMode { get; set; }

        /// <summary>
        /// The render mode for UI components which require interactivity ie. Server,WebAssembly,Auto
        /// </summary>
        public string Runtime { get; set; }

        /// <summary>
        /// If the site supports prerendering (only applies to Interactive rendermode)
        /// </summary>
        public bool Prerender { get; set; }

        /// <summary>
        /// Indicates if a site can be integrated with an external .NET MAUI hybrid application
        /// </summary>
        public bool Hybrid { get; set; }

        /// <summary>
        /// Keeps track of site configuration changes and is used by the ISiteMigration interface
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The home page of the site - the "/" path will be used by default if no home page is specified
        /// </summary>
        public int? HomePageId { get; set; }

        /// <summary>
        /// Content to be included in the head of the page
        /// </summary>
        public string HeadContent { get; set; }

        /// <summary>
        /// Content to be included in the body of the page
        /// </summary>
        public string BodyContent { get; set; }

        /// <summary>
        /// Indicates if site is deleted
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// The user who deleted site
        /// </summary>
        public string DeletedBy { get; set; }

        /// <summary>
        /// Date site was deleted
        /// </summary>
        public DateTime? DeletedOn { get; set; }

        /// <summary>
        /// The allowable iamge file extensions
        /// </summary>
        [NotMapped]
        public string ImageFiles { get; set; }

        /// <summary>
        /// The allowable file extensions which can be uploaded
        /// </summary>
        [NotMapped]
        public string UploadableFiles { get; set; }

        /// <summary>
        /// Used when provisioning a site from a site template
        /// </summary>
        [NotMapped]
        public string SiteTemplateType { get; set; }

        /// <summary>
        /// The settings for the site
        /// </summary>
        [NotMapped]
        public Dictionary<string, string> Settings { get; set; }

        /// <summary>
        /// List of pages for the site
        /// </summary>
        [NotMapped]
        public List<Page> Pages { get; set; }

        /// <summary>
        /// List of languages for the site
        /// </summary>
        [NotMapped]
        public List<Language> Languages { get; set; }

        /// <summary>
        /// List of themes for the site
        /// </summary>
        [NotMapped]
        public List<Theme> Themes { get; set; }

        public Site Clone(Site site)
        {
            return new Site
            {
                SiteId = site.SiteId,
                TenantId = site.TenantId,
                Name = site.Name,
                LogoFileId = site.LogoFileId,
                FaviconFileId = site.FaviconFileId,
                DefaultThemeType = site.DefaultThemeType,
                DefaultContainerType = site.DefaultContainerType,
                AdminContainerType = site.AdminContainerType,
                PwaIsEnabled = site.PwaIsEnabled,
                PwaAppIconFileId = site.PwaAppIconFileId,
                PwaSplashIconFileId = site.PwaSplashIconFileId,
                AllowRegistration = site.AllowRegistration,
                VisitorTracking = site.VisitorTracking,
                CaptureBrokenUrls = site.CaptureBrokenUrls,
                SiteGuid = site.SiteGuid,
                RenderMode = site.RenderMode,
                Runtime = site.Runtime,
                Prerender = site.Prerender,
                Hybrid = site.Hybrid,
                Version = site.Version,
                HomePageId = site.HomePageId,
                HeadContent = site.HeadContent,
                BodyContent = site.BodyContent,
                IsDeleted = site.IsDeleted,
                DeletedBy = site.DeletedBy,
                DeletedOn = site.DeletedOn,
                ImageFiles = site.ImageFiles,
                UploadableFiles = site.UploadableFiles,
                SiteTemplateType = site.SiteTemplateType,
                CreatedBy = site.CreatedBy,
                CreatedOn = site.CreatedOn,
                ModifiedBy = site.ModifiedBy,
                ModifiedOn = site.ModifiedOn,
                Settings = site.Settings.ToDictionary(),
                Pages = site.Pages.ToList(),
                Languages = site.Languages.ToList(),
                Themes = site.Themes.ToList()
            };
        }

        #region Obsolete properties
        [NotMapped]
        [Obsolete("This property is deprecated.", false)]
        public string DefaultLayoutType { get; set; }
        #endregion
    }
}
