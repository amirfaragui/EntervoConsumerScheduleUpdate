// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Entrvo.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace T2WebApplication.Areas.Identity.Pages.Account.Manage
{
  public class ProfileModel : PageModel
  {
    private readonly UserManager<CustomerPortalUser> _userManager;
    private readonly SignInManager<CustomerPortalUser> _signInManager;

    public ProfileModel(
        UserManager<CustomerPortalUser> userManager,
        SignInManager<CustomerPortalUser> signInManager)
    {
      _userManager = userManager;
      _signInManager = signInManager;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string StatusMessage { get; set; }

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
    public class InputModel
    {
      /// <summary>
      ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
      ///     directly from your code. This API may change or be removed in future releases.
      /// </summary>
      [Phone]
      [Display(Name = "Mobile number")]
      public string MobileNumber { get; set; }

      [Phone]
      [Display(Name = "Home number")]
      public string HomeNumber { get; set; }

      [Phone]
      [Display(Name = "Work number")]
      public string WorkNumber { get; set; }


      [Display(Name = "First nmae")]
      public string FirstName { get; set; }

      [Display(Name ="Last name")]
      public string LastName { get; set; }
    }

    private async Task LoadAsync(CustomerPortalUser user)
    {
      //var userName = await _userManager.GetUserNameAsync(user);
      //var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

      Username = await _userManager.GetUserNameAsync(user);

      Input = new InputModel
      {        
        FirstName = user.FirstName,
        LastName = user.LastName,
        MobileNumber = user.PhoneNumber,
        HomeNumber = user.HomePhoneNumber,
        WorkNumber = user.WorkPhoneNumber,
      };
    }

    public async Task<IActionResult> OnGetAsync()
    {
      var user = await _userManager.GetUserAsync(User);
      if (user == null)
      {
        return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
      }

      await LoadAsync(user);
      ViewData["ParentLayout"] = "/Views/Shared/_Layout.cshtml";
      return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
      var user = await _userManager.GetUserAsync(User);
      if (user == null)
      {
        return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
      }

      if (!ModelState.IsValid)
      {
        await LoadAsync(user);
        return Page();
      }

      //var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
      //if (Input.PhoneNumber != phoneNumber)
      //{
      //  var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
      //  if (!setPhoneResult.Succeeded)
      //  {
      //    StatusMessage = "Unexpected error when trying to set phone number.";
      //    return RedirectToPage();
      //  }
      //}

      user.FirstName = Input.FirstName;
      user.LastName = Input.LastName;
      user.PhoneNumber = Input.MobileNumber;
      user.HomePhoneNumber = Input.HomeNumber;
      user.WorkPhoneNumber = Input.WorkNumber;

      var result = await _userManager.UpdateAsync(user);
      if (!result.Succeeded)
      {
        StatusMessage = "Unexpected error when trying to update your profile.";
        return RedirectToPage();
      }


      await _signInManager.RefreshSignInAsync(user);
      StatusMessage = "Your profile has been updated";
      return RedirectToPage();
    }
  }
}
