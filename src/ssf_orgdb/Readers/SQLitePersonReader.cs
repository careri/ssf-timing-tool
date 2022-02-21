using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using log4net;
using SSF.Timing.Tools.OrganizationDb.Models;
using SSF.Timing.Tools.OrganizationDb.Utils;

namespace SSF.Timing.Tools.OrganizationDb.Readers;

internal class SQLitePersonReader : IPersonReader, IReaderWriter, IDisposable
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

	internal class Factory : IPersonReaderFactory, IReaderWriterFactory<IPersonReader>
	{
		public bool TryGet(FileInfo fi, out IPersonReader reader)
		{
			if (fi.Extension.Equals(".sqlite", StringComparison.CurrentCultureIgnoreCase))
			{
				reader = Connect(fi);
			}
			else
			{
				reader = null;
			}
			return reader != null;
		}
	}

	private static readonly ILog s_log = LogManager.GetLogger(typeof(SQLitePersonReader).Name);

	private const string PersonTable = "Person";

	public const string Extension = ".sqlite";

	private readonly SQLiteConnection m_connection;

	private string m_path;

	public FlagsCollection Flags { get; set; }

	private SQLitePersonReader(FileInfo fi, SQLiteConnection con)
	{
		m_path = fi.FullName;
		m_connection = con;
	}

	public IEnumerable<IPerson> Get()
	{
		if (HasTable("Person"))
		{
			return DoGet();
		}
		s_log.Error("No table: Person");
		return Enumerable.Empty<IPerson>();
	}

	private IEnumerable<IPerson> DoGet()
	{
		IReadOnlyCollection<PersonDataColumn> columns = WellKnown.Columns;
		string allCols = "rowid," + string.Join(",", columns);
		string query = "SELECT " + allCols + " FROM Person";
		using SQLiteCommand cmd = new SQLiteCommand(query, m_connection);
		using SQLiteDataReader reader = cmd.ExecuteReader();
		while (reader.Read())
		{
			PersonRow p = ToPerson(columns, reader);
			if (p.Person != null)
			{
				yield return p.Person;
			}
		}
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
			case PersonDataColumn.Nation:
			case PersonDataColumn.Organization:
				return reader.GetString(i);
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

	private static IPersonReader Connect(FileInfo fi)
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
			return new SQLitePersonReader(fi, con);
		}
		catch (Exception)
		{
			throw;
		}
	}
}
