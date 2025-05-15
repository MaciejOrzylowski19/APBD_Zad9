using System.Data;
using APBD_Zaj9.Models;
using Microsoft.Data.SqlClient;

namespace APBD_Zaj9.Services;

public class ProductServiceAlternative : IProductService
{
    
    private readonly IConfiguration _configuration;
    private string command = "EXEC AddProductToWarehouse @IdProduct, @IdWarehouse, @Amount, @CreatedAt";
    
    public async Task<int> addProduct(ProductDTO product)
    {
        
        int result = -1;
        
        using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            
            SqlCommand cmd = new SqlCommand(command, connection);

            cmd.CommandType = CommandType.StoredProcedure;
            
            cmd.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            cmd.Parameters.AddWithValue("@IdWarehouse", product.IdWarehouse);
            cmd.Parameters.AddWithValue("@Amount", product.Amount);
            cmd.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);

            try
            {
                result = (int) await cmd.ExecuteScalarAsync();
            }
            catch (SqlException ex)
            {
                return -1;
            }

        }

        return result;
    }
}