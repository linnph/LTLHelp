using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class TbMenu
{
    public int MenuId { get; set; }

    public string MenuName { get; set; } = null!;

    public string? Url { get; set; }

    public int? ParentId { get; set; }

    public int? OrderIndex { get; set; }

    public string? Slug { get; set; }

    public string? Icon { get; set; }

    public string? CssClass { get; set; }

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public string? MetaKeywords { get; set; }

    public string? Target { get; set; }

    public string? CreatedBy { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }
}
