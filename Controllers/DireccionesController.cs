using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMotors.Data;
using ProyectoMotors.Filtros;
using ProyectoMotors.Modelos;

namespace ProyectoMotors.Controllers;

[ApiController]
[Route("api/direcciones")]
[SesionRequerida]
public class DireccionesController : ControllerBase
{
    private readonly AppDbContext _db;
    public DireccionesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Direccion>>> GetLista()
        => Ok(await _db.Direcciones.AsNoTracking().Where(x => x.Activo).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Direccion>> GetPorId(int id)
    {
        var c = await _db.Direcciones.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdDireccion == id && x.Activo);
        return c is null ? NotFound() : Ok(c);
    }

    [HttpPost]
    public async Task<ActionResult<Direccion>> Post(Direccion direccion)
    {
        if (!await _db.Clientes.AnyAsync(c => c.IdCliente == direccion.IdCliente))
            return BadRequest(new { mensaje = "El cliente indicado no existe." });

        direccion.IdDireccion = 0;
        direccion.Activo = true;

        _db.Direcciones.Add(direccion);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPorId), new { id = direccion.IdDireccion }, direccion);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, Direccion datos)
    {
        var d = await _db.Direcciones.FirstOrDefaultAsync(x => x.IdDireccion == id && x.Activo);
        if (d is null) return NotFound(new { mensaje = "Dirección no encontrada o inactiva." });

        if (!await _db.Clientes.AnyAsync(c => c.IdCliente == datos.IdCliente && c.Activo))
            return BadRequest(new { mensaje = "El cliente indicado no existe o está inactivo." });

        d.IdCliente = datos.IdCliente;
        d.Departamento = datos.Departamento;
        d.Ciudad = datos.Ciudad;
        d.Zona = datos.Zona;
        d.Calle = datos.Calle;
        d.Numero = datos.Numero;
        d.Referencia = datos.Referencia;

        await _db.SaveChangesAsync();
        return Ok(d);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Direcciones.FirstOrDefaultAsync(x => x.IdDireccion == id && x.Activo);
        if (c is null) return NotFound(new { mensaje = "Direccion no encontrada o ya inactiva." });

        c.Activo = false;   // soft delete: la fila se conserva
        await _db.SaveChangesAsync();
        return NoContent();
    }
}