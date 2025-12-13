using LTLHelp.Models.Vnpay;

namespace LTLHelp.Services.Vnpay
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context, string orderId);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);

    }
}
