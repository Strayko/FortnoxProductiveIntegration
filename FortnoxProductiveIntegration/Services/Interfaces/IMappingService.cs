using Fortnox.SDK.Entities;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Services.Interfaces
{
    public interface IMappingService
    {
        Customer CreateFortnoxCustomer(JObject customerJObject);
        InvoiceRow CreateFortnoxInvoiceRow(JToken item);
    }
}