using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProyectoParqueoFinal.Data;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/LogIn/LogIn";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(45);
    options.LogoutPath = "/Home/Logout";
    options.AccessDeniedPath = "/Home/Index";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.Use(async (context, next) =>
{
    if (context.Request.Method == "POST" && context.Request.Form["_method"] == "DELETE")
    {
        context.Request.Method = "DELETE";
    }
    else if (context.Request.Method == "POST" && context.Request.Form["_method"] == "PUT")
    {
        context.Request.Method = "PUT";
    }

    else if (next != null)
    {
        await next();
    }
    else
    {
        // Handle the case where next is null
        context.Response.StatusCode = 500;
    }
});

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
