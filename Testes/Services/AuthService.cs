using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Testes.Controllers;
using Testes.Data;
using Testes.Models;

namespace Testes.Services;

public class AuthService
{
     private readonly AppDbContext _context;
        private readonly JwtSetting _jwtSettings;
        

        public AuthService(AppDbContext context, IOptions<JwtSetting> jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
            
        }

        public async Task<AuthResponse> Register(User user, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return new AuthResponse { Success = false, Message = "Email já está em uso." };
            }

            if (await _context.Users.AnyAsync(u => u.DocumentNumber == user.DocumentNumber))
            {
                return new AuthResponse { Success = false, Message = "CPF/CNPJ já está cadastrado." };
            }

          
            
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Success = true,
                Token = token,
                User = user,
                Message = "Cadastro realizado com sucesso!"
            };
        }

        public async Task<AuthResponse> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return new AuthResponse { Success = false, Message = "Email não encontrado." };
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return new AuthResponse { Success = false, Message = "Senha incorreta." };
            }

            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Success = true,
                Token = token,
                User = user,
                Message = "Login realizado com sucesso!"
            };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Type)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
        
        //Criado agr
        public async Task<UserProfileDto> GetUserProfile(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
        
            if (user == null)
            {
                return null;
            }

            return new UserProfileDto
            {
                Id = user.Id,   
                Type = user.Type,
                Name = user.Name,
                DocumentNumber = user.DocumentNumber,
                NgoType = user.NgoType,
                Email = user.Email,
                Phone = user.Phone,
                CEP = user.CEP,
                Street = user.Street,
                Neighborhood = user.Neighborhood,
                City = user.City,
                State = user.State,
                Number = user.Number,
                CreatedAt = user.CreatedAt
            };
        }
        
        public async Task<User?> UpdateUserProfileAsync(int userId, UserProfileDto update)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            user.Name = update.Name;
            user.DocumentNumber = update.DocumentNumber;
            user.Type = update.Type;
            user.NgoType = update.NgoType;
            user.Email = update.Email;
            user.Phone = update.Phone;
            user.CEP = update.CEP;
            user.Street = update.Street;
            user.Neighborhood = update.Neighborhood;
            user.City = update.City;
            user.State = update.State;
            user.Number = update.Number;

            await _context.SaveChangesAsync();
            return user;
        }
        
        
        
        //Email 
        
        public async Task<string> GeneratePasswordResetToken(User user)
        {
            var token = Guid.NewGuid().ToString();
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return token;
        }
        
        public async Task<User?> FindUserByResetTokenAsync(string token)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == token && u.PasswordResetTokenExpiry > DateTime.UtcNow);
        }
        
        public async Task UpdatePasswordAsync(User user, string newPassword)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        
        public async Task<User?> FindUserByEmailAsync(string email)
        {
            return await _context.Users
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();
        }




}