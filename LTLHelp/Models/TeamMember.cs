using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class TeamMember
{
    public int TeamMemberId { get; set; }

    public string? FullName { get; set; }

    public string? Position { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Experience { get; set; }

    public string? Bio { get; set; }

    public string? Facebook { get; set; }

    public string? Twitter { get; set; }

    public string? Linkedin { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
}
