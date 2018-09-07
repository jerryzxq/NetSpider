--四舍五入
function Round(num, idp)
    local mult = 10^(idp or 0)
    return math.floor(num * mult + 0.5) / mult
end

--相乘，判断了是否有null值
function Multiply(num1,num2)
	if(num1==nil) then
		return 0
	end
	if(num2==nil) then
		return 0
	end
	return num1*num2
end

--相除
function Divide(denominator,numerator)
	if(numerator==nil) then
		return 0
	end
	if(denominator==nil) then
		return 0
	end
	if(numerator==0) then
		return 0
	end
	return denominator/numerator
end

--是否最后一期账单
function IsLastBill(bus)
	if(bus.Bills==nil) then
		return false
	end
	local billCount = 1
	for i=0,bus.Bills.Count-1 do 
		if(bus.Bills[i].IsShelve==false and (bus.Bills[i].BillType==1 or bus.Bills[i].BillType==2)) then
			billCount= billCount + 1
		end
	end
	
	if(billCount==bus.LoanPeriod) then
		return true
	end
	return false
end

--楼一贷最后一个月本金算法
function LastMonthCapital(bus)
	local oldMoney = Round(Multiply(bus.LoanCapital,bus.CapitalRate),2)*(bus.LoanPeriod-1)
	return Round(bus.LoanCapital-oldMoney,2)
end

--根据订单状态确定到款账号
function GetToAccountByBusinessStatus(bus)
	--正常状态下是信托方ID
	if (bus.BusinessStatus == 1) then
		return bus.LendingSideID
	end
	if(bus.GuaranteeSideKey==nil or bus.GuaranteeSideKey=="") then
		return bus.ServiceSideID
	end
	--其他情况是担保方ID Modify by Baker 2014年4月30日 2014年5月1日起规则变更,由原来的服务方转到担保方
	return bus.GuaranteeSideID
end

--新楼一贷是否需要扣款
--1-23期账单,可以扣款
--24期账单,无需扣款
--24期账单无全额还款的情况下,24期+的账单无需扣款
--24期账单全额还款且后24期+的账单,可以扣款
function FoticBuildingCanDeduct(bus)
	if(bus.Bills==nil) then
		return false;
	end
	if(bus.Bills.Count<24) then
		return true;
	end

	--AddMonths是由c#代码注册的方法
	local after24Month = AddMonths(bus.LoanTime,24)
	
	--FormatDate是由c#代码注册的方法
    local MonthStr=FormatDate(after24Month,"yyyy/MM")
    for i = 0,bus.billList.Count-1 do 
		--如果小于24期的月账单欠费可扣款
        if(bus.Bills[i].BillMonth < MonthStr and bus.Bills[i].BillStatus<3) then
                return true;
        end
		if(bus.Bills[i].BillMonth == MonthStr and bus.Bills[i].BillStatus==3) then
			return true;
		end
    end
    return false
end

--新楼一贷提前清贷到款账号
function FoticBuildingAdvToAccount(bus)
	if(bus.GuaranteeSideKey=="") then
		return bus.LendingSideID
	end
	return GetToAccountByBusinessStatus(bus)
end

--判断扣款通道
 ConstDeductPlatform = { 
        Fuiou="Fuiou", 
        TenPay="TenPay",
        Bill99="Bill99",
        ChinaPay="ChinaPay"
  }
--得到扣款通道
function GetDeductPlatform(bus)
	if(bus.BankKey=="BANKLIST/YOUHANG") then
		return ConstDeductPlatform.Fuiou
	else
		return ConstDeductPlatform.TenPay
	end
end

--等额本息月还本息
function GetInterestFromAvgCapInter(capital,yearRate,period)
	if(yearRate==nil) then
		return 0;
	end
	local monthRate=yearRate/12
	return Round(capital*monthRate*((1+monthRate)^period)/((1+monthRate)^period -1),2);
end

