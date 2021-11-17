using mApp.Server.Data;
using mApp.Server.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentityServer()
    .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

builder.Services.AddAuthentication()
     .AddGoogle("google", opt =>
     {
         var googleAuth = builder.Configuration.GetSection("Authentication:Google");
         opt.ClientId = googleAuth["ClientId"];
         opt.ClientSecret = googleAuth["ClientSecret"];
         opt.SignInScheme = IdentityConstants.ExternalScheme;
     }).AddFacebook(facebookOptions =>
     {
         facebookOptions.AppId = builder.Configuration["Facebook:AppId"];
         facebookOptions.AppSecret = builder.Configuration["Facebook:AppSecrete"];

         facebookOptions.Events = new OAuthEvents()
         {
             OnRemoteFailure = loginFailureHandler =>
             {
                 var authProperties = facebookOptions.StateDataFormat.Unprotect(loginFailureHandler.Request.Query["state"]);
                 loginFailureHandler.Response.Redirect("/Identity/Account/Login");
                 loginFailureHandler.HandleResponse();
                 return Task.FromResult(0);
             }
         };
     })
    .AddIdentityServerJwt();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
