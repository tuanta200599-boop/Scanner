using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<Scanner.Services.IApiService, Scanner.Services.ApiService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false; // Tắt check consent để không chặn cookie OIDC
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.AddAuthentication(options =>
 {
     options.DefaultScheme = "Cookies";
     options.DefaultChallengeScheme = "oidc";
     options.DefaultSignOutScheme = "oidc";
 })
.AddCookie(options =>
 {
     options.Cookie.SameSite = SameSiteMode.None; //None Bắt buộc cho HTTPS Proxy
     options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Luôn dùng Secure cookie
     options.Cookie.HttpOnly = true;
     options.Cookie.Name = ".AspNetCore.Cookies";
 })
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = "https://oidc.aubot.vn/";
    options.RequireHttpsMetadata = true;
    options.ClientId = "pwa";
    options.ClientSecret = "ewxHIu7co1Uuj3De4EZvOHjBzqCsDkBJ";
    options.ResponseType = "code";
    
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("roles");
    options.Scope.Add("offline_access");

    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.SignedOutRedirectUri = "/";

    options.NonceCookie.SameSite = SameSiteMode.None;
    options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
    options.CorrelationCookie.SameSite = SameSiteMode.None;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

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
//var tokenBuilder = builder.Services.AddOpenIdConnectAccessTokenManagement();
//tokenBuilder.AddUserAccessTokenHttpClient("pwa", configureClient: client =>
//{
//    client.BaseAddress = new Uri("https://pwa.aubot.vn/");
//});
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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
