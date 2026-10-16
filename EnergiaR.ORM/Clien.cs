using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using EnergiaR.DTO;
using EnergiaR.ORM.Inte;
using EnergiaR.ORM.Xtra;

namespace EnergiaR.ORM;

// Medidores: Clientes/Propietarios
public class Clien(MyContexts _Contexts) : IClien
{
    private readonly MyContexts Contexts = _Contexts;

    #region Cargar datos
    // Cargar clientes/propietarios
    public async Task<IEnumerable<VClien>> Load()
    {
        return await Contexts.VClie.
            AsNoTracking().
            OrderBy(E => E.Nomb).
            ToListAsync();
    }

    // Filtrar clientes/propietarios
    public async Task<IEnumerable<VClien>> Filtered(String _Like)
    {
        if (String.IsNullOrWhiteSpace(_Like) || _Like.Length <= 0) return await Load();

        return await Contexts.VClie.
            Where(E =>
                EF.Functions.Like(E.TDoc, $"%{_Like}%") || 
                EF.Functions.Like(E.NDoc, $"%{_Like}%") ||
                EF.Functions.Like(E.Nomb, $"%{_Like}%")).
            AsNoTracking().
            OrderBy(E => E.Nomb).
            ToListAsync();
    }

    // Clientes/Propietarios por ID
    public async Task<VClien?> ByID(Int64 _ID)
    {
        if (_ID <= 0) return null;

        Boolean NonExis = await Contexts.VClie.AsNoTracking().AnyAsync(E => E.ID == _ID);
        if (!NonExis) return null;

        return await Contexts.VClie.AsNoTracking().FirstOrDefaultAsync(E => E.ID == _ID);
    }

    // ID de cliente/propietario
    public async Task<(Int64 Done, String Mess)> IDRe(String _Docu)
    {
        if (String.IsNullOrWhiteSpace(_Docu) || _Docu.Length > 121)
            return (0, "Cliente/Propietario no válido");

        Int64 Result = await Contexts.TClie.
            Where(E => $"{E.TDoc} {E.NDoc}" == _Docu).
            AsNoTracking().
            Select(E => E.ID).
            FirstOrDefaultAsync();

        if (Result > 0) return (Result, String.Empty);
        else return (0, "Algo salió mal: No fue posible retornar la opción seleccionada");
    }
    
    // Nombre de cliente/propietario
    public async Task<(String Done, String Mess)> Nomb(String _Docu)
    {
        if (String.IsNullOrWhiteSpace(_Docu) || _Docu.Length > 121) 
            return (String.Empty, "Cliente/Propietario no válido");

        String? Result = await Contexts.TClie.
            Where(E => $"{E.TDoc} {E.NDoc}" == _Docu).
            AsNoTracking().
            Select(E => E.Nomb).
            FirstOrDefaultAsync();

        if (Result is not null && !String.IsNullOrWhiteSpace(Result)) return (Result, String.Empty);
        else return (String.Empty, "Algo salió mal: No fue posible retornar la opción seleccionada");
    }

    // Documento de clientes/propietarios para control ComboBox
    public async Task<List<String>> CBox()
    {
        List<String> Lista = await Contexts.TClie.
            Select(E => $"{E.TDoc} {E.NDoc}").
            AsNoTracking().
            OrderBy(E => E).
            ToListAsync();

        // Añadir elemento vacío si hay resultados
        if (Lista.Count > 0) Lista.Insert(0, String.Empty);

        return Lista;
    }
    #endregion

    #region Validaciones
    // Validar existencia de un cliente/propietario
    public async Task<(Boolean Exis, String Mess)> DataEx(String _Docu, Int64 _ID = 0)
    {
        if (String.IsNullOrWhiteSpace(_Docu) || _Docu.Length > 121)
            return (true, "Cliente/Propietario no válido");

        Boolean E = await Contexts.TClie.
            AsNoTracking().
            AnyAsync(E =>
                $"{E.TDoc} {E.NDoc}" == _Docu &&
                E.ID != _ID);

        return (E, E ? "Este cliente/propietario ya está registrado" : String.Empty);
    }

    // Validar datos de clientes/propietarios
    async Task<(Boolean Done, String Mess)> Verifi(TClien _Clien, Boolean Edit = false)
    {
        Boolean Conten = _Clien is null ||
            String.IsNullOrWhiteSpace(_Clien.TDoc) || _Clien.TDoc.Length > 20 ||
            String.IsNullOrWhiteSpace(_Clien.NDoc) || _Clien.NDoc.Length > 100 ||
            String.IsNullOrWhiteSpace(_Clien.Nomb) || _Clien.Nomb.Length > 100;

        if (Conten) return (true, "Operación cancelada por valores o datos no válidos");

        (Boolean SameRegs, String SameMess) = await DataEx($"{_Clien!.TDoc} {_Clien!.NDoc}", Edit ? _Clien.ID : 0);

        if (SameRegs) return (true, $"Operación cancelada: {SameMess}");

        return (false, String.Empty);
    }
    #endregion

    #region Agregar | Editar | Eliminar
    // Agregar clientes/propietarios
    public async Task<(Boolean Done, String Mess)> Inse(TClien _Clien)
    {
        try
        {
            (Boolean VD, String VM) = await Verifi(_Clien);
            if (VD) return (false, VM);

            await Contexts.TClie.AddAsync(_Clien);

            if (await Contexts.SaveChangesAsync() > 0) return (true, "Cliente/Propietario registrado!");
            else return (false, "Algo salió mal: Los datos no se registraron");
        }
        catch (SqliteException ex)
        {
            return await UseFiles.SetError(ex, "El archivo de base de datos está bloqueado o corrupto");
        }
        catch (DbUpdateException ex)
        {
            return await UseFiles.SetError(ex, "No fue posible guardar este cliente/propietario");
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

    // Editar clientes/propietarios
    public async Task<(Boolean Done, String Mess)> Upda(TClien _Clien)
    {
        try
        {
            (Boolean VD, String VM) = await Verifi(_Clien, true);
            if (VD) return (false, VM);

            Boolean NonExis = await Contexts.TCUso.AsNoTracking().AnyAsync(E => E.ID == _Clien.ID);
            if (!NonExis) return (false, "Operación cancelada: Este cliente/propietario no existe");

            Contexts.TClie.Update(_Clien);

            if (await Contexts.SaveChangesAsync() > 0) return (true, "Datos de cliente/propietario actualizados!");
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
            return await UseFiles.SetError(ex, "No fue posible editar este cliente/propietario");
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

    // Eliminar clientes/propietarios
    public async Task<(Boolean Done, String Mess)> Dele(Int64 _ID)
    {
        try
        {
            Boolean NonExis = await Contexts.TClie.AsNoTracking().AnyAsync(E => E.ID == _ID);
            if (!NonExis) return (false, "Operación cancelada: Este cliente/propietario no existe");

            Boolean D = await Contexts.TClie.Where(E => E.ID == _ID).ExecuteDeleteAsync() > 0;

            if (D) return (true, "Cliente/Propietario eliminado");
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
            return await UseFiles.SetError(ex, "No fue posible eliminar este cliente/propietario");
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
