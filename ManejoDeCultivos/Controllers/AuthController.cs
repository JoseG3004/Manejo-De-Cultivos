using Microsoft.AspNetCore.Mvc;
using Modelos.DTOs;
using Servicios.Servicios;
using Swashbuckle.AspNetCore.Annotations;
using System;

namespace ManejoDeCultivos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;

        public AuthController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Inicia sesión y genera un Token con Perfil",
            Description = "Devuelve un token JWT con los permisos correspondientes según el perfil (Admin o Usuario).\nAdmin (admin & admin12) Usuario (user & user123)"
        )]
        public IActionResult Login([FromBody] LoginDTO login)
        {
            if (login == null || string.IsNullOrWhiteSpace(login.Usuario) || string.IsNullOrWhiteSpace(login.Password))
            {
                return BadRequest("Debe ingresar usuario y contraseña.");
            }

            string rolAsignado = "";

            // Validación simulada de perfiles
            if (login.Usuario.ToLower() == "admin" && login.Password == "admin123")
            {
                rolAsignado = "Admin";
            }
            else if (login.Usuario.ToLower() == "user" && login.Password == "user123")
            {
                rolAsignado = "Usuario";
            }
            else
            {
                return Unauthorized("Credenciales inválidas.");
            }

            // Generar token pasando el perfil
            string token = _jwtService.GenerateToken(login.Usuario, rolAsignado);

            return Ok(new AuthDTO.TokenResponseDTO
            {
                Token = token,
                Rol = rolAsignado,
                Expiracion = DateTime.UtcNow.AddMinutes(60).ToString("yyyy-MM-dd HH:mm:ss 'UTC'")
            });
        }
    }
}