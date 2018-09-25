namespace Vcredit.ActivexLogin.Entity.Bank
{
	public class PinganBankRequestEntity : BaseActivexRequestEntity
	{
		public PinganBankRequestEntity()
		{
			this.SiteType = Common.ProjectEnums.WebSiteType.PinganBank;
		}

		public string Account { get; set; }

	}
}
