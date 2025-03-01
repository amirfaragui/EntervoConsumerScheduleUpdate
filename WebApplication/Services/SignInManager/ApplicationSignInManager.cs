using T2Importer.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace T2WebApplication.Identity
{
  public class ApplicationSignInManager : SignInManager<CustomerPortalUser>
  {
    public ApplicationSignInManager(UserManager<CustomerPortalUser> userManager,
                                    IHttpContextAccessor contextAccessor, 
                                    IUserClaimsPrincipalFactory<CustomerPortalUser> claimsFactory, 
                                    IOptions<IdentityOptions> optionsAccessor, 
                                    ILogger<SignInManager<CustomerPortalUser>> logger, 
                                    IAuthenticationSchemeProvider schemes, 
                                    IUserConfirmation<CustomerPortalUser> confirmation)
      : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
    }


    protected async Task OnUserSignedIn(CustomerPortalUser user)
    {
      user.LastTimeSignedIn = DateTimeOffset.Now;
      await UserManager.UpdateAsync(user);
    }

    public override async Task SignInWithClaimsAsync(CustomerPortalUser user, AuthenticationProperties authenticationProperties, IEnumerable<Claim> additionalClaims)
    {
      var userPrincipal = await CreateUserPrincipalAsync(user);
      foreach (var claim in additionalClaims)
      {
        userPrincipal.Identities.First().AddClaim(claim);
      }
      await Context.SignInAsync(IdentityConstants.ApplicationScheme,
          userPrincipal,
          authenticationProperties ?? new AuthenticationProperties());

      await OnUserSignedIn(user);
    }
  }
}
