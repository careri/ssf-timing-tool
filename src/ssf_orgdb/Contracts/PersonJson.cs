using System.Runtime.Serialization;
using AutoMapper.Configuration.Conventions;
using SSF.Timing.Tools.OrganizationDb.Utils;

namespace SSF.Timing.Tools.OrganizationDb.Contracts;

[DataContract]
public class PersonJson : DataContractBase<PersonJson>, IPerson, IAutoMapAble
{
	public object this[PersonDataColumn col] => this.Get(col);

	[DataMember]
	public string ID { get; set; }

	[DataMember]
	public string FirstName { get; set; }

	[DataMember]
	public string Surname { get; set; }

	[DataMember]
	public Gender Sex { get; set; }

	[MapTo(null)]
	[DataMember]
	public string DateOfBirthStr { get; set; }

	public DateOfBirth DateOfBirth
	{
		get
		{
			return DateOfBirth.Parse(DateOfBirthStr);
		}
		set
		{
			DateOfBirthStr = value.ToString();
		}
	}

	[DataMember]
	public string Phone { get; set; }

	[DataMember]
	public string Email { get; set; }

	[DataMember(Name = "Nation")]
	public EntityNameIDJson NationRef { get; set; }

	public IEntityNameID Nation
	{
		get
		{
			return NationRef;
		}
		set
		{
			NationRef = MapHelper.Map<EntityNameIDJson>(value);
		}
	}

	[DataMember(Name = "Organization")]
	public EntityNameIDJson OrganizationRef { get; set; }

	public IEntityNameID Organization
	{
		get
		{
			return OrganizationRef;
		}
		set
		{
			OrganizationRef = MapHelper.Map<EntityNameIDJson>(value);
		}
	}

	[DataMember]
	public string Class { get; set; }
}
