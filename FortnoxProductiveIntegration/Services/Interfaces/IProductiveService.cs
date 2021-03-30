using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Services.Interfaces
{
    public interface IProductiveService
    {
        Task<JObject> GetInvoiceData();
        Task<JObject> GetCustomerData(string customerId);

        Task<JObject> GetLineItemsDataFromInvoice(string invoiceId);
    }
}