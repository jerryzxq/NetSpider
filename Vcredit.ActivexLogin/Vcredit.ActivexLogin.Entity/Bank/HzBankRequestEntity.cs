namespace Vcredit.ActivexLogin.Entity.Bank
{
	public class HzBankRequestEntity : BaseActivexRequestEntity
	{
		public HzBankRequestEntity()
		{
			this.SiteType = Common.ProjectEnums.WebSiteType.HzBank;
		}

		public string Account { get; set; }

	}
}
