using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Services.Interfaces
{
    public interface IProductiveServices
    {
        Task<JObject> GetInvoiceData();
    }
}