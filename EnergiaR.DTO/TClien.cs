using System.ComponentModel.DataAnnotations;

namespace EnergiaR.DTO;

// Medidores: Clientes/Propietarios
public class TClien
{
    // ID de registro
    public Int64 ID { get; set; }
    // Tipo de documento
    [Required]
    [MaxLength(20, ErrorMessage = "Max. 20 caracteres")]
    public String TDoc { get; set; } = null!;
    // No. de documento
    [Required]
    [MaxLength(100, ErrorMessage = "Max. 100 caracteres")]
    public String NDoc { get; set; } = null!;
    // Nombre completo
    [Required]
    [MaxLength(100, ErrorMessage = "Max. 100 caracteres")]
    public String Nomb { get; set; } = null!;
    // Sexo
    [Required]
    public Boolean Sexo { get; set; }

    public virtual ICollection<TMedid> Medid { get; set; } = [];
}
