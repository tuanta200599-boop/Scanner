using System;
using System.Collections.Generic;

namespace Scanner.Models
{
    public class SkuBarcodeItemViewModel
    {
        public int RequestId { get; set; }
        public int OwnerId { get; set; }
        public string ExternalBarcode { get; set; } = string.Empty;
        public string BarcodeType { get; set; } = "Code128";
        public string LabelType { get; set; } = "SKU";
        public int LabelQty { get; set; }
        public string ProposedSkuCode { get; set; } = string.Empty;
        public string ProposedSkuName { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = "New";
        public string PrintStatus { get; set; } = "New";
        public int ApprovedSkuId { get; set; }
        public string PrinterName { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; } = DateTime.Now;
        public string UpdatedBy { get; set; } = string.Empty;
    }

    public class SkuBarcodeListViewModel
    {
        public List<SkuBarcodeItemViewModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class CreateSkuBarcodeRequest
    {
        public int RequestId { get; set; } = 0;
        public int OwnerId { get; set; } = 1;
        public string ExternalBarcode { get; set; } = string.Empty;
        public string BarcodeType { get; set; } = "Code128";
        public string LabelType { get; set; } = "SKU";
        public int LabelQty { get; set; }
        public string ProposedSkuCode { get; set; } = string.Empty;
        public string ProposedSkuName { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = "New";
        public string PrintStatus { get; set; } = "New";
        public int ApprovedSkuId { get; set; } = 0;
        public string PrinterName { get; set; } = "string";
        public string DeviceId { get; set; } = "string";
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = "string";
        public DateTime? UpdatedDate { get; set; } = DateTime.Now;
        public string UpdatedBy { get; set; } = "string";
    }
}
