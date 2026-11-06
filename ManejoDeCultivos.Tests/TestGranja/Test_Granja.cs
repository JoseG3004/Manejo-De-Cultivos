using Microsoft.VisualStudio.TestTools.UnitTesting;
using Servicios.Servicios;
using BaseDatos;
using Modelos;
using static Modelos.DTOs.GranjaDTO;
using System.Collections.Generic;
using System.Linq;

namespace ManejoDeCultivos.Tests
{
    [TestClass]
    public class Test_Granja
    {
        private ServicioGranja _servicio = null!;

        [TestInitialize]
        public void Setup()
        {
            _servicio = new ServicioGranja();

            // Se reinicia la base de datos en memoria para aislar cada prueba
            Datos.BaseGranjas = new List<Granja>();

            for (int i = 1; i <= 100; i++)
            {
                Datos.BaseGranjas.Add(CrearGranjaEntidad(i, $"Granja {i}"));
            }
        }

        #region Métodos Auxiliares
        private Granja CrearGranjaEntidad(int id, string nombre)
        {
            return new Granja
            {
                ID = id,
                Nombre = nombre,
                Cultivo = "Aguacate",
                Ubicacion = "Rionegro",
                Ciclos = 3,
                Procesos = new List<string> { "Siembra", "Cosecha" },
                CanalVenta = "Directo",
                TipoCultivo = new List<string> { "Orgánico" },
                Clima = "Fresco",
                Contacto = new Contacto { Nombre = "Juan Perez", Telefono = "123456789" },
                TamanoPlantacion = 10.5
            };
        }

        private GranjaCreateDTO CrearDTOValido(string nombre = "Granja Central")
        {
            return new GranjaCreateDTO
            {
                Nombre = nombre,
                Cultivo = "Hortalizas",
                Ubicacion = "Oriente Antioqueño",
                Ciclos = 5,
                Procesos = new List<string> { "Riego", "Abono" },
                CanalVenta = "Mayorista",
                TipoCultivo = new List<string> { "Tradicional" },
                Clima = "Templado",
                Contacto = new ContactoDTO { Nombre = "Carlos Gomez", Telefono = "987654321" },
                TamanoPlantacion = 25.0
            };
        }
        #endregion

        #region GetAll & getAll
        [TestMethod]
        public void GetAll_DevuelveLos100Registros()
        {
            var resultado = _servicio.getAll();

            Assert.IsNotNull(resultado);
            Assert.AreEqual(100, resultado.Count);
        }

        [TestMethod]
        public void getAll_Obsoleto_MismoResultadoQueGetAll()
        {
            var resultado = _servicio.getAll();

            Assert.IsNotNull(resultado);
            Assert.AreEqual(100, resultado.Count);
        }
        #endregion

        #region GetById
        [TestMethod]
        public void GetById_Existente_DevuelveGranjaMapeada()
        {
            var resultado = _servicio.GetById(1);

            Assert.IsNotNull(resultado);
            Assert.AreEqual(1, resultado.ID);
            Assert.AreEqual("Granja 1", resultado.Nombre);
        }

        [TestMethod]
        public void GetById_Inexistente_DevuelveNull()
        {
            var resultado = _servicio.GetById(999);

            Assert.IsNull(resultado);
        }
        #endregion

        #region MapearADTO
        [TestMethod]
        public void MapearADTO_ModificarRespuesta_LaEntidadGuardadaNoCambia()
        {
            var dto = _servicio.GetById(1);
            Assert.IsNotNull(dto);

            // Se modifica la respuesta DTO obtenida
            dto.Nombre = "Nombre Modificado En DTO";
            dto.Procesos.Add("Proceso Extra DTO");
            dto.Contacto.Nombre = "Contacto Modificado DTO";

            // La entidad original en la base de datos debe permanecer intacta
            var entidadBD = Datos.BaseGranjas.First(g => g.ID == 1);
            Assert.AreEqual("Granja 1", entidadBD.Nombre);
            Assert.IsFalse(entidadBD.Procesos.Contains("Proceso Extra DTO"));
            Assert.AreEqual("Juan Perez", entidadBD.Contacto.Nombre);
        }
        #endregion

