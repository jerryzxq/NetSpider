INSERT INTO dbo.CloanApplyItem
        ( CloanApplyID ,
          Subject ,
          IsAnnul ,
          Amount ,
          CreateTime 
        )
VALUES  ( @CloanApplyId , -- CloanApplyID - int
          {0} , -- Subject - tinyint
          0 , -- IsAnnul - bit
          {1} , -- Amount - decimal
          GETDATE()  -- CreateTime - datetime
        )
