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

    public virtual DbSet<Testimonial> Testimonials { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAddress> UserAddresses { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<Volunteer> Volunteers { get; set; }

    public virtual DbSet<VolunteerActivity> VolunteerActivities { get; set; }

    public virtual DbSet<VolunteerApplication> VolunteerApplications { get; set; }

    public virtual DbSet<VolunteerAssignment> VolunteerAssignments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("data source= ADMIN-PC\\SQLEXPRESS; initial catalog=LTLHelp; integrated security=True; TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogCategory>(entity =>
        {
            entity.HasKey(e => e.BlogCategoryId).HasName("PK__BlogCate__6BD2DA01A7C83C85");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Slug).HasMaxLength(200);
        });

        modelBuilder.Entity<BlogComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__BlogComm__C3B4DFCA056FCBC5");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.IsApproved).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);

            entity.HasOne(d => d.BlogPost).WithMany(p => p.BlogComments)
                .HasForeignKey(d => d.BlogPostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BlogComme__BlogP__114A936A");

            entity.HasOne(d => d.User).WithMany(p => p.BlogComments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__BlogComme__UserI__123EB7A3");
        });

        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.BlogPostId).HasName("PK__BlogPost__32174169C11D0B9E");

            entity.Property(e => e.ImageUrl).HasMaxLength(300);
            entity.Property(e => e.IsPublished).HasDefaultValue(true);
            entity.Property(e => e.PublishedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Slug).HasMaxLength(250);
            entity.Property(e => e.Summary).HasMaxLength(600);
            entity.Property(e => e.Title).HasMaxLength(250);

            entity.HasOne(d => d.Author).WithMany(p => p.BlogPosts)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("FK__BlogPosts__Autho__1332DBDC");

            entity.HasOne(d => d.BlogCategory).WithMany(p => p.BlogPosts)
                .HasForeignKey(d => d.BlogCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BlogPosts__BlogC__14270015");

            entity.HasMany(d => d.Tags).WithMany(p => p.BlogPosts)
                .UsingEntity<Dictionary<string, object>>(
                    "BlogPostTag",
                    r => r.HasOne<BlogTag>().WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__BlogPostT__TagId__160F4887"),
                    l => l.HasOne<BlogPost>().WithMany()
                        .HasForeignKey("BlogPostId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__BlogPostT__BlogP__151B244E"),
                    j =>
                    {
                        j.HasKey("BlogPostId", "TagId").HasName("PK__BlogPost__E4408EF33F2DB840");
                        j.ToTable("BlogPostTags");
                    });
        });

        modelBuilder.Entity<BlogTag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK__BlogTags__657CF9AC8105380A");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Slug).HasMaxLength(120);
        });

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.CampaignId).HasName("PK__Campaign__3F5E8A99D0D4327F");

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
                .HasConstraintName("FK__Campaigns__Categ__17F790F9");
        });

        modelBuilder.Entity<CampaignCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Campaign__19093A0BC7E65858");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Slug).HasMaxLength(200);
        });

        modelBuilder.Entity<CampaignGallery>(entity =>
        {
            entity.HasKey(e => e.GalleryId).HasName("PK__Campaign__CF4F7BB520C32728");

            entity.ToTable("CampaignGallery");

            entity.Property(e => e.Caption).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.ImageUrl).HasMaxLength(300);

            entity.HasOne(d => d.Campaign).WithMany(p => p.CampaignGalleries)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CampaignG__Campa__17036CC0");
        });

        modelBuilder.Entity<CampaignUpdate>(entity =>
        {
            entity.HasKey(e => e.UpdateId).HasName("PK__Campaign__7A0CF3C5C7DF8D67");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Campaign).WithMany(p => p.CampaignUpdates)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CampaignU__Campa__18EBB532");
        });

        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.HasKey(e => e.ContactId).HasName("PK__ContactM__5C66259B80C58F9B");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.IsHandled).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Subject).HasMaxLength(200);
        });

        modelBuilder.Entity<Donation>(entity =>
        {
            entity.HasKey(e => e.DonationId).HasName("PK__Donation__C5082EFB2EB30682");

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
                .HasConstraintName("FK__Donations__Campa__19DFD96B");

            entity.HasOne(d => d.User).WithMany(p => p.Donations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Donations__UserI__1AD3FDA4");
        });

        modelBuilder.Entity<NewsletterSubscriber>(entity =>
        {
            entity.HasKey(e => e.SubscriberId).HasName("PK__Newslett__7DFEB6D42CBD9AB8");

            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SubscribedAt).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PK__PaymentM__DC31C1D357293009");

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A7BF8DFBB");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.IsSystem).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasKey(e => e.SettingKey).HasName("PK__SystemSe__01E719AC27002A01");

            entity.Property(e => e.SettingKey).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(300);
        });

        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasKey(e => e.TeamMemberId).HasName("PK__TeamMemb__C7C092E531B0CBF3");

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

        modelBuilder.Entity<Testimonial>(entity =>
        {
            entity.HasKey(e => e.TestimonialId).HasName("PK__Testimon__91A23E739FFE51F7");

            entity.Property(e => e.AvatarUrl).HasMaxLength(300);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.Position).HasMaxLength(150);
            entity.Property(e => e.Rating).HasDefaultValue(5);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Transact__55433A6BED201557");

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
                .HasConstraintName("FK__Transacti__Donat__1BC821DD");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacti__Payme__1CBC4616");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CA7D252F2");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105347A9158A7").IsUnique();

            entity.HasIndex(e => e.UserName, "UQ__Users__C9F2845600C0F3BA").IsUnique();

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
                        .HasConstraintName("FK__UserRoles__RoleI__1F98B2C1"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UserRoles__UserI__208CD6FA"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__UserRole__AF2760AD4157CA37");
                        j.ToTable("UserRoles");
                    });
        });

        modelBuilder.Entity<UserAddress>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__UserAddr__091C2AFB441ADC58");

            entity.Property(e => e.AddressLine).HasMaxLength(250);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.Label).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.UserAddresses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserAddre__UserI__1DB06A4F");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__UserProf__1788CC4C85EDEBC5");

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
                .HasConstraintName("FK__UserProfi__UserI__1EA48E88");
        });

        modelBuilder.Entity<Volunteer>(entity =>
        {
            entity.HasKey(e => e.VolunteerId).HasName("PK__Voluntee__716F6F2CB8F29B4F");

            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.JoinedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Hoạt động");

            entity.HasOne(d => d.User).WithMany(p => p.Volunteers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Volunteer__UserI__236943A5");
        });

        modelBuilder.Entity<VolunteerActivity>(entity =>
        {
            entity.HasKey(e => e.ActivityId).HasName("PK__Voluntee__45F4A7910542F3CA");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<VolunteerApplication>(entity =>
        {
            entity.HasKey(e => e.ApplicationId).HasName("PK__Voluntee__C93A4C99404C4CC1");

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
            entity.HasKey(e => e.AssignmentId).HasName("PK__Voluntee__32499E7739D20FC1");

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Đã tham gia");

            entity.HasOne(d => d.Activity).WithMany(p => p.VolunteerAssignments)
                .HasForeignKey(d => d.ActivityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Volunteer__Activ__2180FB33");

            entity.HasOne(d => d.Volunteer).WithMany(p => p.VolunteerAssignments)
                .HasForeignKey(d => d.VolunteerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Volunteer__Volun__22751F6C");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
