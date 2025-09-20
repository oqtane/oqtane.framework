using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using Oqtane.Interfaces;
using Oqtane.Providers;
using Oqtane.Services;
using Oqtane.Shared;
using Radzen;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OqtaneServiceCollectionExtensions
    {
        public static IServiceCollection AddOqtaneAuthentication(this IServiceCollection services)
        {
            services.AddAuthorizationCore();
            services.AddCascadingAuthenticationState();
            services.AddScoped<IdentityAuthenticationStateProvider>();
            services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<IdentityAuthenticationStateProvider>());

            return services;
        }

        public static IServiceCollection AddOqtaneClientScopedServices(this IServiceCollection services)
        {
            services.AddScoped<SiteState>();
            services.AddScoped<IInstallationService, InstallationService>();
            services.AddScoped<IModuleDefinitionService, ModuleDefinitionService>();
            services.AddScoped<IThemeService, Oqtane.Services.ThemeService>();
            services.AddScoped<IAliasService, AliasService>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<ISiteService, SiteService>();
            services.AddScoped<IPageService, PageService>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IPageModuleService, PageModuleService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserRoleService, UserRoleService>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IPackageService, PackageService>();
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<IJobLogService, JobLogService>();
            services.AddScoped<INotificationService, Oqtane.Services.NotificationService>();
            services.AddScoped<IFolderService, FolderService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<ISiteTemplateService, SiteTemplateService>();
            services.AddScoped<ISqlService, SqlService>();
            services.AddScoped<ISystemService, SystemService>();
            services.AddScoped<ILocalizationService, LocalizationService>();
            services.AddScoped<ILanguageService, LanguageService>();
            services.AddScoped<IDatabaseService, DatabaseService>();
            services.AddScoped<IUrlMappingService, UrlMappingService>();
            services.AddScoped<IVisitorService, VisitorService>();
            services.AddScoped<ISyncService, SyncService>();
            services.AddScoped<ILocalizationCookieService, LocalizationCookieService>();
            services.AddScoped<ICookieConsentService, CookieConsentService>();
            services.AddScoped<ITimeZoneService, TimeZoneService>();
            services.AddScoped<IMigrationHistoryService, MigrationHistoryService>();
            services.AddScoped<IOutputCacheService, OutputCacheService>();

            // providers
            services.AddScoped<ITextEditor, Oqtane.Modules.Controls.QuillJSTextEditor>();
            services.AddScoped<ITextEditor, Oqtane.Modules.Controls.TextAreaTextEditor>();
            services.AddScoped<ITextEditor, Oqtane.Modules.Controls.RadzenTextEditor>();

            services.AddRadzenComponents();

            var localizer = services.BuildServiceProvider().GetService<IStringLocalizer<Oqtane.Modules.Controls.RadzenTextEditor>>();
            Oqtane.Modules.Controls.RadzenEditorDefinitions.Localizer = localizer;

            return services;
        }
    }
}
