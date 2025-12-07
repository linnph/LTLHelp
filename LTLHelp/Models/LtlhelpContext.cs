using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LTLHelp.Models;

public partial class LtlhelpContext : DbContext
{
    public LtlhelpContext()
    {
    }

    public LtlhelpContext(DbContextOptions<LtlhelpContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BlogCategory> BlogCategories { get; set; }

    public virtual DbSet<BlogComment> BlogComments { get; set; }

    public virtual DbSet<BlogPost> BlogPosts { get; set; }

    public virtual DbSet<BlogTag> BlogTags { get; set; }

    public virtual DbSet<Campaign> Campaigns { get; set; }

    public virtual DbSet<CampaignCategory> CampaignCategories { get; set; }

    public virtual DbSet<CampaignGallery> CampaignGalleries { get; set; }

    public virtual DbSet<CampaignUpdate> CampaignUpdates { get; set; }

    public virtual DbSet<ContactMessage> ContactMessages { get; set; }

    public virtual DbSet<Donation> Donations { get; set; }

    public virtual DbSet<NewsletterSubscriber> NewsletterSubscribers { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SystemSetting> SystemSettings { get; set; }

    public virtual DbSet<TeamMember> TeamMembers { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAddress> UserAddresses { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<Volunteer> Volunteers { get; set; }

    public virtual DbSet<VolunteerActivity> VolunteerActivities { get; set; }

    public virtual DbSet<VolunteerApplication> VolunteerApplications { get; set; }

    public virtual DbSet<VolunteerAssignment> VolunteerAssignments { get; set; }

 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogCategory>(entity =>
        {
            entity.HasKey(e => e.BlogCategoryId).HasName("PK__BlogCate__6BD2DA01B7D930D9");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Slug).HasMaxLength(200);
        });

        modelBuilder.Entity<BlogComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__BlogComm__C3B4DFCA20997723");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.IsApproved).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);

            entity.HasOne(d => d.BlogPost).WithMany(p => p.BlogComments)
                .HasForeignKey(d => d.BlogPostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BlogComme__BlogP__72910220");

            entity.HasOne(d => d.User).WithMany(p => p.BlogComments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__BlogComme__UserI__73852659");
        });

        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.BlogPostId).HasName("PK__BlogPost__3217416989AC0574");

            entity.Property(e => e.ImageUrl).HasMaxLength(300);
            entity.Property(e => e.IsPublished).HasDefaultValue(true);
            entity.Property(e => e.PublishedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Slug).HasMaxLength(250);
            entity.Property(e => e.Summary).HasMaxLength(600);
            entity.Property(e => e.Title).HasMaxLength(250);

            entity.HasOne(d => d.Author).WithMany(p => p.BlogPosts)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("FK__BlogPosts__Autho__681373AD");

            entity.HasOne(d => d.BlogCategory).WithMany(p => p.BlogPosts)
                .HasForeignKey(d => d.BlogCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BlogPosts__BlogC__671F4F74");

            entity.HasMany(d => d.Tags).WithMany(p => p.BlogPosts)
                .UsingEntity<Dictionary<string, object>>(
                    "BlogPostTag",
                    r => r.HasOne<BlogTag>().WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__BlogPostT__TagId__6DCC4D03"),
                    l => l.HasOne<BlogPost>().WithMany()
                        .HasForeignKey("BlogPostId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__BlogPostT__BlogP__6CD828CA"),
                    j =>
                    {
                        j.HasKey("BlogPostId", "TagId").HasName("PK__BlogPost__E4408EF3395A50C5");
                        j.ToTable("BlogPostTags");
                    });
        });

        modelBuilder.Entity<BlogTag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK__BlogTags__657CF9AC70C033DA");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Slug).HasMaxLength(120);
        });

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.CampaignId).HasName("PK__Campaign__3F5E8A995D06BD62");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.GoalAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ImageUrl).HasMaxLength(300);
            entity.Property(e => e.OrganizerAvatar).HasMaxLength(300);
            entity.Property(e => e.OrganizerName).HasMaxLength(150);
            entity.Property(e => e.RaisedAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Slug).HasMaxLength(250);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Đang diễn ra");
            entity.Property(e => e.Summary).HasMaxLength(600);
            entity.Property(e => e.Title).HasMaxLength(250);

            entity.HasOne(d => d.Category).WithMany(p => p.Campaigns)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Campaigns__Categ__32AB8735");
        });

        modelBuilder.Entity<CampaignCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Campaign__19093A0B0F34EC7A");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Slug).HasMaxLength(200);
        });

        modelBuilder.Entity<CampaignGallery>(entity =>
        {
            entity.HasKey(e => e.GalleryId).HasName("PK__Campaign__CF4F7BB52CB9043E");

            entity.ToTable("CampaignGallery");

            entity.Property(e => e.Caption).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.ImageUrl).HasMaxLength(300);

            entity.HasOne(d => d.Campaign).WithMany(p => p.CampaignGalleries)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CampaignG__Campa__3A4CA8FD");
        });

        modelBuilder.Entity<CampaignUpdate>(entity =>
        {
            entity.HasKey(e => e.UpdateId).HasName("PK__Campaign__7A0CF3C50CD2D8F8");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Campaign).WithMany(p => p.CampaignUpdates)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CampaignU__Campa__367C1819");
        });

        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.HasKey(e => e.ContactId).HasName("PK__ContactM__5C66259BF0EA2E56");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.IsHandled).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Subject).HasMaxLength(200);
        });

        modelBuilder.Entity<Donation>(entity =>
        {
            entity.HasKey(e => e.DonationId).HasName("PK__Donation__C5082EFB93D947C9");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.DonorEmail).HasMaxLength(200);
            entity.Property(e => e.DonorMessage).HasMaxLength(1000);
            entity.Property(e => e.DonorName).HasMaxLength(200);
            entity.Property(e => e.IsAnonymous).HasDefaultValue(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Chờ xác nhận");

            entity.HasOne(d => d.Campaign).WithMany(p => p.Donations)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Donations__Campa__42E1EEFE");

            entity.HasOne(d => d.User).WithMany(p => p.Donations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Donations__UserI__43D61337");
        });

        modelBuilder.Entity<NewsletterSubscriber>(entity =>
        {
            entity.HasKey(e => e.SubscriberId).HasName("PK__Newslett__7DFEB6D4C5753282");

            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SubscribedAt).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PK__PaymentM__DC31C1D3043E31F6");

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A4E93549C");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.IsSystem).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasKey(e => e.SettingKey).HasName("PK__SystemSe__01E719AC7A79ED29");

            entity.Property(e => e.SettingKey).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(300);
        });

        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasKey(e => e.TeamMemberId).HasName("PK__TeamMemb__C7C092E59A563359");

            entity.Property(e => e.AvatarUrl).HasMaxLength(300);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Experience).HasMaxLength(200);
            entity.Property(e => e.Facebook).HasMaxLength(250);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Linkedin).HasMaxLength(250);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Position).HasMaxLength(150);
            entity.Property(e => e.Twitter).HasMaxLength(250);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Transact__55433A6B5A451197");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasDefaultValue("VND");
            entity.Property(e => e.PaidAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Thành công");
            entity.Property(e => e.TransactionCode).HasMaxLength(100);

            entity.HasOne(d => d.Donation).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.DonationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacti__Donat__498EEC8D");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacti__Payme__4A8310C6");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C0E4C36A8");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053495E20769").IsUnique();

            entity.HasIndex(e => e.UserName, "UQ__Users__C9F28456AA136DE1").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.UserName).HasMaxLength(100);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UserRoles__RoleI__208CD6FA"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UserRoles__UserI__1F98B2C1"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__UserRole__AF2760ADF1F2D431");
                        j.ToTable("UserRoles");
                    });
        });

        modelBuilder.Entity<UserAddress>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__UserAddr__091C2AFBBB3593FC");

            entity.Property(e => e.AddressLine).HasMaxLength(250);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.Label).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.UserAddresses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserAddre__UserI__282DF8C2");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__UserProf__1788CC4C68FAAB7E");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.AddressLine).HasMaxLength(250);
            entity.Property(e => e.AvatarUrl).HasMaxLength(300);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(50);

            entity.HasOne(d => d.User).WithOne(p => p.UserProfile)
                .HasForeignKey<UserProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserProfi__UserI__236943A5");
        });

        modelBuilder.Entity<Volunteer>(entity =>
        {
            entity.HasKey(e => e.VolunteerId).HasName("PK__Voluntee__716F6F2C87ED517E");

            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.JoinedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Hoạt động");

            entity.HasOne(d => d.User).WithMany(p => p.Volunteers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Volunteer__UserI__4F47C5E3");
        });

        modelBuilder.Entity<VolunteerActivity>(entity =>
        {
            entity.HasKey(e => e.ActivityId).HasName("PK__Voluntee__45F4A7918EE67488");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<VolunteerApplication>(entity =>
        {
            entity.HasKey(e => e.ApplicationId).HasName("PK__Voluntee__C93A4C99E9AA8A2D");

            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.Occupation).HasMaxLength(150);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Chờ duyệt");
        });

        modelBuilder.Entity<VolunteerAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__Voluntee__32499E77A1432545");

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Đã tham gia");

            entity.HasOne(d => d.Activity).WithMany(p => p.VolunteerAssignments)
                .HasForeignKey(d => d.ActivityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Volunteer__Activ__5BAD9CC8");

            entity.HasOne(d => d.Volunteer).WithMany(p => p.VolunteerAssignments)
                .HasForeignKey(d => d.VolunteerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Volunteer__Volun__5AB9788F");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
