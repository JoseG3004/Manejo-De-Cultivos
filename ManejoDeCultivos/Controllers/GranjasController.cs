using BaseDatos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modelos;
using Modelos.DTOs;
using Servicios.Interfaces;
using Servicios.Servicios;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace ManejoDeCultivos.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class GranjasController : ControllerBase
    {
        private readonly IGranja _granja;

        public GranjasController(IGranja granja)
        {
            this._granja = granja;
        }

        // --- MÉTODOS GET ---

        [HttpGet("GetAllGranjas")]
        [Authorize(Roles = "Admin,Usuario")]
        [SwaggerOperation(
            Summary = "Devuelve todas las granjas",
            Description = "Busca en la base de datos y retorna la información de todas las granjas."
        )]
        public IActionResult GetAllGranjas()
        {
            return Ok(this._granja.getAll());
        }

        [HttpGet("GetGranjaPorID")]
        [Authorize(Roles = "Admin,Usuario")]
        [SwaggerOperation(
            Summary = "Obtiene una granja por su ID",
            Description = "Ingrese el ID de una granja"
        )]
        public IActionResult GetID(int id)
        {
            var granjaDto = this._granja.GetById(id);

            if (granjaDto == null)
            {
                return NotFound($"No se encontró ninguna granja con el ID {id}.");
            }

            return Ok(granjaDto);
        }

        [HttpGet("GetGranjaPorUbicacion")]
        [Authorize(Roles = "Admin,Usuario")]
        [SwaggerOperation(
            Summary = "Devuelve todas las granjas en un municipio",
            Description = "Ingrese un Municipio del Oriente"
        )]
        public IActionResult GetUbicacion(string Ubicacion)
        {
            if (string.IsNullOrWhiteSpace(Ubicacion)) return BadRequest("La ubicación no puede estar vacía.");

            string ubicacionBuscada = ServicioTexto.NormalizarTexto(Ubicacion);

            var granjas = this._granja.getAll()
                .Where(g => g.Ubicacion != null && ServicioTexto.NormalizarTexto(g.Ubicacion) == ubicacionBuscada)
                .ToList();

            return Ok(granjas);
        }

        [HttpGet("GetGranjaPorCultivo")]
        [Authorize(Roles = "Admin,Usuario")]
        [SwaggerOperation(
            Summary = "Devuelve todas las granjas que cultivan su query",
            Description = "Ingrese un cultivo"
        )]
        public IActionResult GetCultivo(string cultivo)
        {
            if (string.IsNullOrWhiteSpace(cultivo)) return BadRequest("El cultivo no puede estar vacío.");

            string cultivoBuscado = ServicioTexto.NormalizarTexto(cultivo);

            var granjas = this._granja.getAll()
                .Where(g => g.Cultivo != null && ServicioTexto.NormalizarTexto(g.Cultivo) == cultivoBuscado)
                .ToList();

            return Ok(granjas);
        }

        [HttpGet("GetGranjaPorCanalDeVenta")]
        [Authorize(Roles = "Admin,Usuario")]
        [SwaggerOperation(
            Summary = "Devuelve todas las granjas por canal de venta",
            Description = "Ingrese Nacional / Internacional"
        )]
        public IActionResult GetCanal(string canal)
        {
            if (string.IsNullOrWhiteSpace(canal)) return BadRequest("El canal de venta no puede estar vacío.");

            string canalBuscado = ServicioTexto.NormalizarTexto(canal);

            var granjas = this._granja.getAll()
                .Where(g => g.CanalVenta != null && ServicioTexto.NormalizarTexto(g.CanalVenta) == canalBuscado)
                .ToList();

            return Ok(granjas);
        }

        [HttpGet("GetGranjaPorTipoDeCultivo")]
        [Authorize(Roles = "Admin,Usuario")]
        [SwaggerOperation(
            Summary = "Devuelve todas por tipo de cultivo",
            Description = "Ingrese: raíces, tubérculo, cereal, leguminosa, cultivo permanente, cultivo industrial"
        )]
        public IActionResult GetTipo(string tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo)) return BadRequest("El tipo de cultivo no puede estar vacío.");

            string tipoBuscado = ServicioTexto.NormalizarTexto(tipo);

            var granjas = this._granja.getAll()
                .Where(g => g.TipoCultivo != null &&
                            g.TipoCultivo.Any(t => ServicioTexto.NormalizarTexto(t) == tipoBuscado))
                .ToList();

            return Ok(granjas);
        }

        [HttpGet("GetGranjaPorMenorACiclos")]
        [Authorize(Roles = "Admin,Usuario")]
        [SwaggerOperation(
            Summary = "Devuelve las granjas con ciclos (en meses) menores o iguales a",
            Description = "Ingrese un número"
        )]
        public IActionResult GetCiclosMenor(int ciclos)
        {
            var granjas = this._granja.getAll().Where(g => g.Ciclos <= ciclos).ToList();
            return Ok(granjas);
        }

        [HttpGet("GetGranjaPorMayorACiclos")]
        [Authorize(Roles = "Admin,Usuario")]
        [SwaggerOperation(
            Summary = "Devuelve las granjas con ciclos (en meses) mayores o iguales a",
            Description = "Ingrese un número"
        )]
        public IActionResult GetCiclosMayor(int ciclos)
        {
            var granjas = this._granja.getAll().Where(g => g.Ciclos >= ciclos).ToList();
            return Ok(granjas);
        }

        [HttpGet("GetGranjaPorProceso")]
        [Authorize(Roles = "Admin,Usuario")]
        [SwaggerOperation(
            Summary = "Devuelve las granjas por Procesos",
            Description = "Ingrese: fructificación, maduración, floración"
        )]
        public IActionResult GetProceso(string tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo)) return BadRequest("El proceso no puede estar vacío.");

            string procesoBuscado = ServicioTexto.NormalizarTexto(tipo);

            var granjas = this._granja.getAll()
                .Where(g => g.Procesos != null &&
                            g.Procesos.Any(p => ServicioTexto.NormalizarTexto(p) == procesoBuscado))
                .ToList();

            return Ok(granjas);
        }

        [HttpGet("GetBuscarPaginado")]
        [Authorize(Roles = "Admin,Usuario")]
        [SwaggerOperation(
            Summary = "Consulta paginada con filtros opcionales combinados",
            Description = "Permite filtrar simultáneamente por municipio, clima y canal de venta, aplicando paginación."
        )]
        public IActionResult GetBuscarPaginado(
            [FromQuery] string? municipio,
            [FromQuery] string? clima,
            [FromQuery] string? canal,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 10)
        {
            if (pagina < 1) pagina = 1;
            if (tamanoPagina < 1 || tamanoPagina > 50) tamanoPagina = 10;

            var consulta = _granja.getAll().AsQueryable();

            // Filtrado dinámico opcional
            if (!string.IsNullOrWhiteSpace(municipio))
            {
                string uNorm = ServicioTexto.NormalizarTexto(municipio);
                consulta = consulta.Where(g => g.Ubicacion != null && ServicioTexto.NormalizarTexto(g.Ubicacion) == uNorm);
            }

            if (!string.IsNullOrWhiteSpace(clima))
            {
                string cNorm = ServicioTexto.NormalizarTexto(clima);
                consulta = consulta.Where(g => g.Clima != null && ServicioTexto.NormalizarTexto(g.Clima) == cNorm);
            }

            if (!string.IsNullOrWhiteSpace(canal))
            {
                string canalNorm = ServicioTexto.NormalizarTexto(canal);
                consulta = consulta.Where(g => g.CanalVenta != null && ServicioTexto.NormalizarTexto(g.CanalVenta) == canalNorm);
            }

            int totalRegistros = consulta.Count();
            int totalPaginas = (int)System.Math.Ceiling((double)totalRegistros / tamanoPagina);

            var resultado = consulta
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToList();

            return Ok(new
            {
                TotalRegistros = totalRegistros,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                TamanoPagina = tamanoPagina,
                Datos = resultado
            });
        }

        // --- MÉTODOS POST ---

        [HttpPost("AnadirGranja")]
        [Authorize(Roles = "Admin,Usuario")]
        [SwaggerOperation(
            Summary = "Añade una granja a la base de datos",
            Description = "Ingrese la información que se le pide"
        )]
        public IActionResult AddGranja([FromBody] GranjaDTO.GranjaCreateDTO granjaDto)
        {
            if (granjaDto == null)
            {
                return BadRequest("Los datos enviados son inválidos.");
            }

            var nuevaGranja = _granja.Create(granjaDto);

            if (nuevaGranja == null)
            {
                return BadRequest("No se pudo crear la granja. Verifique que los valores de Procesos, CanalVenta, TipoCultivo y Clima sean válidos.");
            }

            return CreatedAtAction(nameof(GetID), new { id = nuevaGranja.ID }, nuevaGranja);
        }

        // --- METODOS PUT ---

        [HttpPut("ActualizarGranja/{id}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "Actualiza una granja existente por su ID",
            Description = "Ingrese el ID de la granja y la nueva información"
        )]
        public IActionResult UpdateGranja(int id, [FromBody] GranjaDTO.GranjaCreateDTO granjaDto)
        {
            if (granjaDto == null) return BadRequest("Los datos enviados son inválidos.");

            bool actualizado = _granja.Update(id, granjaDto);
            if (!actualizado) return NotFound($"No se encontró ninguna granja con el ID {id}.");

            return NoContent(); // 204 No Content
        }

        // --- METODOS DELETE ---

        [HttpDelete("EliminarGranja/{id}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "Elimina una granja por su ID",
            Description = "Ingrese el ID de la granja que desea eliminar"
        )]
        public IActionResult DeleteGranja(int id)
        {
            bool eliminado = _granja.Delete(id);
            if (!eliminado) return NotFound($"No se encontró ninguna granja con el ID {id}.");

            return NoContent(); // 204 No Content
        }
    }
}