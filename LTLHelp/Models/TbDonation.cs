using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class TbDonation
{
    public int DonationId { get; set; }

    public int? DonorId { get; set; }

    public int? CampaignId { get; set; }

    public decimal Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual TbCampaign? Campaign { get; set; }

    public virtual TbDonor? Donor { get; set; }
}
