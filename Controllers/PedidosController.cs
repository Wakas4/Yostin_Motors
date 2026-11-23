using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMotors.Data;
using ProyectoMotors.Filtros;
using ProyectoMotors.Modelos;

namespace ProyectoMotors.Controllers;

[ApiController]
[Route("api/pedidos")]
[SesionRequerida]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _db;
    public PedidosController(AppDbContext db) => _db = db;

        [HttpGet]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetLista()
        => Ok(await _db.Pedidos.AsNoTracking().Where(x => x.Activo).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Pedido>> GetPorId(int id)
    {
        var c = await _db.Pedidos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdPedido == id && x.Activo);
        return c is null ? NotFound() : Ok(c);
    }

    [HttpPost]
    public async Task<ActionResult<Pedido>> Post(Pedido pedido)
    {
        if (!await _db.Clientes.AnyAsync(c => c.IdCliente == pedido.IdCliente))
            return BadRequest(new { mensaje = "El cliente indicado no existe." });

        if (!await _db.Direcciones.AnyAsync(d => d.IdDireccion == pedido.IdDireccion))
            return BadRequest(new { mensaje = "La dirección indicada no existe." });

        pedido.IdPedido = 0;
        pedido.FechaPedido = DateTime.UtcNow;
        pedido.Activo = true;

        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPorId), new { id = pedido.IdPedido }, pedido);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, Pedido datos)
    {
        var p = await _db.Pedidos.FirstOrDefaultAsync(x => x.IdPedido == id && x.Activo);
        if (p is null) return NotFound(new { mensaje = "Pedido no encontrado o inactivo." });

        if (!await _db.Clientes.AnyAsync(c => c.IdCliente == datos.IdCliente && c.Activo))
            return BadRequest(new { mensaje = "El cliente indicado no existe o está inactivo." });

        if (!await _db.Direcciones.AnyAsync(d => d.IdDireccion == datos.IdDireccion && d.Activo))
            return BadRequest(new { mensaje = "La dirección indicada no existe o está inactiva." });

        p.IdCliente = datos.IdCliente;
        p.IdDireccion = datos.IdDireccion;
        p.Estado = datos.Estado;
        p.Total = datos.Total;

        await _db.SaveChangesAsync();
        return Ok(p);
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Pedidos.FirstOrDefaultAsync(x => x.IdPedido == id && x.Activo);
        if (c is null) return NotFound(new { mensaje = "Pedido no encontrado o ya inactivo." });

        c.Activo = false;   // soft delete: la fila se conserva
        await _db.SaveChangesAsync();
        return NoContent();
    }
}