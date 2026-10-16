using System.ComponentModel.DataAnnotations;

namespace EnergiaR.DTO;

public class TRecar
{
    // ID de registro
    public Int64 ID { get; set; }
    // Empresa: ID
    [Required]
    public Int64 Empr { get; set; }
    // Fecha de recarga
    [Required]
    public DateOnly FRec { get; set; }
    // Hora de recarga
    [Required]
    public TimeOnly HRec { get; set; }
    // Código de seguridad
    [MaxLength(100, ErrorMessage = "Max. 100 caracteres")]
    public String? CSeg { get; set; }
    // ID de transacción
    [Required]
    [MaxLength(50, ErrorMessage = "Max. 50 caracteres")]
    public String ITra { get; set; } = null!;
    // Código de aprobación
    [Required]
    [MaxLength(100, ErrorMessage = "Max. 100 caracteres")]
    public String Apro { get; set; } = null!;
    // Medidor: ID
    [Required]
    public Int64 Medi { get; set; }
    // PIN de recarga
    [Required]
    [MaxLength(100, ErrorMessage = "Max. 100 caracteres")]
    public String PINR { get; set; } = null!;
    // Valor de recarga
    [Required]
    [Range(0, 999999999999999, ErrorMessage = "El valor debe estar entre 0.00 y 9999999999999.99")]
    public Int64 VRec { get; set; }
    // Abono a la deuda
    [Required]
    [Range(0, 10000, ErrorMessage = "El valor debe estar entre 0.00 y 100.00")]
    public Int64 Abon { get; set; }
    // Valor subsidio
    [Required]
    [Range(0, 999999999999999, ErrorMessage = "El valor debe estar entre 0.00 y 9999999999999.99")]
    public Int64 VSub { get; set; }
    // Valor KWH
    [Required]
    [Range(0, 999999999, ErrorMessage = "El valor debe estar entre 0.00 y 9999999.99")]
    public Int64 VKWH { get; set; }
    // KWH comprados
    [Required]
    [Range(0, 99999999999, ErrorMessage = "El valor debe estar entre 0.00 y 999999999.99")]
    public Int64 Comp { get; set; }
    // Acumulado mes: KWH
    [Required]
    [Range(0, 99999999999, ErrorMessage = "El valor debe estar entre 0.00 y 999999999.99")]
    public Int64 MesK { get; set; }
    // Acumulado mes: Saldo
    [Required]
    [Range(0, 999999999999999, ErrorMessage = "El valor debe estar entre 0.00 y 9999999999999.99")]
    public Int64 MesS { get; set; }

    public virtual TEmpre EmpreNav { get; set; } = null!;

    public virtual TMedid MedidNav { get; set; } = null!;
}
