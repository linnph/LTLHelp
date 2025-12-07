using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class CampaignUpdate
{
    public int UpdateId { get; set; }

    public int CampaignId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Campaign Campaign { get; set; } = null!;
}
