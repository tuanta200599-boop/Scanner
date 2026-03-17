namespace Scanner.Models
{
    public class PalletItemViewModel
    {
        public int Id { get; set; }
        public string PalletCode { get; set; } = string.Empty;
        public string PalletName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class PalletListViewModel
    {
        public List<PalletItemViewModel> Items { get; set; } = new List<PalletItemViewModel>();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SearchPalletCode { get; set; } = string.Empty;
        public int? AsnId { get; set; }
    }
}
