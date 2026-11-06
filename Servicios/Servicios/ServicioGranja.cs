using BaseDatos;
using Modelos;
using Servicios.Interfaces;
using static Modelos.DTOs.GranjaDTO;
using Modelos.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace Servicios.Servicios
{
    public class ServicioGranja : IGranja
    {
        private GranjaResponseDTO MapearADTO(Granja granja)
        {
            return new GranjaResponseDTO
            {
                ID = granja.ID,
                Nombre = granja.Nombre,
                Cultivo = granja.Cultivo,
                Ubicacion = granja.Ubicacion,
                Ciclos = granja.Ciclos,
                Procesos = new List<string>(granja.Procesos ?? new List<string>()),
                CanalVenta = granja.CanalVenta,
                TipoCultivo = new List<string>(granja.TipoCultivo ?? new List<string>()),
                Clima = granja.Clima,
                Contacto = new ContactoDTO
                {
                    Nombre = granja.Contacto?.Nombre ?? "",
                    Telefono = granja.Contacto?.Telefono ?? ""
                },
                TamanoPlantacion = granja.TamanoPlantacion
            };
        }
        private bool ValidarDTO(GranjaCreateDTO dto) => !ObtenerErroresValidacion(dto).Any();
        public List<string> ObtenerErroresValidacion(GranjaCreateDTO dto)
        {
            var errores = new List<string>();
            if (dto == null) { errores.Add("La información de la granja es obligatoria."); return errores; }
            if (string.IsNullOrWhiteSpace(dto.Nombre)) errores.Add("El nombre de la granja es obligatorio.");
            if (dto.Ciclos <= 0) errores.Add("Los ciclos deben ser mayores a 0.");
            return errores;
        }

        public List<GranjaResponseDTO> getAll()
        {
            return Datos.BaseGranjas.Select(g => MapearADTO(g)).ToList();
        }

        public GranjaResponseDTO? GetById(int id)
        {
            var granja = Datos.BaseGranjas.FirstOrDefault(g => g.ID == id);
            return granja == null ? null : MapearADTO(granja);
        }

        public GranjaResponseDTO? Create(GranjaCreateDTO dto)
        {
            if (!ValidarDTO(dto)) return null;

            int nuevoID = Datos.BaseGranjas.Any() ? Datos.BaseGranjas.Max(g => g.ID) + 1 : 1;

            Granja nuevaGranja = new Granja()
            {
                ID = nuevoID,
                Nombre = dto.Nombre?.Trim() ?? "",
                Cultivo = dto.Cultivo?.Trim() ?? "",
                Ubicacion = dto.Ubicacion?.Trim() ?? "",
                Ciclos = dto.Ciclos,
                Procesos = dto.Procesos.Select(p => p.Trim()).ToList(),
                CanalVenta = dto.CanalVenta?.Trim() ?? "",
                TipoCultivo = dto.TipoCultivo.Select(t => t.Trim()).ToList(),
                Clima = dto.Clima?.Trim() ?? "",
                Contacto = new Contacto
                {
                    Nombre = dto.Contacto.Nombre.Trim(),
                    Telefono = dto.Contacto.Telefono.Trim()
                },
                TamanoPlantacion = dto.TamanoPlantacion
            };

            Datos.BaseGranjas.Add(nuevaGranja);
            return MapearADTO(nuevaGranja);
        }

        public bool Update(int id, GranjaCreateDTO dto)
        {
            var granja = Datos.BaseGranjas.FirstOrDefault(g => g.ID == id);
            if (granja == null || !ValidarDTO(dto)) return false;

            granja.Nombre = dto.Nombre?.Trim() ?? "";
            granja.Cultivo = dto.Cultivo?.Trim() ?? "";
            granja.Ubicacion = dto.Ubicacion?.Trim() ?? "";
            granja.Ciclos = dto.Ciclos;
            granja.Procesos = dto.Procesos.Select(p => p.Trim()).ToList();
            granja.CanalVenta = dto.CanalVenta?.Trim() ?? "";
            granja.TipoCultivo = dto.TipoCultivo.Select(t => t.Trim()).ToList();
            granja.Clima = dto.Clima?.Trim() ?? "";
            granja.Contacto = new Contacto
            {
                Nombre = dto.Contacto.Nombre.Trim(),
                Telefono = dto.Contacto.Telefono.Trim()
            };
            granja.TamanoPlantacion = dto.TamanoPlantacion;

            return true;
        }

        public bool Delete(int id)
        {
            var granja = Datos.BaseGranjas.FirstOrDefault(g => g.ID == id);
            if (granja == null) return false;
            return Datos.BaseGranjas.Remove(granja);
        }
    }
}