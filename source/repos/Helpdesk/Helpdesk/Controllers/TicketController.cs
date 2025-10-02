using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Helpdesk.Data;
using Helpdesk.Models;

namespace Helpdesk.Controllers
{
	[Authorize] // tylko zalogowani użytkownicy
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

		// GET: Tickets
		public async Task<IActionResult> TicketIndex()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return Challenge();

			var tickets = await _context.Tickets
				.Where(t => t.UserId == user.Id)
				.ToListAsync();
			return View(tickets);
		}

		// GET: Tickets/Details/5
		public async Task<IActionResult> TicketDetails(int? id)
		{
			if (id == null) return NotFound();

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return Challenge();

			var ticket = await _context.Tickets
				.Include(t => t.Comments)
				.Include(t => t.Attachments)
				.FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);

			if (ticket == null) return NotFound();

			return View(ticket);
		}

		// GET: Tickets/Create
		public IActionResult TicketCreate()
		{
			return View();
		}

		// POST: Tickets/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> TicketCreate(Ticket ticket)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return Challenge();

			ticket.UserId = user.Id;
			ticket.CreatedAt = DateTime.UtcNow;

			if (ModelState.IsValid)
			{
				_context.Tickets.Add(ticket);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(TicketIndex));
			}
			return View(ticket);
		}

		// GET: Tickets/Edit/5
		public async Task<IActionResult> TicketEdit(int? id)
		{
			if (id == null) return NotFound();

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return Challenge();

			var ticket = await _context.Tickets
				.FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);

			if (ticket == null) return NotFound();

			return View(ticket);
		}

		// POST: Tickets/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> TicketEdit(int id, Ticket formModel)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return Challenge();

			if (!ModelState.IsValid)
				return View(formModel);

			var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);
			if (ticket == null) return NotFound();

			ticket.Title = formModel.Title;
			ticket.Description = formModel.Description;
			ticket.Status = formModel.Status;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!_context.Tickets.Any(e => e.Id == id)) return NotFound();
				else throw;
			}

			return RedirectToAction(nameof(TicketIndex));
		}

		// GET: Tickets/Delete/5
		public async Task<IActionResult> TicketDelete(int? id)
		{
			if (id == null) return NotFound();

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return Challenge();

			var ticket = await _context.Tickets
				.FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);

			if (ticket == null) return NotFound();

			return View("TicketDelete", ticket);
		}

		// POST: Tickets/Delete/5
		[HttpPost, ActionName("TicketDelete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> TicketDeleteConfirmed(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return Challenge();

			var ticket = await _context.Tickets
				.FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);

			if (ticket != null)
			{
				_context.Tickets.Remove(ticket);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(TicketIndex));
		}

		// POST: Tickets/AddComment
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

		// POST: Tickets/DeleteComment/5
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

		// POST: Tickets/UploadAttachment
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

		// GET: Tickets/DownloadAttachment/5
		public async Task<IActionResult> DownloadAttachment(int id)
		{
			var attachment = await _context.Attachments.FindAsync(id);
			if (attachment == null) return NotFound();

			var path = Path.Combine(_env.WebRootPath, attachment.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
			var mimeType = "application/octet-stream";
			return PhysicalFile(path, mimeType, attachment.FileName);
		}

		// POST: Tickets/DeleteAttachment/5
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

		public string GetUploadsFolder()
		{
			return Path.Combine(_env.WebRootPath, "upload");
		}
	}
}
