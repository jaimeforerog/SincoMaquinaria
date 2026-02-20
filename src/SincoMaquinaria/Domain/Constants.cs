namespace SincoMaquinaria.Domain;

public static class CacheKeys
{
    public const string TiposMedidor = "configuracion:tiposmedidor";
    public const string Grupos = "configuracion:grupos";
    public const string TiposFalla = "configuracion:tiposfalla";
    public const string CausasFalla = "configuracion:causasfalla";
}

public static class TipoMantenimientoConstants
{
    public const string Correctivo = "Correctivo";
    public const string Preventivo = "Preventivo";
    public const string Predictivo = "Predictivo";

    public static readonly string[] Todos = { Preventivo, Correctivo, Predictivo };
}

public static class EstadoEquipo
{
    public const string Activo = "Activo";
    public const string Inactivo = "Inactivo";

    public static readonly string[] Todos = { Activo, Inactivo };
}
