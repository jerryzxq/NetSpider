--��������
function Round(num, idp)
    local mult = 10^(idp or 0)
    return math.floor(num * mult + 0.5) / mult
end

--��ˣ��ж����Ƿ���nullֵ
function Multiply(num1,num2)
	if(num1==nil) then
		return 0
	end
	if(num2==nil) then
		return 0
	end
	return num1*num2
end

--���
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

--�Ƿ����һ���˵�
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

--¥һ�����һ���±����㷨
function LastMonthCapital(bus)
	local oldMoney = Round(Multiply(bus.LoanCapital,bus.CapitalRate),2)*(bus.LoanPeriod-1)
	return Round(bus.LoanCapital-oldMoney,2)
end

--���ݶ���״̬ȷ�������˺�
function GetToAccountByBusinessStatus(bus)
	--����״̬�������з�ID
	if (bus.BusinessStatus == 1) then
		return bus.LendingSideID
	end
	if(bus.GuaranteeSideKey==nil or bus.GuaranteeSideKey=="") then
		return bus.ServiceSideID
	end
	--��������ǵ�����ID Modify by Baker 2014��4��30�� 2014��5��1���������,��ԭ���ķ���ת��������
	return bus.GuaranteeSideID
end

--��¥һ���Ƿ���Ҫ�ۿ�
--1-23���˵�,���Կۿ�
--24���˵�,����ۿ�
--24���˵���ȫ���������,24��+���˵�����ۿ�
--24���˵�ȫ����Һ�24��+���˵�,���Կۿ�
function FoticBuildingCanDeduct(bus)
	if(bus.Bills==nil) then
		return false;
	end
	if(bus.Bills.Count<24) then
		return true;
	end

	--AddMonths����c#����ע��ķ���
	local after24Month = AddMonths(bus.LoanTime,24)
	
	--FormatDate����c#����ע��ķ���
    local MonthStr=FormatDate(after24Month,"yyyy/MM")
    for i = 0,bus.billList.Count-1 do 
		--���С��24�ڵ����˵�Ƿ�ѿɿۿ�
        if(bus.Bills[i].BillMonth < MonthStr and bus.Bills[i].BillStatus<3) then
                return true;
        end
		if(bus.Bills[i].BillMonth == MonthStr and bus.Bills[i].BillStatus==3) then
			return true;
		end
    end
    return false
end

--��¥һ����ǰ��������˺�
function FoticBuildingAdvToAccount(bus)
	if(bus.GuaranteeSideKey=="") then
		return bus.LendingSideID
	end
	return GetToAccountByBusinessStatus(bus)
end

--�жϿۿ�ͨ��
 ConstDeductPlatform = { 
        Fuiou="Fuiou", 
        TenPay="TenPay",
        Bill99="Bill99",
        ChinaPay="ChinaPay"
  }
--�õ��ۿ�ͨ��
function GetDeductPlatform(bus)
	if(bus.BankKey=="BANKLIST/YOUHANG") then
		return ConstDeductPlatform.Fuiou
	else
		return ConstDeductPlatform.TenPay
	end
end

--�ȶϢ�»���Ϣ
function GetInterestFromAvgCapInter(capital,yearRate,period)
	if(yearRate==nil) then
		return 0;
	end
	local monthRate=yearRate/12
	return Round(capital*monthRate*((1+monthRate)^period)/((1+monthRate)^period -1),2);
end

--�ȶϢ����
function GetCapitalFromAvgCapInter(capital,yearRate,period,periodIndex)
	local monthRate=yearRate/12
	return Round(capital*monthRate*((1+monthRate)^(periodIndex-1))/((1+monthRate)^period -1),2);
end

--�ȶϢ��Ϣ
function GetInterFromAvgCapInter(capital,yearRate,period,periodIndex)
	return Round(GetInterestFromAvgCapInter(capital,yearRate,period)-GetCapitalFromAvgCapInter(capital,yearRate,period,periodIndex),2);
end

--���㵽�˵��ڼ���
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