        #region Create
        [TestMethod]
        public void Create_ConDTOValido_NuevoIDMaximoMasUnoYUnRegistroMas()
        {
            int cantidadInicial = Datos.BaseGranjas.Count;
            int maxIdInicial = Datos.BaseGranjas.Max(g => g.ID);
            var nuevoDTO = CrearDTOValido("Nueva Granja");

            var resultado = _servicio.Create(nuevoDTO);

            Assert.IsNotNull(resultado);
            Assert.AreEqual(maxIdInicial + 1, resultado.ID);
            Assert.AreEqual(cantidadInicial + 1, Datos.BaseGranjas.Count);
        }

        [TestMethod]
        public void Create_RecortaEspacios()
        {
            var dtoConEspacios = CrearDTOValido("  Granja Con Espacios  ");
            dtoConEspacios.Cultivo = "  Maíz  ";
            dtoConEspacios.Ubicacion = "  Marinilla  ";
            dtoConEspacios.CanalVenta = "  Directo  ";
            dtoConEspacios.Clima = "  Frío  ";
            dtoConEspacios.Procesos = new List<string> { "  Siembra  " };
            dtoConEspacios.TipoCultivo = new List<string> { "  Hidropónico  " };
            dtoConEspacios.Contacto = new ContactoDTO { Nombre = "  Ana Maria  ", Telefono = "  3001234567  " };

            var resultado = _servicio.Create(dtoConEspacios);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("Granja Con Espacios", resultado.Nombre);
            Assert.AreEqual("Maíz", resultado.Cultivo);
            Assert.AreEqual("Marinilla", resultado.Ubicacion);
            Assert.AreEqual("Directo", resultado.CanalVenta);
            Assert.AreEqual("Frío", resultado.Clima);
            Assert.AreEqual("Siembra", resultado.Procesos[0]);
            Assert.AreEqual("Hidropónico", resultado.TipoCultivo[0]);
            Assert.AreEqual("Ana Maria", resultado.Contacto.Nombre);
            Assert.AreEqual("3001234567", resultado.Contacto.Telefono);
        }

        [TestMethod]
        public void Create_ConMayusculasYTildes_SeAceptaGraciasANormalizacion()
        {
            var dto = CrearDTOValido("GRANJA EL RAÍZAL EN ÁREA RURAL");

            var resultado = _servicio.Create(dto);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("GRANJA EL RAÍZAL EN ÁREA RURAL", resultado.Nombre);
        }

        [TestMethod]
        public void Create_Con14DTOInvalidos_RetornaNullYBaseNoCambia()
        {
            int cantidadInicial = Datos.BaseGranjas.Count;

            var dtosInvalidos = new List<GranjaCreateDTO>
            {
                // Nombre nulo o vacíos
                new GranjaCreateDTO { Nombre = null, Ciclos = 5 },
                new GranjaCreateDTO { Nombre = "", Ciclos = 5 },
                new GranjaCreateDTO { Nombre = "   ", Ciclos = 5 },

                // Ciclos <= 0
                CrearDTOValidoConCiclos(0),
                CrearDTOValidoConCiclos(-1),
                CrearDTOValidoConCiclos(-10),

                // Combinaciones de ambos errores
                new GranjaCreateDTO { Nombre = null, Ciclos = 0 },
                new GranjaCreateDTO { Nombre = "", Ciclos = 0 },
                new GranjaCreateDTO { Nombre = "   ", Ciclos = 0 },
                new GranjaCreateDTO { Nombre = null, Ciclos = -5 },
                new GranjaCreateDTO { Nombre = "", Ciclos = -5 },
                new GranjaCreateDTO { Nombre = "   ", Ciclos = -5 },
                new GranjaCreateDTO { Nombre = null, Ciclos = -100 },
                new GranjaCreateDTO { Nombre = "", Ciclos = -100 }
            };

            Assert.AreEqual(14, dtosInvalidos.Count, "Se deben probar exactamente 14 DTOs inválidos.");

            foreach (var dtoInvalido in dtosInvalidos)
            {
                var resultado = _servicio.Create(dtoInvalido);
                Assert.IsNull(resultado, "El DTO inválido no debió ser procesado.");
            }

            Assert.AreEqual(cantidadInicial, Datos.BaseGranjas.Count, "La base de datos no debió sufrir cambios.");
        }

