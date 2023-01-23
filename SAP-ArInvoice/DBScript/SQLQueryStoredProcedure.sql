
CREATE PROCEDURE SP_AR_Invoice @Date nvarchar(20)

AS

SELECT * FROM tblArInvoice

WHERE OrderDate = @Date


CREATE PROCEDURE [dbo].[GetCustomer] @CardCode nvarchar(50)
	
AS
	SELECT * FROM tblCustomer

WHERE CardCode = @CardCode

CREATE PROCEDURE GetItems @ItemCode nvarchar(50)
	
AS
	SELECT * FROM [tblItem
]

WHERE ItemCode= @ItemCode