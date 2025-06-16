using System.ComponentModel.DataAnnotations;

namespace Testes.Models;

public class PasswordResetToken
{
    [Key]
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public bool Used { get; set; } = false;
}