namespace Vcredit.ActivexLogin.Entity.Bank
{
	public class BocdBankRequestEntity : BaseActivexRequestEntity
	{
		public BocdBankRequestEntity()
		{
			this.SiteType = Common.ProjectEnums.WebSiteType.BocdBank;
		}

		public string Account { get; set; }

	}
}
