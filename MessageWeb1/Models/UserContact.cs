using System;
using System.Collections.Generic;

namespace MessageWeb1.Models;

public partial class UserContact
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ContactId { get; set; }

    public string? DisplayName { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User Contact { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
