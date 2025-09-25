using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class PokojZdjecie
    {
        public int Id { get; set; }
        public string NazwaPliku { get; set; } = string.Empty;
        public string SciezkaPliku { get; set; } = string.Empty;
        public DateTime DataDodania { get; set; } = DateTime.Now;
        
        public int PokojId { get; set; }
        
        [ForeignKey("PokojId")]
        public Pokoj Pokoj { get; set; } = null!;
    }
}