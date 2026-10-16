using EnergiaR.DTO;

namespace EnergiaR.ORM.Inte;

// Interface para medidores: casos de uso
public interface ICaUso
{
    Task<IEnumerable<TCaUso>> Load();
    Task<IEnumerable<TCaUso>> Filtered(String _Like);
    Task<TCaUso?> ByID(Int64 _ID);
    Task<(Int64 Done, String Mess)> IDRe(String _CUso);
    Task<List<String>> CBox();
    Task<(Boolean Exis, String Mess)> DataEx(String _CUso, Int64 _ID = 0);
    Task<(Boolean Done, String Mess)> Inse(TCaUso _CaUso);
    Task<(Boolean Done, String Mess)> Upda(TCaUso _CaUso);
    Task<(Boolean Done, String Mess)> Dele(Int64 _ID);
}
