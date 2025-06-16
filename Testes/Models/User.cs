using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Testes.Models;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public string Type { get; set; } // "business" or "ngo"
    
    public bool IsNgo => Type?.ToLower() == "ngo";
    [Required]
    public string Name { get; set; }
    [Required]
    public string DocumentNumber { get; set; }
    public string? NgoType { get; set; } // Nullable for business users
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Phone { get; set; }

    //[Required]
    //public string Address { get; set; }
    
    [Required]
    public string CEP { get; set; }
    
    [Required]
    public string Street { get; set; }
    
    [Required]
    public string Neighborhood { get; set; }
    
    [Required]
    public string City { get; set; }
    
    [Required]
    public string State { get; set; }
    
    [Required]
    public string Number { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<Donation> Donations { get; set; }
    
    //Email 
    
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }

}

