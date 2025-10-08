using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Helpdesk.Data;
using Helpdesk.Services; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;   // przywrócone
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


builder.Services.AddHostedService<AutoCloseResolvedTicketsService>();

var app = builder.Build();

// Seed ról i kont (has³o: zaq1@WSX)
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "User", "Agent", "Admin" };
    foreach (var r in roles)
        if (!await roleManager.RoleExistsAsync(r))
            await roleManager.CreateAsync(new IdentityRole(r));

    var seedPassword = "zaq1@WSX"; // spe³nia wymagania (ma ma³e, du¿e, cyfrê, znak @)

    async Task EnsureUser(string email, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };
            var create = await userManager.CreateAsync(user, seedPassword);
            if (create.Succeeded)
                await userManager.AddToRoleAsync(user, role);
        }
        else
        {
            // dopilnuj roli
            if (!await userManager.IsInRoleAsync(user, role))
                await userManager.AddToRoleAsync(user, role);

            // jeœli has³o stare (np. zaq1@wsx) i logowanie nie dzia³a – usuñ rekord i uruchom ponownie,
            // lub u¿yj resetu has³a (ResetPassword z tokenem)
        }
    }

    await EnsureUser("admin@helpdesk.com", "Admin");
    await EnsureUser("agent@helpdesk.com", "Agent");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();