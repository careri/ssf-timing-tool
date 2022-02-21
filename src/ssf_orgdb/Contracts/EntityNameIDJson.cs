namespace SSF.Timing.Tools.OrganizationDb.Contracts;

public class EntityNameIDJson : DataContractBase<EntityNameIDJson>, IEntityNameID, IAutoMapAble
{
	public string Name { get; set; }

	public string ID { get; set; }
}
