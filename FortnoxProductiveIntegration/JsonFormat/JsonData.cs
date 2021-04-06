namespace FortnoxProductiveIntegration.JsonFormat
{
    public static class JsonData
    {
        public static string ContentSentOn(string date)
        {
            var contentSentOn = $@"
                        {{
                          'data': {{
                            'type': 'invoices',
                            'attributes': {{
                                'sent_on': '{date}'
                            }}
                        }}
                    }}".Replace('\'', '"');
            return contentSentOn;
        }
        
        public static string ContentPayments(string amount, string date, string invoiceIdFromSystem)
        {
            var contentPayments = $@"
                        {{
                           'data': {{
                              'type': 'payments',
                              'attributes': {{
                                'amount': {amount},
                                'paid_on': '{date}'
                              }},
                              'relationships': {{
                                'invoice': {{
                                  'data': {{
                                    'type': 'invoices',
                                    'id': '{invoiceIdFromSystem}'
                                   }}
                                }}
                              }}
                            }}
                    }}".Replace('\'', '"');
            return contentPayments;
        }
    }
}