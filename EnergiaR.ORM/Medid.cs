using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using EnergiaR.DTO;
using EnergiaR.ORM.Inte;
using EnergiaR.ORM.Xtra;

namespace EnergiaR.ORM;

// Medidores
public class Medid(MyContexts _Contexts) : IMedid
{
    private readonly MyContexts Contexts = _Contexts;

    #region Cargar datos
    // Cargar medidores
    public async Task<IEnumerable<VMedid>> Load()
    {
        return await Contexts.VMedi.
            AsNoTracking().
            OrderBy(E => E.Medi).
            ToListAsync();
    }

    // Filtrar medidores
    public async Task<IEnumerable<VMedid>> Filtered(String _Like)
    {
        if (String.IsNullOrWhiteSpace(_Like) || _Like.Length <= 0) return await Load();

        return await Contexts.VMedi.
            Where(E =>
                EF.Functions.Like(E.MNIC, $"%{_Like}%") || 
                EF.Functions.Like(E.Medi, $"%{_Like}%") ||
                EF.Functions.Like(E.Dire, $"%{_Like}%") ||
                EF.Functions.Like(E.CUso, $"%{_Like}%") ||
                EF.Functions.Like(E.Docu, $"%{_Like}%") || 
                EF.Functions.Like(E.Nomb, $"%{_Like}%")).
            AsNoTracking().
            OrderBy(E => E.Medi).
            ToListAsync();
    }

    // Medidores por ID
    public async Task<VMedid?> ByID(Int64 _ID)
    {
        if (_ID <= 0) return null;

        Boolean NonExis = await Contexts.VMedi.AsNoTracking().AnyAsync(E => E.ID == _ID);
        if (!NonExis) return null;

        return await Contexts.VMedi.AsNoTracking().FirstOrDefaultAsync(E => E.ID == _ID);
    }

    // ID de medidor
    public async Task<(Int64 Done, String Mess)> IDRe(String _Medi)
    {
        if (String.IsNullOrWhiteSpace(_Medi) || _Medi.Length > 125)
            return (0, "Medidor no válido");

        Int64 Result = await Contexts.TMedi.
            Where(E => E.Medi == _Medi).
            AsNoTracking().
            Select(E => E.ID).
            FirstOrDefaultAsync();

        if (Result > 0) return (Result, String.Empty);
        else return (0, "Algo salió mal: No fue posible retornar la opción seleccionada");
    }

    // Medidores para control ComboBox
    public async Task<List<String>> CBox()
    {
        List<String> Lista = await Contexts.TMedi.
            Select(E => E.Medi).
            AsNoTracking().
            OrderBy(E => E).
            ToListAsync();

        // Añadir elemento vacío si hay resultados
        if (Lista.Count > 0) Lista.Insert(0, String.Empty);

        return Lista;
    }
    #endregion

    #region Validaciones
    // Validar existencia de un medidor
    public async Task<(Boolean Exis, String Mess)> DataEx(String _Medi, Int64 _ID = 0)
    {
        if (String.IsNullOrWhiteSpace(_Medi) || _Medi.Length > 125)
            return (true, "Medidor no válido");

        String Me = _Medi.Replace(" ", String.Empty);

        Boolean E = await Contexts.TMedi.
            AsNoTracking().
            AnyAsync(E => 
                E.Medi.Replace(" ", String.Empty) == Me && 
                E.ID != _ID);

        return (E, E ? "Este medidor ya está registrado" : String.Empty);
    }

    // Validar datos de medidor
    async Task<(Boolean Done, String Mess)> Verifi(TMedid _Medid, Boolean Edit = false)
    {
        Boolean Conten = _Medid is null || 
            String.IsNullOrWhiteSpace(_Medid.MNIC) || _Medid.MNIC.Length > 125 || 
            String.IsNullOrWhiteSpace(_Medid.Medi) || _Medid.Medi.Length > 100 ||
            String.IsNullOrWhiteSpace(_Medid.Dire) || _Medid.Dire.Length > 100;

        if (Conten) return (true, "Operación cancelada por valores o datos no válidos");

        (Boolean SameRegs, String SameMess) = await DataEx(_Medid!.Medi, Edit ? _Medid.ID : 0);

        if (SameRegs) return (true, $"Operación cancelada: {SameMess}");

        return (false, String.Empty);
    }
    #endregion

