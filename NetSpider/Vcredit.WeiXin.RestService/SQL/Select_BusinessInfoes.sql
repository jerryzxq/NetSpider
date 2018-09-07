 select * 
 into #tmp_business
 from
 (select *,ROW_NUMBER() over(order by createtime) as num
 from business
 where BusinessID in
 ({0}))t
 {1}

 select * from #tmp_business

 select l.* from Bill l with(nolock)
 inner join #tmp_business t on l.BusinessID=t.BusinessID
 where l.BillType in(1,2,8)
 order by l.BusinessID,l.BillMonth

 select i.* 
 from BillItem i with(nolock)
 inner join Bill l with(nolock) on i.BillID=l.BillID
 inner join #tmp_business t on l.BusinessID=t.BusinessID
 where l.BillType in(1,2,8)

 drop table #tmp_business
