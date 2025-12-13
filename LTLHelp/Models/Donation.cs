using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace LTLHelp.Models;

public partial class Donation
{
    public int DonationId { get; set; }

    public int CampaignId { get; set; } 

    public int? UserId { get; set; }

    public string? DonorName { get; set; }

    public string? DonorEmail { get; set; }

    public decimal Amount { get; set; }

    public bool? IsAnonymous { get; set; }

    public string? DonorMessage { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    [ValidateNever]
    public virtual Campaign Campaign { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    [ValidateNever]
    public virtual User? User { get; set; }
}
