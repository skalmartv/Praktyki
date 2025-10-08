using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Helpdesk.Data;
using Helpdesk.Models;
using Helpdesk.Models.ViewModels; 
using System.Linq;

namespace Helpdesk.Controllers
{
	[Authorize]
	public class TicketsController : Controller
	{
		private readonly AppDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IWebHostEnvironment _env;

		public TicketsController(AppDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
		{
			_context = context;
			_userManager = userManager;
			_env = env;
		}

		private static readonly string[] _allowedStatuses = new[] { "Nowy", "Otwarte", "W toku", "Oczekuje", "Rozwiązane", "Zamknięty" };
		private static readonly string[] _allowedPriorities = new[] { "Niski", "Normalny", "Wysoki", "Krytyczny" };

		// INDEX z wyszukiwaniem / filtrowaniem / sortowaniem
		public async Task<IActionResult> TicketIndex(string search, string status, string priority, string sort = "desc")
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Challenge();

			bool isAgent = await _userManager.IsInRoleAsync(user, "Agent") || await _userManager.IsInRoleAsync(user, "Admin");

			var query = _context.Tickets
				.Include(t => t.AssignedTo)
				.Include(t => t.CreatedBy)
				.AsQueryable();

			if (!isAgent)
				query = query.Where(t => t.UserId == user.Id);

			// Filtrowanie status
			if (!string.IsNullOrWhiteSpace(status) && _allowedStatuses.Contains(status))
				query = query.Where(t => t.Status == status);

			// Filtrowanie priorytetu
			if (!string.IsNullOrWhiteSpace(priority) && _allowedPriorities.Contains(priority))
				query = query.Where(t => t.Priority == priority);

			// Wyszukiwanie w tytule i opisie
			if (!string.IsNullOrWhiteSpace(search))
			{
				string s = search.Trim();
				query = query.Where(t =>
					t.Title.Contains(s) ||
					t.Description.Contains(s));
			}

			// Sortowanie po dacie (CreatedAt)
			query = sort == "asc"
				? query.OrderBy(t => t.CreatedAt)
				: query.OrderByDescending(t => t.CreatedAt);

			var tickets = await query.ToListAsync();

			ViewBag.CurrentUserId = user.Id;
			ViewBag.Statuses = _allowedStatuses;
			ViewBag.Priorities = _allowedPriorities;
			ViewBag.FilterSearch = search;
			ViewBag.FilterStatus = status;
			ViewBag.FilterPriority = priority;
			ViewBag.Sort = sort;

			return View(tickets);
		}

		public async Task<IActionResult> TicketDetails(int? id)
		{
			if (id == null) return NotFound();
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Challenge();

			bool isAgent = await _userManager.IsInRoleAsync(user, "Agent") || await _userManager.IsInRoleAsync(user, "Admin");

			var ticket = await _context.Tickets
				.Include(t => t.CreatedBy)
				.Include(t => t.AssignedTo)
				.Include(t => t.Comments).ThenInclude(c => c.User)
				.Include(t => t.Attachments).ThenInclude(a => a.User)
				.FirstOrDefaultAsync(t => t.Id == id && (isAgent || t.UserId == user.Id));

			if (ticket == null) return NotFound();

			ViewBag.AllowedStatuses = _allowedStatuses;
			ViewBag.CurrentUserId = user.Id;
			return View(ticket);
		}

		public IActionResult TicketCreate()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> TicketCreate(Ticket ticket)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return Challenge();

			ticket.UserId = user.Id;
			ticket.CreatedAt = DateTime.UtcNow;

			
			if (!_allowedPriorities.Contains(ticket.Priority))
				ticket.Priority = "Normalny";

