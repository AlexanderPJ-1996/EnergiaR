using EnergiaR.DTO;

namespace EnergiaR.ORM.Inte;

// Interface para empresas
public interface IEmpre
{
    Task<IEnumerable<TEmpre>> Load();
    Task<IEnumerable<TEmpre>> Filtered(String _Like);
    Task<TEmpre?> ByID(Int64 _ID);
    Task<(Int64 Done, String Mess)> IDRe(String _NDoc);
    Task<List<String>> CBox();
    Task<(Boolean Exis, String Mess)> DataEx(String _NDoc, Int64 _ID = 0);
    Task<(Boolean Done, String Mess)> Inse(TEmpre _Empre);
    Task<(Boolean Done, String Mess)> Upda(TEmpre _Empre);
    Task<(Boolean Done, String Mess)> Dele(Int64 _ID);
}
