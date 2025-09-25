using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Services
{
	public static class SeedService
	{
		public static async Task SeedDatabase(IServiceProvider services)
		{
			// Utwórz scope, aby mieć dostęp do serwisów wbudowanych
			using var scope = services.CreateScope();
			var serviceProvider = scope.ServiceProvider;

			var userManager = serviceProvider.GetRequiredService<UserManager<Users>>();
			var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

			// Rolę Administrator, jeśli nie istnieje
			const string adminRole = "Administrator";
			if (!await roleManager.RoleExistsAsync(adminRole))
			{
				await roleManager.CreateAsync(new IdentityRole(adminRole));
			}

			// Domyślny użytkownik administratora, jeśli nie istnieje
			const string adminEmail = "admin@local";
			const string adminPassword = "Admin123!";

			var adminUser = await userManager.FindByEmailAsync(adminEmail);
			if (adminUser == null)
			{
				adminUser = new Users
				{
					UserName = adminEmail,
					Email = adminEmail,
					EmailConfirmed = true
				};

				var result = await userManager.CreateAsync(adminUser, adminPassword);
				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(adminUser, adminRole);
				}
			}
		}
	}
}
