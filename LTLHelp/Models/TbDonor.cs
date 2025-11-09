using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class TbDonor
{
    public int DonorId { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<TbDonation> TbDonations { get; set; } = new List<TbDonation>();
}
