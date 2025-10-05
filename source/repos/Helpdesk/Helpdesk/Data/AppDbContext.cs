using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Helpdesk.Models;

namespace Helpdesk.Data
{
	public class AppDbContext : IdentityDbContext<ApplicationUser>
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<Ticket> Tickets { get; set; }
		public DbSet<Comment> Comments { get; set; }
		public DbSet<Attachment> Attachments { get; set; }
	}
}