using System;
using System.Collections.Generic;
using log4net;
using SSF.Timing.Tools.OrganizationDb.Utils;

namespace SSF.Timing.Tools.OrganizationDb;

public class CompetitionClassFormatter : ICompetitionClassFormatter
{
	private static readonly ILog s_log = LogManager.GetLogger(typeof(CompetitionClassFormatter).Name);

	public static readonly ICompetitionClassFormatter Swedish = new CompetitionClassFormatter(GenderFormatter.Swedish, " ", "-");

	public static readonly ICompetitionClassFormatter SSF = new CompetitionClassFormatter(GenderFormatter.SSF, " ", "-");

	private readonly IGenderFormatter m_translator;

	private readonly string m_delimiter;

	private readonly string m_rangeDelimiter;

	public CompetitionClassFormatter(IGenderFormatter translator, string delimiter, string rangeDelimiter)
	{
		m_translator = translator ?? GenderFormatter.Swedish;
		m_delimiter = delimiter ?? "";
		m_rangeDelimiter = rangeDelimiter ?? "";
	}

	public string GetName(CompetitionClass c)
	{
		int min = Math.Min(c.AgeFrom, c.AgeTo);
		int max = Math.Max(c.AgeFrom, c.AgeTo);
		string ageStr = ((min != max) ? $"{min}{m_rangeDelimiter}{max}" : min.ToString());
		string prefix = m_translator.GetName(c.Sex) ?? string.Empty;
		string name = prefix + m_delimiter + ageStr;
		if (name.Equals("H 10-8"))
		{
		}
		return name;
	}

	public CompetitionClass Parse(string str)
	{
		if (str == null)
		{
			return null;
		}
		try
		{
			using IIndexedEnumerator<char> strE = IndexedEnumerator.Create(str.ToLowerInvariant().GetEnumerator());
			foreach (Gender g in WellKnown.Genders)
			{
				if (Find(strE, m_translator.GetName(g)) && Find(strE, m_delimiter))
				{
					int pos = strE.Index;
					bool isValid = true;
					int ageFrom = -1;
					int ageTo = -1;
					while (strE.MoveNext())
					{
						char c = strE.Current;
						if (!char.IsDigit(c))
						{
							int end = strE.Index;
							if (!Find(strE, m_rangeDelimiter, checkCurrent: true))
							{
								isValid = false;
								break;
							}
							ageFrom = int.Parse(str.Substring(pos, end - pos));
							ageTo = int.Parse(str.Substring(strE.Index + 1));
						}
					}
					if (isValid)
					{
						if (ageTo == -1)
						{
							ageTo = (ageFrom = int.Parse(str.Substring(pos)));
						}
						return new CompetitionClass(g, ageFrom, ageTo, this);
					}
				}
				strE.Reset();
			}
		}
		catch (Exception ex)
		{
			s_log.Debug("Parse: " + str, ex);
		}
		return null;
	}

	private bool Find(IEnumerator<char> strE, string gStr)
	{
		return Find(strE, gStr, checkCurrent: false);
	}

	private bool Find(IEnumerator<char> strE, string gStr, bool checkCurrent)
	{
		bool isvalid = false;
		bool skipMove = checkCurrent;
		using (CharEnumerator gE = (gStr ?? string.Empty).ToLowerInvariant().GetEnumerator())
		{
			while (gE.MoveNext())
			{
				if (skipMove || strE.MoveNext())
				{
					skipMove = false;
					isvalid = gE.Current == strE.Current;
					if (!isvalid)
					{
						break;
					}
				}
			}
		}
		return isvalid;
	}
}