--等额本息本金
function GetCapitalFromAvgCapInter(capital,yearRate,period,periodIndex)
	local monthRate=yearRate/12
	return Round(capital*monthRate*((1+monthRate)^(periodIndex-1))/((1+monthRate)^period -1),2);
end

--等额本息利息
function GetInterFromAvgCapInter(capital,yearRate,period,periodIndex)
	return Round(GetInterestFromAvgCapInter(capital,yearRate,period)-GetCapitalFromAvgCapInter(capital,yearRate,period,periodIndex),2);
end

--计算到账单第几期
function GetBusBillCount(bus)
	local billCount = 1
	if(bus.Bills~=nil) then
		for i=0,bus.Bills.Count-1 do 
			if(bus.Bills[i].BillType==1 or bus.Bills[i].BillType==2) then
				billCount= billCount + 1
			end
		end
	end
	return billCount;
end

--订单等额本息本金
function GetCapFromAvgCapInterByBusiness(bus)
	if(bus.YearRate==nil) then
		return 0
	end
	local billCount = GetBusBillCount(bus)
	return GetCapitalFromAvgCapInter(bus.LoanCapital,bus.YearRate,bus.LoanPeriod,billCount)
end

--订单等额本息利息
function GetInterFromAvgCapInterByBusiness(bus)
	if(bus.YearRate==nil) then
		return 0
	end
	local billCount = GetBusBillCount(bus)
	return GetInterFromAvgCapInter(bus.LoanCapital,bus.YearRate,bus.LoanPeriod,billCount)
end

--得到本息逾期金额
function GetCapInterDueAmount(bus)
	if(bus.Bills==nil) then
		return 0
	end
	local amount=0
	for i=0,bus.Bills.Count-1 do
		if(bus.Bills[i].BillItems~=nil and bus.Bills[i].IsCurrent==true and bus.Bills[i].IsShelve==false and (bus.Bills[i].BillType==1 or bus.Bills[i].BillType==2)) then
			for j=0,bus.Bills[i].BillItems.Count-1 do
				if(bus.Bills[i].BillItems[j].IsShelve==false and (bus.Bills[i].BillItems[j].Subject==1 or bus.Bills[i].BillItems[j].Subject==2)) then
					amount=amount+bus.Bills[i].BillItems[j].DueAmt-bus.Bills[i].BillItems[j].ReceivedAmt
				end
			end
		end
	end
	return amount
end

--得到卡卡贷扣款失败违约金
function GetDueManageFee(bus)
	if(GetBusBillCount(bus)>bus.LoanPeriod + 1) then
		return 0
	end
	local duaAmount = GetCapInterDueAmount(bus)
	if(duaAmount<=0) then
		return 0
	end
	local amount=0.005 * bus.LoanCapital
	if(amount>200) then
		return 200
	end
	if(amount<50) then
		return 50
	end
	return amount
end

--得到月综合费率
function GetMonthTotalRate(bus)
	local rate=0;
	--月利率
	if(bus.InterestRate ~=nil) then
		rate = rate + bus.InterestRate;
	end
	--月服务费率
	if(bus.ServiceRate ~=nil) then
		rate = rate + bus.ServiceRate;
	end
	--月管理费率
	if(bus.ManagementRate ~=nil) then
		rate = rate + bus.ManagementRate;
	end
	return rate;
end

--月服务费
function GetServiceFee(bus)
	local rate= GetMonthTotalRate(bus);
	local monthReturn=bus.LoanCapital/bus.LoanPeriod+bus.LoanCapital*rate;
	local interest = GetInterestFromAvgCapInter(bus.LoanCapital,bus.YearRate,bus.LoanPeriod);
	local platManage = Multiply(bus.LoanCapital,bus.PlatformRate);
	return Round(monthReturn-interest-platManage,2);
end

