using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class TbGallery
{
    public int GalleryId { get; set; }

    public int? CampaignId { get; set; }

    public string? Title { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? Caption { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual TbCampaign? Campaign { get; set; }
}
