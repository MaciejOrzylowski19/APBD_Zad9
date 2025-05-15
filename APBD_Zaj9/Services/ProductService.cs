namespace APBD_Zaj9.Services;
using APBD_Zaj9.Models;
using Microsoft.Data.SqlClient;

public class ProductService : IProductService
{
    
    private SqlConnection _connection;
    private readonly IConfiguration _configuration;
    
    //Commands:
    
    private readonly string _productExists = "SELECT 1 FROM Product WHERE IdProduct = @IdProduct";
    private readonly string _warehouseExists = "SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
    private readonly string _productAmount = "SELECT Count(1) FROM Product WHERE IdProduct = @IdProduct";
    private readonly string _ordedExists = "SELECT IdOrder FROM \"Order\" WHERE IdOrder = @IdOrder AND Amount = @Amount";
    private readonly string _updateRealizationDate = "UPDATE \"Order\" Set FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder";
    private readonly string _insertIntoProductWarehouse = @"INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, CreatedAt, Product_Warehouse.Price) 
        VALUES (@IdWarehouse, @IdProduct, @IdOrder,@Amount , @CreatedAt, @Price); Select SCOPE_IDENTITY() as last";

    private readonly string _getPrice = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
    
    private int _idOrder;
    
    public async Task<int> addProduct(ProductDTO product)
    {

        using (_connection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            
            await _connection.OpenAsync();
            if (product.Amount <= 0)
            {
                // Invalid amount
                return -1;
            }
            
            if (!await IsProductPresent(product.IdProduct))
            {
                // Product does not exist
                return -2;
                
            }
            if (!await IsWarehousePresent(product.IdWarehouse))
            {
                // Warehouse does not exist
                return -3;
            }

            if (product.CreatedAt > DateTime.Now)
            {
                // Invalid date
                return -4;
            }
            
            
            if ( (_idOrder = await ExistsSamePreviousOrder(product.IdProduct, product.Amount)) == 0) {
                // Product already exists
                return -6;
            }

            Decimal _price;
            using (SqlCommand getPriceCommand = new SqlCommand(_getPrice, _connection))
            {
                getPriceCommand.Parameters.AddWithValue("@IdProduct", product.IdProduct);
                _price = (Decimal) await getPriceCommand.ExecuteScalarAsync();
            }

            using (var transaction = _connection.BeginTransaction())
            {


                using (SqlCommand updateCommand = new SqlCommand(_updateRealizationDate, _connection, transaction))
                using (SqlCommand insertCommand = new SqlCommand(_insertIntoProductWarehouse, _connection, transaction))

                {

                    try
                    {
                        updateCommand.Parameters.AddWithValue("@IdOrder", _idOrder);
                        updateCommand.Parameters.AddWithValue("@FulfilledAt", DateTime.Now);
                        await updateCommand.ExecuteNonQueryAsync();

                        insertCommand.Parameters.AddWithValue("@IdWarehouse", product.IdWarehouse);
                        insertCommand.Parameters.AddWithValue("@IdProduct", product.IdProduct);
                        insertCommand.Parameters.AddWithValue("@IdOrder", _idOrder);
                        insertCommand.Parameters.AddWithValue("@Amount", product.Amount);
                        insertCommand.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);
                        insertCommand.Parameters.AddWithValue("@Price", _price * product.Amount);

                        var res = await insertCommand.ExecuteScalarAsync();
                        int target = Convert.ToInt16(res);    
                        
                        await transaction.CommitAsync();
                        return target;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception("", ex);
                    }
                }
            }
        }
    }

    public ProductService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    private async Task<bool> IsProductPresent(int idProduct)
    {
        using (SqlCommand command = new SqlCommand(_productExists, _connection))
        {
            command.Parameters.AddWithValue("@IdProduct", idProduct);
            var result = await command.ExecuteScalarAsync();
            return result != null;
        }
    }
    
    private async Task<bool> IsWarehousePresent(int idWarehouse)
    {
        using (SqlCommand command = new SqlCommand(_warehouseExists, _connection))
        {
            command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
            var result = await command.ExecuteScalarAsync();
            return result != null;
        }
    }
    
    
    private async Task<int> ExistsSamePreviousOrder(int idOrder, int amount)
    {
        using (SqlCommand command = new SqlCommand(_ordedExists, _connection))
        {
            command.Parameters.AddWithValue("@Amount", amount);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            var result = await command.ExecuteScalarAsync();
            return result == null ? 0 : (int)result;
        }
    }
    
}