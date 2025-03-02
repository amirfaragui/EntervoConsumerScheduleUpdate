using Entrvo.DAL;
using Entrvo.DAL.Attributes;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entrvo.Identity
{
  public enum UserType : int
  {
    CustomerPortalUser = 0,
    AdminPortalUser = 1,
  }

  public abstract class ApplicationUser : IdentityUser<Guid>
  {
    [StringLength(36)]
    public string? FirstName { get; set; }

    [StringLength(36)]
    public string? LastName { get; set; }

    [AuditIgnore]
    public DateTimeOffset? LastTimeSignedIn { get; set; }

    [AuditIgnore]
    public override string SecurityStamp { get => base.SecurityStamp; set => base.SecurityStamp = value; }

    [AuditIgnore]
    public override string ConcurrencyStamp { get => base.ConcurrencyStamp; set => base.ConcurrencyStamp = value; }

    public virtual UserType UserType { get; set; }
  }

  public class CustomerPortalUser : ApplicationUser
  {

    [StringLength(256)]
    public string Comment { get; set; }
    [PersonalData]
    [StringLength(36)]
    public string HomePhoneNumber { get; set; }
    [PersonalData]
    [StringLength(36)]
    public string WorkPhoneNumber { get; set; }
    public DateTimeOffset TimeRegistered { get; set; }
    [Column("IsActive")] 
    public bool IsActive { get; set; }

    public CustomerPortalUser()
    {
      UserType = UserType.CustomerPortalUser;
      Id = Guid.NewGuid();
      TimeRegistered = DateTimeOffset.Now;
      IsActive = true;
    }

  }

  public class AdminPortalUser : ApplicationUser
  {
    [Column("IsActive")]
    public bool IsActive { get; set; }

    public AdminPortalUser()
    {
      UserType = UserType.AdminPortalUser;
      IsActive = true;
    }

  }

}
