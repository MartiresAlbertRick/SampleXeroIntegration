namespace OQAS.XeroIntegration.Objects
{
    public class CheckManagerVoucher
    {
        public int Id { get; set; }
        public string Check_number { get; set; }
        public string Payee { get; set; }
        public string Particulars { get; set; }
        public decimal? Particulars_amount { get; set; }
        public string Remarks { get; set; }
        public string Amount_in_words { get; set; }
        public string Prepared_by { get; set; }
        public string Approved_by { get; set; }
        public string Posted_by { get; set; }
        public System.DateTime? Created_at { get; set; }
        public System.DateTime? Updated_at { get; set; }

        public System.Collections.Generic.ICollection<CheckManagerItem> Items { get; set; }
    }
}
