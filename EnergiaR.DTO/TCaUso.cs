using System.ComponentModel.DataAnnotations;

namespace EnergiaR.DTO;

// Medidores: Casos de uso
public class TCaUso
{
    // ID de registro
    public Int64 ID { get; set; }
    // Caso de uso
    [Required]
    [MaxLength(20, ErrorMessage = "Max. 20 caracteres")]
    public String CUso { get; set; } = null!;
    // Descripción
    [Required]
    [MaxLength(100, ErrorMessage = "Max. 100 caracteres")]
    public String CDes { get; set; } = null!;

    public virtual ICollection<TMedid> Medid { get; set; } = [];
}
