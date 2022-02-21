using System;
using System.Collections.Generic;
using log4net;
using SSF.Timing.Tools.OrganizationDb.Utils;

namespace SSF.Timing.Tools.OrganizationDb;

public class GenderFormatter : IGenderFormatter
{
	private static readonly ILog s_log = LogManager.GetLogger(typeof(GenderFormatter).Name);

	public static readonly IGenderFormatter Swedish = new GenderFormatter(SexToSwedish);

	public static readonly IGenderFormatter SSF = new GenderFormatter(SexToSSF);

	private readonly Func<Gender, string> m_translator;

	private static string SexToSSF(Gender arg)
	{
		return arg switch
		{
			Gender.Male => "M", 
			Gender.Female => "L", 
			_ => "", 
		};
	}

	private static string SexToSwedish(Gender arg)
	{
		return arg switch
		{
			Gender.Male => "H", 
			Gender.Female => "D", 
			_ => "", 
		};
	}

	public GenderFormatter(Func<Gender, string> translator)
	{
		m_translator = translator ?? new Func<Gender, string>(SexToSwedish);
	}

	public string GetName(Gender c)
	{
		return m_translator(c);
	}

	public Gender? Parse(string str)
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
				if (Find(strE, m_translator(g)))
				{
					return g;
				}
			}
			strE.Reset();
		}
		catch (Exception ex)
		{
			s_log.Debug("Parse: " + str, ex);
		}
		return null;
	}

	private bool Find(IEnumerator<char> strE, string gStr)
	{
		bool isvalid = false;
		using (CharEnumerator gE = (gStr ?? string.Empty).ToLowerInvariant().GetEnumerator())
		{
			while (gE.MoveNext())
			{
				if (strE.MoveNext())
				{
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
