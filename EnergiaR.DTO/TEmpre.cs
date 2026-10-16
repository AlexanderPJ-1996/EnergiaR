using System.ComponentModel.DataAnnotations;

namespace EnergiaR.DTO;

// Empresas prestadoras de servicio de energía
public class TEmpre
{
    // ID de registro
    public Int64 ID { get; set; }
    // NIT o no. de registro
    [Required]
    [MaxLength(125, ErrorMessage = "Max. 125 caracteres")]
    public String NDoc { get; set; } = null!;
    // Razón social o nombre
    [Required]
    [MaxLength(250, ErrorMessage = "Max. 250 caracteres")]
    public String RaSo { get; set; } = null!;
    // Dirección
    [Required]
    [MaxLength(2500, ErrorMessage = "Max. 2500 caracteres")]
    public String Dire { get; set; } = null!;
    // No. de teléfonos
    [Required]
    [MaxLength(2500, ErrorMessage = "Max. 2500 caracteres")]
    public String Tele { get; set; } = null!;
    // Correo electrónico
    [Required]
    [MaxLength(2500, ErrorMessage = "Max. 2500 caracteres")]
    public String Mail { get; set; } = null!;

    public virtual ICollection<TRecar> Recar { get; set; } = [];
}
