namespace LTLHelp.Models.Vnpay
{
    public class PaymentInformationModel
    {
        public string Name { get; set; } = string.Empty;
        public string OrderDescription { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? OrderType { get; set; }
    }
}
