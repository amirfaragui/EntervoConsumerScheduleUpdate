// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using T2Importer.Identity;

namespace T2WebApplication.Areas.Identity.Pages.Account
{
  public class LoginVerifyCodeModel : PageModel
  {
    private readonly SignInManager<CustomerPortalUser> _signInManager;
    private readonly UserManager<CustomerPortalUser> _userManager;
    private readonly ILogger<LoginWith2faModel> _logger;
    private readonly IEmailSender _emailSender;

    public LoginVerifyCodeModel(
        SignInManager<CustomerPortalUser> signInManager,
        UserManager<CustomerPortalUser> userManager,
        IEmailSender emailSender,
        ILogger<LoginWith2faModel> logger)
    {
      _signInManager = signInManager;
      _userManager = userManager;
      _emailSender = emailSender;
      _logger = logger;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public bool RememberMe { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string ReturnUrl { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
      /// <summary>
      ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
      ///     directly from your code. This API may change or be removed in future releases.
      /// </summary>
      [Required]
      [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
      [DataType(DataType.Text)]
      [Display(Name = "Authenticator code")]
      public string TwoFactorCode { get; set; }

      /// <summary>
      ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
      ///     directly from your code. This API may change or be removed in future releases.
      /// </summary>
      [Display(Name = "Remember this machine")]
      public bool RememberMachine { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
    {
      // Ensure the user has gone through the username & password screen first
      var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

      if (user == null)
      {
        throw new InvalidOperationException($"Unable to load two-factor authentication user.");
      }

      ReturnUrl = returnUrl;
      RememberMe = rememberMe;


      //// make sure the user email is valid
      //var user = await _userManager.FindByNameAsync(id);
      //if (user == null)
      //  return RedirectToPage("/Error/StatusCode", new { code = "403" });

      //Input = new InputModel
      //{
      //  Email = user.Email,
      //  UserName = user.UserName,
      //  ReturnUrl = returnUrl
      //};

      // generate the 2fa token
      var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
      
      await _emailSender.SendEmailAsync(user.Email, "Your Verification Code", $"Verification Code: {token}");

      return Page();
    }

    public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
    {
      if (!ModelState.IsValid)
      {
        return Page();
      }

      returnUrl = returnUrl ?? Url.Content("~/");

      var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
      if (user == null)
      {
        throw new InvalidOperationException($"Unable to load two-factor authentication user.");
      }

      var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

      //var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, Input.RememberMachine);
      //var userId = await _userManager.GetUserIdAsync(user);
      var result = await _signInManager.TwoFactorSignInAsync("Email", Input.TwoFactorCode, false, Input.RememberMachine);

      if (result.Succeeded)
      {
        _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
        return LocalRedirect(returnUrl);
      }
      else if (result.IsLockedOut)
      {
        _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
        return RedirectToPage("./Lockout");
      }
      else
      {
        _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
        ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
        return Page();
      }
    }
  }
}