			if (ModelState.IsValid)
			{
				_context.Tickets.Add(ticket);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(TicketIndex));
			}
			return View(ticket);
		}

		
		public async Task<IActionResult> TicketEdit(int? id)
		{
			return Forbid(); 
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> TicketEdit(int id, Ticket formModel)
		{
			return Forbid(); 
		}

		
		public async Task<IActionResult> TicketDelete(int? id)
		{
			if (id == null) return NotFound();
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Challenge();
			bool isAgent = await _userManager.IsInRoleAsync(user, "Agent") || await _userManager.IsInRoleAsync(user, "Admin");
			if (!isAgent) return Forbid();

			var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
			if (ticket == null) return NotFound();
			return View("TicketDelete", ticket);
		}

		[HttpPost, ActionName("TicketDelete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> TicketDeleteConfirmed(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return Challenge();

			bool isAgent = await _userManager.IsInRoleAsync(user, "Agent") || await _userManager.IsInRoleAsync(user, "Admin");

			var ticket = await _context.Tickets
				.FirstOrDefaultAsync(t => t.Id == id && (t.UserId == user.Id || isAgent));

			if (ticket != null)
			{
				_context.Tickets.Remove(ticket);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(TicketIndex));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateStatus(int id, string status)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Challenge();

			bool isAgent = await _userManager.IsInRoleAsync(user, "Agent") || await _userManager.IsInRoleAsync(user, "Admin");
			if (!isAgent) return Forbid();

			var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
			if (ticket == null) return NotFound();

			if (!string.IsNullOrWhiteSpace(status) && _allowedStatuses.Contains(status))
			{
				var old = ticket.Status;
				ticket.Status = status;

				if (status == "Rozwiązane" && ticket.ResolvedAt == null)
					ticket.ResolvedAt = DateTime.UtcNow;

				// jeśli zmieniono z "Rozwiązane" na inny niż "Zamknięty" – resetuj ResolvedAt
				if (old == "Rozwiązane" && status != "Rozwiązane")
					ticket.ResolvedAt = null;

				// manualne zamknięcie nie jest systemowe
				if (status == "Zamknięty")
				{
					ticket.SystemClosed = ticket.SystemClosed; // bez zmiany jeśli już systemowo
					if (!ticket.SystemClosed)
						ticket.SystemClosedAt = ticket.SystemClosedAt ?? DateTime.UtcNow;
				}
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(TicketDetails), new { id });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CloseTicket(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Challenge();

			var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
			if (ticket == null) return NotFound();

			// tylko właściciel może zamknąć swoje zgłoszenie
			if (ticket.UserId != user.Id) return Forbid();

			if (ticket.Status != "Zamknięty")
			{
				ticket.Status = "Zamknięty";
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(TicketDetails), new { id });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddComment(int ticketId, string content)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return Challenge();

			if (string.IsNullOrWhiteSpace(content))
				return RedirectToAction(nameof(TicketDetails), new { id = ticketId });

			var comment = new Comment
			{
				TicketId = ticketId,
				Content = content,
				UserId = user.Id,
				CreatedAt = DateTime.UtcNow
			};

			_context.Comments.Add(comment);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(TicketDetails), new { id = ticketId });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteComment(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return Challenge();

			var comment = await _context.Comments
				.FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

			if (comment == null)
				return NotFound();

			int ticketId = comment.TicketId;

			_context.Comments.Remove(comment);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(TicketDetails), new { id = ticketId });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UploadAttachment(int ticketId, IFormFile file)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Challenge();

			if (file == null || file.Length == 0)
				return RedirectToAction(nameof(TicketDetails), new { id = ticketId });

			try
			{
				var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
				if (!Directory.Exists(uploadsFolder))
					Directory.CreateDirectory(uploadsFolder);

				var safeFileName = Path.GetFileName(file.FileName);
				var uniqueFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(safeFileName);
				var filePath = Path.Combine(uploadsFolder, uniqueFileName);

				using (var stream = System.IO.File.Create(filePath))
				{
					await file.CopyToAsync(stream);
				}

				var attachment = new Attachment
				{
					TicketId = ticketId,
					FileName = safeFileName,
					FilePath = "/uploads/" + uniqueFileName,
					UserId = user.Id,
					UploadedAt = DateTime.UtcNow
				};

				_context.Attachments.Add(attachment);
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				TempData["UploadError"] = "Nie udało się zapisać pliku: " + ex.Message;
			}

			return RedirectToAction(nameof(TicketDetails), new { id = ticketId });
		}

		public async Task<IActionResult> DownloadAttachment(int id)
		{
			var attachment = await _context.Attachments.FindAsync(id);
			if (attachment == null) return NotFound();

			var path = Path.Combine(_env.WebRootPath, attachment.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
			var mimeType = "application/octet-stream";
			return PhysicalFile(path, mimeType, attachment.FileName);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteAttachment(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Challenge();

			var attachment = await _context.Attachments.FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);
			if (attachment == null) return NotFound();

			int ticketId = attachment.TicketId;

			try
			{
				var filePath = Path.Combine(_env.WebRootPath, attachment.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
				if (System.IO.File.Exists(filePath))
					System.IO.File.Delete(filePath);
			}
			catch (Exception ex)
			{
				TempData["UploadError"] = "Nie udało się usunąć pliku: " + ex.Message;
			}

			_context.Attachments.Remove(attachment);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(TicketDetails), new { id = ticketId });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AssignToMe(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Challenge();

			bool isAgent = await _userManager.IsInRoleAsync(user, "Agent") || await _userManager.IsInRoleAsync(user, "Admin");

			if (!isAgent) return Forbid();

			var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
			if (ticket == null) return NotFound();

			ticket.AssignedToId = user.Id;
			if (ticket.Status == "Nowy") ticket.Status = "Otwarte";

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(TicketDetails), new { id });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Unassign(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Challenge();

			bool isAgent = await _userManager.IsInRoleAsync(user, "Agent") || await _userManager.IsInRoleAsync(user, "Admin");
			if (!isAgent) return Forbid();

			var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
			if (ticket == null) return NotFound();

			ticket.AssignedToId = null;
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(TicketDetails), new { id });
		}

		[Authorize(Roles = "Agent,Admin")]
		public async Task<IActionResult> Dashboard()
		{
			var todayStartUtc = DateTime.UtcNow.Date;
			var tomorrowUtc = todayStartUtc.AddDays(1);

			var baseQuery = _context.Tickets.AsNoTracking();

			var total = await baseQuery.CountAsync();
			var newToday = await baseQuery.CountAsync(t => t.CreatedAt >= todayStartUtc && t.CreatedAt < tomorrowUtc);
			var unassigned = await baseQuery.CountAsync(t => t.AssignedToId == null);

			var grouped = await baseQuery
				.GroupBy(t => t.Status)
				.Select(g => new { Status = g.Key, Count = g.Count() })
				.ToListAsync();

			var byStatus = _allowedStatuses
				.Select(s => new TicketStatusCount
				{
					Status = s,
					Count = grouped.FirstOrDefault(x => x.Status == s)?.Count ?? 0
				})
				.ToList();

			var vm = new TicketDashboardViewModel
			{
				TotalTickets = total,
				NewToday = newToday,
				Unassigned = unassigned,
				ByStatus = byStatus,
				AllowedStatuses = _allowedStatuses,
				GeneratedAtUtc = DateTime.UtcNow
			};

			return View(vm);
		}

		public string GetUploadsFolder()
		{
			return Path.Combine(_env.WebRootPath, "upload");
		}
	}
}
