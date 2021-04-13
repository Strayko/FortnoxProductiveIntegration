using Fortnox.SDK.Connectors;

namespace FortnoxProductiveIntegration.Connectors
{
    public interface IConnector
    {
        CustomerConnector FortnoxCustomer();
        InvoiceConnector FortnoxInvoice();
        string ProductiveXAuthToken();
        string ProductiveXOrganizationId();
        string FortnoxAccessToken();
        string FortnoxClientSecret();
    }
}