        private GranjaCreateDTO CrearDTOValidoConCiclos(int ciclos)
        {
            var dto = CrearDTOValido();
            dto.Ciclos = ciclos;
            return dto;
        }

        [TestMethod]
        public void Create_Null_DevuelveNull()
        {
            int cantidadInicial = Datos.BaseGranjas.Count;

            var resultado = _servicio.Create(null!);

            Assert.IsNull(resultado);
            Assert.AreEqual(cantidadInicial, Datos.BaseGranjas.Count);
        }
        #endregion

        #region ObtenerErroresValidacion
        [TestMethod]
        public void ObtenerErroresValidacion_Valido_DevuelveListaVacia()
        {
            var dtoValido = CrearDTOValido();

            var errores = _servicio.ObtenerErroresValidacion(dtoValido);

            Assert.IsNotNull(errores);
            Assert.AreEqual(0, errores.Count);
        }

        [TestMethod]
        public void ObtenerErroresValidacion_Con2Errores_Devuelve2Mensajes()
        {
            // Nombre vacío y ciclos <= 0 generan exactamente los errores configurados en ObtenerErroresValidacion
            var dtoInvalido = new GranjaCreateDTO
            {
                Nombre = "",
                Ciclos = 0
            };

            var errores = _servicio.ObtenerErroresValidacion(dtoInvalido);

            Assert.IsNotNull(errores);
            Assert.AreEqual(2, errores.Count);
            CollectionAssert.Contains(errores, "El nombre de la granja es obligatorio.");
            CollectionAssert.Contains(errores, "Los ciclos deben ser mayores a 0.");
        }

        [TestMethod]
        public void ObtenerErroresValidacion_Null_Devuelve1Mensaje()
        {
            var errores = _servicio.ObtenerErroresValidacion(null!);

            Assert.IsNotNull(errores);
            Assert.AreEqual(1, errores.Count);
            Assert.AreEqual("La información de la granja es obligatoria.", errores[0]);
        }
        #endregion

        #region Update
        [TestMethod]
        public void Update_Valido_DevuelveTrueYDatosCambiados()
        {
            var dtoActualizacion = CrearDTOValido("Granja Actualizada");

            bool resultado = _servicio.Update(1, dtoActualizacion);

            Assert.IsTrue(resultado);
            var granjaModificada = Datos.BaseGranjas.First(g => g.ID == 1);
            Assert.AreEqual("Granja Actualizada", granjaModificada.Nombre);
        }

        [TestMethod]
        public void Update_IDInexistente_DevuelveFalse()
        {
            var dtoActualizacion = CrearDTOValido();

            bool resultado = _servicio.Update(999, dtoActualizacion);

            Assert.IsFalse(resultado);
        }

        [TestMethod]
        public void Update_DTOInvalido_DevuelveFalseSinCambios()
        {
            var dtoInvalido = new GranjaCreateDTO { Nombre = "", Ciclos = 0 };
            var nombreOriginal = Datos.BaseGranjas.First(g => g.ID == 1).Nombre;

            bool resultado = _servicio.Update(1, dtoInvalido);

            Assert.IsFalse(resultado);
            Assert.AreEqual(nombreOriginal, Datos.BaseGranjas.First(g => g.ID == 1).Nombre);
        }
        #endregion

        #region Delete
        [TestMethod]
        public void Delete_Existente_DevuelveTrueYEliminado()
        {
            int cantidadInicial = Datos.BaseGranjas.Count;

            bool resultado = _servicio.Delete(1);

            Assert.IsTrue(resultado);
            Assert.AreEqual(cantidadInicial - 1, Datos.BaseGranjas.Count);
            Assert.IsNull(Datos.BaseGranjas.FirstOrDefault(g => g.ID == 1));
        }

        [TestMethod]
        public void Delete_Inexistente_DevuelveFalse()
        {
            int cantidadInicial = Datos.BaseGranjas.Count;

            bool resultado = _servicio.Delete(999);

            Assert.IsFalse(resultado);
            Assert.AreEqual(cantidadInicial, Datos.BaseGranjas.Count);
        }
        #endregion
    }
}