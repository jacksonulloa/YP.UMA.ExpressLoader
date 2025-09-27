CREATE OR ALTER PROCEDURE [Transac].[Usp_Insertar_Pago]  
@p_tipo_validacion char(1),  
@p_fecha_hora_transaccion datetime,  
@p_id_canal_pago varchar(2),  
@p_id_forma_pago varchar(2),  
@p_numero_operacion varchar(12),  
@p_id_consulta varchar(16),  
@p_servicio varchar(5),  
@p_numero_documento varchar(16),  
@p_importe_pagado numeric(12, 2),  
@p_moneda char(1),  
@p_id_empresa varchar(3),  
@p_tipo_transac char(1),  
@p_id_banco varchar(4),  
@p_voucher varchar(50),  
@p_cuenta_banco varchar(50),  
@p_id_deuda bigint,  
@p_estado_notificacion char(1),  
@p_id_transaccion bigint output,  
@p_nombre_cliente varchar(30) output  
AS  
BEGIN  
    SET NOCOUNT ON;  
    BEGIN TRY  
        BEGIN TRANSACTION;  
  /**** se le quito id = @iddeuda y Order by D.id****/  
  SET @p_nombre_cliente = (SELECT TOP 1 nombre_cliente  
            FROM [Process].[Tbl_Deuda] D  
            WHERE id_empresa = @p_id_empresa  
              AND servicio = @p_servicio  
              AND numero_documento = @p_numero_documento  
              AND saldo >= @p_importe_pagado)  
        -- Validar saldo antes de descontar  
        IF @p_nombre_cliente IS NULL  
        BEGIN  
            RAISERROR('Saldo insuficiente o deuda no encontrada.', 16, 1);  
            ROLLBACK TRANSACTION;  
            RETURN;  
        END  
        -- Insertar transacción  
        INSERT INTO [Transac].[Tbl_Transaccion] (  
            fecha_hora_transaccion,  
            id_canal_pago,  
            id_forma_pago,  
            numero_operacion,  
            id_consulta,  
            servicio,  
            numero_documento,  
            importe_pagado,  
            moneda,  
            id_empresa,  
            tipo_transac,  
            id_banco,  
            voucher,  
   cuenta_banco,  
   id_deuda_ref,  
   nombre_cliente,  
   estado_notificacion  
        )  
        VALUES (  
            @p_fecha_hora_transaccion,  
            @p_id_canal_pago,  
            @p_id_forma_pago,  
            @p_numero_operacion,  
            @p_id_consulta,  
            @p_servicio,  
            @p_numero_documento,  
            @p_importe_pagado,  
            @p_moneda,  
            @p_id_empresa,  
            @p_tipo_transac,  
            @p_id_banco,  
            @p_voucher,  
   @p_cuenta_banco,  
   @p_id_deuda,  
   @p_nombre_cliente,  
   @p_estado_notificacion  
        );  
        -- Obtener ID generado  
        SET @p_id_transaccion = SCOPE_IDENTITY();  
        -- Actualizar deuda si es de validacion completa
		IF @p_tipo_validacion <> 'P'
			BEGIN
				UPDATE [Process].[Tbl_Deuda]  
				SET saldo = saldo - @p_importe_pagado,  
					estado = CASE WHEN saldo - @p_importe_pagado = 0 THEN 'C' ELSE estado END  
				WHERE id_empresa = @p_id_empresa  
					AND servicio = @p_servicio  
					AND numero_documento = @p_numero_documento  
		END
        COMMIT TRANSACTION;  
    END TRY  
    BEGIN CATCH  
        IF @@TRANCOUNT > 0  
            ROLLBACK TRANSACTION;  
  
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();  
        RAISERROR(@ErrorMessage, 16, 1);  
    END CATCH  
END  

GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns 
    WHERE Name = N'numero_cuenta'
      AND Object_ID = Object_ID(N'Profile.Tbl_Servicio')
)
BEGIN
    ALTER TABLE [Profile].[Tbl_Servicio]
    ADD [numero_cuenta] VARCHAR(50) NULL;
END

GO

ALTER   PROCEDURE [Profile].[Usp_Listar_Empresas_Por_Empresa_Estado]
@id_empresa int,
@emp_estado int
AS
SET NOCOUNT ON;
SELECT E.id as id_empresa, E.id_proveedor, E.nombre as nombre_empresa, E.ruc, E.estado as estado_empresa, 
	S.id as id_servicio, S.codigo, S.moneda, S.nombre as nombre_servicio, S.tipo_validacion, S.tipo_pago, S.estado as estado_servicio, S.numero_cuenta as numero_cuenta
FROM [Profile].Tbl_Empresa E
INNER JOIN [Profile].Tbl_Servicio S
ON E.id = S.id_empresa
WHERE (@id_empresa = -1 OR E.id = @id_empresa)
AND (@emp_estado = -1 OR E.estado = @emp_estado)