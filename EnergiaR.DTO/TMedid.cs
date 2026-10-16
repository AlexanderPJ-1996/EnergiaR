using System.ComponentModel.DataAnnotations;

namespace EnergiaR.DTO;

// Medidores
public class TMedid
{
    // ID de registro
    public Int64 ID { get; set; }
    // NIC de medidor
    [Required]
    [MaxLength(125, ErrorMessage = "Max. 125 caracteres")]
    public String MNIC { get; set; } = null!;
    // ID de medidor
    [Required]
    [MaxLength(125, ErrorMessage = "Max. 125 caracteres")]
    public String Medi { get; set; } = null!;
    // Dirección
    [Required]
    [MaxLength(2500, ErrorMessage = "Max. 2500 caracteres")]
    public String Dire { get; set; } = null!;
    // Caso de uso: ID
    [Required]
    public Int64 CUso { get; set; }
    // Clientes/Propietarios: ID
    [Required]
    public Int64 Prop { get; set; }

    public virtual TCaUso CaUsoNav { get; set; } = null!;

    public virtual TClien ClienNav { get; set; } = null!;

    public virtual ICollection<TRecar> Recar { get; set; } = [];
}
