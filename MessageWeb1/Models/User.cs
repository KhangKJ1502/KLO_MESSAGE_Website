using System;
using System.Collections.Generic;

namespace MessageWeb1.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? FullName { get; set; }

    public string? AvatarUrl { get; set; }

    public bool IsOnline { get; set; }

    public DateTime? LastSeen { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<ConversationMember> ConversationMembers { get; set; } = new List<ConversationMember>();

    public virtual ICollection<MessageReadStatus> MessageReadStatuses { get; set; } = new List<MessageReadStatus>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<UserContact> UserContactContacts { get; set; } = new List<UserContact>();

    public virtual ICollection<UserContact> UserContactUsers { get; set; } = new List<UserContact>();
}
