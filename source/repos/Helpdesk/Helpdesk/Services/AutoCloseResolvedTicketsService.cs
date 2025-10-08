using Helpdesk.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Helpdesk.Services
{
	public class AutoCloseResolvedTicketsService : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<AutoCloseResolvedTicketsService> _logger;
		private static readonly TimeSpan _interval = TimeSpan.FromHours(1); // co godzin�

		public AutoCloseResolvedTicketsService(IServiceScopeFactory scopeFactory, ILogger<AutoCloseResolvedTicketsService> logger)
		{
			_scopeFactory = scopeFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("AutoCloseResolvedTicketsService wystartowa�.");
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					await ProcessAsync(stoppingToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "B��d w AutoCloseResolvedTicketsService");
				}
				await Task.Delay(_interval, stoppingToken);
			}
		}

		private async Task ProcessAsync(CancellationToken ct)
		{
			using var scope = _scopeFactory.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			var limit = DateTime.UtcNow.AddDays(-7);

			var candidates = await db.Tickets
				.Where(t => t.Status == "Rozwi�zane"
							&& t.ResolvedAt != null
							&& t.ResolvedAt <= limit
							&& !t.SystemClosed
							&& t.Status != "Zamkni�ty")
				.ToListAsync(ct);

			if (candidates.Count == 0) return;

			foreach (var t in candidates)
			{
				t.Status = "Zamkni�ty";
				t.SystemClosed = true;
				t.SystemClosedAt = DateTime.UtcNow;

				
			}

			await db.SaveChangesAsync(ct);
			_logger.LogInformation("Automatycznie zamkni�to {Count} zg�osze�.", candidates.Count);
		}
	}
}