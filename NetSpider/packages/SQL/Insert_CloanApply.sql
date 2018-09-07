INSERT dbo.CloanApply
        ( BusinessID ,
          UnexpiredMonth ,
          OverMonth ,
          Path ,
          CloanApplyKind ,
          CloanApplyStatus ,
          ApplyerID ,
          CheckerID ,
          ApplyTime ,
          CheckTime 
        )
VALUES  ( {0} , -- BusinessID - int
          '{1}' , -- UnexpiredMonth - nvarchar(50)
          '{2}' , -- OverMonth - nvarchar(50)
          '' , -- Path - nvarchar(100)
          1 , -- CloanApplyKind - tinyint
          2 , -- CloanApplyStatus - tinyint
          0 , -- ApplyerID - int
          0 , -- CheckerID - int
          GETDATE() , -- ApplyTime - datetime
          GETDATE()  -- CheckTime - datetime
        )
DECLARE @CloanApplyId INT
SELECT @CloanApplyId=@@IDENTITY
