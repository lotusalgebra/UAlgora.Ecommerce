using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace UAlgora.Ecommerce.Web.Composers;

/// <summary>
/// Algora Commerce Section Permission Composer
///
/// Grants Algora section access to the Administrators group on startup.
/// This ensures admin users can access the Algora Commerce section immediately.
/// </summary>
public class EcommerceSectionPermissionComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<UmbracoApplicationStartedNotification, EcommerceSectionPermissionHandler>();
    }
}

/// <summary>
/// Algora Commerce Section Permission Handler
///
/// Handles the UmbracoApplicationStarted notification to grant
/// Algora section access to admin users.
/// </summary>
public class EcommerceSectionPermissionHandler : INotificationHandler<UmbracoApplicationStartedNotification>
{
    private readonly IUserGroupService _userGroupService;
    private readonly ILogger<EcommerceSectionPermissionHandler> _logger;

    /// <summary>
    /// Algora section alias (matches umbraco-package.json)
    /// </summary>
    private const string AlgoraSectionAlias = "Umb.Section.Ecommerce";

    public EcommerceSectionPermissionHandler(
        IUserGroupService userGroupService,
        ILogger<EcommerceSectionPermissionHandler> logger)
    {
        _userGroupService = userGroupService;
        _logger = logger;
    }

    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        try
        {
            // Get the Administrators group
            var adminGroup = _userGroupService.GetAsync(Umbraco.Cms.Core.Constants.Security.AdminGroupAlias).GetAwaiter().GetResult();

            if (adminGroup == null)
            {
                _logger.LogWarning("Algora Commerce: Administrators group not found");
                return;
            }

            // Check if the group already has access to the Algora section
            if (adminGroup.AllowedSections.Contains(AlgoraSectionAlias))
            {
                _logger.LogDebug("Algora Commerce: Administrators group already has Algora section access");
                return;
            }

            // Add the Algora section to the allowed sections
            adminGroup.AddAllowedSection(AlgoraSectionAlias);

            // Save the group
            _userGroupService.UpdateAsync(adminGroup, Umbraco.Cms.Core.Constants.Security.SuperUserKey).GetAwaiter().GetResult();

            _logger.LogInformation("Algora Commerce: Granted Algora section access to Administrators group");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Algora Commerce: Error granting Algora section access");
        }
    }
}
