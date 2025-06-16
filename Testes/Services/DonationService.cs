using Microsoft.EntityFrameworkCore;
using Testes.Controllers;
using Testes.Data;
using Testes.Models;

namespace Testes.Services;

public interface IDonationService
{
    Task<Donation> CreateDonationAsync(DonationCreateDto donationDto, int userId);
    Task<IEnumerable<Donation>> GetUserDonationsAsync(int userId);
    Task<IEnumerable<Donation>> GetAvailableDonationsAsync(string sort = "date");
    Task<IEnumerable<Donation>> GetDonationsByUserIdAsync(int userId, string sort = "date");
    Task<IEnumerable<Donation>> GetAllDonationsAsync(string sort = "date");
    Task CancelarReservasExpiradasAsync();
}

public class DonationService : IDonationService
{
    private readonly AppDbContext _context;
    private readonly OpenCageService _openCageService;

    public DonationService(AppDbContext context, OpenCageService openCageService)
    {
        _context = context;
        _openCageService = openCageService;
    }

    public async Task<Donation> CreateDonationAsync(DonationCreateDto donationDto, int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new Exception("Usuário não encontrado");

        // Se não veio latitude/longitude, tenta geocodificar o endereço do usuário
        if (donationDto.PickupLatitude == null || donationDto.PickupLongitude == null)
        {
            var fullAddress = $"{user.Street}, {user.Number}, {user.Neighborhood}, {user.City}, {user.State}, {user.CEP}, Brasil";
            var (latitude, longitude) = await _openCageService.GetCoordinatesAsync(fullAddress);

            donationDto.PickupLatitude = latitude;
            donationDto.PickupLongitude = longitude;
        }

        var donation = new Donation
        {
            Title = donationDto.Title,
            Category = donationDto.Category,
            Description = donationDto.Description,
            Quantity = donationDto.Quantity,
            Unit = donationDto.Unit,
            ExpirationDate = donationDto.ExpirationDate,
            PickupLatitude = donationDto.PickupLatitude,
            PickupLongitude = donationDto.PickupLongitude,
            UserId = userId,
            CO2Impact = CalculateCO2Impact(donationDto.Category, donationDto.Quantity),
            WaterImpact = CalculateWaterImpact(donationDto.Category, donationDto.Quantity)
        };

        _context.Donations.Add(donation);
        await _context.SaveChangesAsync();

        return donation;
    }

    public async Task<IEnumerable<Donation>> GetUserDonationsAsync(int userId)
    {
        var expired = await _context.Donations
            .Where(d => d.UserId == userId && d.ExpirationDate < DateTime.UtcNow)
            .ToListAsync();

        if (expired.Any())
        {
            _context.Donations.RemoveRange(expired);
            await _context.SaveChangesAsync();
        }

        return await _context.Donations
            .Include(d => d.User)
            .Include(d => d.ReservedByUser)
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Donation>> GetAvailableDonationsAsync(string sort)
    {
        var expiredDonations = await _context.Donations
            .Where(d => d.ExpirationDate < DateTime.UtcNow)
            .ToListAsync();

        if (expiredDonations.Any())
        {
            _context.Donations.RemoveRange(expiredDonations);
            await _context.SaveChangesAsync();
        }

        var query = _context.Donations
            .Include(d => d.User)
            .Include(d => d.ReservedByUser)
            .Where(d => d.ExpirationDate >= DateTime.UtcNow);

        query = sort switch
        {
            "distance" => query.OrderBy(d => d.PickupLatitude),
            "expiration" => query.OrderBy(d => d.ExpirationDate),
            _ => query.OrderByDescending(d => d.CreatedAt)
        };

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Donation>> GetDonationsByUserIdAsync(int userId, string sort = "date")
    {
        var query = _context.Donations
            .Include(d => d.User)
            .Include(d => d.ReservedByUser)
            .Where(d => d.UserId == userId);

        query = sort switch
        {
            "date" => query.OrderByDescending(d => d.CreatedAt),
            "expiration" => query.OrderBy(d => d.ExpirationDate),
            _ => query.OrderByDescending(d => d.CreatedAt)
        };

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Donation>> GetAllDonationsAsync(string sort = "date")
    {
        var expired = await _context.Donations
            .Where(d => d.ExpirationDate < DateTime.UtcNow)
            .ToListAsync();

        if (expired.Any())
        {
            _context.Donations.RemoveRange(expired);
            await _context.SaveChangesAsync();
        }

        var query = _context.Donations
            .Include(d => d.User)
            .Include(d => d.ReservedByUser)
            .AsQueryable();

        query = sort switch
        {
            "date" => query.OrderByDescending(d => d.CreatedAt),
            "expiration" => query.OrderBy(d => d.ExpirationDate),
            "distance" => query.OrderBy(d => d.PickupLatitude),
            _ => query.OrderByDescending(d => d.CreatedAt)
        };

        return await query.ToListAsync();
    }

    public async Task CancelarReservasExpiradasAsync()
    {
        var expiradas = await _context.Donations
            .Where(d => d.IsReserved && d.ReservedAt != null)
            .Where(d => DateTime.UtcNow > d.ReservedAt.Value.AddSeconds(24))
            .ToListAsync();

        foreach (var doacao in expiradas)
        {
            doacao.IsReserved = false;
            doacao.ReservedAt = null;
            doacao.ReservedByUserId = null;
        }

        if (expiradas.Any())
            await _context.SaveChangesAsync();
    }

    public async Task<Donation> ReserveDonationAsync(int donationId, int userId)
    {
        var donation = await _context.Donations
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == donationId);

        if (donation == null)
            throw new ArgumentException("Doação não encontrada");

        if (donation.IsReserved)
            throw new InvalidOperationException("Esta doação já foi reservada");

        if (donation.ExpirationDate < DateTime.UtcNow)
            throw new InvalidOperationException("Esta doação está expirada");

        if (donation.UserId == userId)
            throw new InvalidOperationException("Você não pode reservar sua própria doação");

        donation.IsReserved = true;
        donation.ReservedByUserId = userId;
        donation.ReservedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return donation;
    }

    private decimal? CalculateCO2Impact(string category, decimal quantity)
    {
        var impactFactors = new Dictionary<string, decimal>
        {
            {"prepared", 2.5m},
            {"produce", 0.8m},
            {"bakery", 1.2m},
            {"canned", 1.5m},
            {"dairy", 3.0m},
            {"meat", 5.0m},
            {"grain", 0.5m},
            {"other", 1.0m}
        };

        return impactFactors.TryGetValue(category, out var factor) ? quantity * factor : null;
    }

    private decimal? CalculateWaterImpact(string category, decimal quantity)
    {
        var impactFactors = new Dictionary<string, decimal>
        {
            {"prepared", 500m},
            {"produce", 300m},
            {"bakery", 400m},
            {"canned", 200m},
            {"dairy", 1000m},
            {"meat", 1500m},
            {"grain", 250m},
            {"other", 350m}
        };

        return impactFactors.TryGetValue(category, out var factor) ? quantity * factor : null;
    }
}
