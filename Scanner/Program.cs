using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<Scanner.Services.IApiService, Scanner.Services.ApiService>()
    .AddUserAccessTokenHandler();
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    options.JsonWriterOptions = new System.Text.Json.JsonWriterOptions
    {
        Indented = false // Bắt buộc là false để mỗi log là 1 object trên 1 dòng
    };
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false; // Tắt check consent để không chặn cookie OIDC
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified; // Android 7 không hỗ trợ SameSite=None
    options.OnAppendCookie = cookieContext => CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
    options.OnDeleteCookie = cookieContext => CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
});

builder.Services.AddAuthentication(options =>
 {
     options.DefaultScheme = "Cookies";
     options.DefaultChallengeScheme = "oidc";
     options.DefaultSignOutScheme = "oidc";
 })
.AddCookie(options =>
 {
     options.Cookie.SameSite = SameSiteMode.None; // None Bắt buộc cho HTTPS Proxy
     options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Luôn dùng Secure cookie
     options.Cookie.HttpOnly = true;
     options.Cookie.Name = ".AspNetCore.Cookies";
     options.Cookie.IsEssential = true; // Đảm bảo cookie luôn được gửi
 })
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = "https://oidc.aubot.vn/";
    //options.Authority = "http://localhost:44310/";
    options.RequireHttpsMetadata = true;
    options.ClientId = "pwa";
    //options.ClientSecret = "vNN8PWQhYwoKGukxST4Y41W1Wf2AxD6w";
    options.ClientSecret = "ewxHIu7co1Uuj3De4EZvOHjBzqCsDkBJ";
    options.ResponseType = "code";

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("roles");
    options.Scope.Add("offline_access");

    options.SaveTokens = true;
    //save toàn bộ vào token nếu bạn cấu hình savetoken =true
    options.GetClaimsFromUserInfoEndpoint = true;
    options.SignedOutRedirectUri = "/";

    options.NonceCookie.SameSite = SameSiteMode.None;
    options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
    options.NonceCookie.IsEssential = true;
    options.CorrelationCookie.SameSite = SameSiteMode.None;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    options.CorrelationCookie.IsEssential = true;

    options.ClaimActions.MapUniqueJsonKey("departmentId", "departmentId");
    options.ClaimActions.MapUniqueJsonKey("departmentName", "departmentName");
    options.ClaimActions.MapUniqueJsonKey("isDepartmentManager", "isDepartmentManager");

    options.Events = new OpenIdConnectEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("OIDC Auth Failed: " + context.Exception.Message);
            return System.Threading.Tasks.Task.CompletedTask;
        },
        OnRemoteFailure = context =>
        {
            Console.WriteLine("--- OIDC Remote Failure Diagnostic ---");
            Console.WriteLine("Method: " + context.Request.Method);
            Console.WriteLine("Path: " + context.Request.Path);
            Console.WriteLine("Failure: " + context.Failure?.Message);

            if (context.Failure != null)
            {
                Console.WriteLine("Failure Inner: " + context.Failure.InnerException?.Message);
            }

            if (context.Request.Query.Count > 0)
            {
                foreach (var param in context.Request.Query)
                    Console.WriteLine($"Query: {param.Key} = {param.Value}");
            }

            if (context.Request.HasFormContentType)
            {
                foreach (var param in context.Request.Form)
                    Console.WriteLine($"Form: {param.Key} = {param.Value}");
            }
            Console.WriteLine("---------------------------------------");
            return System.Threading.Tasks.Task.CompletedTask;
        },
        OnRedirectToIdentityProvider = context =>
        {
            Console.WriteLine("OIDC Redirecting to: " + context.ProtocolMessage.IssuerAddress);
            Console.WriteLine("OIDC Redirect URI: " + context.ProtocolMessage.RedirectUri);
            return System.Threading.Tasks.Task.CompletedTask;
        }

    };
});
//refresh token
builder.Services.AddOpenIdConnectAccessTokenManagement();
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
//});
var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Default HSTS for non-dev environments
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Tắt HTTPS redirection trong Docker nếu Nginx đã xử lý
app.UseStaticFiles();

app.UseRouting();
app.UseCors(option => option.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}").RequireAuthorization();

app.Run();

#region SameSite Compatibility
void CheckSameSite(HttpContext httpContext, CookieOptions options)
{
    if (options.SameSite == SameSiteMode.None)
    {
        var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
        if (DisallowsSameSiteNone(userAgent))
        {
            // Trình duyệt cũ (Android 7, iOS 12) không hiểu SameSite=None
            // Đặt về Unspecified để trình duyệt tự xử lý theo cách cũ
            options.SameSite = SameSiteMode.Unspecified;
        }
    }
}

bool DisallowsSameSiteNone(string userAgent)
{
    if (string.IsNullOrEmpty(userAgent)) return false;

    // iOS 12 Safari và các trình duyệt cũ hơn
    if (userAgent.Contains("CPU iPhone OS 12") || userAgent.Contains("iPad; CPU OS 12")) return true;

    // MacOS Safari 12
    if (userAgent.Contains("Safari") && userAgent.Contains("Macintosh; Intel Mac OS X 10_14") && userAgent.Contains("Version/12")) return true;

    // Chrome phiên bản 51 tới 66 (Phổ biến trên Android 7)
    if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6")) return true;

    return false;
}
#endregion
