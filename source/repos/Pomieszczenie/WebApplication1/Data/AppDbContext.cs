using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
	public class AppDbContext : IdentityDbContext<Users>
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<Pokoj> Pokoje { get; set; }
		public DbSet<PunktLokacji> PunktyLokacji { get; set; }
		public DbSet<PokojZdjecie> PokojZdjecia { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder); 

			modelBuilder.Entity<Pokoj>()
				.HasOne(p => p.User)
				.WithMany() 
				.HasForeignKey(p => p.UserId)
				.OnDelete(DeleteBehavior.SetNull);

			modelBuilder.Entity<PokojZdjecie>()
				.HasOne(pz => pz.Pokoj)
				.WithMany(p => p.Zdjecia)
				.HasForeignKey(pz => pz.PokojId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}