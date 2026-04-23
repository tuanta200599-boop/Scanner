using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scanner.Constants;
using Scanner.Models;
using Scanner.Services;

namespace Scanner.Controllers
{
    [Authorize]
    public class InboundController : Controller
    {
        private readonly ILogger<InboundController> _logger;
        private readonly IApiService _apiService;
        private readonly IConfiguration _configuration;

        public InboundController(ILogger<InboundController> logger, IApiService apiService, IConfiguration configuration)
        {
            _logger = logger;
            _apiService = apiService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(int page = 1, int? pageSize = null)
        {
            _logger.LogInformation("Scanner App: Inbound Index accessed at {Time}", DateTime.UtcNow);
            int actualPageSize = pageSize ?? _configuration.GetValue<int>("ApiSettings:PageSize", 20);

            var viewModel = new AsnListViewModel
            {
                PageIndex = page,
                PageSize = actualPageSize
            };

            try
            {
                string endpoint = $"{ApiEndpoints.Inbound.GetAsnList}?page={page}&pageSize={actualPageSize}";
                var apiResult = await _apiService.GetAsync<ApiResponse<List<AsnItemViewModel>>>(endpoint);

                if (apiResult?.IsSuccess == true && apiResult.Data != null)
                {
                    viewModel.Items = apiResult.Data;
                    viewModel.TotalCount = apiResult.TotalRecords;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Không thể tải danh sách ASN từ máy chủ. " + apiResult?.Message);
                }
            }
            catch (Exception ex)
            {
                // In a real app, log the exception.
                ModelState.AddModelError(string.Empty, $"Lỗi kết nối: {ex.Message}");
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, decimal? actualQty, string? actualTemp, DateTime? actualArrival)
        {
            try
            {
                // Gọi API PUT UpdateExpressStatus.
                // Truyền các tham số qua query string vì backend có thể chỉ nhận từ đây.
                string endpoint = $"{ApiEndpoints.Inbound.UpdateExpressStatus}?asnId={id}";

                if (actualQty.HasValue)
                {
                    endpoint += $"&actualQty={actualQty}";
                }

                if (!string.IsNullOrEmpty(actualTemp))
                {
                    endpoint += $"&actualTemp={Uri.EscapeDataString(actualTemp)}";
                }

                if (actualArrival.HasValue)
                {
                    endpoint += $"&actualArrival={Uri.EscapeDataString(actualArrival.Value.ToString("o"))}";
                }

                // Gửi PUT request.
                var apiResult = await _apiService.PutAsync<ApiResponse<object>>(endpoint, null);

                if (apiResult != null && apiResult.IsSuccess)
                {
                    return Json(new { success = true, message = "Cập nhật thành công!", timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") });
                }

                return Json(new { success = false, message = apiResult?.Message ?? "Lỗi từ phía máy chủ khi cập nhật." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi kết nối: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Putaway(int page = 1, int? pageSize = null, string? palletCode = "", int? asnId = null)
        {
            int actualPageSize = pageSize ?? _configuration.GetValue<int>("ApiSettings:PageSize", 20);

            var viewModel = new PalletListViewModel
            {
                PageIndex = page,
                PageSize = actualPageSize,
                SearchPalletCode = palletCode ?? string.Empty,
                AsnId = asnId
            };

            try
            {
                string endpoint = $"{ApiEndpoints.Configuration.GetPalletList}?page={page}&pageSize={actualPageSize}";
                if (!string.IsNullOrEmpty(palletCode))
                {
                    // Assuming API supports filtering by palletCode like this.
                    // Adjust query parameter name if it's different in the actual WCS API.
                    endpoint += $"&palletCode={Uri.EscapeDataString(palletCode)}";
                }

                var apiResult = await _apiService.GetAsync<ApiResponse<List<PalletItemViewModel>>>(endpoint);

                if (apiResult?.IsSuccess == true && apiResult.Data != null)
                {
                    viewModel.Items = apiResult.Data;
                    viewModel.TotalCount = apiResult.TotalRecords;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Không thể tải danh sách Pallet code từ máy chủ. " + apiResult?.Message);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi kết nối: {ex.Message}");
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ScanBarcode(string barcode, string palletCode, int asnId)
        {
            try
            {
                if (string.IsNullOrEmpty(barcode) || string.IsNullOrEmpty(palletCode))
                {
                    return Json(new { success = false, message = "Mã vạch hoặc mã Pallet không hợp lệ." });
                }

                int qty = 1;
                int skuId = 0;

                string skuCode = "";
                // Cú pháp mã vạch có thể là {SkuCode}:{Qty} hoặc chỉ {SkuCode}
                var parts = barcode.Split(':');
                if (parts.Length == 2)
                {
                    skuCode = parts[0];
                    if (!int.TryParse(parts[1], out qty))
                    {
                        return Json(new { success = false, message = "Định dạng mã vạch không hợp lệ. Vui lòng thử lại." });
                    }
                }
                else if (parts.Length == 1)
                {
                    skuCode = parts[0];
                }
                else
                {
                    return Json(new { success = false, message = "Mã vạch không đúng cấu trúc." });
                }

                // Gọi API Sku/GetIdByCode để lấy SkuId từ SkuCode
                string lookupUrl = $"{ApiEndpoints.Configuration.GetSkuIdByCode}?skuCode={Uri.EscapeDataString(skuCode)}";
                var skuResult = await _apiService.GetAsync<ApiResponse<SkuIdResponseData>>(lookupUrl);

                if (skuResult == null || !skuResult.IsSuccess || skuResult.Data == null)
                {
                    return Json(new { success = false, message = "Mã vạch SKU không hợp lệ. Vui lòng thử lại." });
                }

                skuId = skuResult.Data.SkuId;

                var requestPayload = new ScanHandheldRequest
                {
                    AsnLineId = 0,
                    AsnId = asnId,
                    SkuId = skuId,
                    PalletCode = palletCode,
                    ExpectedQty = qty,
                    StatusReciept = "New"
                };

                var apiResult = await _apiService.PostAsync<ApiResponse<ScanHandheldResult>>(ApiEndpoints.Inbound.ScanHandheld, requestPayload);

                if (apiResult != null && apiResult.IsSuccess)
                {
                    return Json(new { success = true, message = $"Đã quét thành công SKU {skuId} vào Pallet {palletCode}.", asnLineId = apiResult.Data?.AsnLineId });
                }

                return Json(new { success = false, message = apiResult?.Message ?? "Lỗi từ máy chủ khi lưu mã quét." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePalletActiveStatus(int palletId, string palletCode, string palletName)
        {
            try
            {
                var payload = new
                {
                    id = palletId,
                    palletCode = palletCode,
                    palletName = palletName,
                    isActive = false
                };

                var apiResult = await _apiService.PutAsync<ApiResponse<object>>(ApiEndpoints.Configuration.UpdatePallet, payload);

                if (apiResult != null && apiResult.IsSuccess)
                {
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = apiResult?.Message ?? "Lỗi từ máy chủ khi cập nhật Pallet." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetScanHistory(int asnId, string palletCode)
        {
            try
            {
                string endpoint = $"{ApiEndpoints.Inbound.GetHistoryScan}?asnId={asnId}&palletCode={Uri.EscapeDataString(palletCode)}";
                var apiResult = await _apiService.GetAsync<ApiResponse<List<ScanHistoryItemViewModel>>>(endpoint);

                if (apiResult?.IsSuccess == true && apiResult.Data != null)
                {
                    return Json(new { success = true, data = apiResult.Data });
                }

                return Json(new { success = true, data = new List<object>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsnLine(int id)
        {
            try
            {
                string endpoint = $"{ApiEndpoints.Inbound.DeleteAsnLine}?id={id}";
                var apiResult = await _apiService.DeleteAsync<ApiResponse<object>>(endpoint);

                if (apiResult != null && apiResult.IsSuccess)
                {
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = apiResult?.Message ?? "Lỗi từ máy chủ khi xóa." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetReceivingCount()
        {
            _logger.LogInformation("Scanner App: AJAX GetReceivingCount called at {Time}", DateTime.UtcNow);
            try
            {
                string endpoint = $"{ApiEndpoints.Inbound.GetAsnList}?page=1&pageSize=1";
                var apiResult = await _apiService.GetAsync<ApiResponse<List<AsnItemViewModel>>>(endpoint);

                if (apiResult?.IsSuccess == true)
                {
                    return Json(new { count = apiResult.TotalRecords });
                }

                return Json(new { count = 0 });
            }
            catch
            {
                return Json(new { count = 0 });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLpnCount()
        {
            try
            {
                string endpoint = $"{ApiEndpoints.Inbound.GetLpnList}?page=1&pageSize=1";
                var apiResult = await _apiService.GetAsync<ApiResponse<List<object>>>(endpoint);
                return Json(new { totalRecords = apiResult?.TotalRecords ?? 0 });
            }
            catch
            {
                return Json(new { totalRecords = 0 });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateLpn([FromBody] CreateLpnRequest request)
        {
            try
            {
                var apiResult = await _apiService.PostAsync<ApiResponse<object>>(ApiEndpoints.Inbound.CreateLpn, request);

                if (apiResult != null && apiResult.IsSuccess)
                {
                    return Json(new { success = true, message = apiResult.Message });
                }

                return Json(new { success = false, message = apiResult?.Message ?? "Lỗi từ máy chủ khi tạo LPN." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}
