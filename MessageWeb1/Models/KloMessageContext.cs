using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MessageWeb1.Models;

public partial class KloMessageContext : DbContext
{
    public KloMessageContext()
    {
    }

    public KloMessageContext(DbContextOptions<KloMessageContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<ConversationMember> ConversationMembers { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<MessageReadStatus> MessageReadStatuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserContact> UserContacts { get; set; }

    private string GetConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();
        var strConn = config["ConnectionStrings:DefaultConnectionStringDB"];
        //                          DẤU CÁCH VÀ 9 TRIỆU (6T + 3T
        //Console.Write($"🔍 Chuỗi kết nối đọc được: {strConn}1"); // Debug
        //if (string.IsNullOrEmpty(strConn)) {
        //    throw new Exception("⚠ Lỗi: Không tìm thấy chuỗi kết nối trong appsettings.json!");
        //}
        return strConn;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(GetConnectionString());
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.ConversationId).HasName("PK__Conversa__C050D8772D62D46F");

            entity.HasIndex(e => e.ConversationType, "IX_Conversations_Type");

            entity.Property(e => e.ConversationType).HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<ConversationMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Conversa__3214EC079F1DF213");

            entity.HasIndex(e => new { e.ConversationId, e.UserId }, "IX_ConversationMembers_ConversationId_UserId");

            entity.HasIndex(e => new { e.ConversationId, e.UserId }, "UQ_ConversationMembers").IsUnique();

            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LeftAt).HasColumnType("datetime");

            entity.HasOne(d => d.Conversation).WithMany(p => p.ConversationMembers)
                .HasForeignKey(d => d.ConversationId)
                .HasConstraintName("FK_ConversationMembers_Conversations");

            entity.HasOne(d => d.User).WithMany(p => p.ConversationMembers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ConversationMembers_Users");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Messages__C87C0C9C4D48849F");

            entity.HasIndex(e => new { e.ConversationId, e.SentAt }, "IX_Messages_ConversationId_SentAt");

            entity.HasIndex(e => e.SenderId, "IX_Messages_SenderId");

            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationId)
                .HasConstraintName("FK_Messages_Conversations");

            entity.HasOne(d => d.Sender).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Messages_Users");
        });

        modelBuilder.Entity<MessageReadStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MessageR__3214EC07EC18F7DE");

            entity.ToTable("MessageReadStatus");

            entity.HasIndex(e => e.UserId, "IX_MessageReadStatus_UserId");

            entity.HasIndex(e => new { e.MessageId, e.UserId }, "UQ_MessageReadStatus").IsUnique();

            entity.Property(e => e.ReadAt).HasColumnType("datetime");

            entity.HasOne(d => d.Message).WithMany(p => p.MessageReadStatuses)
                .HasForeignKey(d => d.MessageId)
                .HasConstraintName("FK_MessageReadStatus_Messages");

            entity.HasOne(d => d.User).WithMany(p => p.MessageReadStatuses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_MessageReadStatus_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CA7E365CA");

            entity.HasIndex(e => e.Email, "IX_Users_Email");

            entity.HasIndex(e => e.Username, "IX_Users_Username");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E44781BC64").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534F2A32D77").IsUnique();

            entity.Property(e => e.AvatarUrl).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.LastSeen).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<UserContact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserCont__3214EC071974FD80");

            entity.HasIndex(e => new { e.UserId, e.ContactId }, "UQ_UserContacts").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DisplayName).HasMaxLength(100);

            entity.HasOne(d => d.Contact).WithMany(p => p.UserContactContacts)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserContacts_Contact");

            entity.HasOne(d => d.User).WithMany(p => p.UserContactUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserContacts_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
