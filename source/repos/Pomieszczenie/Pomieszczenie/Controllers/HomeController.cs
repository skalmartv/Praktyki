using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly WebApplication1DbContext _context;

		public HomeController(ILogger<HomeController> logger, WebApplication1DbContext context)
		{
			_logger = logger;
			_context = context;
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Pomieszczenia()
		{
			var allPomieszczenia = _context.Pomieszczenia
										   .Include(p => p.PunktyLokacji)
										   .ToList();
			return View(allPomieszczenia);
		}

		[HttpGet]
		public IActionResult CreateEditPomieszczenia(int? id)
		{
			if (id.HasValue)
			{
				var existing = _context.Pomieszczenia
									   .Include(p => p.PunktyLokacji)
									   .FirstOrDefault(p => p.Id == id.Value);
				if (existing == null) return NotFound();
				if(id != null)
				{
					var PomieszczeniaiInDb = _context.Pomieszczenia.SingleOrDefault(pomieszczenie => pomieszczenie.Id == id);
					return View(PomieszczeniaiInDb);
				}
				return View(existing);
			}

			return View(new Pomieszczenia
			{
				PunktyLokacji = new List<PunktLokacji> { new PunktLokacji() }
			});
		}
		public IActionResult DeletePomieszczenia(int id)
		{
			var PomieszczeniaiInDb = _context.Pomieszczenia.SingleOrDefault(pomieszczenie => pomieszczenie.Id == id);
			_context.Pomieszczenia.Remove(PomieszczeniaiInDb);
			_context.SaveChanges();
			return RedirectToAction("Pomieszczenia");

		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult CreateEditPomieszczeniaForm(Pomieszczenia model)
		{
			if (model.PunktyLokacji == null || model.PunktyLokacji.Count == 0)
			{
				ModelState.AddModelError("", "Dodaj co najmniej jeden punkt lokacji.");
			}

			if (!ModelState.IsValid)
			{
				if (model.PunktyLokacji == null)
					model.PunktyLokacji = new List<PunktLokacji>();
				return View("CreateEditPomieszczenia", model);
			}

			if (model.Id == 0)
			{
				_context.Pomieszczenia.Add(model);
			}
			else
			{
				var existing = _context.Pomieszczenia
									   .Include(p => p.PunktyLokacji)
									   .FirstOrDefault(p => p.Id == model.Id);
				if (existing == null) return NotFound();

				existing.Nazwa = model.Nazwa;
				existing.SzacowanaWielkosc = model.SzacowanaWielkosc;
				existing.Opis = model.Opis;
				existing.ThumbnailUrl = model.ThumbnailUrl;

				existing.PunktyLokacji.Clear();
				foreach (var pkt in model.PunktyLokacji)
				{
					existing.PunktyLokacji.Add(pkt);
				}

				_context.Pomieszczenia.Update(existing);
			}

			_context.SaveChanges();
			return RedirectToAction(nameof(Pomieszczenia));
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}