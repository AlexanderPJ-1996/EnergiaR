using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EnergiaR.ORM.Xtra;

// Registrar Exception en archivo .json
public static class UseFiles
{
    #region Registrar Exception en archivo .json
    private static readonly Lock LogL = new();

    static async Task SaveLogs(String UserMsg, Int32 Code, String Message, String StackTrace)
    {
        String NFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "What's Happened.json");

        lock (LogL)
        {
            JsonObject Root = [];

            if (File.Exists(NFile))
            {
                try
                {
                    Root = JsonNode.Parse(File.ReadAllText(NFile))?.AsObject() ?? [];
                }
                catch (Exception)
                {
                    Root = [];
                }
            }
            else
            {
                Root = [];
            }

            String FHRe = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            Root[FHRe] = new JsonObject()
            {
                ["Show"] = UserMsg,
                ["Code"] = Code,
                ["Message"] = Message,
                ["StackTrace"] = StackTrace
            };

            JsonObject Ord = [];

            foreach (var KV in Root.OrderByDescending(K => K.Key))
            {
                Ord[KV.Key] = KV.Value?.DeepClone();
            }

            JsonSerializerOptions Opts = new() { WriteIndented = true };
            File.WriteAllTextAsync(NFile, Ord.ToJsonString(Opts));
        }
    }

    // Registrar errores en operaciones de bases de datos
    public static async Task<(Boolean Done, String Mess)> SetError(Exception ex, String UserMsg)
    {
        await SaveLogs(UserMsg, ex.HResult, ex.Message, ex.StackTrace ?? String.Empty);

        return (false, UserMsg);
    }
    #endregion

    #region Archivos varios
    // Leer archivos de texto de tipo: Recuros incrustado, y retornar su contenido cómo variable String
    public static async Task<String> TextFile(String NFile, Assembly? MyAssembly = null)
    {
        if (String.IsNullOrWhiteSpace(NFile)) return String.Empty;

        Assembly Asse = MyAssembly ?? Assembly.GetExecutingAssembly();

        String? NF = Asse.GetManifestResourceNames().
            FirstOrDefault(N => N.EndsWith($".{NFile}", StringComparison.OrdinalIgnoreCase));

        if (NF is null) return String.Empty;

        using Stream? S = Asse.GetManifestResourceStream(NF);
        if (S is null) return String.Empty;
        using StreamReader SR = new(S, Encoding.UTF8);
        return await SR.ReadToEndAsync();
    }
    #endregion
}
