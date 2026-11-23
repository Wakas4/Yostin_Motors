using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMotors.Data;
using ProyectoMotors.Filtros;
using ProyectoMotors.Modelos;

namespace ProyectoMotors.Controllers;

[ApiController]
[Route("api/clientes")]
[SesionRequerida]
public class ClientesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ClientesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cliente>>> GetLista()
        => Ok(await _db.Clientes.AsNoTracking().ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cliente>> GetPorId(int id)
    {
        var c = await _db.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.IdCliente == id);
        return c is null ? NotFound() : Ok(c);
    }

        // PR DE PRÁCTICA: mismo método HTTP (POST) y misma ruta (api/clientes) que Post()
    [HttpPost]
    public async Task<ActionResult<Cliente>> CrearCliente(Cliente cliente)
    {
        cliente.IdCliente = 0;
        cliente.FechaRegistro = DateTime.UtcNow;
        cliente.Activo = true;

        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPorId), new { id = cliente.IdCliente }, cliente);
    }

    [HttpPost]
    public async Task<ActionResult<Cliente>> Post(Cliente cliente)
    {
        cliente.IdCliente = 0;
        cliente.FechaRegistro = DateTime.UtcNow;
        cliente.Activo = true;

        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPorId), new { id = cliente.IdCliente }, cliente);
    }
}