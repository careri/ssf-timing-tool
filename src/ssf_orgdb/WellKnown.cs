using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SSF.Timing.Tools.OrganizationDb;

internal class WellKnown
{
	public abstract class Flags
	{
		public const string NoInsert = "no-insert";

		public const string SSF_Mode = "ssf";

		public const string JSON_Mode = "json";

		public const string Excel_Mode = "excel";
	}

	private static readonly string s_sqliteFile = Path.Combine(Program.WorkingDI.FullName, "person.sqlite");

	private static readonly IReadOnlyCollection<string> s_columnNames = Enum.GetNames(typeof(PersonDataColumn)).Cast<string>().ToList();

	private static readonly IReadOnlyCollection<PersonDataColumn> s_columnValues = Enum.GetValues(typeof(PersonDataColumn)).Cast<PersonDataColumn>().ToList();

	private static readonly IReadOnlyCollection<string> s_genderNames = Enum.GetNames(typeof(Gender)).Cast<string>().ToList();

	private static readonly IReadOnlyCollection<Gender> s_genderValues = new List<Gender>
	{
		Gender.Male,
		Gender.Female,
		Gender.Other
	};

	public const string SSFEntryPrefix = "SSFEntry_";

	public static FileInfo SqliteFI => new FileInfo(s_sqliteFile);

	public static IReadOnlyCollection<string> ColumnNames => s_columnNames;

	public static IReadOnlyCollection<PersonDataColumn> Columns => s_columnValues;

	public static IReadOnlyCollection<string> GenderNames => s_genderNames;

	public static IReadOnlyCollection<Gender> Genders => s_genderValues;
}
