using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.WeiXin.RestService.Models
{
    public partial class BusinessOverview
    {
        /// <summary>
        /// Bid
        /// </summary>		
        private int _bid;
        public int Bid
        {
            get { return _bid; }
            set { _bid = value; }
        }
        /// <summary>
        /// IdentityNo
        /// </summary>		
        private string _identityno;
        public string IdentityNo
        {
            get { return _identityno; }
            set { _identityno = value; }
        }
        /// <summary>
        /// Name
        /// </summary>		
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        /// <summary>
        /// Region
        /// </summary>		
        private string _region;
        public string Region
        {
            get { return _region; }
            set { _region = value; }
        }
        /// <summary>
        /// Branch
        /// </summary>		
        private string _branch;
        public string Branch
        {
            get { return _branch; }
            set { _branch = value; }
        }
        /// <summary>
        /// Team
        /// </summary>		
        private string _team;
        public string Team
        {
            get { return _team; }
            set { _team = value; }
        }
        /// <summary>
        /// Agent
        /// </summary>		
        private string _agent;
        public string Agent
        {
            get { return _agent; }
            set { _agent = value; }
        }
        /// <summary>
        /// ContractNo
        /// </summary>		
        private string _contractno;
        public string ContractNo
        {
            get { return _contractno; }
            set { _contractno = value; }
        }
        /// <summary>
        /// Step
        /// </summary>		
        private int? _step;
        public int? Step
        {
            get { return _step; }
            set { _step = value; }
        }
        /// <summary>
        /// Section
        /// </summary>		
        private string _section;
        public string Section
        {
            get { return _section; }
            set { _section = value; }
        }
        /// <summary>
        /// ApplyTime
        /// </summary>		
        private DateTime? _applytime;
        public DateTime? ApplyTime
        {
            get { return _applytime; }
            set { _applytime = value; }
        }
        /// <summary>
        /// FaceIdentyfyTime#CR091
        /// </summary>		
        private DateTime? _faceidentyfytime;
        public DateTime? FaceIdentyfyTime
        {
            get { return _faceidentyfytime; }
            set { _faceidentyfytime = value; }
        }
        /// <summary>
        /// ExamineTime
        /// </summary>		
        private DateTime? _examinetime;
        public DateTime? ExamineTime
        {
            get { return _examinetime; }
            set { _examinetime = value; }
        }
        /// <summary>
        /// SignTime
        /// </summary>		
        private DateTime? _signtime;
        public DateTime? SignTime
        {
            get { return _signtime; }
            set { _signtime = value; }
        }
        /// <summary>
        /// LendingTime
        /// </summary>		
        private DateTime? _lendingtime;
        public DateTime? LendingTime
        {
            get { return _lendingtime; }
            set { _lendingtime = value; }
        }
        /// <summary>
        /// ProductType
        /// </summary>		
        private string _producttype;
        public string ProductType
        {
            get { return _producttype; }
            set { _producttype = value; }
        }
        /// <summary>
        /// LoanKind
        /// </summary>		
        private string _loankind;
        public string LoanKind
        {
            get { return _loankind; }
            set { _loankind = value; }
        }
        /// <summary>
        /// LoanPeriod
        /// </summary>		
        private string _loanperiod;
        public string LoanPeriod
        {
            get { return _loanperiod; }
            set { _loanperiod = value; }
        }
        /// <summary>
        /// ClearLoanTime
        /// </summary>		
        private DateTime? _clearloantime;
        public DateTime? ClearLoanTime
        {
            get { return _clearloantime; }
            set { _clearloantime = value; }
        }
        /// <summary>
        /// ToGuaranteeTime
        /// </summary>		
        private DateTime? _toguaranteetime;
        public DateTime? ToGuaranteeTime
        {
            get { return _toguaranteetime; }
            set { _toguaranteetime = value; }
        }
        /// <summary>
        /// Amount
        /// </summary>		
        private decimal? _amount;
        public decimal? Amount
        {
            get { return _amount; }
            set { _amount = value; }
        }

        private string _customerType;
        public string CustomerType
        {
            get { return _customerType; }
            set { _customerType = value; }
        }

        private string _household;
        public string Household
        {
            get { return _household; }
            set { _household = value; }
        }

        private string _returnReason;
        public string ReturnReason
        {
            get { return _returnReason; }
            set { _returnReason = value; }
        }

        private string _retrunRemark;
        public string RetrunRemark
        {
            get { return _retrunRemark; }
            set { _retrunRemark = value; }
        }

        private int _returnType;
        public int ReturnType
        {
            get { return _returnType; }
            set { _returnType = value; }
        }
    }
}
