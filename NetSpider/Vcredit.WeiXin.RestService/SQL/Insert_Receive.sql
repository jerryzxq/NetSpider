INSERT INTO dbo.Received
        ( BillID ,
          BillItemID ,
          Amount ,
          ReceivedType ,
          PayID ,
          ReceivedTime ,
          CreateTime ,
          OperatorID ,
          Explain ,
          DeductionID ,
          ToAccountID ,
          ToAcountTime
        )
VALUES  ( {0} , -- BillID - bigint
          {1} , -- BillItemID - bigint
          {2} , -- Amount - decimal
          {3} , -- ReceivedType - tinyint
          0 , -- PayID - int
          '{4}' , -- ReceivedTime - datetime
          GETDATE() , -- CreateTime - datetime
          0 , -- OperatorID - int
          N'{5}' , -- Explain - nvarchar(100)
          0 , -- DeductionID - int
          0 , -- ToAccountID - int
          NULL  -- ToAcountTime - datetime
        )