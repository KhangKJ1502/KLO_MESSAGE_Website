using System;
using System.Collections.Generic;

namespace MessageWeb1.Models;

public partial class MessageReadStatus
{
    public int Id { get; set; }

    public int MessageId { get; set; }

    public int UserId { get; set; }

    public DateTime? ReadAt { get; set; }

    public virtual Message Message { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
