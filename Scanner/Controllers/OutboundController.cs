using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Scanner.Constants;
using Scanner.Models;
using Scanner.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scanner.Controllers
{
    [Authorize]
    public class OutboundController : Controller
    {
        private readonly IApiService _apiService;
        private readonly IConfiguration _configuration;

        public OutboundController(IApiService apiService, IConfiguration configuration)
        {
            _apiService = apiService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Picking(int page = 1, int? pageSize = null)
        {
            int actualPageSize = pageSize ?? _configuration.GetValue<int>("ApiSettings:PageSize", 20);

            var viewModel = new PickTaskListViewModel
            {
                PageIndex = page,
                PageSize = actualPageSize
            };

            try
            {
                string endpoint = $"{ApiEndpoints.Outbound.GetPickTaskList}?page={page}&pageSize={actualPageSize}";
                var apiResult = await _apiService.GetAsync<ApiResponse<List<PickTaskItemViewModel>>>(endpoint);

                if (apiResult?.IsSuccess == true && apiResult.Data != null)
                {
                    viewModel.Items = apiResult.Data;
                    viewModel.TotalCount = apiResult.TotalRecords;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Không thể tải danh sách nhiệm vụ từ máy chủ. " + apiResult?.Message);
                }
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi kết nối: {ex.Message}");
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RecordPickingScan(int taskId, string barcode)
        {
            try
            {
                if (string.IsNullOrEmpty(barcode) || taskId <= 0)
                {
                    return Json(new { success = false, message = "Mã vạch hoặc ID nhiệm vụ không hợp lệ." });
                }

                // Gửi request GET tới API RecordPickingScan kèm query string params
                int pickingQty = 1;
                string endpoint = $"{ApiEndpoints.Outbound.RecordPickingScan}?taskId={taskId}&Pickingqty={pickingQty}&SkuId={barcode}";

                var apiResult = await _apiService.GetAsync<ApiResponse<object>>(endpoint);

                if (apiResult != null && apiResult.IsSuccess)
                {
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = apiResult?.Message ?? "Lỗi từ máy chủ khi lưu mã quét." });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}
