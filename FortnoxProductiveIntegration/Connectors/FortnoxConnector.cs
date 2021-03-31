using Fortnox.SDK.Connectors;

namespace FortnoxProductiveIntegration.Connectors
{
    public static class FortnoxConnector
    {
        private const string AccessToken = "c58e5d93-432f-4d7b-a677-f6c1e23621c3";
        private const string ClientSecret = "WTGWLoVtqW";
        public static CustomerConnector Customer()
        {
            var customerConnector = new CustomerConnector
            {
                AccessToken = AccessToken,
                ClientSecret = ClientSecret
            };

            return customerConnector;
        }

        public static InvoiceConnector Invoice()
        {
            var invoiceConnector = new InvoiceConnector
            {
                AccessToken = AccessToken,
                ClientSecret = ClientSecret
            };

            return invoiceConnector;
        }
    }
}