using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class UserAddress
{
    public int AddressId { get; set; }

    public int UserId { get; set; }

    public string? Label { get; set; }

    public string? AddressLine { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public bool? IsDefault { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
