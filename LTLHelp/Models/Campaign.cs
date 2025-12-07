using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class Campaign
{
    public int CampaignId { get; set; }

    public int? CategoryId { get; set; }

    public string Title { get; set; } = null!;

    public string? Slug { get; set; }

    public string? Summary { get; set; }

    public string? Content { get; set; }

    public decimal? GoalAmount { get; set; }

    public decimal? RaisedAmount { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Status { get; set; }

    public string? ImageUrl { get; set; }

    public string? OrganizerName { get; set; }

    public string? OrganizerAvatar { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<CampaignGallery> CampaignGalleries { get; set; } = new List<CampaignGallery>();

    public virtual ICollection<CampaignUpdate> CampaignUpdates { get; set; } = new List<CampaignUpdate>();

    public virtual CampaignCategory? Category { get; set; }

    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
}
