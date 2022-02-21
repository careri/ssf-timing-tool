using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;
using log4net;
using SSF.Timing.Tools.OrganizationDb.Models;
using SSF.Timing.Tools.OrganizationDb.Utils;

namespace SSF.Timing.Tools.OrganizationDb.Writers;

internal class SQLitePersonWriter : IPersonWriter, IReaderWriter, IDisposable, IEqualityComparer<IEntityNameID>
{
	private struct PersonRow
	{
		public long RowID { get; }

		public IPerson Person { get; }

		public PersonRow(long rowID, IPerson person)
		{
			RowID = rowID;
			Person = person;
		}
	}

	internal class Factory : IPersonWriterFactory, IReaderWriterFactory<IPersonWriter>
	{
		public bool TryGet(FileInfo fi, out IPersonWriter writer)
		{
			if (fi.Extension.Equals(".sqlite", StringComparison.CurrentCultureIgnoreCase))
			{
				writer = Connect(fi);
			}
			else
			{
				writer = null;
			}
			return writer != null;
		}
	}

	private static readonly ILog s_log = LogManager.GetLogger(typeof(SQLitePersonWriter).Name);

	private const string PersonTable = "Person";

	public const string Extension = ".sqlite";

	private readonly SQLiteConnection m_connection;

	private readonly string m_path;

	private FlagsCollection m_flags = new FlagsCollection();

	private bool m_checkTable = true;

	private bool m_isNoInsert;

	public FlagsCollection Flags
	{
		get
		{
			return m_flags;
		}
		set
		{
			m_flags = value;
			m_isNoInsert = value?["no-insert"] ?? false;
		}
	}

	private SQLitePersonWriter(FileInfo fi, SQLiteConnection con)
	{
		m_path = fi.FullName;
		m_connection = con;
	}

	public void Write(IEnumerable<IPerson> persons)
	{
		try
		{
			DoWrite(persons);
		}
		catch (Exception ex)
		{
			s_log.Error("Write Failed", ex);
			throw;
		}
	}

	private void DoWrite(IEnumerable<IPerson> persons)
	{
		if (m_checkTable)
		{
			m_checkTable = false;
			EnsurePersonTable();
		}
		foreach (IPerson p in persons)
		{
			s_log.Debug("Write: " + p.ID + ", " + p.FirstName + " " + p.Surname);
			PersonRow? pRow = Read(p);
			if (!pRow.HasValue)
			{
				if (!m_isNoInsert)
				{
					Insert(p);
				}
				else
				{
					s_log.Debug("NoInsert");
				}
			}
			else
			{
				Update(p, pRow.Value);
			}
		}
	}

	private void Update(IPerson newPerson, PersonRow row)
	{
		Dictionary<PersonDataColumn, object> dic = newPerson.ToDictionary();
		Dictionary<PersonDataColumn, object> old = row.Person.ToDictionary();
		Dictionary<PersonDataColumn, object> merged = new Dictionary<PersonDataColumn, object>();
		s_log.Debug("Updated");
		foreach (PersonDataColumn col in WellKnown.Columns)
		{
			dic.TryGetValue(col, out var newO);
			old.TryGetValue(col, out var o);
			if (IsUpdatedValue(col, newO, o))
			{
				merged[col] = newO;
			}
		}
		if (merged.Count > 0)
		{
			PersonData pMerged = new PersonData(merged);
			Update(row.RowID, pMerged);
		}
		else
		{
			s_log.Debug("NOOP");
		}
	}

	private bool IsUpdatedValue(PersonDataColumn col, object newO, object oldO)
	{
		switch (col)
		{
		case PersonDataColumn.DateOfBirth:
		{
			DateOfBirth newDoB = (DateOfBirth)newO;
			DateOfBirth oldDoB = (DateOfBirth)oldO;
			if (oldDoB.Type == DateOfBirth.DateType.NotDefined && newDoB.Type == DateOfBirth.DateType.Date)
			{
				return oldDoB.Date != newDoB.Date;
			}
			return false;
		}
		case PersonDataColumn.ID:
		case PersonDataColumn.FirstName:
		case PersonDataColumn.Surname:
		case PersonDataColumn.Phone:
		case PersonDataColumn.Email:
		case PersonDataColumn.Class:
		{
			string newS = (string)newO;
			string oldS = (string)oldO;
			if (!string.IsNullOrWhiteSpace(newS))
			{
				return !string.Equals(newS, oldS);
			}
			return false;
		}
		case PersonDataColumn.Sex:
			return !object.Equals(newO, oldO);
		case PersonDataColumn.Nation:
		case PersonDataColumn.Organization:
		{
			IEntityNameID newE = (IEntityNameID)newO;
			IEntityNameID oldE = (IEntityNameID)oldO;
			if (newE != null)
			{
				return !Equals(newE, oldE);
			}
			return false;
		}
		default:
			throw new NotImplementedException(col.ToString());
		}
	}

