namespace Vcredit.ActivexLogin.Entity.Bank
{
	public class CommBankRequestEntity : BaseActivexRequestEntity
	{
		public CommBankRequestEntity()
		{
			this.SiteType = Common.ProjectEnums.WebSiteType.CommBank;
		}

		public string Account { get; set; }

		public string RandomText { get; set; }

		public string ReqSafeFields { get; set; }

		public string PSessionId { get; set; }

	}
}
