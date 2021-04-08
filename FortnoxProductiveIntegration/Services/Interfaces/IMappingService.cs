using Fortnox.SDK.Entities;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Services.Interfaces
{
    public interface IMappingService
    {
        Customer CreateFortnoxCustomer(JObject companyJObject);
        InvoiceRow CreateFortnoxInvoiceRow(JToken item);
    }
}