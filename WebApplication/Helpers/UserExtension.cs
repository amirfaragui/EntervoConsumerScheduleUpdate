using System.Security.Claims;

namespace Entrvo
{
  static class UserExtension
  {
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
      if (user.Identity.IsAuthenticated)
      {
        if (Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id))
        {
          return id;
        }
      }
      return null;
    }
  }
}
