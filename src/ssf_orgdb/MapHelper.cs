using System;
using System.Collections.Generic;
using AutoMapper;
using log4net;

namespace SSF.Timing.Tools.OrganizationDb;

public class MapHelper
{
	private static readonly ILog s_log = LogManager.GetLogger(typeof(MapHelper).Name);

	private static readonly Lazy<MapHelper> s_singleton = new Lazy<MapHelper>(() => new MapHelper(), isThreadSafe: true);

	private readonly HashSet<string> m_initialized = new HashSet<string>();

	private MapHelper()
	{
	}

	public static void Define<T1, T2>(Action<IMapperConfigurationExpression> initAction)
	{
		Action<IMapperConfigurationExpression> init = initAction ?? new Action<IMapperConfigurationExpression>(DefaultInit<T1, T2>);
		Type inType = typeof(T1);
		Type outType = typeof(T2);
		string key = inType.FullName + "->" + outType.FullName;
		if (s_singleton.Value.m_initialized.Contains(key))
		{
			throw new ApplicationException("[" + key + "] Already defined");
		}
		Mapper.Initialize(init);
	}

	public static void Define<T1, T2>()
	{
		Define<T1, T2>(null);
	}

	private static void DefaultInit<T1, T2>(IMapperConfigurationExpression cfg)
	{
		Type inType = typeof(T1);
		Type outType = typeof(T2);
		cfg.CreateMap(inType, outType);
	}

	internal static T Map<T>(IAutoMapAble input) where T : IAutoMapAble
	{
		return s_singleton.Value.DoMap<T>(input);
	}

	private T DoMap<T>(IAutoMapAble input) where T : IAutoMapAble
	{
		if (input != null)
		{
			Type inType = input.GetType();
			Type outType = typeof(T);
			return (T)Mapper.Map(input, inType, outType);
		}
		return default(T);
	}
}
