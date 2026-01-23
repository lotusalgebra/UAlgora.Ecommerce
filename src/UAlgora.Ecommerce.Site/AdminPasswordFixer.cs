using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Notifications;

namespace UAlgora.Ecommerce.Site;

/// <summary>
/// Fixes the admin password on startup if it's not properly hashed.
/// </summary>
public class AdminPasswordFixerComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<UmbracoApplicationStartedNotification, AdminPasswordFixerHandler>();
    }
}

public class AdminPasswordFixerHandler : INotificationHandler<UmbracoApplicationStartedNotification>
{
    private readonly IUserService _userService;
    private readonly IBackOfficeUserManager _backOfficeUserManager;
    private readonly ILogger<AdminPasswordFixerHandler> _logger;

    public AdminPasswordFixerHandler(
        IUserService userService,
        IBackOfficeUserManager backOfficeUserManager,
        ILogger<AdminPasswordFixerHandler> logger)
    {
        _userService = userService;
        _backOfficeUserManager = backOfficeUserManager;
        _logger = logger;
    }

    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        Task.Run(async () =>
        {
            try
            {
                // Find admin user
                var adminUser = _userService.GetUserById(-1);
                if (adminUser == null)
                {
                    _logger.LogWarning("Admin user not found");
                    return;
                }

                // Check if password needs to be fixed (if it's "default" or very short hash)
                var rawPassword = adminUser.RawPasswordValue;
                if (rawPassword == "default" || (rawPassword != null && rawPassword.Length < 20))
                {
                    _logger.LogInformation("Fixing admin password...");

                    // Get the BackOfficeIdentityUser
                    var identityUser = await _backOfficeUserManager.FindByIdAsync(adminUser.Id.ToString());
                    if (identityUser != null)
                    {
                        // Reset password using UserManager
                        var token = await _backOfficeUserManager.GeneratePasswordResetTokenAsync(identityUser);
                        var result = await _backOfficeUserManager.ResetPasswordAsync(identityUser, token, "Admin@12345");

                        if (result.Succeeded)
                        {
                            _logger.LogInformation("Admin password has been reset successfully");
                        }
                        else
                        {
                            _logger.LogError("Failed to reset admin password: {Errors}",
                                string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing admin password");
            }
        }).GetAwaiter().GetResult();
    }
}
