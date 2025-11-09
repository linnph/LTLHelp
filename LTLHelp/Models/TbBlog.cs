using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class TbBlog
{
    public int BlogId { get; set; }

    public string Title { get; set; } = null!;

    public string? Slug { get; set; }

    public string? ShortDesc { get; set; }

    public string? Content { get; set; }

    public int? CategoryId { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual TbCategory? Category { get; set; }

    public virtual ICollection<TbBlogComment> TbBlogComments { get; set; } = new List<TbBlogComment>();
}
