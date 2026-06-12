using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models.DTOs;
using Backend.Services;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthController(AppDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        // Email ile kullanıcıyı bul
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return Unauthorized("Email veya şifre hatalı.");
        }

        // Şifreyi doğrula (girilen şifre ile kayıttaki hash'i karşılaştır)
        bool passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!passwordValid)
        {
            return Unauthorized("Email veya şifre hatalı.");
        }

        // Token üret ve dön
        var token = _tokenService.CreateToken(user);

        return new LoginResponse
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Role = user.Role
        };
    }
}