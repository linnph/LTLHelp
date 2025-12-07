using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class CampaignGallery
{
    public int GalleryId { get; set; }

    public int CampaignId { get; set; }

    public string? ImageUrl { get; set; }

    public string? Caption { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Campaign Campaign { get; set; } = null!;
}
