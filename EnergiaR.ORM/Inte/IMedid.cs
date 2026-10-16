using EnergiaR.DTO;

namespace EnergiaR.ORM.Inte;

// 
public interface IMedid
{
    Task<IEnumerable<VMedid>> Load();
    Task<IEnumerable<VMedid>> Filtered(String _Like);
    Task<VMedid?> ByID(Int64 _ID);
    Task<(Int64 Done, String Mess)> IDRe(String _Medi);
    Task<List<String>> CBox();
    Task<(Boolean Exis, String Mess)> DataEx(String _Medi, Int64 _ID = 0);
    Task<(Boolean Done, String Mess)> Inse(TMedid _Medid);
    Task<(Boolean Done, String Mess)> Upda(TMedid _Medid);
    Task<(Boolean Done, String Mess)> Dele(Int64 _ID);
}
