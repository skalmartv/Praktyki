using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
	public class WebApplication1DbContext : DbContext
	{
		public DbSet<Pomieszczenia> Pomieszczenia { get; set; }


		public WebApplication1DbContext(DbContextOptions<WebApplication1DbContext> options)
			: base(options)
		{

		}
	}
}
