using System;
using System.Collections.Generic;

namespace LTLHelp.Models;

public partial class BlogPost
{
    public int BlogPostId { get; set; }

    public int BlogCategoryId { get; set; }

    public int? AuthorId { get; set; }

    public string? Title { get; set; }

    public string? Slug { get; set; }

    public string? Summary { get; set; }

    public string? Content { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime? PublishedAt { get; set; }

    public bool? IsPublished { get; set; }

    public virtual User? Author { get; set; }

    public virtual BlogCategory BlogCategory { get; set; } = null!;

    public virtual ICollection<BlogComment> BlogComments { get; set; } = new List<BlogComment>();

    public virtual ICollection<BlogTag> Tags { get; set; } = new List<BlogTag>();
}
