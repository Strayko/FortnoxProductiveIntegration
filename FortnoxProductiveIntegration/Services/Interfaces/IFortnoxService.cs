using System.Threading.Tasks;
using Fortnox.SDK.Entities;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Services.Interfaces
{
    public interface IFortnoxService
    {
        Task<long?> CreateInvoice(JToken invoiceJObject);
        Task<int> CheckPaidInvoices(JToken productiveInvoices);
    }
}