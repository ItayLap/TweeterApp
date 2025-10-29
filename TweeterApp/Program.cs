using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TweeterApp.Controllers;
using TweeterApp.Data;
using TweeterApp.Hubs;
using TweeterApp.Models;
using TweeterApp.Repository;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddAuthentication().AddCookie();
builder.Services.AddAuthorization();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    options.SignIn.RequireConfirmedEmail = false; options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);
})

.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(config =>
{
    config.Cookie.Name = "MyCookie";
    config.LoginPath = "/Account/Login";
    config.AccessDeniedPath = "/Account/AccessDenied";
});
builder.Services.AddMemoryCache();

builder.Services.AddScoped<IPostRepository, PostRepository>(); 
builder.Services.AddScoped<IFollowRepository, FollowRepository>(); 
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();
builder.Services.AddScoped<ISavedPostsRepository, SavedPostsRepository>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    async Task EnsureRole(string name)
    {
        if (!await roleManager.RoleExistsAsync(name))
            await roleManager.CreateAsync(new IdentityRole<int>(name));
    }
    await EnsureRole("User");
    await EnsureRole("Admin");

    var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var adminCfg = cfg.GetSection("AdminSeed");
    var adminEmail = adminCfg["Email"];
    var adminPass = adminCfg["Password"];

    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPass))
    {
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = adminCfg["FirstName"] ?? "Admin",
                LastName = adminCfg["LastName"] ?? "User",
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow,
                ActiveAccount = true,
                Bio = "Administrator account",
                GenderId = 0,
                DateOfBirth = new DateTime(2000,1,1),
                AvatarPath = "/Uploads/1b6675c3 - fd4f - 4604 - 854e-e3a0e094f5a6.png"
            };

            var create = await userManager.CreateAsync(admin, adminPass);
            if (create.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
        else if(!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();
// change pattern 

//Перенаправление в AccountController

//[Authorize] + LoginPath —> builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.LoginPath = "/Account/Login";
//});

app.Run();
