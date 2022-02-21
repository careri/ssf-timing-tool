using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;

namespace SSF.Timing.Tools.OrganizationDb.Contracts;

[DataContract]
public abstract class DataContractBase<TSelf> where TSelf : DataContractBase<TSelf>
{
	private static readonly Encoding s_defaultEncoding = Encoding.Unicode;

	private Encoding m_customEncoding;

	public Encoding StreamEncoding
	{
		get
		{
			return m_customEncoding ?? s_defaultEncoding;
		}
		set
		{
			m_customEncoding = value;
		}
	}

	protected List<TOut> ConvertList<TIn, TOut>(List<TIn> inList) where TIn : TOut
	{
		if (inList != null)
		{
			List<TOut> i = new List<TOut>(inList.Count);
			i.AddRange(inList.Cast<TOut>());
			return i;
		}
		return null;
	}

	protected List<TOut> AutoMapList<TIn, TOut>(List<TIn> value) where TIn : IAutoMapAble where TOut : TIn
	{
		if (value != null)
		{
			List<TOut> list = new List<TOut>(value.Count);
			foreach (TIn iIn in value)
			{
				TOut iOut = ((!(iIn is TOut)) ? MapHelper.Map<TOut>(iIn) : ((TOut)(object)iIn));
				list.Add(iOut);
			}
			return list;
		}
		return null;
	}

	public string ToJson()
	{
		return ToJson(null);
	}

	public string ToJson(Encoding encoding)
	{
		Encoding enc = encoding ?? StreamEncoding;
		using MemoryStream stream = new MemoryStream();
		Serialize(stream, enc);
		return enc.GetString(stream.ToArray());
	}

	public void Serialize(Stream stream, Encoding encoding)
	{
		Encoding enc = encoding ?? StreamEncoding;
		using XmlDictionaryWriter writer = JsonReaderWriterFactory.CreateJsonWriter(stream, enc, ownsStream: true, indent: true);
		DataContractJsonSerializer ser = new DataContractJsonSerializer(GetType());
		ser.WriteObject(writer, this);
	}

	public void Serialize(Stream stream)
	{
		Serialize(stream, null);
	}

	public void Save(FileInfo fi)
	{
		fi.Directory.Create();
		using (FileStream stream = fi.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.None))
		{
			Serialize(stream);
		}
		fi.Refresh();
	}

	public static TSelf Load(FileInfo fi)
	{
		using FileStream stream = fi.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
		return Deserialize(stream);
	}

	public static TSelf Deserialize(Stream stream)
	{
		return Deserialize(stream, null);
	}

	public static TSelf Deserialize(Stream stream, Encoding encoding)
	{
		Encoding enc = encoding ?? s_defaultEncoding;
		using XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(stream, enc, null, null);
		DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(TSelf));
		object o = ser.ReadObject(reader);
		return (TSelf)o;
	}
}
