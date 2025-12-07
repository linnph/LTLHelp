using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class UserProfile
{
    public int UserId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public string? AvatarUrl { get; set; }

    public string? AddressLine { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public string? Bio { get; set; }

    public virtual User User { get; set; } = null!;
}
