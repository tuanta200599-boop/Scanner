namespace Scanner.Models
{
    public class ScanHandheldRequest
    {
        public int AsnLineId { get; set; } = 0;
        public int AsnId { get; set; }
        public int SkuId { get; set; }
        public string PalletCode { get; set; } = string.Empty;
        public int ExpectedQty { get; set; } = 0;
        public string StatusReciept { get; set; } = "New";
    }
}
