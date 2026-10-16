using EnergiaR.DTO;

namespace EnergiaR.ORM.Inte;

// Interface para medidores: clientes/propietarios
public interface IClien
{
    Task<IEnumerable<VClien>> Load();
    Task<IEnumerable<VClien>> Filtered(String _Like);
    Task<VClien?> ByID(Int64 _ID);
    Task<(Int64 Done, String Mess)> IDRe(String _Docu);
    Task<(String Done, String Mess)> Nomb(String _Docu);
    Task<List<String>> CBox();
    Task<(Boolean Exis, String Mess)> DataEx(String _Docu, Int64 _ID = 0);
    Task<(Boolean Done, String Mess)> Inse(TClien _Clien);
    Task<(Boolean Done, String Mess)> Upda(TClien _Clien);
    Task<(Boolean Done, String Mess)> Dele(Int64 _ID);
}
