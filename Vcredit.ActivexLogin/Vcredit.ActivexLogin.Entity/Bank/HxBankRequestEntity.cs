namespace Vcredit.ActivexLogin.Entity.Bank
{
	public class HxBankRequestEntity : BaseActivexRequestEntity
	{
		public HxBankRequestEntity()
		{
			this.SiteType = Common.ProjectEnums.WebSiteType.HxBank;
		}

		public string Account { get; set; }

		public string RandCode { get; set; }

	}
}
