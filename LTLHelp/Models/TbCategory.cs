using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class TbCategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Slug { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<TbBlog> TbBlogs { get; set; } = new List<TbBlog>();
}
