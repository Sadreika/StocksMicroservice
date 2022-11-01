using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Stock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        [MaxLength(5)]
        public string Currency { get; set; }
        [Required]
        [MaxLength(10)]
        public string Symbol { get; set; }
        public decimal Yield { get; set; }
        [Required]
        [MaxLength(50)]
        public string MarketCap { get; set; }
    }
}

