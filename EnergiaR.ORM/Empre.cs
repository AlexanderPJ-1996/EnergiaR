using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using EnergiaR.DTO;
using EnergiaR.ORM.Inte;
using EnergiaR.ORM.Xtra;

namespace EnergiaR.ORM;

// Empresas prestadoras de servicio de energía
public class Empre(MyContexts _Contexts) : IEmpre
{
    private readonly MyContexts Contexts = _Contexts;

    #region Cargar datos
    // Cargar empresas
    public async Task<IEnumerable<TEmpre>> Load()
    {
        return await Contexts.TEmpr.
            AsNoTracking().
            OrderBy(E => E.RaSo).
            ToListAsync();
    }

    // Filtrar empresas
    public async Task<IEnumerable<TEmpre>> Filtered(String _Like)
    {
        if (String.IsNullOrWhiteSpace(_Like) || _Like.Length <= 0) return await Load();

        return await Contexts.TEmpr.
            Where(E => 
                EF.Functions.Like(E.NDoc, $"%{_Like}%") || 
                EF.Functions.Like(E.RaSo, $"%{_Like}%") || 
                EF.Functions.Like(E.Dire, $"%{_Like}%") || 
                EF.Functions.Like(E.Tele, $"%{_Like}%") || 
                EF.Functions.Like(E.Mail, $"%{_Like}%")).
            AsNoTracking().
            OrderBy(E => E.RaSo).
            ToListAsync();
    }

    // Empresas por ID
    public async Task<TEmpre?> ByID(Int64 _ID)
    {
        if (_ID <= 0) return null;

        Boolean NonExis = await Contexts.TEmpr.AsNoTracking().AnyAsync(E => E.ID == _ID);
        if (!NonExis) return null;

        return await Contexts.TEmpr.AsNoTracking().FirstOrDefaultAsync(E => E.ID == _ID);
    }

    // ID de empresa
    public async Task<(Int64 Done, String Mess)> IDRe(String _NDoc)
    {
        if (String.IsNullOrWhiteSpace(_NDoc) || _NDoc.Length > 125) 
            return (0, "NIT o no. de registro no válido");

        String ND = _NDoc.Replace(" ", String.Empty);

        Int64 Result = await Contexts.TEmpr.
            Where(E => E.NDoc.Replace(" ", String.Empty) == ND).
            AsNoTracking().
            Select(E => E.ID).
            FirstOrDefaultAsync();

        if (Result > 0) return (Result, String.Empty);
        else return (0, "Algo salió mal: No fue posible retornar la opción seleccionada");
    }

    // Empresas control ComboBox
    public async Task<List<String>> CBox()
    {
        List<String> Lista = await Contexts.TEmpr.
            Select(E => E.RaSo).
            AsNoTracking().
            OrderBy(E => E).
            ToListAsync();

        // Añadir elemento vacío si hay resultados
        if (Lista.Count > 0) Lista.Insert(0, String.Empty);

        return Lista;
    }
    #endregion

    #region Validaciones
    // Validar existencia de una empresa
    public async Task<(Boolean Exis, String Mess)> DataEx(String _NDoc, Int64 _ID = 0)
    {
        if (String.IsNullOrWhiteSpace(_NDoc) || _NDoc.Length > 125)
            return (true, "NIT o no. de registro no válido");

        String ND = _NDoc.Replace(" ", String.Empty);

        Boolean E = await Contexts.TEmpr.
            AsNoTracking().
            AnyAsync(E =>
                E.NDoc.Replace(" ", String.Empty) == ND &&
                E.ID != _ID);

        return (E, E ? "Esta empresa ya está registrada" : String.Empty);
    }

    // Validar datos de empresa
    async Task<(Boolean Done, String Mess)> Verifi(TEmpre _Empre, Boolean Edit = false)
    {
        Boolean Conten = _Empre is null ||
            String.IsNullOrWhiteSpace(_Empre.NDoc) || _Empre.NDoc.Length > 125 ||
            String.IsNullOrWhiteSpace(_Empre.RaSo) || _Empre.RaSo.Length > 250 ||
            String.IsNullOrWhiteSpace(_Empre.Dire) || _Empre.Dire.Length > 2500 ||
            String.IsNullOrWhiteSpace(_Empre.Tele) || _Empre.Tele.Length > 2500 ||
            String.IsNullOrWhiteSpace(_Empre.Mail) || _Empre.Mail.Length > 2500;

        if (Conten) return (true, "Operación cancelada por valores o datos no válidos");

        (Boolean SameRegs, String SameMess) = await DataEx(_Empre!.NDoc, Edit ? _Empre.ID : 0);

        if (SameRegs) return (true, $"Operación cancelada: {SameMess}");

        return (false, String.Empty);
    }
    #endregion

    #region Agregar | Editar | Eliminar
    // Agregar empresa
    public async Task<(Boolean Done, String Mess)> Inse(TEmpre _Empre)
    {
        try
        {
            (Boolean VD, String VM) = await Verifi(_Empre);
            if (VD) return (false, VM);

            await Contexts.TEmpr.AddAsync(_Empre);

            if (await Contexts.SaveChangesAsync() > 0) return (true, "Empresa registrada!");
            else return (false, "Algo salió mal: Los datos no se registraron");
        }
        catch (SqliteException ex)
        {
            return await UseFiles.SetError(ex, "El archivo de base de datos está bloqueado o corrupto");
        }
        catch (DbUpdateException ex)
        {
            return await UseFiles.SetError(ex, "No fue posible guardar esta empresa");
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

    // Editar empresa
    public async Task<(Boolean Done, String Mess)> Upda(TEmpre _Empre)
    {
        try
        {
            (Boolean VD, String VM) = await Verifi(_Empre, true);
            if (VD) return (false, VM);

            Boolean NonExis = await Contexts.TEmpr.AsNoTracking().AnyAsync(E => E.ID == _Empre.ID);
            if (!NonExis) return (false, "Operación cancelada: Esta empresa no existe");

            Contexts.TEmpr.Update(_Empre);

            if (await Contexts.SaveChangesAsync() > 0) return (true, "Datos de empresa actualizados!");
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
            return await UseFiles.SetError(ex, "No fue posible editar esta empresa");
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

    // Eliminar empresa
    public async Task<(Boolean Done, String Mess)> Dele(Int64 _ID)
    {
        try
        {
            Boolean NonExis = await Contexts.TEmpr.AsNoTracking().AnyAsync(E => E.ID == _ID);
            if (!NonExis) return (false, "Operación cancelada: Esta empresa no existe");

            Boolean D = await Contexts.TEmpr.Where(E => E.ID == _ID).ExecuteDeleteAsync() > 0;

            if (D) return (true, "Empresa eliminada");
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
            return await UseFiles.SetError(ex, "No fue posible eliminar esta empresa");
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
