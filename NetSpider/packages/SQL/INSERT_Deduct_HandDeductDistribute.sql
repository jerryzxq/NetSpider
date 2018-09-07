if exists(select top 1 * from Business where Businessid={1} and FrozenNo='')
begin
	INSERT INTO deduct.HandDeductDistribute
			( DistributeGuid ,
			  Businessid ,
			  Amount ,
			  DeductAmount ,
			  Balance ,
			  AccountNo ,
			  AccountName ,
			  IdentityCard ,
			  AccountBank ,
			  BillItems ,
			  HasLitigation ,
			  Createtime ,
			  Updatetime ,
			  OperationUserId ,
			  FrozenNo ,
			  DistributeTimes ,
			  Priority ,
			  Result ,
			  ResultDesc ,
			  Status,
			  DeductSource
			)
	VALUES  ( '{0}' , -- DistributeGuid - uniqueidentifier
			  {1} , -- Businessid - int
			  {2} , -- Amount - decimal
			  {3} , -- DeductAmount - decimal
			  {4} , -- Balance - decimal
			  '{5}' , -- AccountNo - nvarchar(50)
			  '{6}' , -- AccountName - nvarchar(50)
			  '{7}' , -- IdentityCard - nvarchar(50)
			  '{8}', -- AccountBank - varchar(250)
			  '{9}' , -- BillItems - nvarchar(max)
			  {10} , -- HasLitigation - bit
			  getdate() , -- Createtime - datetime
			  getdate() , -- Updatetime - datetime
			  {11} , -- OperationUserId - int
			  '{12}' , -- FrozenNo - nvarchar(50)
			  0 , -- DistributeTimes - int
			  {13} , -- Priority - int
			  0 , -- Result - int
			  NULL , -- ResultDesc - varchar(250)
			  0,  -- Status - int
			  {14}
			)

	update Business Set FrozenNo='{12}' where BusinessID ={1}
end