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
}
