using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class TbCampaign
{
    public int CampaignId { get; set; }

    public string Title { get; set; } = null!;

    public string? Slug { get; set; }

    public string? ShortDesc { get; set; }

    public string? Content { get; set; }

    public decimal? GoalAmount { get; set; }

    public decimal? RaisedAmount { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<TbDonation> TbDonations { get; set; } = new List<TbDonation>();

    public virtual ICollection<TbGallery> TbGalleries { get; set; } = new List<TbGallery>();
}
