using System;
using System.Collections.Generic;
using SSF.Timing.Tools.OrganizationDb.Utils;

namespace SSF.Timing.Tools.OrganizationDb;

public static class PersonPropertyReader
{
	public static object Get(this IPerson person, PersonDataColumn col)
	{
		if (person == null)
		{
			return null;
		}
		return col switch
		{
			PersonDataColumn.ID => person.ID, 
			PersonDataColumn.FirstName => person.FirstName, 
			PersonDataColumn.Surname => person.Surname, 
			PersonDataColumn.DateOfBirth => person.DateOfBirth, 
			PersonDataColumn.Phone => person.Phone, 
			PersonDataColumn.Email => person.Email, 
			PersonDataColumn.Sex => person.Sex, 
			PersonDataColumn.Nation => person.Nation, 
			PersonDataColumn.Organization => person.Organization, 
			PersonDataColumn.Class => person.Class, 
			_ => throw new NotImplementedException(col.ToString()), 
		};
	}

	public static Dictionary<PersonDataColumn, object> ToDictionary(this IPerson person)
	{
		Dictionary<PersonDataColumn, object> dic = new Dictionary<PersonDataColumn, object>();
		IReadOnlyCollection<PersonDataColumn> cols = WellKnown.Columns;
		foreach (PersonDataColumn col in cols)
		{
			object o = person.Get(col);
			if (o == null)
			{
				continue;
			}
			switch (col)
			{
			case PersonDataColumn.Sex:
				if ((Gender)o == Gender.NotDefined)
				{
					continue;
				}
				break;
			case PersonDataColumn.DateOfBirth:
				if (((DateOfBirth)o).Type == DateOfBirth.DateType.NotDefined)
				{
					continue;
				}
				break;
			default:
				throw new NotImplementedException(col.ToString());
			case PersonDataColumn.ID:
			case PersonDataColumn.FirstName:
			case PersonDataColumn.Surname:
			case PersonDataColumn.Phone:
			case PersonDataColumn.Email:
			case PersonDataColumn.Nation:
			case PersonDataColumn.Organization:
			case PersonDataColumn.Class:
				break;
			}
			dic[col] = o;
		}
		return dic;
	}
}
