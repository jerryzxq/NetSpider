INSERT dbo.Bill
        ( BusinessID ,
          CustomerID ,
          BillType ,
          BillStatus ,
          BillMonth ,
          CompanyKey ,
          BeginTime ,
          EndTime ,
          LimitTime ,
          CreateTime ,
          OperatorID ,
          IsCurrent ,
          FullPaidTime ,
          IsShelve ,
          DeductionID ,
          IsFixed ,
          DueDate 
        )
VALUES  ( {0} , -- BusinessID - int
          {1} , -- CustomerID - int
          3 , -- BillType - tinyint
          1 , -- BillStatus - tinyint
          '{2}' , -- BillMonth - varchar(7)
          '{3}' , -- CompanyKey - nvarchar(100)
          '{4}' , -- BeginTime - datetime
          '{5}' , -- EndTime - datetime
          '{5}' , -- LimitTime - datetime
          GETDATE() , -- CreateTime - datetime
          0 , -- OperatorID - int
          0 , -- IsCurrent - bit
          NULL , -- FullPaidTime - datetime
          0 , -- IsShelve - bit
          0 , -- DeductionID - int
          0 , -- IsFixed - int
          NULL  -- DueDate - datetime
        )
DECLARE @BillID BIGINT
SELECT @BillID=@@IDENTITY
