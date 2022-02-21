namespace SSF.Timing.Tools.OrganizationDb;

public interface IEntityNameID : IAutoMapAble
{
	string Name { get; set; }

	string ID { get; set; }
}
