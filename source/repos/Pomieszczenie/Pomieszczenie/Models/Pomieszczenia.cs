namespace WebApplication1.Models
{
	public class Pomieszczenia
	{
		public int Id { get; set; }
		public string Nazwa { get; set; }
		public double SzacowanaWielkosc {  get; set; }

		public string Opis { get; set; }

		public string ThumbnailUrl { get; set; }

		public List<PunktLokacji> PunktyLokacji { get; set; } = new List<PunktLokacji>();

	}
	public class PunktLokacji
	{
		public int Id { get; set; }
		public double Szerokość { get; set; }
		public double Wysokość { get; set; }
		public DateTime DataPomiaru { get; set; }
	}
}
