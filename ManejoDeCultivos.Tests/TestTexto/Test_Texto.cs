namespace ManejoDeCultivos.Tests;

using Servicios.Servicios;
[TestClass]
public class Test_Texto
{
    [TestMethod]
    public void TestStringNormal()
    {
        string result = ServicioTexto.NormalizarTexto("el peñol");
        Assert.AreEqual("el peñol", result);
    }

    [TestMethod]
    public void TestStringVacio()
    {
        string result = ServicioTexto.NormalizarTexto("");
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void TestStringCombinado()
    {
        string result1 = ServicioTexto.NormalizarTexto("Papá");
        string result2 = ServicioTexto.NormalizarTexto("Mi Nombre Es José");
        string result3 = ServicioTexto.NormalizarTexto("Fríjol Cargamanto");
        string result4 = ServicioTexto.NormalizarTexto("Papa Criolla");
        string result5 = ServicioTexto.NormalizarTexto("ClaseDePoOLV");
        string result6 = ServicioTexto.NormalizarTexto("          Inglés");
        string result7 = ServicioTexto.NormalizarTexto("Azúcar          ");
        string result8 = ServicioTexto.NormalizarTexto("Quiero Que Sea Sábado");

        Assert.AreEqual("papa", result1);
        Assert.AreEqual("mi nombre es jose", result2);
        Assert.AreEqual("frijol cargamanto", result3);
        Assert.AreEqual("papa criolla", result4);
        Assert.AreEqual("clasedepoolv", result5);
        Assert.AreEqual("ingles", result6);
        Assert.AreEqual("azucar", result7);
        Assert.AreEqual("quiero que sea sabado", result8);
    }
}
