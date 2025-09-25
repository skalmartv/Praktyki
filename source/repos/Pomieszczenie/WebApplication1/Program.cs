using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

var builder = WebApplication.CreateBuilder(args);

// Dodaj AppDbContext
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dodaj Identity z w³asn¹ klas¹ Users
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
	options.Password.RequireDigit = true;
	options.Password.RequiredLength = 6;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = true;
	options.Password.RequireLowercase = true;

	options.User.RequireUniqueEmail = true; // jeœli chcesz wymuszaæ unikalny email
})
.AddEntityFrameworkStores<AppDbContext>() // korzystamy z AppDbContext
.AddDefaultTokenProviders();

// Dodaj kontrolery z widokami
builder.Services.AddControllersWithViews();

// Dodaj obs³ugê Razor Pages (Identity czasami wymaga)
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

app.UseAuthentication(); // musi byæ przed Authorization
app.UseAuthorization();

// Mapowanie kontrolerów + opcjonalnie Razor Pages
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // konieczne, jeœli korzystasz z Identity Pages np. Logout, Register

app.Run();
