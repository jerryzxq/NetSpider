UPDATE dbo.Business SET CLoanStatus=7,FrozenNo='' WHERE BusinessID={0}

EXEC dbo.pro_Business_UpdateForBusinessID {0}

