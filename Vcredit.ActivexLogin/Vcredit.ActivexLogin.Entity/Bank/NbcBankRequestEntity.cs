namespace Vcredit.ActivexLogin.Entity.Bank
{
	public class NbcBankRequestEntity : BaseActivexRequestEntity
	{
		public NbcBankRequestEntity()
		{
			this.SiteType = Common.ProjectEnums.WebSiteType.NbcBank;
		}

		public string Account { get; set; }

	}
}
