using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using EnergiaR.DTO;
using EnergiaR.ORM.Inte;
using EnergiaR.Fls;

namespace EnergiaR.ORM;

// Medidores: Casos de uso
public class CaUso(MyContexts _Contexts) : ICaUso
{
    private readonly MyContexts Contexts = _Contexts;

    #region Cargar datos
    // Cargar casos de uso
    public async Task<IEnumerable<TCaUso>> Load()
    {
        return await Contexts.TCUso.
            AsNoTracking().
            OrderBy(E => E.CUso).
            ToListAsync();
    }

    // Filtrar casos de uso
    public async Task<IEnumerable<TCaUso>> Filtered(String _Like)
    {
        if (String.IsNullOrWhiteSpace(_Like) || _Like.Length <= 0) return await Load();

        return await Contexts.TCUso.
            Where(E => 
                EF.Functions.Like(E.CUso, $"%{_Like}%") || 
                EF.Functions.Like(E.CDes, $"%{_Like}%")).
            AsNoTracking().
            OrderBy(E => E.CUso).
            ToListAsync();
    }

    // Casos de uso por ID
    public async Task<TCaUso?> ByID(Int64 _ID)
    {
        if (_ID <= 0) return null;

        Boolean NonExis = await Contexts.TCUso.AsNoTracking().AnyAsync(E => E.ID == _ID);
        if (!NonExis) return null;

        return await Contexts.TCUso.AsNoTracking().FirstOrDefaultAsync(E => E.ID == _ID);
    }

    // ID de caso de uso
    public async Task<(Int64 Done, String Mess)> IDRe(String _CUso)
    {
        if (String.IsNullOrWhiteSpace(_CUso) || _CUso.Length > 20) 
            return (0, "Caso de uso no válido");

        String CU = _CUso.Replace(" ", String.Empty);

        Int64 Result = await Contexts.TCUso.
            Where(E => E.CUso.Replace(" ", String.Empty) == CU).
            AsNoTracking().
            Select(E => E.ID).
            FirstOrDefaultAsync();

        if (Result > 0) return (Result, String.Empty);
        else return (0, "Algo salió mal: No fue posible retornar la opción seleccionada");
    }

    // Casos de uso para control ComboBox
    public async Task<List<String>> CBox()
    {
        List<String> Lista = await Contexts.TCUso.
            Select(E => E.CUso).
            AsNoTracking().
            OrderBy(E => E).
            ToListAsync();

        // Añadir elemento vacío si hay resultados
        if (Lista.Count > 0) Lista.Insert(0, String.Empty);

        return Lista;
    }
    #endregion

    #region Validaciones
    // Validar existencia de un caso de uso
    public async Task<(Boolean Exis, String Mess)> DataEx(String _CUso, Int64 _ID = 0)
    {
        if (String.IsNullOrWhiteSpace(_CUso) || _CUso.Length > 20)
            return (true, "Caso de uso no válido");

        String CU = _CUso.Replace(" ", String.Empty);

        Boolean E = await Contexts.TCUso.
            AsNoTracking().
            AnyAsync(E =>
                E.CUso.Replace(" ", String.Empty) == CU &&
                E.ID != _ID);

        return (E, E ? "Este caso de uso ya está registrado" : String.Empty);
    }

    // Validar datos de caso de uso
    async Task<(Boolean Done, String Mess)> Verifi(TCaUso _CaUso, Boolean Edit = false)
    {
        Boolean Conten = _CaUso is null ||
            String.IsNullOrWhiteSpace(_CaUso.CUso) || _CaUso.CUso.Length > 20 ||
            String.IsNullOrWhiteSpace(_CaUso.CDes) || _CaUso.CDes.Length > 100;

        if (Conten) return (true, "Operación cancelada por valores o datos no válidos");

        (Boolean SameRegs, String SameMess) = await DataEx(_CaUso!.CUso, Edit ? _CaUso.ID : 0);

        if (SameRegs) return (true, $"Operación cancelada: {SameMess}");

        return (false, String.Empty);
    }
    #endregion

    #region Agregar | Editar | Eliminar
    // Agregar caso de uso
    public async Task<(Boolean Done, String Mess)> Inse(TCaUso _CaUso)
    {
        try
        {
            (Boolean VD, String VM) = await Verifi(_CaUso);
            if (VD) return (false, VM);

            await Contexts.TCUso.AddAsync(_CaUso);

            if (await Contexts.SaveChangesAsync() > 0) return (true, "Caso de uso registrado!");
            else return (false, "Algo salió mal: Los datos no se registraron");
        }
        catch (SqliteException ex)
        {
            return await UseFiles.SetError(ex, "El archivo de base de datos está bloqueado o corrupto");
        }
        catch (DbUpdateException ex)
        {
            return await UseFiles.SetError(ex, "No fue posible guardar este caso de uso");
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

    // Editar caso de uso
    public async Task<(Boolean Done, String Mess)> Upda(TCaUso _CaUso)
    {
        try
        {
            (Boolean VD, String VM) = await Verifi(_CaUso, true);
            if (VD) return (false, VM);

            Boolean NonExis = await Contexts.TCUso.AsNoTracking().AnyAsync(E => E.ID == _CaUso.ID);
            if (!NonExis) return (false, "Operación cancelada: Este caso de uso no existe");

            Contexts.TCUso.Update(_CaUso);

            if (await Contexts.SaveChangesAsync() > 0) return (true, "Datos de caso de uso actualizados!");
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
            return await UseFiles.SetError(ex, "No fue posible editar este caso de uso");
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

    // Eliminar caso de uso
    public async Task<(Boolean Done, String Mess)> Dele(Int64 _ID)
    {
        try
        {
            Boolean NonExis = await Contexts.TCUso.AsNoTracking().AnyAsync(E => E.ID == _ID);
            if (!NonExis) return (false, "Operación cancelada: Este caso de uso no existe");

            Boolean D = await Contexts.TCUso.Where(E => E.ID == _ID).ExecuteDeleteAsync() > 0;

            if (D) return (true, "Caso de uso eliminado");
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
            return await UseFiles.SetError(ex, "No fue posible eliminar este caso de uso");
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
