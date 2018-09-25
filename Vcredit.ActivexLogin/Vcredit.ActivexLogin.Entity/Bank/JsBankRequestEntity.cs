namespace Vcredit.ActivexLogin.Entity.Bank
{
	public class JsBankRequestEntity : BaseActivexRequestEntity
	{
		public JsBankRequestEntity()
		{
			this.SiteType = Common.ProjectEnums.WebSiteType.JsBank;
		}

		public string Account { get; set; }

	}
}
