UPDATE dbo.Business SET LoanPeriod={1},ProceduresAmout={3},
InterestRate={4},ServiceRate={5}
FROM dbo.Business b
WHERE b.FrozenNo='' AND 
b.BusinessID={0} AND
NOT EXISTS
(SELECT 1 
FROM dbo.Bill l WITH(NOLOCK)
WHERE b.BusinessID=l.BusinessID AND
l.BillType != 10)

IF(@@ROWCOUNT=1)
BEGIN
IF({2}=1)
	UPDATE dbo.BusinessExtend SET IsByStages={2},ExemptionDay=0
	WHERE BusinessID={0}
ELSE
	UPDATE dbo.BusinessExtend SET IsByStages={2}
	WHERE BusinessID={0}
END

