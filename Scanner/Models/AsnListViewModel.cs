using System.Collections.Generic;

namespace Scanner.Models
{
    public class AsnListViewModel
    {
        public List<AsnItemViewModel> Items { get; set; } = new List<AsnItemViewModel>();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class AsnItemViewModel
    {
        public int AsnId { get; set; }
        public string VehicleNo { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string DriverPhone { get; set; } = string.Empty;
        public string RouteCode { get; set; } = string.Empty;
        public string DockCode { get; set; } = string.Empty;
        public int NumOfSku { get; set; }
        public decimal? ActualQty { get; set; }
        public string? ActualTemp { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Code { get; set; }
        public T? Data { get; set; }
        public int TotalRecords { get; set; }
    }

    public class ScanHistoryItemViewModel
    {
        public int AsnLineId { get; set; }
        public int AsnId { get; set; }
        public int SkuId { get; set; }
        public string PalletCode { get; set; } = string.Empty;
        public int ExpectedQty { get; set; }
        public string StatusReciept { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
    }

    public class ScanHandheldResult
    {
        public int AsnLineId { get; set; }
    }

    public class CreateLpnRequest
    {
        public string LpnCode { get; set; } = string.Empty;
        public string LpnLevel { get; set; } = "1";
        public int Qty { get; set; }
        public string Status { get; set; } = "New";
        public List<int> AsnLineIds { get; set; } = new();
    }
}
