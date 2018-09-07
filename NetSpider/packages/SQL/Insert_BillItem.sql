INSERT INTO dbo.BillItem
        ( BillID ,
          Subject ,
          DueDate ,
          Amount ,
          DueAmt ,
          ReceivedAmt ,
          CreateTime ,
          FullPaidTime ,
          Overdue ,
          SubjectType ,
          OperatorID ,
          IsCurrent ,
          IsShelve ,
          BusinessID ,
          PenaltyIntAmt ,
          LastCalcPenaltyDate
        )
VALUES  ( @BillID , -- BillID - bigint
          {0} , -- Subject - tinyint
          NULL , -- DueDate - date
          {1} , -- Amount - decimal
          {1} , -- DueAmt - decimal
          0 , -- ReceivedAmt - decimal
          GETDATE() , -- CreateTime - datetime
          Null , -- FullPaidTime - datetime
          Null , -- Overdue - int
          1 , -- SubjectType - tinyint
          0 , -- OperatorID - int
          1 , -- IsCurrent - bit
          0 , -- IsShelve - bit
          {2} , -- BusinessID - int
          0 , -- PenaltyIntAmt - decimal
          NULL  -- LastCalcPenaltyDate - date
        )
