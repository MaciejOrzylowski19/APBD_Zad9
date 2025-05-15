namespace APBD_Zaj9.Services;
using APBD_Zaj9.Models;

public interface IProductService
{
    
    
    public Task<int> addProduct(ProductDTO product);
    
    
}