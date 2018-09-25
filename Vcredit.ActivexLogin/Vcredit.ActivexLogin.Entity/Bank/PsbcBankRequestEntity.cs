namespace Vcredit.ActivexLogin.Entity.Bank
{
	public class PsbcBankRequestEntity : BaseActivexRequestEntity
	{
		public PsbcBankRequestEntity()
		{
			this.SiteType = Common.ProjectEnums.WebSiteType.PsbcBank;
		}

		public string Account { get; set; }

		public string TS { get; set; }

	}
}
