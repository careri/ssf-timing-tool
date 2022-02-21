using System;

namespace SSF.Timing.Tools.OrganizationDb.Utils;

public struct DateOfBirth
{
	public enum DateType
	{
		NotDefined,
		Date
	}

	public DateTime Date { get; }

	public DateType Type { get; }

	public DateOfBirth(DateTime dt)
	{
		Date = dt;
		Type = ((!(dt == default(DateTime))) ? DateType.Date : DateType.NotDefined);
	}

	public static DateOfBirth Parse(string value)
	{
		string trimmed = value?.Trim();
		if (!string.IsNullOrWhiteSpace(trimmed) && DateTime.TryParse(trimmed, out var dt))
		{
			return new DateOfBirth(dt);
		}
		return default(DateOfBirth);
	}

	public override string ToString()
	{
		return Type switch
		{
			DateType.NotDefined => null, 
			DateType.Date => Date.ToString(), 
			_ => throw new NotImplementedException(Type.ToString()), 
		};
	}
}
