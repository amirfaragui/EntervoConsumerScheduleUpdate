// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using T2Importer.Identity;

namespace T2WebApplication.Areas.Identity.Pages.Account.Manage
{
  public class Enable2faModel : PageModel
    {
        private readonly UserManager<CustomerPortalUser> _userManager;
        private readonly ILogger<Disable2faModel> _logger;

        public Enable2faModel(
            UserManager<CustomerPortalUser> userManager,
            ILogger<Disable2faModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (await _userManager.GetTwoFactorEnabledAsync(user))
            {
                throw new InvalidOperationException($"Cannot enable 2FA for user as it's already enabled.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var enable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, true);
            if (!enable2faResult.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred enabling 2FA.");
            }

            _logger.LogInformation("User with ID '{UserId}' has enabled 2fa.", _userManager.GetUserId(User));
            StatusMessage = "2fa has been enabled. You can disable 2fa when managing your profile.";
            return RedirectToPage("./TwoFactorAuthentication");
        }
    }
}
