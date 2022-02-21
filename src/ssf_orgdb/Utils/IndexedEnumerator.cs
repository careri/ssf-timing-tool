using System;
using System.Collections;
using System.Collections.Generic;

namespace SSF.Timing.Tools.OrganizationDb.Utils;

public abstract class IndexedEnumerator : IEnumerator
{
	public abstract object Current { get; }

	public static IIndexedEnumerator<T> Create<T>(IEnumerator<T> e)
	{
		return new IndexedEnumerator<T>(e);
	}

	public abstract bool MoveNext();

	public abstract void Reset();
}
public class IndexedEnumerator<T> : IndexedEnumerator, IIndexedEnumerator<T>, IEnumerator<T>, IDisposable, IEnumerator
{
	private readonly IEnumerator<T> m_decorated;

	public int Index { get; private set; } = -1;


	public override object Current => m_decorated.Current;

	T IEnumerator<T>.Current => m_decorated.Current;

	object IEnumerator.Current => m_decorated.Current;

	public IndexedEnumerator(IEnumerator<T> decorated)
	{
		m_decorated = decorated;
	}

	public void Dispose()
	{
		m_decorated.Dispose();
	}

	public override bool MoveNext()
	{
		if (m_decorated.MoveNext())
		{
			Index++;
			return true;
		}
		return false;
	}

	public override void Reset()
	{
		Index = -1;
		m_decorated.Reset();
	}
}
