using Fortnox.SDK.Connectors;
using Fortnox.SDK.Entities;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Services.Interfaces
{
    public interface IMappingService
    {
        Customer CreateFortnoxCustomer(JObject companyJObject, CustomerConnector customerConnector);
        InvoiceRow CreateFortnoxInvoiceRow(JToken item, JToken taxValue);
    }
}