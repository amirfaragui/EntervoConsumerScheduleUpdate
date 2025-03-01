using T2Importer.DAL.Attributes;
using T2Importer.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
#nullable disable

namespace T2Importer.DAL
{
  public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>, IdentityUserRole<Guid>, ApplicationUserLogin, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
  {
    public DbSet<Audit> DbAuditLogs { get; set; }
    public DbSet<WebAccessAuditLog> AccessLogs { get; set; }
    public DbSet<Setting> Settings { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Batch> Batchs { get; set; }
    public DbSet<Export01Batch> Export01Batchs { get; set; }
    public DbSet<Export04Batch> Export04Batchs { get; set; }

    public DbSet<Card> Cards { get; set; }
    public DbSet<Consumer> Consumers { get; set; }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Permit> Permits { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<CustomerNote> CustomerNotes { get; set; }
    public DbSet<PermitNote> PermitNotes { get; set; }
    public DbSet<Email> Emails { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }

    /// <summary>
    /// Represents individule user in each location
    /// </summary>


    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public override void Dispose()
    {
      base.Dispose();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      builder.Entity<ApplicationUser>(b =>
      {
        var oldNameIndex = b.HasIndex(e => e.NormalizedUserName).Metadata;
        b.Metadata.RemoveIndex(oldNameIndex);
        var oldEmailIndex = b.HasIndex(e => e.NormalizedEmail).Metadata;
        b.Metadata.RemoveIndex(oldEmailIndex);

        b.HasIndex(e => new { e.UserType, e.NormalizedUserName }).HasDatabaseName("UserNameIndex").IsUnique().HasFilter("[NormalizedUserName] IS NOT NULL");
        b.HasIndex(e => new { e.UserType, e.NormalizedEmail }).HasDatabaseName("EmailIndex").IsUnique().HasFilter("[NormalizedEmail] IS NOT NULL");

        b.HasDiscriminator(e => e.UserType)
          .HasValue<AdminPortalUser>(UserType.AdminPortalUser)
          .HasValue<CustomerPortalUser>(UserType.CustomerPortalUser);
      });

      builder.Entity<ApplicationUserLogin>(b =>
      {
        var oldPrimaryKey = b.HasKey(e => new { e.LoginProvider, e.ProviderKey }).Metadata;
        b.Metadata.RemoveKey(oldPrimaryKey);
        b.HasKey(e => new { e.LoginProvider, e.ProviderKey, e.UserType });

        var oldIndex = b.HasIndex(e => e.UserId).Metadata;
        b.Metadata.RemoveIndex(oldIndex);
        b.HasIndex(e => new { e.UserId, e.UserType });

        b.HasDiscriminator(e => e.UserType)
          .HasValue<AdminPortalUserLogin>(UserType.AdminPortalUser)
          .HasValue<CustomerPortalUserLogin>(UserType.CustomerPortalUser);
      });

      builder.Entity<CustomerPortalUser>(b =>
      {
        b.Property(e => e.TimeRegistered).HasDefaultValueSql("SYSDATETIMEOFFSET()");
      });

      builder.Entity<Audit>(b =>
      {
        b.HasIndex(e => e.UserId);
        b.HasIndex(e => e.TableName);
      });

      builder.Entity<Event>(b =>
      {
        b.HasIndex(e => new { e.Time }).IsDescending();
      });

      builder.Entity<Customer>(b =>
      {
        b.Property(e => e.CustomerUID).ValueGeneratedNever();
        b.HasMany(e => e.Permits).WithOne(e => e.Customer).HasForeignKey(e => e.PurchasingCustomer).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(e => e.Emails).WithOne().HasForeignKey(e => e.CustomerUID).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(e => e.Notes).WithOne().HasForeignKey(e => e.SourceObjectUID).OnDelete(DeleteBehavior.Cascade);
      });

      builder.Entity<Permit>(b =>
      {
        b.Property(e => e.PermitUID).ValueGeneratedNever();
        b.HasMany(e => e.Notes).WithOne().HasForeignKey(e => e.SourceObjectUID);
      });

      //builder.Entity<Note>(b =>
      //{
      //  b.Property(e => e.NoteUID).ValueGeneratedNever();
      //  b.HasDiscriminator<int>("TableUIDofSourceObject")
      //    .HasValue<Note>(46)
      //    .HasValue<CustomerNote>(1)
      //    .HasValue<PermitNote>(10);
          
      //});

      builder.Entity<Email>(b =>
      {
        b.Property(e => e.EmailAddressUID).ValueGeneratedNever();
      });

      builder.Entity<Vehicle>(b =>
      {
        b.Property(e => e.VehicleUID).ValueGeneratedNever();
      });
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken, string? userId = null)
    {
      OnBeforeSaveChanges(userId);
      return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
    public virtual async Task<int> SaveChangesAsync(string? userId = null)
    {
      OnBeforeSaveChanges(userId);
      return await base.SaveChangesAsync(CancellationToken.None);
    }

    private void OnBeforeSaveChanges(string? userId)
    {
      ChangeTracker.DetectChanges();
      var auditEntries = new List<AuditEntry>();
      foreach (var entry in ChangeTracker.Entries())
      {
        if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
          continue;

        var entityType = entry.Entity.GetType();
        if (entityType.GetCustomAttribute(typeof(AuditIgnoreAttribute), false) != null) continue;

        var auditEntry = new AuditEntry(entry);
        auditEntry.TableName = Model.FindEntityType(entityType)?.GetTableName();
        auditEntry.UserId = userId;
        auditEntries.Add(auditEntry);

        var ignoredEntityProperties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance).OfType<PropertyInfo>()
          .ToDictionary(i => i.Name, i => i.GetCustomAttribute(typeof(AuditIgnoreAttribute), true))
          .Where(i => i.Value != null)
          .Select(i => i.Key)
          .ToArray();

        foreach (var property in entry.Properties)
        {
          string propertyName = property.Metadata.Name;


          if (ignoredEntityProperties.Contains(propertyName)) continue;

          if (property.Metadata.IsPrimaryKey())
          {
            auditEntry.KeyValues[propertyName] = property.CurrentValue;
            continue;
          }
          switch (entry.State)
          {
            case EntityState.Added:
              auditEntry.AuditType = AuditType.Create;
              auditEntry.NewValues[propertyName] = property.CurrentValue;
              break;
            case EntityState.Deleted:
              auditEntry.AuditType = AuditType.Delete;
              auditEntry.OldValues[propertyName] = property.OriginalValue;
              break;
            case EntityState.Modified:
              if (property.IsModified)
              {
                var oldValue = property.OriginalValue;
                var newValue = property.CurrentValue;
                if (oldValue != null && !oldValue.Equals(newValue))
                {
                  auditEntry.ChangedColumns.Add(propertyName);
                  auditEntry.AuditType = AuditType.Update;
                  auditEntry.OldValues[propertyName] = oldValue;
                  auditEntry.NewValues[propertyName] = newValue;
                }
                else if (newValue != null && !newValue.Equals(oldValue))
                {
                  auditEntry.ChangedColumns.Add(propertyName);
                  auditEntry.AuditType = AuditType.Update;
                  auditEntry.OldValues[propertyName] = oldValue;
                  auditEntry.NewValues[propertyName] = newValue;
                }
              }
              break;
          }
        }
      }
      foreach (var auditEntry in auditEntries)
      {
        if (auditEntry.AuditType != AuditType.None) DbAuditLogs.Add(auditEntry.ToAudit());
      }
    }
  }
}