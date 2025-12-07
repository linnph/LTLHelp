using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public int DonationId { get; set; }

    public int PaymentMethodId { get; set; }

    public string? TransactionCode { get; set; }

    public decimal? Amount { get; set; }

    public string? Currency { get; set; }

    public string? Status { get; set; }

    public DateTime? PaidAt { get; set; }

    public string? RawResponse { get; set; }

    public virtual Donation Donation { get; set; } = null!;

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}
