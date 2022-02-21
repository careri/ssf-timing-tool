using System;
using System.Collections.Generic;
using SSF.Timing.Tools.OrganizationDb.Contracts;
using SSF.Timing.Tools.OrganizationDb.Utils;

namespace SSF.Timing.Tools.OrganizationDb.Models;

internal class PersonData : IPerson, IAutoMapAble
{
	public object this[PersonDataColumn col] => this.Get(col);

	public string ID { get; set; }

	public string FirstName { get; set; }

	public string Surname { get; set; }

	public Gender Sex { get; set; }

	public IEntityNameID Nation { get; set; }

	public IEntityNameID Organization { get; set; }

	public DateOfBirth DateOfBirth { get; set; }

	public string Phone { get; set; }

	public string Email { get; set; }

	public string Class { get; set; }

	public PersonData(Dictionary<PersonDataColumn, object> dic)
	{
		ID = GetString(dic, PersonDataColumn.ID);
		FirstName = GetString(dic, PersonDataColumn.FirstName);
		Surname = GetString(dic, PersonDataColumn.Surname);
		Gender sex = Get<Gender>(dic, PersonDataColumn.Sex);
		Sex = ((sex > Gender.NotDefined) ? sex : GetEnum<Gender>(dic, PersonDataColumn.Sex));
		Nation = Get<IEntityNameID>(dic, PersonDataColumn.Nation) ?? EntityNameIDData.Parse(GetString(dic, PersonDataColumn.Nation));
		Organization = Get<IEntityNameID>(dic, PersonDataColumn.Organization) ?? EntityNameIDData.Parse(GetString(dic, PersonDataColumn.Organization));
		DateOfBirth dt = Get<DateOfBirth>(dic, PersonDataColumn.DateOfBirth);
		DateOfBirth = ((dt.Type == DateOfBirth.DateType.Date) ? dt : ParseValue(dic, PersonDataColumn.DateOfBirth, DateOfBirth.Parse));
		Phone = GetString(dic, PersonDataColumn.Phone);
		Email = GetString(dic, PersonDataColumn.Email);
		Class = GetString(dic, PersonDataColumn.Class);
	}

	public override string ToString()
	{
		PersonJson json = MapHelper.Map<PersonJson>(this);
		return json.ToJson();
	}

	private static object GetValue(Dictionary<PersonDataColumn, object> dic, PersonDataColumn col)
	{
		dic.TryGetValue(col, out var value);
		return value;
	}

	private T Get<T>(Dictionary<PersonDataColumn, object> dic, PersonDataColumn col)
	{
		object value = GetValue(dic, col);
		if (value != null)
		{
			Type tType = typeof(T);
			Type vType = value.GetType();
			if (tType.IsInterface)
			{
				if (vType.GetInterface(tType.FullName) != null)
				{
					return (T)value;
				}
			}
			else
			{
				if (value.GetType().IsAssignableFrom(typeof(T)))
				{
					return (T)value;
				}
				if (typeof(T).IsAssignableFrom(value.GetType()))
				{
					return (T)value;
				}
			}
		}
		return default(T);
	}

	private static string GetString(Dictionary<PersonDataColumn, object> dic, PersonDataColumn col)
	{
		dic.TryGetValue(col, out var value);
		return ((string)value)?.Trim();
	}

	private static T GetEnum<T>(Dictionary<PersonDataColumn, object> dic, PersonDataColumn col) where T : struct
	{
		string value = GetString(dic, col);
		Enum.TryParse<T>(value, ignoreCase: true, out var eValue);
		return eValue;
	}

	private T ParseValue<T>(Dictionary<PersonDataColumn, object> dic, PersonDataColumn col, Func<string, T> parser)
	{
		string value = GetString(dic, col);
		if (value != null && value.Length > 0)
		{
			return parser(value);
		}
		return default(T);
	}
}
