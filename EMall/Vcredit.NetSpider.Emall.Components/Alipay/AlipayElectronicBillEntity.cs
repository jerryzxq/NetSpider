using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Alipay_ElectronicBill")]
    
	public class AlipayElectronicBillEntity
	{
	
		public AlipayElectronicBillEntity() { }


		#region Attributes
      
		private long iD;
       
     [AutoIncrement]
        /// <summary>
        /// 
        /// </summary>
		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private long? baicID;
       

        /// <summary>
        /// 
        /// </summary>
		public long? BaicID
		{
			get { return baicID; }
			set { baicID = value; }
		}
      
		private DateTime? startTime;
       

        /// <summary>
        /// 
        /// </summary>
		public DateTime? StartTime
		{
			get { return startTime; }
			set { startTime = value; }
		}
      
		private DateTime? endTime;
       

        /// <summary>
        /// 
        /// </summary>
		public DateTime? EndTime
		{
			get { return endTime; }
			set { endTime = value; }
		}
      
		private decimal? expenditure;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal? Expenditure
		{
			get { return expenditure; }
			set { expenditure = value; }
		}
      
		private decimal? income;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal? Income
		{
			get { return income; }
			set { income = value; }
		}
      
		private decimal? yuEBaoProfit;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal? YuEBaoProfit
		{
			get { return yuEBaoProfit; }
			set { yuEBaoProfit = value; }
		}
      
		private decimal? yuEBaoIncome;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal? YuEBaoIncome
		{
			get { return yuEBaoIncome; }
			set { yuEBaoIncome = value; }
		}
      
		private decimal? yuEBaoTurnOut;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal? YuEBaoTurnOut
		{
			get { return yuEBaoTurnOut; }
			set { yuEBaoTurnOut = value; }
		}

        private decimal? yuEBaoConsume;
       

        /// <summary>
        /// 
        /// </summary>
        public decimal? YuEBaoConsume
		{
            get { return yuEBaoConsume; }
            set { yuEBaoConsume = value; }
		}

        private string address;

        public string Address
        {
            get { return address; }
            set { address = value; }
        }

        
		private string consumptionAnalysis;
       

        /// <summary>
        /// 
        /// </summary>
		public string ConsumptionAnalysis
		{
			get { return consumptionAnalysis; }
			set { consumptionAnalysis = value; }
		}
      
		private string enviProIndex;
       

        /// <summary>
        /// 
        /// </summary>
		public string EnviProIndex
		{
			get { return enviProIndex; }
			set { enviProIndex = value; }
		}
      
		private DateTime? createTime;
       

        /// <summary>
        /// 
        /// </summary>
		public DateTime? CreateTime
		{
            get
            {

                if (createTime == null)
                {
                    return DateTime.Now;
                }
                else
                {
                    return createTime;
                }
            }
			set { createTime = value; }
		}

        private List<AlipayConsumptionEntity> _Consumption = new List<AlipayConsumptionEntity>();
        [Ignore]
        public List<AlipayConsumptionEntity> Consumption
        {
            get { return _Consumption; }
            set { _Consumption = value; }
        }
        private List<AlipayExpBankEntity> _ExpBank = new List<AlipayExpBankEntity>();
        [Ignore]
        public List<AlipayExpBankEntity> ExpBank
        {
            get { return _ExpBank; }
            set { _ExpBank = value; }
        }

        private List<AlipayExpSummaryEntity> _ExpSummary = new List<AlipayExpSummaryEntity>();
        [Ignore]
        public List<AlipayExpSummaryEntity> ExpSummary
        {
            get { return _ExpSummary; }
            set { _ExpSummary = value; }
        }

        private List<AlipayIncomeSummaryEntity> _IncomeSummary = new List<AlipayIncomeSummaryEntity>();
        [Ignore]
        public List<AlipayIncomeSummaryEntity> IncomeSummary
        {
            get { return _IncomeSummary; }
            set { _IncomeSummary = value; }
        }
		#endregion

	}
}
