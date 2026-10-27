using System;
using System.Text;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EnergiaR.Fls;

// Procedimientos con archivos y carpetas
public static class UseFiles
{
    #region Archivos y carpetas
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
    
    // Cargar carpeta de lectura de la aplicación
    public static String PathBase()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EnergiaR"); 
    }
    
    // Carpetas para la aplicación
    public static String[] PathApps { get; } =
    [
        "Empre", 
    ];
    
    // Crear carpetas
    public static async Task<(Boolean Done, String Mess)> MakePath()
    {
        try
        {
            if (!Directory.Exists(PathBase())) Directory.CreateDirectory(PathBase());
            
            foreach (String P in PathApps)
            {
                String F = Path.Combine(PathBase(), P);
                if (!Directory.Exists(F)) Directory.CreateDirectory(F);
            }
            
            return (true, String.Empty);
        }
        catch (ArgumentNullException ex)
        {
            return await SetError(ex, "Ruta de carpeta inválida: no es posible crear el directorio");
        }
        catch (ArgumentException ex)
        {
            return await SetError(ex, "Error: ruta con caracteres inválidos o vacíos");
        }
        catch (PathTooLongException ex)
        {
            return await SetError(ex, "La ruta es demasiado larga para el sistema");
        }
        catch (DirectoryNotFoundException ex)
        {
            return await SetError(ex, "No se encontró una parte de la ruta base de la aplicación");
        }
        catch (NotSupportedException ex)
        {
            return await SetError(ex, "La ruta contiene un formato no soportado por el sistema de archivos");
        }
        catch (UnauthorizedAccessException ex)
        {
            return await SetError(ex, "La aplicación no tiene permisos para crear las carpetas necesarias");
        }
        catch (IOException ex)
        {
            return await SetError(ex, "Error de E/S: existe un archivo con el mismo nombre o la carpeta está en uso");
        }
        catch (Exception ex)
        {
            return await SetError(ex, "No fue posible crear o acceder a las carpetas necesarias para la aplicación");
        }
    }
    #endregion
    
    #region Registrar Exception en archivo .json
    private static readonly Lock LogL = new();

    static async Task SaveLogs(String UserMsg, Int32 Code, String Message, String StackTrace)
    {
        String NFile = Path.Combine(PathBase(), "What's Happened.json");
        
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

    // Registro de errores que retorna Boolean|String
    public static async Task<(Boolean Done, String Mess)> SetError(Exception ex, String UserMsg)
    {
        await SaveLogs(UserMsg, ex.HResult, ex.Message, ex.StackTrace ?? String.Empty);

        return (false, UserMsg);
    }

    // Registro de errores que no retorna valores
    public static async Task SetLog(Exception ex, String UserMsg)
        => await SaveLogs(UserMsg, ex.HResult, ex.Message, ex.StackTrace ?? String.Empty);
    #endregion
}
