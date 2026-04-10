using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scanner.Constants;
using Scanner.Models;
using Scanner.Services;

namespace Scanner.Controllers
{
    [Authorize]
    public class BarcodeController : Controller
    {
        private readonly IApiService _apiService;
        private readonly IConfiguration _configuration;

        public BarcodeController(IApiService apiService, IConfiguration configuration)
        {
            _apiService = apiService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(int page = 1, int? pageSize = null)
        {
            int actualPageSize = pageSize ?? _configuration.GetValue<int>("ApiSettings:PageSize", 20);

            var viewModel = new SkuBarcodeListViewModel
            {
                PageIndex = page,
                PageSize = actualPageSize
            };

            try
            {
                string endpoint = $"{ApiEndpoints.Configuration.GetSkuBarcodeList}?page={page}&pageSize={actualPageSize}";
                var apiResult = await _apiService.GetAsync<ApiResponse<List<SkuBarcodeItemViewModel>>>(endpoint);

                if (apiResult?.IsSuccess == true && apiResult.Data != null)
                {
                    viewModel.Items = apiResult.Data;
                    viewModel.TotalCount = apiResult.TotalRecords;
                }
                else
                {
                    // Fallback for demo or if API returns successfully but with empty data
                    if (apiResult != null && apiResult.IsSuccess) {
                         viewModel.Items = new List<SkuBarcodeItemViewModel>();
                    } else {
                        ModelState.AddModelError(string.Empty, "Không thể tải danh sách Barcode từ máy chủ. " + apiResult?.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi kết nối: {ex.Message}");
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSkuBarcodeRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.ExternalBarcode))
                {
                    return Json(new { success = false, message = "Thông tin SKU không hợp lệ." });
                }

                // Ensure dates are set if not provided
                request.CreatedDate = DateTime.Now;
                request.UpdatedDate = DateTime.Now;

                var apiResult = await _apiService.PostAsync<ApiResponse<object>>(ApiEndpoints.Configuration.CreateSkuBarcode, request);

                if (apiResult != null && apiResult.IsSuccess)
                {
                    return Json(new { success = true, message = "Đã gửi yêu cầu in barcode thành công!" });
                }

                return Json(new { success = false, message = apiResult?.Message ?? "Lỗi từ máy chủ khi tạo yêu cầu in." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetSkuList(int page = 1, int pageSize = 100)
        {
            try
            {
                string endpoint = $"{ApiEndpoints.Configuration.GetSkuList}?page={page}&pageSize={pageSize}";
                // We use dynamic/object here because we just want to forward the JSON
                var apiResult = await _apiService.GetAsync<ApiResponse<List<object>>>(endpoint);
                return Json(apiResult);
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Printer([FromBody] object payload)
        {
            try
            {
                var apiResult = await _apiService.PostAsync<ApiResponse<object>>(ApiEndpoints.Configuration.Printer, payload);
                return Json(apiResult);
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetPrinterDriverList()
        {
            try
            {
                var apiResult = await _apiService.GetAsync<ApiResponse<List<object>>>(ApiEndpoints.Configuration.GetPrinterDriverList);
                return Json(apiResult);
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}


