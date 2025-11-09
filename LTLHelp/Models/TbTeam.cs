using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class TbTeam
{
    public int TeamId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Position { get; set; }

    public string? Avatar { get; set; }

    public string? Bio { get; set; }

    public string? Facebook { get; set; }

    public string? Twitter { get; set; }

    public string? Linkedin { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
}
