using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Testes.Data;
using Testes.Services;

namespace Testes.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DonationController : ControllerBase
{
    private readonly IDonationService _donationService;
    private readonly AppDbContext _context;
    private readonly OpenCageService _openCageService;

    public DonationController(IDonationService donationService, AppDbContext context, OpenCageService openCageService)
    {
        _donationService = donationService;
        _context = context;
        _openCageService = openCageService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateDonation([FromBody] DonationCreateDto donationDto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        try
        {
            var donation = await _donationService.CreateDonationAsync(donationDto, userId);

            // Retornar como DTO (para evitar erro de ciclo no JSON)
            var response = new DonationResponseDto
            {
                Id = donation.Id,
                Title = donation.Title,
                Category = donation.Category,
                Description = donation.Description,
                Quantity = donation.Quantity,
                Unit = donation.Unit,
                ExpirationDate = donation.ExpirationDate,
                PickupLatitude = donation.PickupLatitude,
                PickupLongitude = donation.PickupLongitude,
                CO2Impact = donation.CO2Impact,
                WaterImpact = donation.WaterImpact,
                CreatedAt = donation.CreatedAt,

                // Dados do criador
                CreatorStreet = donation.User?.Street,
                CreatorNumber = donation.User?.Number,
                CreatorNeighborhood = donation.User?.Neighborhood,
                CreatorCity = donation.User?.City,
                CreatorState = donation.User?.State,

                IsReserved = donation.IsReserved,
                ReservedByUserId = donation.ReservedByUserId,
                ReservedByUserName = donation.ReservedByUser?.Name,
                ReservedAt = donation.ReservedAt,
                CreatorName = donation.User?.Name,
                CreatorPhone = donation.User?.Phone,
                CreatorEmail = donation.User?.Email,
                CreatorId = donation.UserId
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Demais métodos mantidos iguais ao seu original (GetUserDonations, GetAvailableDonations, MyDonations, AllDonations, ReserveDonation, etc)...

    [HttpGet]
    public async Task<IActionResult> GetUserDonations()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var donations = await _donationService.GetUserDonationsAsync(userId);
        return Ok(donations);
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetAvailableDonations([FromQuery] string sort = "date")
    {
        var donations = await _donationService.GetAvailableDonationsAsync(sort);

        var response = donations.Select(d => new DonationResponseDto
        {
            Id = d.Id,
            Title = d.Title,
            Category = d.Category,
            Description = d.Description,
            Quantity = d.Quantity,
            Unit = d.Unit,
            ExpirationDate = d.ExpirationDate,
            PickupLatitude = d.PickupLatitude,
            PickupLongitude = d.PickupLongitude,
            CreatorStreet = d.User.Street,
            CreatorNumber = d.User.Number,
            CreatorNeighborhood = d.User.Neighborhood,
            CreatorCity = d.User.City,
            CreatorState = d.User.State,
            IsReserved = d.IsReserved,
            ReservedByUserId = d.ReservedByUserId,
            ReservedByUserName = d.ReservedByUser?.Name,
            ReservedAt = d.ReservedAt,
            CreatorName = d.User.Name,
            CreatorPhone = d.User.Phone,
            CreatorEmail = d.User.Email,
            CreatorId = d.UserId
        }).ToList();

        return Ok(response);
    }

    [HttpGet("MyDonations")]
    public async Task<ActionResult> GetMyDonations([FromQuery] string sort = "date")
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var donations = await _donationService.GetDonationsByUserIdAsync(userId, sort);

        var response = donations.Select(d => new DonationResponseDto
        {
            Id = d.Id,
            Title = d.Title,
            Category = d.Category,
            Description = d.Description,
            Quantity = d.Quantity,
            Unit = d.Unit,
            ExpirationDate = d.ExpirationDate,
            PickupLatitude = d.PickupLatitude,
            PickupLongitude = d.PickupLongitude,
            CO2Impact = d.CO2Impact,
            WaterImpact = d.WaterImpact,
            CreatedAt = d.CreatedAt,
            CreatorStreet = d.User.Street,
            CreatorNumber = d.User.Number,
            CreatorNeighborhood = d.User.Neighborhood,
            CreatorCity = d.User.City,
            CreatorState = d.User.State,
            IsReserved = d.IsReserved,
            ReservedByUserId = d.ReservedByUserId,
            ReservedByUserName = d.ReservedByUser?.Name,
            ReservedAt = d.ReservedAt,
            CreatorName = d.User.Name,
            CreatorPhone = d.User.Phone,
            CreatorEmail = d.User.Email,
            CreatorId = d.UserId
            
        }).ToList();

        return Ok(response);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllDonations([FromQuery] string sort = "date")
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var user = await _context.Users.FindAsync(userId);

        if (user?.Type?.ToLower() != "ngo")
            return Forbid();

        var donations = await _donationService.GetAllDonationsAsync(sort);

        var response = donations.Select(d => new DonationResponseDto
        {
            Id = d.Id,
            Title = d.Title,
            Category = d.Category,
            Description = d.Description,
            Quantity = d.Quantity,
            Unit = d.Unit,
            ExpirationDate = d.ExpirationDate,
            PickupLatitude = d.PickupLatitude,
            PickupLongitude = d.PickupLongitude,
            CO2Impact = d.CO2Impact,
            WaterImpact = d.WaterImpact,
            CreatedAt = d.CreatedAt,
            CreatorStreet = d.User.Street,
            CreatorNumber = d.User.Number,
            CreatorNeighborhood = d.User.Neighborhood,
            CreatorCity = d.User.City,
            CreatorState = d.User.State,
            IsReserved = d.IsReserved,
            ReservedByUserId = d.ReservedByUserId,
            ReservedByUserName = d.ReservedByUser?.Name,
            ReservedAt = d.ReservedAt,
            CreatorName = d.User.Name,
            CreatorPhone = d.User.Phone,
            CreatorEmail = d.User.Email,
            CreatorId = d.UserId
        }).ToList();

        return Ok(response);
    }

    [HttpPatch("{id}/reserve")]
    public async Task<IActionResult> ReserveDonation(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var user = await _context.Users.FindAsync(userId);

        if (user?.Type?.ToLower() != "ngo")
            return Forbid();

        var donation = await _context.Donations
            .Include(d => d.User)
            .Include(d => d.ReservedByUser)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (donation == null)
            return NotFound("Doação não encontrada");

        if (donation.IsReserved)
            return BadRequest("Esta doação já foi reservada");

        if (donation.ExpirationDate < DateTime.UtcNow)
            return BadRequest("Esta doação está expirada");

        if (donation.UserId == userId)
            return BadRequest("Você não pode reservar sua própria doação");

        donation.IsReserved = true;
        donation.ReservedByUserId = userId;
        donation.ReservedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var response = new DonationResponseDto
        {
            Id = donation.Id,
            Title = donation.Title,
            Category = donation.Category,
            Description = donation.Description,
            Quantity = donation.Quantity,
            Unit = donation.Unit,
            ExpirationDate = donation.ExpirationDate,
            PickupLatitude = donation.PickupLatitude,
            PickupLongitude = donation.PickupLongitude,
            CO2Impact = donation.CO2Impact,
            WaterImpact = donation.WaterImpact,
            CreatedAt = donation.CreatedAt,
            CreatorStreet = donation.User.Street,
            CreatorNumber = donation.User.Number,
            CreatorNeighborhood = donation.User.Neighborhood,
            CreatorCity = donation.User.City,
            CreatorState = donation.User.State,
            IsReserved = donation.IsReserved ,
            ReservedByUserId = donation.ReservedByUserId,
            ReservedByUserName = donation.ReservedByUser?.Name,
            ReservedAt = donation.ReservedAt,
            CreatorId = donation.UserId
        };

        return Ok(response);
    }

    [HttpGet("reserved-by-me")]
    public async Task<IActionResult> GetDonationsReservedByMe()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        await _donationService.CancelarReservasExpiradasAsync();

        var donations = await _context.Donations
            .Include(d => d.User)
            .Include(d => d.ReservedByUser)
            .Where(d => d.ReservedByUserId == userId)
            .OrderByDescending(d => d.ReservedAt)
            .ToListAsync();

        var response = donations.Select(d => new DonationResponseDto
        {
            Id = d.Id,
            Title = d.Title,
            Category = d.Category,
            Description = d.Description,
            Quantity = d.Quantity,
            Unit = d.Unit,
            ExpirationDate = d.ExpirationDate,
            PickupLatitude = d.PickupLatitude,
            PickupLongitude = d.PickupLongitude,
            CO2Impact = d.CO2Impact,
            WaterImpact = d.WaterImpact,
            CreatedAt = d.CreatedAt,
            CreatorStreet = d.User.Street,
            CreatorNumber = d.User.Number,
            CreatorNeighborhood = d.User.Neighborhood,
            CreatorCity = d.User.City,
            CreatorState = d.User.State,
            IsReserved = d.IsReserved,
            ReservedByUserId = d.ReservedByUserId, 
            ReservedByUserName = d.ReservedByUser?.Name,
            ReservedAt = d.ReservedAt,
            CreatorName = d.User.Name,
            CreatorPhone = d.User.Phone,
            CreatorEmail = d.User.Email,
            CreatorId = d.UserId
        }).ToList();

        return Ok(response);
    }
    
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteDonation(int id)
    {
        var donation = await _context.Donations.FindAsync(id);
        if (donation == null)
        {
            return NotFound("Doação não encontrada.");
        }

        // Verifica se o usuário logado é o mesmo que fez a reserva (se quiser esse controle)
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        if (donation.ReservedByUserId != userId)
        {
            return Forbid("Você só pode excluir suas próprias doações reservadas.");
        }

        _context.Donations.Remove(donation);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    [HttpDelete("{id:int}/creator")]
    [Authorize]
    public async Task<IActionResult> DeleteCreatedDonation(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var donation = await _context.Donations.FindAsync(id);
        if (donation == null)
        {
            return NotFound("Doação não encontrada.");
        }

        if (donation.UserId != userId)
        {
            return Forbid("Você só pode excluir doações que você criou.");
        }

        _context.Donations.Remove(donation);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    
    
}


public class DonationCreateDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Category { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0.1, double.MaxValue)]
    public decimal Quantity { get; set; }
    
    [Required]
    public string Unit { get; set; } = string.Empty;
    
    [Required]
    public DateTime ExpirationDate { get; set; }
    
    public double? PickupLatitude { get; set; }
    public double? PickupLongitude { get; set; }
}

public class DonationResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
    public DateTime ExpirationDate { get; set; }
    public double? PickupLatitude { get; set; }
    public double? PickupLongitude { get; set; }
    
    public decimal? CO2Impact { get; set; } // kg de CO2 economizado
    public decimal? WaterImpact { get; set; } // litros de água economizados
    public DateTime CreatedAt { get; set; } // data de criação da doação
    
    // Dados do criador
    public string CreatorStreet { get; set; }
    public string CreatorNumber { get; set; }
    public string CreatorNeighborhood { get; set; }
    public string CreatorCity { get; set; }
    public string CreatorState { get; set; }
    
    
    public bool IsReserved { get; set; }
    public int? ReservedByUserId { get; set; }
    public string? ReservedByUserName { get; set; }
    public DateTime? ReservedAt { get; set; }
    
    public string CreatorName { get; set; }
    public string CreatorPhone { get; set; }
    public string CreatorEmail { get; set; }
    
    public int CreatorId { get; set; }
}