    #region Agregar | Editar | Eliminar
    // Agregar medidores
    public async Task<(Boolean Done, String Mess)> Inse(TMedid _Medid)
    {
        try
        {
            (Boolean VD, String VM) = await Verifi(_Medid);
            if (VD) return (false, VM);

            await Contexts.TMedi.AddAsync(_Medid);

            if (await Contexts.SaveChangesAsync() > 0) return (true, "Medidor registrado!");
            else return (false, "Algo salió mal: Los datos no se registraron");
        }
        catch (SqliteException ex)
        {
            return await UseFiles.SetError(ex, "El archivo de base de datos está bloqueado o corrupto");
        }
        catch (DbUpdateException ex)
        {
            return await UseFiles.SetError(ex, "No fue posible guardar este medidor");
        }
        catch (InvalidOperationException ex)
        {
            return await UseFiles.SetError(ex, "Operación no válida");
        }
        catch (Exception ex)
        {
            return await UseFiles.SetError(ex, "Ha ocurrido un error desconocido");
        }
    }

    // Editar medidores
    public async Task<(Boolean Done, String Mess)> Upda(TMedid _Medid)
    {
        try
        {
            (Boolean VD, String VM) = await Verifi(_Medid, true);
            if (VD) return (false, VM);

            Boolean NonExis = await Contexts.TMedi.AsNoTracking().AnyAsync(E => E.ID == _Medid.ID);
            if (!NonExis) return (false, "Operación cancelada: Este medidor no existe");

            Contexts.TMedi.Update(_Medid);

            if (await Contexts.SaveChangesAsync() > 0) return (true, "Datos de medidor actualizados!");
            else return (false, "Algo salió mal: Los datos no se actualizaron");
        }
        catch (SqliteException ex)
        {
            return await UseFiles.SetError(ex, "El archivo de base de datos está bloqueado o corrupto");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return await UseFiles.SetError(ex, "El registro fue editado o eliminado desde otra instancia o otro usuario");
        }
        catch (DbUpdateException ex)
        {
            return await UseFiles.SetError(ex, "No fue posible editar este medidor");
        }
        catch (InvalidOperationException ex)
        {
            return await UseFiles.SetError(ex, "Operación no válida");
        }
        catch (Exception ex)
        {
            return await UseFiles.SetError(ex, "Ha ocurrido un error desconocido");
        }
    }

    // Eliminar medidores
    public async Task<(Boolean Done, String Mess)> Dele(Int64 _ID)
    {
        try
        {
            Boolean NonExis = await Contexts.TMedi.AsNoTracking().AnyAsync(E => E.ID == _ID);
            if (!NonExis) return (false, "Operación cancelada: Este medidor no existe");

            Boolean D = await Contexts.TClie.Where(E => E.ID == _ID).ExecuteDeleteAsync() > 0;

            if (D) return (true, "Medidor eliminado");
            else return (false, "Algo salió mal: Los datos no se eliminaron");
        }
        catch (SqliteException ex)
        {
            return await UseFiles.SetError(ex, "El archivo de base de datos está bloqueado o corrupto");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return await UseFiles.SetError(ex, "El registro fue editado o eliminado desde otra instancia o otro usuario");
        }
        catch (DbUpdateException ex)
        {
            return await UseFiles.SetError(ex, "No fue posible eliminar este medidor");
        }
        catch (InvalidOperationException ex)
        {
            return await UseFiles.SetError(ex, "Operación no válida");
        }
        catch (Exception ex)
        {
            return await UseFiles.SetError(ex, "Ha ocurrido un error desconocido");
        }
    }
    #endregion
}
