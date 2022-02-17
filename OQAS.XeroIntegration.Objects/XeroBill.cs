using System;
using System.Collections.Generic;

namespace OQAS.XeroIntegration.Objects
{
    public class XeroBill
    {
        //public string Type { get; set; }
        //public string InvoiceID { get; set; }
        public string InvoiceNumber { get; set; }
        //public string Reference { get; set; }
        //public decimal? AmountDue { get; set; }
        //public decimal? AmountPaid { get; set; }
        //public decimal? AmountCredited { get; set; }
        //public bool? IsDiscounted { get; set; }
        public XeroContact Contact { get; set; }
        public string DateString { get; set; }
        public DateTime? Date { get; set; }
        //public string DueDateString { get; set; }
        //public DateTime? DueDate { get; set; }
        //public string Status { get; set; }
        //public string LineAmountTypes { get; set; }
        public List<XeroLineItem> LineItems { get; } = new List<XeroLineItem>();
        //public decimal? SubTotal { get; set; }
        //public decimal? TotalTax { get; set; }
        public decimal? Total { get; set; }
        public string TotalInWords { get; set; }
        //public DateTime? UpdatedDateUTC { get; set; }
        //public string CurrencyCode { get; set; }

        //public bool IncludeInImport { get; set; }
    }
}
