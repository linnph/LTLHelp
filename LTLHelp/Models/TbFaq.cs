using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class TbFaq
{
    public int FaqId { get; set; }

    public string Question { get; set; } = null!;

    public string Answer { get; set; } = null!;

    public string? Category { get; set; }

    public DateTime? CreatedAt { get; set; }
}
