namespace Vcredit.ActivexLogin.Entity.Bank
{
	public class CiticBankRequestEntity : BaseActivexRequestEntity
	{
		public CiticBankRequestEntity()
		{
			this.SiteType = Common.ProjectEnums.WebSiteType.CiticBank;
		}

		public string Account { get; set; }

		public string EMP_SID { get; set; }

	}
}
