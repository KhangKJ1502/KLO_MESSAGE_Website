using System;
using System.Collections.Generic;

namespace MessageWeb1.Models;

public partial class Conversation
{
    public int ConversationId { get; set; }

    public string ConversationType { get; set; } = null!;

    public string? Title { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int? LastMessageId { get; set; }

    public virtual ICollection<ConversationMember> ConversationMembers { get; set; } = new List<ConversationMember>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