	private void Update(long rowID, IPerson pMerged)
	{
		Dictionary<PersonDataColumn, object> dic = pMerged.ToDictionary();
		StringBuilder sb = new StringBuilder("UPDATE Person SET ");
		foreach (KeyValuePair<PersonDataColumn, object> pair in dic)
		{
			sb.Append($"{pair.Key} = {GetValueString(pair.Key, pair.Value)},");
		}
		sb.Length--;
		sb.Append($" WHERE rowid={rowID}");
		string stmt = sb.ToString();
		using SQLiteCommand cmd = new SQLiteCommand(stmt, m_connection);
		int o = cmd.ExecuteNonQuery();
	}

	private PersonRow? Read(IPerson p)
	{
		string id = p.ID;
		PersonRow? p2 = null;
		if (!string.IsNullOrEmpty(id))
		{
			p2 = GetByID(id);
		}
		if (!p2.HasValue)
		{
			p2 = GetByName(p.FirstName, p.Surname);
		}
		return p2;
	}

	private PersonRow? GetByName(string firstName, string surname)
	{
		IReadOnlyCollection<PersonDataColumn> columns = WellKnown.Columns;
		string allCols = "rowid," + string.Join(",", columns);
		string query = string.Format("SELECT {0} FROM {1} WHERE {2} = '{3}' AND {4} = '{5}' ", allCols, "Person", PersonDataColumn.FirstName, firstName, PersonDataColumn.Surname, surname);
		using (SQLiteCommand cmd = new SQLiteCommand(query, m_connection))
		{
			using SQLiteDataReader reader = cmd.ExecuteReader();
			if (reader.Read())
			{
				return ToPerson(columns, reader);
			}
		}
		return null;
	}

	private PersonRow? GetByID(string id)
	{
		IReadOnlyCollection<PersonDataColumn> columns = WellKnown.Columns;
		string allCols = "rowid," + string.Join(",", columns);
		string query = string.Format("SELECT {0} FROM {1} WHERE {2} = '{3}'", allCols, "Person", PersonDataColumn.ID, id);
		using (SQLiteCommand cmd = new SQLiteCommand(query, m_connection))
		{
			using SQLiteDataReader reader = cmd.ExecuteReader();
			if (reader.Read())
			{
				return ToPerson(columns, reader);
			}
		}
		return null;
	}

	private PersonRow ToPerson(IReadOnlyCollection<PersonDataColumn> cols, SQLiteDataReader reader)
	{
		Dictionary<PersonDataColumn, object> dic = new Dictionary<PersonDataColumn, object>();
		int i = 1;
		long rowId = reader.GetInt64(0);
		foreach (PersonDataColumn col in cols)
		{
			object value = ReadValue(col, i++, reader);
			if (value != null)
			{
				dic[col] = value.ToString();
			}
		}
		return new PersonRow(rowId, new PersonData(dic));
	}

	private void Insert(IPerson p)
	{
		Dictionary<PersonDataColumn, object> dic = p.ToDictionary();
		if (dic.Count <= 0)
		{
			return;
		}
		StringBuilder sb = new StringBuilder("INSERT INTO Person (");
		s_log.Debug("Insert");
		foreach (KeyValuePair<PersonDataColumn, object> item in dic)
		{
			sb.Append(item.Key).Append(",");
		}
		sb[sb.Length - 1] = ')';
		sb.Append(" VALUES (");
		foreach (KeyValuePair<PersonDataColumn, object> pair in dic)
		{
			sb.Append(GetValueString(pair.Key, pair.Value)).Append(",");
		}
		sb[sb.Length - 1] = ')';
		string stmt = sb.ToString();
		using SQLiteCommand cmd = new SQLiteCommand(stmt, m_connection);
		int o = cmd.ExecuteNonQuery();
	}

