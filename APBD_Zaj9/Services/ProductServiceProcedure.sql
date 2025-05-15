CREATE PROCEDURE AddProductToWarehouse @IdProduct INT, @IdWarehouse INT, @Amount INT,
                                       @CreatedAt DATETIME
AS
BEGIN

    DECLARE @IdProductFromDb INT, @IdOrder INT, @Price DECIMAL(5,2), @isInRealization INT;

    IF NOT EXISTS (Select 1 From Product Where Product.IdProduct = @IdProduct)
        BEGIN
            RAISERROR('Invalid parameter: Provided IdProduct does not exist', 18, 0);
            RETURN;
        END;

    IF NOT EXISTS(SELECT 1 FROM Warehouse WHERE IdWarehouse=@IdWarehouse)
        BEGIN
            RAISERROR('Invalid parameter: Provided IdWarehouse does not exist', 18, 0);
            RETURN;
        END;

    If @Amount <= 0
        BEGIN
            RAISERROR('Invalid parameter: Amount should be greater than 0', 18, 0);
            RETURN;
        END;


    SELECT TOP 1 @IdOrder = o.IdOrder FROM "Order" as o
    WHERE o.IdProduct=@IdProduct AND o.Amount=@Amount AND o.CreatedAt < @CreatedAt;

    SELECT @isInRealization = 1 From Product_Warehouse Where
        Exists (Select 1 From Product_Warehouse WHERE Product_Warehouse.IdOrder = @IdOrder);

    IF @IdOrder IS NULL OR @isInRealization Is Not Null
        BEGIN
            RAISERROR('Invalid parameter: There is no order to fullfill', 18, 0);
            RETURN;
        END;

    SELECT @Price=Product.Price FROM Product WHERE IdProduct=@IdProduct

    SET XACT_ABORT ON;
    BEGIN TRAN;

    UPDATE "Order" SET
        FulfilledAt=@CreatedAt
    WHERE IdOrder=@IdOrder;

    INSERT INTO Product_Warehouse(IdWarehouse,
                                  IdProduct, IdOrder, Amount, Price, CreatedAt)
    VALUES(@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Amount*@Price, @CreatedAt);

    SELECT @@IDENTITY AS NewId;

    COMMIT;
END