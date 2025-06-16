using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Testes.Models;

public class Donation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    public string Title { get; set; }
    
    [Required]
    public string Category { get; set; } // prepared, produce, bakery, etc.
    
    [Required]
    public string Description { get; set; }
    
    [Required]
    public decimal Quantity { get; set; }
    
    [Required]
    public string Unit { get; set; } // kg, g, l, ml, units, etc.
    
    [Required]
    public DateTime ExpirationDate { get; set; }
    
    public double? PickupLatitude { get; set; }
    public double? PickupLongitude { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User User { get; set; }
    
    // Campos para impacto ambiental (podem ser calculados ou fornecidos)
    public decimal? CO2Impact { get; set; } // em kg
    public decimal? WaterImpact { get; set; } // em litros
    
    
    // Adicionar estas novas propriedades
    public bool IsReserved { get; set; } = false;
    public int? ReservedByUserId { get; set; }
    
    [ForeignKey("ReservedByUserId")]
    public User? ReservedByUser { get; set; }
    
    public DateTime? ReservedAt { get; set; }
    
    
    

    
}