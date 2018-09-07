SELECT  BillItemID ,
        BillID ,
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
        PenaltyIntAmt 
FROM dbo.BillItem WITH(NOLOCK)
WHERE BillID IN({0})