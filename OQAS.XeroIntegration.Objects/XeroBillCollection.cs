using System.Collections.Generic;

namespace OQAS.XeroIntegration.Objects
{
    public class XeroBillCollection
    {
        public List<XeroBill> Invoices { get; } = new List<XeroBill>();
    }
}
