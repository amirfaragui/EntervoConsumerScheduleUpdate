using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace T2Importer.Identity
{
  public class ClaimsTransformer<TUser> : IClaimsTransformation where TUser : ApplicationUser
  {
    private readonly UserManager<TUser> _userManager;

    public ClaimsTransformer(UserManager<TUser> userManager)
    {
      _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
      if (principal.Identity is ClaimsIdentity existingClaimsIdentity)
      {
        var nameClaim = existingClaimsIdentity.FindFirst(ClaimTypes.Name);
        var emailClaim = existingClaimsIdentity.FindFirst(ClaimTypes.Email);
        if (nameClaim != null && emailClaim != null)
        {
          if (string.Compare(nameClaim?.Value, emailClaim?.Value, StringComparison.OrdinalIgnoreCase) == 0)
          {
            var claims = new List<Claim>(existingClaimsIdentity.Claims);
            claims.Remove(nameClaim);
            var user = await _userManager.FindByEmailAsync(emailClaim.Value);
            claims.Add(new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()));

            var newClaimsIdentity = new ClaimsIdentity(claims, existingClaimsIdentity.AuthenticationType);
            return new ClaimsPrincipal(newClaimsIdentity);
          }
        }
      }
      return principal;
    }
  }
}