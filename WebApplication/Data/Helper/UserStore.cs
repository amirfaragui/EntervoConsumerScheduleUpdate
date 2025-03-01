using T2Importer.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace T2Importer.Identity.EntityFramework
{
  public class UserStore<TUser, TUserLogin> : Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<TUser, ApplicationRole, ApplicationDbContext, Guid,
    IdentityUserClaim<Guid>, IdentityUserRole<Guid>, TUserLogin, IdentityUserToken<Guid>, IdentityRoleClaim<Guid>>
    where TUser : ApplicationUser
    where TUserLogin : ApplicationUserLogin, new()
  {
    private readonly IHttpContextAccessor _httpContextAccessor;



    public UserStore(ApplicationDbContext context, IdentityErrorDescriber describer = null) : base(context, describer)
    
    {
      _httpContextAccessor = null;
    }

    public UserStore(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context, IdentityErrorDescriber describer = null) : base(context, describer)

    {
      _httpContextAccessor = httpContextAccessor;
    }

    private DbSet<TUser> UsersSet { get { return Context.Set<TUser>(); } }
    private DbSet<TUserLogin> UserLogins { get { return Context.Set<TUserLogin>(); } }


    static UserType? _userType;
    private static UserType UserType
    {
      get
      {
        if (_userType == null)
        {
          TUser user = Activator.CreateInstance<TUser>();
          _userType = user.UserType;
        }
        return _userType.Value;
      }
    }

    static UserType? _loginType;
    private static UserType LoginType
    {
      get
      {
        if (_loginType == null)
        {
          TUserLogin user = Activator.CreateInstance<TUserLogin>();
          _loginType = user.UserType;
        }
        return _loginType.Value;
      }
    }



#pragma warning disable CS8603 // Possible null reference return.
    public override async Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      var user = await UsersSet.FirstOrDefaultAsync(i => i.NormalizedEmail == normalizedEmail && i.UserType == UserType, cancellationToken);
      return user;
    }

    public override async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      var id = ConvertIdFromString(userId);
      var user = await UsersSet.FirstOrDefaultAsync(i => i.Id == id && i.UserType == UserType, cancellationToken);
      return user;
    }

    public override async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      var user = await UsersSet.FirstOrDefaultAsync(i => i.NormalizedUserName == normalizedUserName && i.UserType == UserType, cancellationToken);
      return user;
    }

    protected override async Task<TUser> FindUserAsync(Guid userId, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      var user = await UsersSet.FirstOrDefaultAsync(i => i.Id == userId && i.UserType == UserType, cancellationToken);
      return user;
    }

    public override async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      var userLogin = await FindUserLoginAsync(loginProvider, providerKey, cancellationToken);
      if (userLogin != null)
      {
        return await FindUserAsync(userLogin.UserId, cancellationToken);
      }
      return null;
    }

    protected override async Task<TUserLogin> FindUserLoginAsync(Guid userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
      var login = await UserLogins.SingleOrDefaultAsync(userLogin => userLogin.UserId.Equals(userId) && userLogin.LoginProvider == loginProvider && userLogin.ProviderKey == providerKey && userLogin.UserType == LoginType, cancellationToken);
      return login;
    }

    protected override async Task<TUserLogin> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
      var login = await UserLogins.SingleOrDefaultAsync(userLogin => userLogin.LoginProvider == loginProvider && userLogin.ProviderKey == providerKey && userLogin.UserType == LoginType, cancellationToken);
      return login;
    }
#pragma warning restore CS8603 // Possible null reference return.


    protected string? GetUserId()
    {
      if (_httpContextAccessor != null)
      {
        var user = _httpContextAccessor.HttpContext.User;
        if (user?.Identity is ClaimsIdentity identity)
        {
          return identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
      }
      return null;
    }

    public override async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      if (user == null)
      {
        throw new ArgumentNullException(nameof(user));
      }

      Context.Attach(user);
      user.ConcurrencyStamp = Guid.NewGuid().ToString();
      Context.Update(user);
      try
      {
        await Context.SaveChangesAsync(cancellationToken, GetUserId());
      }
      catch (DbUpdateConcurrencyException)
      {
        return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
      }
      return IdentityResult.Success;
    }

    public override async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      if (user == null)
      {
        throw new ArgumentNullException(nameof(user));
      }
      Context.Add(user);
      await Context.SaveChangesAsync(cancellationToken, GetUserId());
      return IdentityResult.Success;
    }

    public override async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      if (user == null)
      {
        throw new ArgumentNullException(nameof(user));
      }

      Context.Remove(user);
      try
      {
        await Context.SaveChangesAsync(cancellationToken, GetUserId());
      }
      catch (DbUpdateConcurrencyException)
      {
        return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
      }
      return IdentityResult.Success;
    }


  }
}
