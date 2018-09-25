namespace Vcredit.ActivexLogin.Entity.Bank
{
	public class ShhBankRequestEntity : BaseActivexRequestEntity
	{
		public ShhBankRequestEntity()
		{
			this.SiteType = Common.ProjectEnums.WebSiteType.ShhBank;
		}

		public string Account { get; set; }

		public string TS { get; set; }

	}
}
