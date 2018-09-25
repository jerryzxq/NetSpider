namespace Vcredit.ActivexLogin.Entity.Bank
{
	public class GznsBankRequestEntity : BaseActivexRequestEntity
	{
		public GznsBankRequestEntity()
		{
			this.SiteType = Common.ProjectEnums.WebSiteType.GznsBank;
		}

		public string Account { get; set; }

		public string CheckCode { get; set; }

	}
}
