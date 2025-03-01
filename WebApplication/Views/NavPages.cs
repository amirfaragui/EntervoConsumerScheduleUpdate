// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace T2WebApplication.Views
{
  /// <summary>
  ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
  ///     directly from your code. This API may change or be removed in future releases.
  /// </summary>
  public static class NavPages
  {
    // Customers
    public static bool IsCustomersExpanded(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      return string.Equals(parts[0], "Customers", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsCustomerListSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return IsCustomersExpanded(viewContext) && string.Equals(parts[1], "List", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsCustomerEditSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return IsCustomersExpanded(viewContext) && ((parts[1] == "Edit") || (parts[1] == "Create"));
    }

    // Permits
    public static bool IsPermitsExpanded(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      return string.Equals(parts[0], "Permits", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsPermitListSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return IsPermitsExpanded(viewContext) && string.Equals(parts[1], "List", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsPermitEditSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return IsPermitsExpanded(viewContext) && ((parts[1] == "Edit") || (parts[1] == "Create"));
    }


    // Import Data
    public static bool IsImportExpanded(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      return string.Equals(parts[0], "Import", StringComparison.OrdinalIgnoreCase);
    }
    public static bool IsCustomersImportSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return IsImportExpanded(viewContext) && string.Equals(parts[1], "Customers", StringComparison.OrdinalIgnoreCase);
    }
    public static bool IsPermitsImportSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return IsImportExpanded(viewContext) && string.Equals(parts[1], "Permits", StringComparison.OrdinalIgnoreCase);
    }
    public static bool IsEmailsImportSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return IsImportExpanded(viewContext) && string.Equals(parts[1], "Emails", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsNotesImportSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return IsImportExpanded(viewContext) && string.Equals(parts[1], "Notes", StringComparison.OrdinalIgnoreCase);
    }

    // Settings
    public static bool IsSettingsExpanded(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      return string.Equals(parts[0], "Settings", StringComparison.OrdinalIgnoreCase) ? true : false;
    }

    public static bool IsUsersSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return string.Equals(parts[1], "Users", StringComparison.OrdinalIgnoreCase) ? true : false;
    }

    public static bool IsFtpSourceSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return string.Equals(parts[1], "Source", StringComparison.OrdinalIgnoreCase) ? true : false;
    }

    public static bool IsApiEndpointSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return string.Equals(parts[1], "Destination", StringComparison.OrdinalIgnoreCase) ? true : false;
    }
    // Account
    public static bool IsAccountExpanded(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      return string.Equals(parts[0], "Account", StringComparison.OrdinalIgnoreCase) ? true : false;
    } 
    
    public static bool IsProfileSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return string.Equals(parts[1], "Profile", StringComparison.OrdinalIgnoreCase) ? true : false;
    }

    public static bool IsEmailSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return string.Equals(parts[1], "Email", StringComparison.OrdinalIgnoreCase) ? true : false;
    }

    public static bool IsChangePasswordSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return string.Equals(parts[1], "ChangePassword", StringComparison.OrdinalIgnoreCase) ? true : false;
    }

    public static bool IsExternalLoginsSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return string.Equals(parts[1], "ExternalLogins", StringComparison.OrdinalIgnoreCase) ? true : false;
    }

    public static bool IsTwoFactorAuthenticationSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return string.Equals(parts[1], "TwoFactorAuthentication", StringComparison.OrdinalIgnoreCase) ? true : false;
    }

    public static bool IsPersonalDataSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return string.Equals(parts[1], "PersonalData", StringComparison.OrdinalIgnoreCase) ? true : false;
    }

    public static bool IsLoginSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return string.Equals(parts[1], "Login", StringComparison.OrdinalIgnoreCase) ? true : false;
    }

    public static bool IsRegisterSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return string.Equals(parts[1], "Register", StringComparison.OrdinalIgnoreCase) ? true : false;
    }

    public static bool IsForgotPasswordSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return string.Equals(parts[1], "ForgotPassword", StringComparison.OrdinalIgnoreCase) ? true : false;
    }

    public static bool IsResendEmailConfirmationSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return string.Equals(parts[1], "ResendEmailConfirmation", StringComparison.OrdinalIgnoreCase) ? true : false;
    }

    // Privacy
    public static bool IsPrivacySelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      return string.Equals(parts[0], "Privacy", StringComparison.OrdinalIgnoreCase) ? true : false;
    }


    // Consumers
    public static bool IsConsumersExpanded(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      return string.Equals(parts[0], "Cards", StringComparison.OrdinalIgnoreCase) ? true : false;
    }

    public static bool IsConsumersSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return string.Equals(parts[1], "Consumers", StringComparison.OrdinalIgnoreCase) ? true : false;
    }
    public static bool IsUploadSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      if (parts.Length < 2) return false;
      return string.Equals(parts[1], "Upload", StringComparison.OrdinalIgnoreCase) ? true : false;
    }

    // Dashboard
    public static bool IsDashboardSelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      return string.Equals(parts[0], "Dashboard", StringComparison.OrdinalIgnoreCase) ? true : false;
    }

    // History
    public static bool IsHistorySelected(ViewContext viewContext)
    {
      var activePage = viewContext.ViewData["ActivePage"] as string
          ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
      var parts = activePage.Split('|');
      return string.Equals(parts[0], "History", StringComparison.OrdinalIgnoreCase) ? true : false;
    }
  }
}
