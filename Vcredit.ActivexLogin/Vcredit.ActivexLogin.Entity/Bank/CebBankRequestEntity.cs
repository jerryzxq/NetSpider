namespace Vcredit.ActivexLogin.Entity.Bank
{
	public class CebBankRequestEntity : BaseActivexRequestEntity
	{
		public CebBankRequestEntity()
		{
			this.SiteType = Common.ProjectEnums.WebSiteType.CebBank;
		}

		public string Account { get; set; }

	}
}
