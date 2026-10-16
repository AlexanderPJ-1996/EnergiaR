namespace EnergiaR.DTO;

// Historíal que registra operaciones INSERT, UPDATE y DELETE
public class THistory
{
    // ID de registro
    public Int64 ID { get; set; }
    // Tabla donde se realizó la operación
    public String Tabl { get; set; } = null!;
    // Operación realizada
    public String Oper { get; set; } = null!;
    // Fecha y hora de operación
    public DateTime FHRe { get; set; }
    // Estado previo del registro
    public String? RPre { get; set; }
    // Nuevo estado del registro
    public String? RPos { get; set; }
}
