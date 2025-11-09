using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class TbRole
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<TbAccount> TbAccounts { get; set; } = new List<TbAccount>();
}
