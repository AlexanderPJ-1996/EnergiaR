using System;
using System.Collections.Generic;
using System.Text;

namespace EnergiaR.DTO;

// Medidores: Clientes/Propietarios | Valores inferidos
public class VClien
{
    // ID de registro
    public Int64 ID { get; set; }
    // Tipo de documento
    public String TDoc { get; set; } = null!;
    // No. de documento
    public String NDoc { get; set; } = null!;
    // Nombre completo
    public String Nomb { get; set; } = null!;
    // Sexo
    public String Sexo { get; set; } = null!;
}
