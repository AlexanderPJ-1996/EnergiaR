namespace EnergiaR.DTO;

// Medidores | Valores inferidos
public class VMedid
{
    // ID de registro
    public Int64 ID { get; set; }
    // NIC de medidor
    public String MNIC { get; set; } = null!;
    // ID de medidor
    public String Medi { get; set; } = null!;
    // Dirección
    public String Dire { get; set; } = null!;
    // Caso de uso
    public String CUso { get; set; } = null!;
    // Clientes/Propietarios: No. de documento
    public String Docu { get; set; } = null!;
    // Clientes/Propietarios: Nombre completo
    public String Nomb { get; set; } = null!;

}
