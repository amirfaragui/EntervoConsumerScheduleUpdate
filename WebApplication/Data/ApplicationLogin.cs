using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Entrvo.Identity
{

  public abstract class ApplicationUserLogin : IdentityUserLogin<Guid>
  {
    public virtual UserType UserType { get; set; }
    public ApplicationUserLogin() { }
  }

  public class CustomerPortalUserLogin : ApplicationUserLogin
  {

    public CustomerPortalUserLogin()
    {
      UserType = UserType.CustomerPortalUser;
    }
  }

  public class AdminPortalUserLogin : ApplicationUserLogin
  {
    public AdminPortalUserLogin()
    {
      UserType = UserType.AdminPortalUser;
    }

  }

}
