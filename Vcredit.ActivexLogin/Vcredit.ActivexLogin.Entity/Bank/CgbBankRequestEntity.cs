namespace Vcredit.ActivexLogin.Entity.Bank
{
	public class CgbBankRequestEntity : BaseActivexRequestEntity
	{
		public CgbBankRequestEntity()
		{
			this.SiteType = Common.ProjectEnums.WebSiteType.CgbBank;
		}

		public string Account { get; set; }

    }
}
