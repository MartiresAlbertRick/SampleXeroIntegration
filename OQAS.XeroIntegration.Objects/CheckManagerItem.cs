namespace OQAS.XeroIntegration.Objects
{
    public class CheckManagerItem
    {
        public int Id { get; set; }
        public int Voucher_id { get; set; }
        public string Account { get; set; }
        public string Account_title { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public System.DateTime? Created_at { get; set; }
        public System.DateTime? Updated_at { get; set; }
    }
}
