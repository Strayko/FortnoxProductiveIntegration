using Fortnox.SDK.Connectors;

namespace FortnoxProductiveIntegration.Connectors
{
    public static class FortnoxConnector
    {
        public static CustomerConnector Customer()
        {
            var customerConnector = new CustomerConnector
            {
                AccessToken = FortnoxCredentials.AccessToken,
                ClientSecret = FortnoxCredentials.ClientSecret
            };

            return customerConnector;
        }

        public static InvoiceConnector Invoice()
        {
            var invoiceConnector = new InvoiceConnector
            {
                AccessToken = FortnoxCredentials.AccessToken,
                ClientSecret = FortnoxCredentials.ClientSecret
            };

            return invoiceConnector;
        }
    }
}