using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EnergiaR.Fls;
using System.Reflection;

namespace EnergiaR.ORM;

// Base de datos
public static class Conn
{
    #region Conectar con base de datos
    // Probar conexión con base de datos
    public static async Task<(Boolean Done, String Mess)> Connect(String CadenaDB)
    {
        if (String.IsNullOrWhiteSpace(CadenaDB)) return (false, "La cadena de conexión está vacía");

        try
        {
            await using SqliteConnection Conn = new(CadenaDB);
            await Conn.OpenAsync();
            (Boolean D, String M) = await ExecuteSQL(Conn);
            await Conn.CloseAsync();

            if (D) return (true, String.Empty);
            else return (false, "Base de datos o estructura inaccesible");
        }
        catch (SqliteException ex)
        {
            return await UseFiles.SetError(ex, "El archivo de base de datos está bloqueado o corrupto");
        }
        catch (Exception ex)
        {
            return await UseFiles.SetError(ex, "No fue posible conectar con la base de Datos");
        }
    }

    // Crear tablas, vistas y disparadores
    static async Task<(Boolean Done, String Mess)> ExecuteSQL(SqliteConnection Conn)
    {
        String SQL = await UseFiles.TextFile("EnergiaRDB.sql", Assembly.GetExecutingAssembly());

        if (SQL is null || String.IsNullOrWhiteSpace(SQL))
            return (false, "No fue posible acceder a la estructura de la base de datos");

        await using SqliteCommand CMD = new(SQL, Conn);
        await CMD.ExecuteNonQueryAsync();
        return (true, String.Empty);
    }
    #endregion

    #region Servicio para iniciar Backend
    public static IServiceCollection DBServices(this IServiceCollection Servicio, String CadenaDB)
    {
        if (String.IsNullOrWhiteSpace(CadenaDB))
            throw new InvalidOperationException("Cadena de conexión vacía o inválida");

        // Registrar el DbContext con SQLite
        Servicio.AddDbContext<MyContexts>(Op =>
            Op.UseSqlite(CadenaDB, OpSql => OpSql.MigrationsAssembly(typeof(Conn).Assembly.FullName)));

        return Servicio;
    }
    #endregion
}
