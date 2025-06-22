using kaban.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContextPool<Context>(opts => opts.UseMySql(builder.Configuration.GetConnectionString("MainContext"), new MariaDbServerVersion(new Version(11, 3, 2))));

builder.Services.AddAuthentication().AddCookie("session", options =>
{
    options.LoginPath = "/login";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".ssid";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions()
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.UseStatusCodePages(async options =>
{
    await Task.Run(() =>
    {
        if (options.HttpContext.Response.StatusCode == 401)
            options.HttpContext.Response.Redirect("/login");
        else if (options.HttpContext.Response.StatusCode == 404)
            options.HttpContext.Response.Redirect("/notfound");
    });
});

app.UseRouting();
app.UseHttpsRedirection();
app.MapStaticAssets();

app.UseCookiePolicy();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{action=Index}",
    defaults: new { controller = "Home" })
    .WithStaticAssets();

app.Run();