	private object ReadValue(PersonDataColumn col, int i, SQLiteDataReader reader)
	{
		try
		{
			if (reader.IsDBNull(i))
			{
				return null;
			}
			switch (col)
			{
			case PersonDataColumn.ID:
			case PersonDataColumn.FirstName:
			case PersonDataColumn.Surname:
			case PersonDataColumn.Phone:
			case PersonDataColumn.Email:
			case PersonDataColumn.Class:
				return reader.GetString(i);
			case PersonDataColumn.Nation:
				return EntityNameIDData.Parse(reader.GetString(i));
			case PersonDataColumn.Organization:
				return EntityNameIDData.Parse(reader.GetString(i));
			case PersonDataColumn.Sex:
			{
				string str = reader.GetString(i);
				Enum.TryParse<Gender>(str, ignoreCase: true, out var sex);
				return sex;
			}
			case PersonDataColumn.DateOfBirth:
			{
				DateTime dt = new DateTime(reader.GetInt64(i));
				return new DateOfBirth(dt);
			}
			default:
				throw new NotImplementedException(col.ToString());
			}
		}
		catch (Exception ex)
		{
			throw new ApplicationException($"[{col}] @ {i}", ex);
		}
	}

	private string GetValueString(PersonDataColumn col, object value)
	{
		switch (col)
		{
		case PersonDataColumn.ID:
		case PersonDataColumn.FirstName:
		case PersonDataColumn.Surname:
		case PersonDataColumn.Phone:
		case PersonDataColumn.Email:
		case PersonDataColumn.Sex:
		case PersonDataColumn.Class:
			return $"'{value}'";
		case PersonDataColumn.Nation:
		case PersonDataColumn.Organization:
		{
			IEntityNameID e = (IEntityNameID)value;
			string key = e.Name ?? e.ID;
			return "'" + key + "'";
		}
		case PersonDataColumn.DateOfBirth:
			return $"{((DateOfBirth)value).Date.Ticks}";
		default:
			throw new NotImplementedException(col.ToString());
		}
	}

	public void Write(IPerson person)
	{
		Write(new IPerson[1] { person });
	}

	private void EnsurePersonTable()
	{
		if (!HasTable("Person"))
		{
			CreatePersonTable();
		}
	}

	private void CreatePersonTable()
	{
		Dictionary<PersonDataColumn, string> specialTypes = new Dictionary<PersonDataColumn, string>
		{
			{
				PersonDataColumn.ID,
				"Text PRIMARY KEY"
			},
			{
				PersonDataColumn.DateOfBirth,
				"int64"
			}
		};
		StringBuilder sb = new StringBuilder("CREATE TABLE Person (");
		IReadOnlyCollection<PersonDataColumn> cols = WellKnown.Columns;
		foreach (PersonDataColumn col in cols)
		{
			if (!specialTypes.TryGetValue(col, out var type))
			{
				type = "Text";
			}
			sb.Append($"{col} {type},");
		}
		sb[sb.Length - 1] = ')';
		string stmt = sb.ToString();
		using SQLiteCommand cmd = new SQLiteCommand(stmt, m_connection);
		int o = cmd.ExecuteNonQuery();
	}

	private bool HasTable(string table_name)
	{
		string query = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = '" + table_name + "'";
		using SQLiteCommand cmd = new SQLiteCommand(query, m_connection);
		object o = cmd.ExecuteScalar();
		return o != null;
	}

	public void Dispose()
	{
		m_connection.Dispose();
	}

	private static IPersonWriter Connect(FileInfo fi)
	{
		try
		{
			SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
			builder.DataSource = fi.FullName;
			builder.DateTimeFormat = SQLiteDateFormats.Ticks;
			builder.DateTimeFormatString = "yyyy-MM-dd";
			builder.DateTimeKind = DateTimeKind.Local;
			builder.DefaultDbType = DbType.String;
			builder.DefaultIsolationLevel = IsolationLevel.Serializable;
			builder.SyncMode = SynchronizationModes.Full;
			SQLiteConnection con = new SQLiteConnection(builder.ToString());
			con.Open();
			return new SQLitePersonWriter(fi, con);
		}
		catch (Exception)
		{
			throw;
		}
	}

	public bool Equals(IEntityNameID x, IEntityNameID y)
	{
		return string.Equals(x?.ID, y?.ID) && string.Equals(x?.Name, y?.Name);
	}

	public int GetHashCode(IEntityNameID obj)
	{
		throw new NotImplementedException();
	}
}