--得到当前账单的罚息
function GetCurrentPenalty(bus)
	if(bus.Bills==nil or bus.PeriodType ~= 32 or bus.IsRepayment==false) then
		return 0
	end
	local amount=0
	for i=0,bus.Bills.Count-1 do
		if(bus.Bills[i].BillItems~=nil and bus.Bills[i].IsCurrent==true 
		 and (bus.Bills[i].BillType==1 or bus.Bills[i].BillType==2)) then
			for j=0,bus.Bills[i].BillItems.Count-1 do
				if(bus.Bills[i].BillItems[j].IsShelve==false and bus.Bills[i].BillItems[j].Subject== 23) then
					 return bus.Bills[i].BillItems[j].DueAmt;
				end
			end
		end
	end
	return amount;
end

--得到当前账单是否欠费
function IsCurrentBillDue(bus)
	if(bus.Bills==nil or bus.PeriodType ~= 32 or bus.IsRepayment==false) then
		return false
	end
	
	for i=0,bus.Bills.Count-1 do
		if(bus.Bills[i].BillItems~=nil and bus.Bills[i].IsCurrent==true 
		 and (bus.Bills[i].BillType==1 or bus.Bills[i].BillType==2)) then
			for j=0,bus.Bills[i].BillItems.Count-1 do
				if(bus.Bills[i].BillItems[j].IsShelve==false and 
					bus.Bills[i].BillItems[j].DueAmt>bus.Bills[i].BillItems[j].ReceivedAmt) then
					 return true
				end
			end
		end
	end
	return false
end

--得到违约金
function GetBreachingFee(bus)
	if(bus.BreachingRate==nil) then
		return 0;
	end
	local currentPenalty = GetCurrentPenalty(bus);
	return Round(currentPenalty*bus.BreachingRate/bus.PenaltyRate,2);
end
--得到扣款失败服务费
function GetServiceDeductFail(bus)
	if(IsCurrentBillDue(bus)==true and GetBusBillCount(bus)<=bus.LoanPeriod + 1) then
		return 100
	else
		return 0
	end
end

--得到剩余本金
function GetRemCapital(bus)
	local remCapital = bus.LoanCapital
	if(bus.Bills==nil) then
		return remCapital
	end
	
	for i=0,bus.Bills.Count-1 do
		if(bus.Bills[i].BillItems~=nil and (bus.Bills[i].BillType==1 or bus.Bills[i].BillType==2)) then
			for j=0,bus.Bills[i].BillItems.Count-1 do
				if(bus.Bills[i].BillItems[j].Subject==1) then
					remCapital=remCapital - bus.Bills[i].BillItems[j].Amount
				end
			end
		end
	end
	return remCapital
end

--得到月利息：剩余本金*年利率/12
function GetInterFromRemCapital(bus)
	if(bus.YearRate==nil) then
		return 0
	end
	local remCapital =GetRemCapital(bus)
	return Round(remCapital*bus.YearRate/12,2)
end
--得到月本金:等额本息月本息-月利息
function GetCapFromRemCapital(bus)
	local interestCap = GetInterestFromAvgCapInter(bus.LoanCapital,bus.YearRate,bus.LoanPeriod)
	local interest = GetInterFromRemCapital(bus)
	return Round(interestCap-interest,2)
end

--最后一期账单月服务费
function GetLastBillServiceFee(bus)
	local rate=0;
	--月利率
	if(bus.InterestRate ~=nil) then
		rate = rate + bus.InterestRate;
	end
	--月服务费率
	if(bus.ServiceRate ~=nil) then
		rate = rate + bus.ServiceRate;
	end
	--月管理费率
	if(bus.ManagementRate ~=nil) then
		rate = rate + bus.ManagementRate;
	end
	local monthReturn=bus.LoanCapital/bus.LoanPeriod+bus.LoanCapital*rate;
	local remCapital = GetRemCapital(bus)
	local interest = remCapital+ Round(remCapital*bus.YearRate/12,2);
	local platManage = Multiply(bus.LoanCapital,bus.PlatformRate);
	return Round(monthReturn-interest-platManage,2);
end

--得到管理及担保费
function GetManageGuarantee(bus)
	local rate= GetMonthTotalRate(bus);
	return Round(bus.LoanCapital*rate-CSharp_GetFinancingInterest(bus),2)
end



