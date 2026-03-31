using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<Scanner.Services.IApiService, Scanner.Services.ApiService>();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
});
builder.Services.AddAuthentication(options =>
 {
     options.DefaultScheme = "Cookies";
     options.DefaultChallengeScheme = "oidc";
     options.DefaultSignOutScheme = "oidc";
 })
.AddCookie(options =>
 {
     options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
     options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
     options.Cookie.HttpOnly = true;
     options.Cookie.Name = ".AspNetCore.Cookies";
 })
.AddOpenIdConnect("oidc", options =>
{
    //options.Authority = "http://localhost:44310/";
    options.Authority = "https://oidc.aubot.vn/";
    options.RequireHttpsMetadata = false;
    //options.NonceCookie = false;
    options.ClientId = "Wms";
    //options.ClientSecret = "SDb26R1TrQN9wich6tf1PZO37odxGH3X";
    options.ClientSecret = "9d4840ba-a3ee-5fb9-e0ef-986f9eb6bf96";
    options.ResponseType = "code";
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("roles");
    //options.Scope.Add("department");

    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.SignedOutRedirectUri = "/";

    options.NonceCookie.SameSite = SameSiteMode.Unspecified;
    options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;

    options.ClaimActions.MapUniqueJsonKey("departmentId", "departmentId");
    options.ClaimActions.MapUniqueJsonKey("departmentName", "departmentName");
    options.ClaimActions.MapUniqueJsonKey("isDepartmentManager", "isDepartmentManager");
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Default HSTS for non-dev environments
    app.UseHsts();
}

app.UseHttpsRedirection();
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
