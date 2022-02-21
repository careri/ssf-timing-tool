using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using log4net;
using SSF.Timing.Tools.OrganizationDb.Models;
using SSF.Timing.Tools.OrganizationDb.Utils;

namespace SSF.Timing.Tools.OrganizationDb.Readers;

internal class SSFPersonXmlReader : IPersonReader, IReaderWriter, IDisposable
{
	public class Factory : IPersonReaderFactory, IReaderWriterFactory<IPersonReader>
	{
		public bool TryGet(FileInfo fi, out IPersonReader reader)
		{
			if (fi.Extension.StartsWith(".xml", StringComparison.InvariantCultureIgnoreCase) && fi.Name.Contains("SSFPersons") && TryOpen(fi, out var xmlReader))
			{
				reader = xmlReader;
				return true;
			}
			reader = null;
			return false;
		}
	}

	private static readonly ILog s_log = LogManager.GetLogger(typeof(ExcelPersonReader).Name);

	private readonly string m_path;

	private readonly XmlReader m_reader;

	private const string c_prefix = "SSFPersons";

	public FlagsCollection Flags { get; set; }

	private SSFPersonXmlReader(FileInfo fi, XmlReader reader)
	{
		m_path = fi.FullName;
		m_reader = reader;
	}

	public void Dispose()
	{
		m_reader.Dispose();
	}

	public IEnumerable<IPerson> Get()
	{
		if (!m_reader.ReadToDescendant("Person"))
		{
			yield break;
		}
		while (!m_reader.EOF)
		{
			IPerson p = ReadPerson();
			if (p != null)
			{
				yield return p;
			}
			if (!m_reader.ReadToNextSibling("Person"))
			{
				break;
			}
		}
	}

	private IPerson ReadPerson()
	{
		Dictionary<PersonDataColumn, object> dic = new Dictionary<PersonDataColumn, object>();
		string id = m_reader.GetAttribute("Id");
		PersonDataColumn? currColumn = null;
		dic[PersonDataColumn.ID] = id;
		bool done = false;
		while (!done && m_reader.Read())
		{
			switch (m_reader.NodeType)
			{
			case XmlNodeType.Element:
				currColumn = NameMapping.GetColumn(m_reader.Name);
				if (currColumn.HasValue)
				{
					ReadColumn(dic, currColumn.Value);
				}
				break;
			case XmlNodeType.Text:
				if (currColumn.HasValue)
				{
					SetValue(dic, currColumn.Value, m_reader.Value);
				}
				break;
			case XmlNodeType.EndElement:
				currColumn = null;
				done = m_reader.Name.Equals("Person", StringComparison.CurrentCulture);
				break;
			}
		}
		if (dic.Count > 0)
		{
			return new PersonData(dic);
		}
		return null;
	}

	private void SetValue(Dictionary<PersonDataColumn, object> dic, PersonDataColumn col, string value)
	{
		object o;
		switch (col)
		{
		case PersonDataColumn.ID:
		case PersonDataColumn.FirstName:
		case PersonDataColumn.Surname:
		case PersonDataColumn.Phone:
		case PersonDataColumn.Email:
			dic[col] = value;
			break;
		case PersonDataColumn.DateOfBirth:
		{
			DateOfBirth dt = DateOfBirth.Parse(value);
			if (dt.Type == DateOfBirth.DateType.Date)
			{
				dic[col] = dt;
			}
			break;
		}
		case PersonDataColumn.Sex:
			if ("m".Equals(value, StringComparison.InvariantCultureIgnoreCase))
			{
				dic[col] = Gender.Male;
			}
			else if ("f".Equals(value, StringComparison.InvariantCultureIgnoreCase) || "l".Equals(value, StringComparison.InvariantCultureIgnoreCase))
			{
				dic[col] = Gender.Female;
			}
			break;
		case PersonDataColumn.Nation:
			if (!dic.TryGetValue(col, out o))
			{
				dic[col] = new EntityNameIDData(value, null);
			}
			else
			{
				((EntityNameIDData)o).Name = value;
			}
			break;
		case PersonDataColumn.Organization:
			if (!dic.TryGetValue(col, out o))
			{
				dic[col] = new EntityNameIDData(value, null);
			}
			else
			{
				((EntityNameIDData)o).Name = value;
			}
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private void ReadColumn(Dictionary<PersonDataColumn, object> dic, PersonDataColumn col)
	{
		switch (col)
		{
		case PersonDataColumn.ID:
		case PersonDataColumn.FirstName:
		case PersonDataColumn.Surname:
		case PersonDataColumn.DateOfBirth:
		case PersonDataColumn.Phone:
		case PersonDataColumn.Email:
		case PersonDataColumn.Sex:
			break;
		case PersonDataColumn.Nation:
		{
			string id = m_reader.GetAttribute("Id");
			dic[col] = new EntityNameIDData(null, id);
			break;
		}
		case PersonDataColumn.Organization:
		{
			string id = m_reader.GetAttribute("Id");
			dic[col] = new EntityNameIDData(null, id);
			break;
		}
		default:
			throw new NotImplementedException(col.ToString());
		}
	}

	private static bool TryOpen(FileInfo fi, out SSFPersonXmlReader xmlReader)
	{
		try
		{
			XmlReader reader = XmlReader.Create(fi.FullName, new XmlReaderSettings
			{
				IgnoreWhitespace = true
			});
			xmlReader = new SSFPersonXmlReader(fi, reader);
			return true;
		}
		catch (Exception ex)
		{
			s_log.Error(fi.FullName, ex);
			xmlReader = null;
			return false;
		}
	}
}
