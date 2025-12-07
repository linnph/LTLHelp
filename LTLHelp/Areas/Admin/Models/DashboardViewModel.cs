namespace LTLHelp.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        // 1. Dữ liệu từ tb_Donation
        public decimal TotalDonationAmount { get; set; }
        public int PendingDonationsCount { get; set; }

        // 2. Dữ liệu từ tb_Campaign
        public int TotalActiveCampaigns { get; set; }

        // 3. Dữ liệu từ tb_Donor
        public int TotalDonorCount { get; set; }

        // 4. Dữ liệu cho Báo cáo (Top Campaign)
        public List<CampaignStat> TopCampaigns { get; set; } = new List<CampaignStat>();
    }

    public class CampaignStat
    {
        public string Title { get; set; }
        public decimal TotalAmount { get; set; }
    }
}