--�����ȶϢ����
function GetCapFromAvgCapInterByBusiness(bus)
	if(bus.YearRate==nil) then
		return 0
	end
	local billCount = GetBusBillCount(bus)
	return GetCapitalFromAvgCapInter(bus.LoanCapital,bus.YearRate,bus.LoanPeriod,billCount)
end

--�����ȶϢ��Ϣ
function GetInterFromAvgCapInterByBusiness(bus)
	if(bus.YearRate==nil) then
		return 0
	end
	local billCount = GetBusBillCount(bus)
	return GetInterFromAvgCapInter(bus.LoanCapital,bus.YearRate,bus.LoanPeriod,billCount)
end

--�õ���Ϣ���ڽ��
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

--�õ��������ۿ�ʧ��ΥԼ��
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

--�õ����ۺϷ���
function GetMonthTotalRate(bus)
	local rate=0;
	--������
	if(bus.InterestRate ~=nil) then
		rate = rate + bus.InterestRate;
	end
	--�·������
	if(bus.ServiceRate ~=nil) then
		rate = rate + bus.ServiceRate;
	end
	--�¹������
	if(bus.ManagementRate ~=nil) then
		rate = rate + bus.ManagementRate;
	end
	return rate;
end

--�·����
function GetServiceFee(bus)
	local rate= GetMonthTotalRate(bus);
	local monthReturn=bus.LoanCapital/bus.LoanPeriod+bus.LoanCapital*rate;
	local interest = GetInterestFromAvgCapInter(bus.LoanCapital,bus.YearRate,bus.LoanPeriod);
	local platManage = Multiply(bus.LoanCapital,bus.PlatformRate);
	return Round(monthReturn-interest-platManage,2);
end

--�õ���ǰ�˵��ķ�Ϣ
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

--�õ���ǰ�˵��Ƿ�Ƿ��
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

--�õ�ΥԼ��
function GetBreachingFee(bus)
	if(bus.BreachingRate==nil) then
		return 0;
	end
	local currentPenalty = GetCurrentPenalty(bus);
	return Round(currentPenalty*bus.BreachingRate/bus.PenaltyRate,2);
end
--�õ��ۿ�ʧ�ܷ����
function GetServiceDeductFail(bus)
	if(IsCurrentBillDue(bus)==true and GetBusBillCount(bus)<=bus.LoanPeriod + 1) then
		return 100
	else
		return 0
	end
end

--�õ�ʣ�౾��
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

--�õ�����Ϣ��ʣ�౾��*������/12
function GetInterFromRemCapital(bus)
	if(bus.YearRate==nil) then
		return 0
	end
	local remCapital =GetRemCapital(bus)
	return Round(remCapital*bus.YearRate/12,2)
end
--�õ��±���:�ȶϢ�±�Ϣ-����Ϣ
function GetCapFromRemCapital(bus)
	local interestCap = GetInterestFromAvgCapInter(bus.LoanCapital,bus.YearRate,bus.LoanPeriod)
	local interest = GetInterFromRemCapital(bus)
	return Round(interestCap-interest,2)
end

--���һ���˵��·����
function GetLastBillServiceFee(bus)
	local rate=0;
	--������
	if(bus.InterestRate ~=nil) then
		rate = rate + bus.InterestRate;
	end
	--�·������
	if(bus.ServiceRate ~=nil) then
		rate = rate + bus.ServiceRate;
	end
	--�¹������
	if(bus.ManagementRate ~=nil) then
		rate = rate + bus.ManagementRate;
	end
	local monthReturn=bus.LoanCapital/bus.LoanPeriod+bus.LoanCapital*rate;
	local remCapital = GetRemCapital(bus)
	local interest = remCapital+ Round(remCapital*bus.YearRate/12,2);
	local platManage = Multiply(bus.LoanCapital,bus.PlatformRate);
	return Round(monthReturn-interest-platManage,2);
end

--�õ�����������
function GetManageGuarantee(bus)
	local rate= GetMonthTotalRate(bus);
	return Round(bus.LoanCapital*rate-CSharp_GetFinancingInterest(bus),2)
end



