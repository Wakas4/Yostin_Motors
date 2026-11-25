using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMotors.Data;

namespace ProyectoMotors.Controllers;

/// Reporte: Inventario y catálogo de productos
/// Autor: Misael Choque (@Mixsat547)
/// Tablas: Productos, Categorias, Marcas, Inventario
/// Propósito: consultar productos, precios y disponibilidad de stock.
/// Acceso: solo el rol Administrador.
[ApiController]
[Route("api/reportes")]
[Authorize(Roles = "Administrador")]
public class ReporteInventarioController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReporteInventarioController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("inventario-productos")]
    public async Task<IActionResult> InventarioProductos()
    {
        var datos = await _db.Productos
            .AsNoTracking()
            .Where(p => p.Activo &&
                        p.Categoria != null && p.Categoria.Activo &&
                        p.Marca != null && p.Marca.Activo)
            .Select(p => new
            {
                p.IdProducto,
                Producto = p.Nombre,
                Categoria = p.Categoria!.Nombre,
                Marca = p.Marca!.Nombre,
                p.Precio,
                Stock = p.Inventario != null && p.Inventario.Activo
                    ? (int?)p.Inventario.Stock
                    : null,
                StockMinimo = p.Inventario != null && p.Inventario.Activo
                    ? (int?)p.Inventario.StockMinimo
                    : null,
                EstadoStock = p.Inventario == null || !p.Inventario.Activo
                    ? "SIN INVENTARIO"
                    : p.Inventario.Stock <= p.Inventario.StockMinimo
                        ? "STOCK BAJO"
                        : "NORMAL"
            })
            .OrderBy(p => p.Producto)
            .ToListAsync();

        return Ok(datos);
    }
}
