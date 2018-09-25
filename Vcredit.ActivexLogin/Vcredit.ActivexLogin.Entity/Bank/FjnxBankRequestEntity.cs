namespace Vcredit.ActivexLogin.Entity.Bank
{
	public class FjnxBankRequestEntity : BaseActivexRequestEntity
	{
		public FjnxBankRequestEntity()
		{
			this.SiteType = Common.ProjectEnums.WebSiteType.FjnxBank;
		}

		public string Account { get; set; }

		public string TS { get; set; }

	}
}
