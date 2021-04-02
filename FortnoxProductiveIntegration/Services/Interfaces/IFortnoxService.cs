using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Services.Interfaces
{
    public interface IFortnoxService
    {
        Task<long?> CreateInvoice(JToken invoiceJObject);
    }
}