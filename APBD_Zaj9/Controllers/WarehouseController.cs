using APBD_Zaj9.Services;
using Microsoft.AspNetCore.Mvc;
using APBD_Zaj9.Models;

namespace APBD_Zaj9.Controllers;

[ApiController]
[Route("api/warehouse")]
public class WarehouseController : ControllerBase
{
    private IProductService _productService { get; set; }
    
    
    public WarehouseController(IProductService productService)
    {
        _productService = productService;
    }
    
    [HttpPut]
    public async Task<IActionResult> AddProduct([FromBody] ProductDTO product)
    {
        
        var result = await _productService.addProduct(product);

        switch (result)
        {
            case -1:
                return BadRequest("Invalid amount.");
            case -2:
                return BadRequest("Product does not exist.");
            case -3:
                return BadRequest("Warehouse does not exist.");
            case -4:
                return BadRequest("Invalid date.");
            case -5:
                return BadRequest("The amount exceeds available stock.");
            case -6:
                return BadRequest("The order does not exist.");
        }
        return Ok(result);
    }
    
}