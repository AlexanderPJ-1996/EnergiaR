namespace EnergiaR.DTO;

// Recargas | Valores calculados e inferidos
public class VRecar
{
    // ID de registro
    public Int64 ID { get; set; }
    // Empresa: Razón social
    public String? RaSo { get; set; }
    // Empresa: NIT o no. de registro
    public String? NDoc { get; set; }
    // Fecha de recarga
    public DateOnly FRec { get; set; }
    // Hora de recarga
    public TimeOnly HRec { get; set; }
    // Código de seguridad
    public String? CSeg { get; set; }
    // ID de transacción
    public String ITra { get; set; } = null!;
    // Código de aprobación
    public String Apro { get; set; } = null!;
    // Medidor: ID
    public String Medi { get; set; } = null!;
    // Medidor: Dirección
    public String Dire { get; set; } = null!;
    // Medidor: Caso de uso
    public String CUso { get; set; } = null!;
    // Clientes/Propietarios: Tipo y no. de documento
    public String Docu { get; set; } = null!;
    // Clientes/Propietarios: Nombre completo
    public String Nomb { get; set; } = null!;
    // PIN de recarga
    public String PINR { get; set; } = null!;

    #region Mapeo INTEGER
    public Int64 IntVRec { get; set; }
    public Int64 IntAbon { get; set; }
    public Int64 IntVSub { get; set; }
    public Int64 IntVKWH { get; set; }
    public Int64 IntComp { get; set; }
    public Int64 IntMesK { get; set; }
    public Int64 IntMesS { get; set; }
    #endregion

    #region Propiedades calculadas (Decimal)
    // Valor de recarga
    public Decimal VRec => IntVRec / 100m;
    // Abono a la deuda
    public Decimal Abon => IntAbon / 100m;
    // Valor de abono a la deuda
    public Decimal VAbo => VRec * (IntAbon / 10000m);
    // Saldo de recarga
    public Decimal SRec => VRec - VAbo;
    // Valor subsidio
    public Decimal VSub => IntVSub / 100m;
    // Saldo neto
    public Decimal Neto => SRec + VSub;
    // Valor KWH
    public Decimal VKWH => IntVKWH / 100m;
    // KWH comprados
    public Decimal Comp => IntComp / 100m;
    // Acumulado mes: KWH
    public Decimal MesK => IntMesK / 100m;
    // Acumulado mes: Saldo
    public Decimal MesS => IntMesS / 100m;
    #endregion
}
