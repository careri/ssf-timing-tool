using System;

namespace SSF.Timing.Tools.OrganizationDb;

public abstract class NameMapping
{
	public static PersonDataColumn? GetColumn(string str)
	{
		string key = str?.ToLower()?.Trim();
		if (Enum.TryParse<PersonDataColumn>(key, ignoreCase: true, out var pdc))
		{
			return pdc;
		}
		switch (key)
		{
		case "förnamn":
		case "name":
			return PersonDataColumn.FirstName;
		case "efternamn":
		case "lastname":
			return PersonDataColumn.Surname;
		case "födelsedat./personnr.":
		case "födelsedat":
		case "personnr":
		case "birthdate":
		case "socialsecuritynumber":
			return PersonDataColumn.DateOfBirth;
		case "telefon mobil":
		case "telefon":
		case "mobil":
		case "cell":
			return PersonDataColumn.Phone;
		case "e-post kontakt":
		case "e-post":
		case "epost":
		case "mail":
			return PersonDataColumn.Email;
		default:
			return null;
		}
	}
}
