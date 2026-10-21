using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using EnergiaR.DTO;
using EnergiaR.ORM.Inte;
using EnergiaR.Fls;

namespace EnergiaR.ORM;

// 
public class Recar(MyContexts _Contexts) : IRecar
{
    private readonly MyContexts Contexts = _Contexts;
}
