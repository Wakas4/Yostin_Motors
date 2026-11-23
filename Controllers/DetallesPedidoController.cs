using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMotors.Data;
using ProyectoMotors.Filtros;
using ProyectoMotors.Modelos;

namespace ProyectoMotors.Controllers;

[ApiController]
[Route("api/detalles-pedido")]
[SesionRequerida]
public class DetallesPedidoController : ControllerBase
{
    private readonly AppDbContext _db;
    public DetallesPedidoController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DetallePedido>>> GetLista()
        => Ok(await _db.DetallesPedido.AsNoTracking().Where(x => x.Activo).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DetallePedido>> GetPorId(int id)
    {
        var c = await _db.DetallesPedido.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdDetalle == id && x.Activo);
        return c is null ? NotFound() : Ok(c);
    }

    [HttpPost]
    public async Task<ActionResult<DetallePedido>> Post(DetallePedido detalle)
    {
        if (!await _db.Pedidos.AnyAsync(p => p.IdPedido == detalle.IdPedido))
            return BadRequest(new { mensaje = "El pedido indicado no existe." });

        if (detalle.Cantidad <= 0)
            return BadRequest(new { mensaje = "La cantidad debe ser mayor que cero." });

        detalle.IdDetalle = 0;
        detalle.Subtotal = detalle.Cantidad * detalle.PrecioUnitario;
        detalle.Activo = true;

        _db.DetallesPedido.Add(detalle);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPorId), new { id = detalle.IdDetalle }, detalle);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, DetallePedido datos)
    {
        var d = await _db.DetallesPedido.FirstOrDefaultAsync(x => x.IdDetalle == id && x.Activo);
        if (d is null) return NotFound(new { mensaje = "Detalle no encontrado o inactivo." });

        if (!await _db.Pedidos.AnyAsync(p => p.IdPedido == datos.IdPedido && p.Activo))
            return BadRequest(new { mensaje = "El pedido indicado no existe o está inactivo." });

        if (datos.Cantidad <= 0)
            return BadRequest(new { mensaje = "La cantidad debe ser mayor que cero." });

        d.IdPedido = datos.IdPedido;
        d.IdProducto = datos.IdProducto;
        d.Cantidad = datos.Cantidad;
        d.PrecioUnitario = datos.PrecioUnitario;
        d.Subtotal = datos.Cantidad * datos.PrecioUnitario;

        await _db.SaveChangesAsync();
        return Ok(d);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.DetallesPedido.FirstOrDefaultAsync(x => x.IdDetalle == id && x.Activo);
        if (c is null) return NotFound(new { mensaje = "Detalle no encontrado o ya inactivo." });

        c.Activo = false;   // soft delete: la fila se conserva
        await _db.SaveChangesAsync();
        return NoContent();
    }
}