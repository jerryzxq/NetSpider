namespace Vcredit.ActivexLogin.Entity.Bank
{
	public class CmbcBankRequestEntity : BaseActivexRequestEntity
	{
		public CmbcBankRequestEntity()
		{
			this.SiteType = Common.ProjectEnums.WebSiteType.CmbcBank;
		}

		public string Account { get; set; }

		public string RandNum { get; set; }

	}
}
