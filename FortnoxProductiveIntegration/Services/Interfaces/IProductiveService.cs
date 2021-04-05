using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Services.Interfaces
{
    public interface IProductiveService
    {
        Task<JObject> GetUnpaidInvoiceData();
        Task<JObject> GetCustomerData(string customerId);
        Task<JObject> GetLineItemsDataFromInvoice(string invoiceId);
        JArray DailyInvoicesFilter(JToken invoicesData);
        Task<JArray> NewInvoices(JToken dailyInvoices);
        Task<JObject> SentOn(string invoiceId, string contentSentOn);
        Task<JObject> Payments(string contentPayments);
    }
}