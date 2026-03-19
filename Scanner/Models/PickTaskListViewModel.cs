using System;
using System.Collections.Generic;

namespace Scanner.Models
{
    public class PickTaskListViewModel
    {
        public List<PickTaskItemViewModel> Items { get; set; } = new List<PickTaskItemViewModel>();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class PickTaskItemViewModel
    {
        public int PickingTaskId { get; set; }
        public int? WaveId { get; set; }
        public int? PickingBatchId { get; set; }
        public string TaskCode { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public int OrderLineId { get; set; }
        public int InventoryId { get; set; }
        public int SkuId { get; set; }
        public string LotNo { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public string FromLocation { get; set; } = string.Empty;
        public string ToLocation { get; set; } = string.Empty;
        public int RequestedQty { get; set; }
        public int PickedQty { get; set; }
        public string Uom { get; set; } = string.Empty;
        public string TaskType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
