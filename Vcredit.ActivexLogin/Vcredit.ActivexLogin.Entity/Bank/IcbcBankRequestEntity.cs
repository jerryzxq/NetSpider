namespace Vcredit.ActivexLogin.Entity.Bank
{
	public class IcbcBankRequestEntity : BaseActivexRequestEntity
	{
		public IcbcBankRequestEntity()
		{
			this.SiteType = Common.ProjectEnums.WebSiteType.IcbcBank;
		}

		public string Account { get; set; }

		public string RandomId { get; set; }

		public string RandomCode { get; set; }

		public string Rules { get; set; }

		public string ChangeRules { get; set; }

		public string PublicKey { get; set; }

	}
}
