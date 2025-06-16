using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Testes.Data;
using Testes.Models;
using Testes.Services;
using AuthService = Testes.Services.AuthService;

namespace Testes.Controllers;

  [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;
        private readonly AppDbContext _context;


        public AuthController(AuthService authService, IConfiguration config, EmailService emailService , AppDbContext context)
        {
            _context = context;
            _authService = authService;
            _config = config;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto userDto)
        {
            var user = new User
            {
                Type = userDto.Type,
                Name = userDto.Name,
                DocumentNumber = userDto.DocumentNumber,
                NgoType = userDto.Type == "ngo" ? userDto.NgoType : null,
                Email = userDto.Email,
                Phone = userDto.Phone,
                //Address = userDto.Address
                
                CEP = userDto.CEP,
                Street = userDto.Street,
                Neighborhood = userDto.Neighborhood,
                City = userDto.City,
                State = userDto.State,
                Number = userDto.Number
            };

            var result = await _authService.Register(user, userDto.Password);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userDto)
        {
            var result = await _authService.Login(userDto.Email, userDto.Password);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            // Obter o ID do usuário a partir da token JWT
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    
            var profile = await _authService.GetUserProfile(userId);
            if (profile == null)
            {
                return NotFound();
            }
            return Ok(profile);
            
        }
        
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto updateDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _authService.UpdateUserProfileAsync(userId, updateDto);
    
            if (user == null)
                return NotFound();

            return Ok(user);
        }
        
        
        //Email 
    
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await _authService.FindUserByEmailAsync(request.Email);
            if (user == null)
                return BadRequest("Usuário não encontrado.");

            var token = await _authService.GeneratePasswordResetToken(user);
            var resetLink = $"{_config["AppSettings:ClientUrl"]}/reset-password.html?token={token}";

            await _emailService.SendEmailAsync(
                user.Email,
                "Recuperação de Senha - Feed the Future BR",
                $"<p>Você solicitou uma recuperação de senha.</p><p><a href='{resetLink}'>Clique aqui para redefinir sua senha</a></p>"
            );

            return Ok("Link de recuperação enviado ao e-mail.");
        }
        
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = await _authService.FindUserByResetTokenAsync(request.Token);
            if (user == null)
                return BadRequest("Token inválido ou expirado.");

            await _authService.UpdatePasswordAsync(user, request.NewPassword);

            return Ok(new { message = "Senha redefinida com sucesso!" });
        }
        
        
        [HttpGet("user-stats")]
        [Authorize] // Ajuste as roles se quiser
        
        public async Task<IActionResult> GetUserStatistics()
        {
            var totalNGOs = await _context.Users.CountAsync(u => u.Type.ToLower() == "ngo");
            var totalBusinesses = await _context.Users.CountAsync(u => u.Type.ToLower() == "business");

            return Ok(new
            {
                ngos = totalNGOs,
                businesses = totalBusinesses
            });
        }

// Endpoint para retornar o total de doações cadastradas
        [HttpGet("donation-stats")]
        [Authorize] // Ajuste as roles se quiser
        
        public async Task<IActionResult> GetDonationStatistics()
        {
            var totalDonations = await _context.Donations.CountAsync();

            return Ok(new
            {
                totalDonations = totalDonations
            });
        }

        
    }


        //Dentro do controller
    public class UserRegistrationDto
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string? NgoType { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        //public string Address { get; set; } = string.Empty;
        
        public string CEP { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Neighborhood { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UserLoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    
    //Criado agr
    public class UserProfileDto
    {
        public int Id { get; set; }  
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string? NgoType { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CEP { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Neighborhood { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }


    //Email
    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }


    
    