using System;
using System.Collections.Generic;
using System.Text;

namespace Modelos.DTOs
{
    public class AuthDTO
    {
        // DTO de respuesta con el Token firmado y su información de expiración
        public class TokenResponseDTO
        {
            public string Token { get; set; } = string.Empty;
            public string Rol { get; set; } = string.Empty;
            public string Expiracion { get; set; } = string.Empty;
        }
    }
}
