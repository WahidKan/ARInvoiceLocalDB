CREATE TABLE [dbo].[tblArInvoice] (
    [CustName]     NVARCHAR (50) NOT NULL,
    [OrderCode]    NVARCHAR (50) NULL,
    [OrderDate]    NVARCHAR (50) NULL,
    [ItemCode]     NVARCHAR (50) NULL,
    [Quantity]     NVARCHAR (50) NULL,
    [WareHouse]    NVARCHAR (50) NULL,
    [CostCenter]   NVARCHAR (50) NULL,
    [BankDiscount] NVARCHAR (50) NULL,
    [TaxCode]      NVARCHAR (50) NULL,
    [TaxAmount]    NVARCHAR (50) NULL,
    [Section]      NVARCHAR (50) NULL,
    [UnitPrice]    NVARCHAR (50) NULL,
    [BankCode]     NVARCHAR (50) NULL
);

CREATE TABLE [dbo].[tblCustomer] (
    [CardCode] NVARCHAR (50) NOT NULL,
    [CustName] NVARCHAR (50) NULL,
    [Phone]    NVARCHAR (50) NULL
);


CREATE TABLE [dbo].[tblItem
] (
    [ItemCode]        NVARCHAR (50) NOT NULL,
    [ItemDescription] NVARCHAR (50) NULL,
    [Quantity]        NVARCHAR (50) NULL,
    [UnitPrice]       NVARCHAR (50) NULL
);