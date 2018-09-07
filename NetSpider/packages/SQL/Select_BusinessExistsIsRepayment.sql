SELECT TOP 1 BusinessID
FROM dbo.Business
WHERE IsRepayment=1 AND
Operable IN (0,1) AND
BusinessID IN
({0})