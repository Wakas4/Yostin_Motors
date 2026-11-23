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
        => Ok(await _db.Clientes.AsNoTracking().Where(x => x.Activo).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cliente>> GetPorId(int id)
    {
        var c = await _db.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdCliente == id && x.Activo);
        return c is null ? NotFound() : Ok(c);
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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, Cliente datos)
    {
        var c = await _db.Clientes.FirstOrDefaultAsync(x => x.IdCliente == id && x.Activo);
        if (c is null) return NotFound(new { mensaje = "Cliente no encontrado o inactivo." });

        c.Nombre = datos.Nombre;
        c.Apellido = datos.Apellido;
        c.Correo = datos.Correo;
        c.Telefono = datos.Telefono;

        await _db.SaveChangesAsync();
        return Ok(c);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Clientes.FirstOrDefaultAsync(x => x.IdCliente == id && x.Activo);
        if (c is null) return NotFound(new { mensaje = "Cliente no encontrado o ya inactivo." });

        c.Activo = false;   // soft delete: la fila se conserva
        await _db.SaveChangesAsync();
        return NoContent();
    }
}