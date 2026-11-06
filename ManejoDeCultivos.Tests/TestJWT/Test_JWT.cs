using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Servicios.Servicios;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ManejoDeCultivos.Tests
{
    [TestClass]
    public class Test_JWT
    {
        private IConfiguration CrearConfiguracion(Dictionary<string, string?> inMemorySettings)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        #region Generación Estructura Básica
        [TestMethod]
        public void GenerateToken_TokenConTresPartes_FormatoEncabezadoCargaFirma()
        {
            var config = CrearConfiguracion(new Dictionary<string, string?>());
            var jwtService = new JwtService(config);

            string token = jwtService.GenerateToken("usuarioTest", "Admin");

            Assert.IsFalse(string.IsNullOrWhiteSpace(token));
            string[] partes = token.Split('.');
            Assert.AreEqual(3, partes.Length, "Un JWT estructurado debe tener exactamente 3 partes separadas por puntos (header.payload.signature).");
        }
        #endregion

        #region Claims: Usuario y Rol
        [TestMethod]
        public void GenerateToken_UsuarioRolYFirma_TokenValidoConNameAdminYRolAdmin()
        {
            var config = CrearConfiguracion(new Dictionary<string, string?>
            {
                { "Jwt:Key", "ClaveSuperSecretaDePruebaParaGenerarTokens123!" }
            });
            var jwtService = new JwtService(config);

            string token = jwtService.GenerateToken("admin", "Admin");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var claimNombre = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "unique_name");
            var claimRol = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role");

            Assert.IsNotNull(claimNombre, "Debe incluir el claim del nombre.");
            Assert.AreEqual("admin", claimNombre.Value);

            Assert.IsNotNull(claimRol, "Debe incluir el claim del rol.");
            Assert.AreEqual("Admin", claimRol.Value);
        }
        #endregion

        #region Configuración: Emisor, Audiencia y Duración
        [TestMethod]
        public void GenerateToken_EmisorAudienciaYDuracion_CoincidenConConfiguracion()
        {
            string issuerEsperado = "MiEmisorPersonalizado";
            string audienceEsperada = "MiAudienciaPersonalizada";
            int duracionMinutos = 45;

            var config = CrearConfiguracion(new Dictionary<string, string?>
            {
                { "Jwt:Issuer", issuerEsperado },
                { "Jwt:Audience", audienceEsperada },
                { "Jwt:DurationInMinutes", duracionMinutos.ToString() },
                { "Jwt:Key", "ClaveSuperSecretaDePruebaParaGenerarTokens123!" }
            });

            var jwtService = new JwtService(config);
            DateTime fechaGeneracionUtc = DateTime.UtcNow;

            string token = jwtService.GenerateToken("usuario", "Cliente");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            Assert.AreEqual(issuerEsperado, jwtToken.Issuer);
            Assert.IsTrue(jwtToken.Audiences.Contains(audienceEsperada));

            // Verificamos que la expiración corresponda aproximadamente a 45 minutos (tolerancia de +- 2 segundos)
            DateTime fechaExpiracionEsperada = fechaGeneracionUtc.AddMinutes(duracionMinutos);
            TimeSpan diferencia = jwtToken.ValidTo - fechaExpiracionEsperada;
            Assert.IsTrue(Math.Abs(diferencia.TotalSeconds) < 2, $"La expiración del token no coincide con los {duracionMinutos} minutos configurados.");
        }
        #endregion

        #region Jti Único por Token
        [TestMethod]
        public void GenerateToken_DosTokensSeguidos_JtiDistintoEnCadaUno()
        {
            var config = CrearConfiguracion(new Dictionary<string, string?>());
            var jwtService = new JwtService(config);

            string token1 = jwtService.GenerateToken("usuario", "Admin");
            string token2 = jwtService.GenerateToken("usuario", "Admin");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken1 = handler.ReadJwtToken(token1);
            var jwtToken2 = handler.ReadJwtToken(token2);

            var jti1 = jwtToken1.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var jti2 = jwtToken2.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

            Assert.IsNotNull(jti1);
            Assert.IsNotNull(jti2);
            Assert.AreNotEqual(jti1, jti2, "Cada token generado debe tener un identificador Jti único.");
        }
        #endregion

        #region Validación con Clave Distinta
        [TestMethod]
        public void GenerateToken_ValidarConOtraClave_LanzaSecurityTokenException()
        {
            string claveOriginal = "ClaveDeFirmaOriginalSuperSegura123456!";
            string claveDiferente = "ClaveTotalmenteDiferenteYFalsa9876543!";

            var config = CrearConfiguracion(new Dictionary<string, string?>
            {
                { "Jwt:Key", claveOriginal },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            });

            var jwtService = new JwtService(config);
            string token = jwtService.GenerateToken("usuarioTest", "Admin");

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveDiferente)),
                ValidateIssuer = true,
                ValidIssuer = "TestIssuer",
                ValidateAudience = true,
                ValidAudience = "TestAudience",
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out _);
                Assert.Fail("Se esperaba una excepción SecurityTokenException al usar una clave incorrecta, pero no se lanzó ninguna.");
            }
            catch (SecurityTokenException ex)
            {
                // La prueba es exitosa si entra a este bloque de excepción
                Assert.IsNotNull(ex);
            }
        }
        #endregion

        #region Fallback a Valores por Defecto
        [TestMethod]
        public void GenerateToken_SinConfiguracion_UsaLosValoresPorDefecto()
        {
            // Configuración completamente vacía
            var configVacia = CrearConfiguracion(new Dictionary<string, string?>());
            var jwtService = new JwtService(configVacia);

            DateTime fechaGeneracionUtc = DateTime.UtcNow;
            string token = jwtService.GenerateToken("usuarioDefault", "User");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            Assert.AreEqual("ManejoDeCultivosAPI", jwtToken.Issuer);
            Assert.IsTrue(jwtToken.Audiences.Contains("ManejoDeCultivosUsuarios"));

            // El valor por defecto de duración es 60 minutos
            DateTime fechaExpiracionEsperada = fechaGeneracionUtc.AddMinutes(60);
            TimeSpan diferencia = jwtToken.ValidTo - fechaExpiracionEsperada;
            Assert.IsTrue(Math.Abs(diferencia.TotalSeconds) < 2, "La duración por defecto debe ser de 60 minutos.");
        }
        #endregion
    }
}