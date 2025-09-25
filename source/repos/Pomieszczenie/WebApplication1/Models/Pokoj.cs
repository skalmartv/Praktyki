using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Models
{
    public class Pokoj
    {
        public int Id { get; set; }
        public string Nazwa { get; set; } = string.Empty;
        public int SzacowanaWielkosc { get; set; }
        public string Opis { get; set; } = string.Empty;
        
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public Users? User { get; set; }
        
        public List<PunktLokacji> PunktyLokacji { get; set; } = new();
        public List<PokojZdjecie> Zdjecia { get; set; } = new();
    }
}
