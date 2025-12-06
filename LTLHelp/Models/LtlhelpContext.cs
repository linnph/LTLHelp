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

    public virtual DbSet<TbAccount> TbAccounts { get; set; }

    public virtual DbSet<TbBlog> TbBlogs { get; set; }

    public virtual DbSet<TbBlogComment> TbBlogComments { get; set; }

    public virtual DbSet<TbCampaign> TbCampaigns { get; set; }

    public virtual DbSet<TbCategory> TbCategories { get; set; }

    public virtual DbSet<TbContact> TbContacts { get; set; }

    public virtual DbSet<TbDonation> TbDonations { get; set; }

    public virtual DbSet<TbDonor> TbDonors { get; set; }

    public virtual DbSet<TbFaq> TbFaqs { get; set; }

    public virtual DbSet<TbGallery> TbGalleries { get; set; }

    public virtual DbSet<TbMenu> TbMenus { get; set; }

    public virtual DbSet<TbRole> TbRoles { get; set; }

    public virtual DbSet<TbTeam> TbTeams { get; set; }

    public virtual DbSet<TbTestimonial> TbTestimonials { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TbAccount>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__tb_Accou__349DA586154A3607");

            entity.ToTable("tb_Account");

            entity.HasIndex(e => e.Email, "UQ__tb_Accou__A9D1053490B82307").IsUnique();

            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.Avatar).HasMaxLength(300);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");

            entity.HasOne(d => d.Role).WithMany(p => p.TbAccounts)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Account_Role");
        });

        modelBuilder.Entity<TbBlog>(entity =>
        {
            entity.HasKey(e => e.BlogId).HasName("PK__tb_Blog__54379E504612C0E0");

            entity.ToTable("tb_Blog");

            entity.Property(e => e.BlogId).HasColumnName("BlogID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ImageUrl).HasMaxLength(400);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ShortDesc).HasMaxLength(600);
            entity.Property(e => e.Slug).HasMaxLength(300);
            entity.Property(e => e.Title).HasMaxLength(300);

            entity.HasOne(d => d.Category).WithMany(p => p.TbBlogs)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__tb_Blog__Categor__68487DD7");
        });

        modelBuilder.Entity<TbBlogComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__tb_BlogC__C3B4DFAA8EFA18CB");

            entity.ToTable("tb_BlogComment");

            entity.Property(e => e.CommentId).HasColumnName("CommentID");
            entity.Property(e => e.BlogId).HasColumnName("BlogID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Message).HasMaxLength(2000);
            entity.Property(e => e.Name).HasMaxLength(150);

            entity.HasOne(d => d.Blog).WithMany(p => p.TbBlogComments)
                .HasForeignKey(d => d.BlogId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tb_BlogCo__BlogI__6C190EBB");
        });

        modelBuilder.Entity<TbCampaign>(entity =>
        {
            entity.HasKey(e => e.CampaignId).HasName("PK__tb_Campa__3F5E8D790D9F57AF");

            entity.ToTable("tb_Campaign");

            entity.Property(e => e.CampaignId).HasColumnName("CampaignID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.GoalAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ImageUrl).HasMaxLength(400);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RaisedAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ShortDesc).HasMaxLength(500);
            entity.Property(e => e.Slug).HasMaxLength(250);
            entity.Property(e => e.Title).HasMaxLength(250);
        });

        modelBuilder.Entity<TbCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__tb_Categ__19093A2BCFAA6A0F");

            entity.ToTable("tb_Category");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(150);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Slug).HasMaxLength(200);
        });

        modelBuilder.Entity<TbContact>(entity =>
        {
            entity.HasKey(e => e.ContactId).HasName("PK__tb_Conta__5C6625BB8973882B");

            entity.ToTable("tb_Contact");

            entity.Property(e => e.ContactId).HasColumnName("ContactID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(50);
        });

        modelBuilder.Entity<TbDonation>(entity =>
        {
            entity.HasKey(e => e.DonationId).HasName("PK__tb_Donat__C5082EDB857461DD");

            entity.ToTable("tb_Donation");

            entity.Property(e => e.DonationId).HasColumnName("DonationID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CampaignId).HasColumnName("CampaignID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DonorId).HasColumnName("DonorID");
            entity.Property(e => e.PaymentMethod).HasMaxLength(100);

            entity.HasOne(d => d.Campaign).WithMany(p => p.TbDonations)
                .HasForeignKey(d => d.CampaignId)
                .HasConstraintName("FK__tb_Donati__Campa__7E37BEF6");

            entity.HasOne(d => d.Donor).WithMany(p => p.TbDonations)
                .HasForeignKey(d => d.DonorId)
                .HasConstraintName("FK__tb_Donati__Donor__7D439ABD");
        });

        modelBuilder.Entity<TbDonor>(entity =>
        {
            entity.HasKey(e => e.DonorId).HasName("PK__tb_Donor__052E3F98566B1F4A");

            entity.ToTable("tb_Donor");

            entity.Property(e => e.DonorId).HasColumnName("DonorID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(50);
        });

        modelBuilder.Entity<TbFaq>(entity =>
        {
            entity.HasKey(e => e.FaqId).HasName("PK__tb_Faq__9C741C23AAE17E76");

            entity.ToTable("tb_Faq");

            entity.Property(e => e.FaqId).HasColumnName("FaqID");
            entity.Property(e => e.Category).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Question).HasMaxLength(500);
        });

        modelBuilder.Entity<TbGallery>(entity =>
        {
            entity.HasKey(e => e.GalleryId).HasName("PK__tb_Galle__CF4F7B95F62879EA");

            entity.ToTable("tb_Gallery");

            entity.Property(e => e.GalleryId).HasColumnName("GalleryID");
            entity.Property(e => e.CampaignId).HasColumnName("CampaignID");
            entity.Property(e => e.Caption).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ImageUrl).HasMaxLength(400);
            entity.Property(e => e.Title).HasMaxLength(250);

            entity.HasOne(d => d.Campaign).WithMany(p => p.TbGalleries)
                .HasForeignKey(d => d.CampaignId)
                .HasConstraintName("FK__tb_Galler__Campa__6383C8BA");
        });

        modelBuilder.Entity<TbMenu>(entity =>
        {
            entity.HasKey(e => e.MenuId).HasName("PK__tb_Menu__C99ED250DBDB2FD7");

            entity.ToTable("tb_Menu");

            entity.Property(e => e.MenuId).HasColumnName("MenuID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.CssClass).HasMaxLength(100);
            entity.Property(e => e.Icon).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.MenuName).HasMaxLength(150);
            entity.Property(e => e.MetaDescription).HasMaxLength(500);
            entity.Property(e => e.MetaKeywords).HasMaxLength(500);
            entity.Property(e => e.MetaTitle).HasMaxLength(250);
            entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            entity.Property(e => e.OrderIndex).HasDefaultValue(0);
            entity.Property(e => e.ParentId).HasColumnName("ParentID");
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.Property(e => e.Target).HasMaxLength(20);
            entity.Property(e => e.Url).HasMaxLength(300);
        });

        modelBuilder.Entity<TbRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__tb_Role__8AFACE3A116695B4");

            entity.ToTable("tb_Role");

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<TbTeam>(entity =>
        {
            entity.HasKey(e => e.TeamId).HasName("PK__tb_Team__123AE7B990509E89");

            entity.ToTable("tb_Team");

            entity.Property(e => e.TeamId).HasColumnName("TeamID");
            entity.Property(e => e.Avatar).HasMaxLength(400);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Facebook).HasMaxLength(250);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Linkedin).HasMaxLength(250);
            entity.Property(e => e.Position).HasMaxLength(150);
            entity.Property(e => e.Twitter).HasMaxLength(250);
        });

        modelBuilder.Entity<TbTestimonial>(entity =>
        {
            entity.HasKey(e => e.TestimonialId).HasName("PK__tb_Testi__91A23E5375DC2379");

            entity.ToTable("tb_Testimonial");

            entity.Property(e => e.TestimonialId).HasColumnName("TestimonialID");
            entity.Property(e => e.Avatar).HasMaxLength(400);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DonorName).HasMaxLength(200);
            entity.Property(e => e.DonorRole).HasMaxLength(200);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
