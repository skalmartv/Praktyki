using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

var builder = WebApplication.CreateBuilder(args);

// Dodaj AppDbContext
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dodaj Identity z w�asn� klas� Users
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
	options.Password.RequireDigit = true;
	options.Password.RequiredLength = 6;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = true;
	options.Password.RequireLowercase = true;

	options.User.RequireUniqueEmail = true; // je�li chcesz wymusza� unikalny email
})
.AddEntityFrameworkStores<AppDbContext>() // korzystamy z AppDbContext
.AddDefaultTokenProviders();

// Dodaj kontrolery z widokami
builder.Services.AddControllersWithViews();

// Dodaj obs�ug� Razor Pages (Identity czasami wymaga)
builder.Services.AddRazorPages();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // musi by� przed Authorization
app.UseAuthorization();

// Mapowanie kontroler�w + opcjonalnie Razor Pages
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // konieczne, je�li korzystasz z Identity Pages np. Logout, Register

app.Run();
