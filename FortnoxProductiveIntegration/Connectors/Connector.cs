using Fortnox.SDK.Connectors;
using Microsoft.Extensions.Configuration;

namespace FortnoxProductiveIntegration.Connectors
{
    public class Connector : IConnector
    {
        private static IConfiguration _configuration;

        public Connector(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ProductiveXAuthToken()
        {
            return _configuration["Productive:X-Auth-Token"];
        }

        public string ProductiveXOrganizationId()
        {
            return _configuration["Productive:X-Organization-Id"];
        }
        
        public CustomerConnector FortnoxCustomer()
        {
            var customerConnector = new CustomerConnector
            {
                AccessToken = _configuration["Fortnox:AccessToken"],
                ClientSecret = _configuration["Fortnox:ClientSecret"]
            };

            return customerConnector;
        }

        public InvoiceConnector FortnoxInvoice()
        {
            var invoiceConnector = new InvoiceConnector
            {
                AccessToken = _configuration["Fortnox:AccessToken"],
                ClientSecret = _configuration["Fortnox:ClientSecret"]
            };

            return invoiceConnector;
        }
    }
}