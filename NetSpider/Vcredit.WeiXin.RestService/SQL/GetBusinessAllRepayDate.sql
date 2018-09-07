SELECT BusinessID,RelativeDate,LoanPeriod,LoanTime 
FROM dbo.Business WITH (NOLOCK) 
WHERE BusinessID IN ({0}) AND LOANKIND = 'LOANKIND/KAKADAI'

