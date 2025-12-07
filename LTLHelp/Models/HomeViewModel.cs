namespace LTLHelp.Models;

public class HomeViewModel
{
    public List<Campaign> Campaigns { get; set; } = new List<Campaign>();
    public List<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
    public List<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    public List<TestimonialViewModel> Testimonials { get; set; } = new List<TestimonialViewModel>();
    public List<FAQ> FAQs { get; set; } = new List<FAQ>();
    public HomeStatistics Statistics { get; set; } = new HomeStatistics();
}

public class TestimonialViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Rating { get; set; } = 5.0m;
}

public class FAQ
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}

public class HomeStatistics
{
    public int TotalCampaigns { get; set; }
    public decimal TotalDonations { get; set; }
    public int TotalVolunteers { get; set; }
    public int TotalDonationCount { get; set; }
}

