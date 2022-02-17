namespace OQAS.XeroIntegration.Objects
{
    public class XeroLineItem
    {
        public string Description { get; set; }
        public decimal? UnitAmount { get; set; }
        //public string TaxType { get; set; }
        //public decimal? TaxAmount { get; set; }
        public decimal? LineAmount { get; set; }
        public string AccountCode { get; set; }
        //public decimal? Quantity { get; set; }
        //public string LineItemID { get; set; }
    }
}
