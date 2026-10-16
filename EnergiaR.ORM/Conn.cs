using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EnergiaR.ORM.Xtra;

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
            await ExecuteSQL(Conn);
            await Conn.CloseAsync();
            return (true, String.Empty);
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
    static async Task ExecuteSQL(SqliteConnection Conn)
    {
        String SQL = await UseFiles.TextFile("EnergiaRDB.sql");

        if (String.IsNullOrWhiteSpace(SQL)) return;

        await using SqliteCommand CMD = new(SQL, Conn);
        await CMD.ExecuteNonQueryAsync();
    }
    #endregion

    #region Servicio para iniciar Backend
    public static IServiceCollection DBServices(this IServiceCollection Servicio, String CadenaDB)
    {
        if (String.IsNullOrWhiteSpace(CadenaDB) || !File.Exists(CadenaDB))
            throw new InvalidOperationException("Servicio no registrado por valor nulo o vacío");

        // Registrar el DbContext con SQLite
        Servicio.AddDbContext<MyContexts>(Op =>
            Op.UseSqlite(CadenaDB, OpSql => OpSql.MigrationsAssembly(typeof(Conn).Assembly.FullName)));

        return Servicio;
    }
    #endregion
}
