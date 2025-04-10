using System;
using System.Collections.Generic;

namespace MessageWeb1.Models;

public partial class ConversationMember
{
    public int Id { get; set; }

    public int ConversationId { get; set; }

    public int UserId { get; set; }

    public DateTime JoinedAt { get; set; }

    public bool IsAdmin { get; set; }

    public DateTime? LeftAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
