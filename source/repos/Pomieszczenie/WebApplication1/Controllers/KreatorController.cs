using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Controllers
{
	[Authorize]
	public class KreatorController : Controller
	{
		private readonly AppDbContext _context;
		private readonly UserManager<Users> _userManager;

		public KreatorController(AppDbContext context, UserManager<Users> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		[HttpGet]
		public IActionResult Index()
		{
			var userId = _userManager.GetUserId(User);
			var pokoje = _context.Pokoje
				.Include(p => p.PunktyLokacji)
				.Include(p => p.Zdjecia)
				.Where(p => p.UserId == userId)
				.ToList();
			return View(pokoje);
		}

		[HttpPost]
		public async Task<IActionResult> Index(string nazwa, int szacowanaWielkosc, string opis, IFormFile? zdjecie,
											   double? latitude, double? longitude, DateTime? dataPomiaru)
		{
			var userId = _userManager.GetUserId(User);

			var pokoj = new Pokoj
			{
				Nazwa = nazwa,
				SzacowanaWielkosc = szacowanaWielkosc,
				Opis = opis,
				UserId = userId
			};

			if (latitude.HasValue && longitude.HasValue && dataPomiaru.HasValue)
			{
				pokoj.PunktyLokacji.Add(new PunktLokacji
				{
					Latitude = latitude.Value,
					Longitude = longitude.Value,
					DataPomiaru = dataPomiaru.Value
				});
			}

			_context.Pokoje.Add(pokoj);
			await _context.SaveChangesAsync();

			if (zdjecie != null && zdjecie.Length > 0)
			{
				await SaveZdjecie(zdjecie, pokoj.Id);
			}

			return RedirectToAction("Index");
		}

		[HttpGet]
		public IActionResult Edit(int id)
		{
			var userId = _userManager.GetUserId(User);
			var pokoj = _context.Pokoje
				.Include(p => p.PunktyLokacji)
				.Include(p => p.Zdjecia)
				.FirstOrDefault(p => p.Id == id && p.UserId == userId);

			if (pokoj == null) return NotFound();
			return View(pokoj);
		}

		[HttpPost]
		public IActionResult Edit(Pokoj pokoj)
		{
			if (!ModelState.IsValid) return View(pokoj);

			var userId = _userManager.GetUserId(User);
			var istniejacyPokoj = _context.Pokoje
				.Include(p => p.PunktyLokacji)
				.FirstOrDefault(p => p.Id == pokoj.Id);

			if (istniejacyPokoj == null || istniejacyPokoj.UserId != userId)
			{
				return NotFound();
			}

			istniejacyPokoj.Nazwa = pokoj.Nazwa;
			istniejacyPokoj.SzacowanaWielkosc = pokoj.SzacowanaWielkosc;
			istniejacyPokoj.Opis = pokoj.Opis;

			if (pokoj.PunktyLokacji != null)
			{
				foreach (var punkt in pokoj.PunktyLokacji)
				{
					var istniejacyPunkt = istniejacyPokoj.PunktyLokacji
						.FirstOrDefault(p => p.Id == punkt.Id);

					if (istniejacyPunkt != null)
					{
						istniejacyPunkt.Latitude = punkt.Latitude;
						istniejacyPunkt.Longitude = punkt.Longitude;
						istniejacyPunkt.DataPomiaru = punkt.DataPomiaru;
					}
				}
			}

			_context.SaveChanges();
			return RedirectToAction("Index");
		}

		[HttpGet]
		public IActionResult Delete(int id)
		{
			var userId = _userManager.GetUserId(User);
			var pokoj = _context.Pokoje
				.Include(p => p.PunktyLokacji)
				.FirstOrDefault(p => p.Id == id && p.UserId == userId);

			if (pokoj == null) return NotFound();

			if (pokoj.PunktyLokacji.Any())
			{
				TempData["ErrorMessage"] = "Nie można usunąć pokoju, ponieważ ma przypisane punkty lokacji. Usuń najpierw wszystkie punkty lokacji.";
				return RedirectToAction("Index");
			}

			_context.Pokoje.Remove(pokoj);
			_context.SaveChanges();

			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult DeletePunktLokacji(int punktId, int pokojId)
		{
			var userId = _userManager.GetUserId(User);
			var pokoj = _context.Pokoje.FirstOrDefault(p => p.Id == pokojId && p.UserId == userId);

			if (pokoj == null) return NotFound();

			var punkt = _context.PunktyLokacji.FirstOrDefault(p => p.Id == punktId && p.PokojId == pokojId);
			if (punkt != null)
			{
				_context.PunktyLokacji.Remove(punkt);
				_context.SaveChanges();
			}

			return RedirectToAction("Edit", new { id = pokojId });
		}

		[HttpPost]
		public IActionResult AddPunktLokacji(int pokojId, double latitude, double longitude, DateTime dataPomiaru)
		{
			var userId = _userManager.GetUserId(User);
			var pokoj = _context.Pokoje.FirstOrDefault(p => p.Id == pokojId && p.UserId == userId);

			if (pokoj == null) return NotFound();

			var nowyPunkt = new PunktLokacji
			{
				PokojId = pokojId,
				Latitude = latitude,
				Longitude = longitude,
				DataPomiaru = dataPomiaru
			};

			_context.PunktyLokacji.Add(nowyPunkt);
			_context.SaveChanges();

			return RedirectToAction("Edit", new { id = pokojId });
		}

		[HttpPost]
		public async Task<IActionResult> AddZdjecie(int pokojId, IFormFile zdjecie)
		{
			var userId = _userManager.GetUserId(User);
			var pokoj = _context.Pokoje.FirstOrDefault(p => p.Id == pokojId && p.UserId == userId);

			if (pokoj == null) return NotFound();

			if (zdjecie != null && zdjecie.Length > 0)
			{
				await SaveZdjecie(zdjecie, pokojId);
			}

			return RedirectToAction("Edit", new { id = pokojId });
		}

		[HttpPost]
		public IActionResult DeleteZdjecie(int zdjecieId, int pokojId)
		{
			var userId = _userManager.GetUserId(User);
			var pokoj = _context.Pokoje.FirstOrDefault(p => p.Id == pokojId && p.UserId == userId);

			if (pokoj == null) return NotFound();

			var zdjecie = _context.PokojZdjecia.FirstOrDefault(z => z.Id == zdjecieId && z.PokojId == pokojId);
			if (zdjecie != null)
			{
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", zdjecie.NazwaPliku);
				if (System.IO.File.Exists(filePath))
				{
					System.IO.File.Delete(filePath);
				}

				_context.PokojZdjecia.Remove(zdjecie);
				_context.SaveChanges();
			}

			return RedirectToAction("Edit", new { id = pokojId });
		}

		private async Task SaveZdjecie(IFormFile zdjecie, int pokojId)
		{
			try
			{
				var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
				if (!Directory.Exists(uploadsFolder))
				{
					Directory.CreateDirectory(uploadsFolder);
				}

				var fileExtension = Path.GetExtension(zdjecie.FileName);
				var fileName = $"{Guid.NewGuid()}{fileExtension}";
				var filePath = Path.Combine(uploadsFolder, fileName);

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await zdjecie.CopyToAsync(stream);
				}

				var pokojZdjecie = new PokojZdjecie
				{
					PokojId = pokojId,
					NazwaPliku = fileName,
					SciezkaPliku = filePath,
					DataDodania = DateTime.Now
				};

				_context.PokojZdjecia.Add(pokojZdjecie);
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				// Zaloguj błąd lub wyświetl komunikat użytkownikowi
				// Możesz użyć loggera lub TempData
				TempData["ErrorMessage"] = "Wystąpił błąd podczas dodawania zdjęcia: " + ex.Message;
			}
		}
	}
}
