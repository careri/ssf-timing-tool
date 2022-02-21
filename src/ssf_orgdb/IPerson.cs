using SSF.Timing.Tools.OrganizationDb.Utils;

namespace SSF.Timing.Tools.OrganizationDb;

public interface IPerson : IAutoMapAble
{
	string ID { get; set; }

	string FirstName { get; set; }

	string Surname { get; set; }

	Gender Sex { get; set; }

	IEntityNameID Nation { get; set; }

	IEntityNameID Organization { get; set; }

	DateOfBirth DateOfBirth { get; set; }

	string Phone { get; set; }

	string Email { get; set; }

	string Class { get; set; }

	object this[PersonDataColumn col] { get; }
}
