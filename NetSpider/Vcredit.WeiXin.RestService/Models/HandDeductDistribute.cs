using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.WeiXin.RestService.Models
{
   public class HandDeductDistribute
	{
		/// <summary>
		/// 序列号
		/// </summary>
        public string DistributeGuid { get; set; }
		/// <summary>
		/// 订单号
		/// </summary>
		public int Businessid { get; set; }
		/// <summary>
		/// 填写金额
		/// </summary>
		public decimal Amount { get; set; }
		/// <summary>
		/// 扣款金额
		/// </summary>
		public decimal DeductAmount { get; set; }
		/// <summary>
		/// 余额
		/// </summary>
		public decimal Balance { get; set; }
		/// <summary>
		/// 账号
		/// </summary>
		public string AccountNo { get; set; }
		/// <summary>
		/// 用户
		/// </summary>
		public string AccountName { get; set; }
		/// <summary>
		/// 身份证号
		/// </summary>
		public string IdentityCard { get; set; }
		/// <summary>
		/// 银行
		/// </summary>
		public string AccountBank { get; set; }
		/// <summary>
		/// 账单编号
		/// </summary>
		public string BillItems { get; set; }
		/// <summary>
		/// 是否有诉讼
		/// </summary>
		public bool HasLitigation { get; set; }
		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime Createtime { get; set; }
		/// <summary>
		/// 更新时间
		/// </summary>
		public DateTime Updatetime { get; set; }
		/// <summary>
		/// 操作员编号
		/// </summary>
		public int OperationUserId { get; set; }
		/// <summary>
		/// 冻结码
		/// </summary>
		public string FrozenNo { get; set; }
		/// <summary>
		/// 分配的次数
		/// </summary>
		public int DistributeTimes { get; set; }
		/// <summary>
		/// 返回的结果
		/// </summary>
		public int Result { get; set; }
		/// <summary>
		/// 结果描述
		/// </summary>
		public string ResultDesc { get; set; }
		/// <summary>
		/// 分配的优先级，越小越高
		/// </summary>
		public int Priority { get; set; }
		/// <summary>
		/// 状态,0、正在处理 1、处理结束
		/// </summary>
		public int Status { get; set; }

		/// <summary>
		/// 当前扣款序列编号
		/// </summary>
		public Guid CurrentSequence { get; set; }

		/// <summary>
		/// 当前扣款序列结果
		/// </summary>
		public string SequenceResult { get; set; }

		/// <summary>
		/// 当前扣款序列结果描述
		/// </summary>
		public string SequenceResultDesc { get; set; }

		/// <summary>
		/// 当前已经发出的扣款指令总额
		/// </summary>
		public decimal CurrentDeductAmount { get; set; }
	}

}
