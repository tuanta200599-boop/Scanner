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
        private readonly IApiService _apiService;
        private readonly IConfiguration _configuration;

        public InboundController(IApiService apiService, IConfiguration configuration)
        {
            _apiService = apiService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(int page = 1, int? pageSize = null)
        {
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
        public async Task<IActionResult> UpdateStatus(int id)
        {
            try
            {
                // Gọi API PUT UpdateExpressStatus.
                // Tuỳ vào backend yêu cầu query string hay body, truyền asnId lên server WCS.
                string endpoint = $"{ApiEndpoints.Inbound.UpdateExpressStatus}?asnId={id}";

                // Gửi PUT request. Pass new { asnId = id } as body for foolproof binding.
                var apiResult = await _apiService.PutAsync<ApiResponse<object>>(endpoint, new { asnId = id });

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
                    ModelState.AddModelError(string.Empty, "Không thể tải danh sách Pallet từ máy chủ. " + apiResult?.Message);
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

                // Cú pháp mã vạch có thể là {AsnId}-{SkuId} hoặc chỉ {SkuId} nếu đã truyền AsnId từ UI
                var parts = barcode.Split('-');
                if (parts.Length == 2)
                {
                    if (!int.TryParse(parts[0], out skuId) || !int.TryParse(parts[1], out qty))
                    {
                        return Json(new { success = false, message = "Định dạng mã vạch không hợp lệ. Vui lòng quét mã dạng AsnId-SkuId." });
                    }
                }
                else if (parts.Length == 1 && asnId != 0)
                {
                    if (!int.TryParse(parts[0], out skuId))
                    {
                        return Json(new { success = false, message = "Mã vạch SKU không hợp lệ. Vui lòng thử lại." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Mã vạch không đúng cấu trúc (thiếu AsnId)." });
                }

                var requestPayload = new ScanHandheldRequest
                {
                    AsnLineId = 0,
                    AsnId = asnId,
                    SkuId = skuId,
                    PalletCode = palletCode,
                    ExpectedQty = qty,
                    StatusReciept = "New"
                };

                var apiResult = await _apiService.PostAsync<ApiResponse<object>>(ApiEndpoints.Inbound.ScanHandheld, requestPayload);

                if (apiResult != null && apiResult.IsSuccess)
                {
                    return Json(new { success = true, message = $"Đã quét thành công SKU {skuId} vào Pallet {palletCode}." });
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
                    isActive = true
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
    }
}
