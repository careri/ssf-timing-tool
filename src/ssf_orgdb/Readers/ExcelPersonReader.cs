using System;
using System.Collections.Generic;
using System.IO;
using log4net;
using NPOI.OpenXml4Net.OPC;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SSF.Timing.Tools.OrganizationDb.Models;
using SSF.Timing.Tools.OrganizationDb.Utils;

namespace SSF.Timing.Tools.OrganizationDb.Readers;

internal class ExcelPersonReader : IPersonReader, IReaderWriter, IDisposable
{
	public class Factory : IPersonReaderFactory, IReaderWriterFactory<IPersonReader>
	{
		public bool TryGet(FileInfo fi, out IPersonReader reader)
		{
			if (fi.Extension.StartsWith(".xls", StringComparison.InvariantCultureIgnoreCase) && TryOpen(fi, out var xlsReader))
			{
				reader = xlsReader;
				return true;
			}
			reader = null;
			return false;
		}
	}

	private static readonly ILog s_log = LogManager.GetLogger(typeof(ExcelPersonReader).Name);

	private readonly string m_path;

	private readonly XSSFWorkbook m_wb;

	public FlagsCollection Flags { get; set; }

	private ExcelPersonReader(FileInfo fi, XSSFWorkbook wb)
	{
		m_path = fi.FullName;
		m_wb = wb;
		s_log.Debug("Opened: " + fi.FullName);
	}

	private static bool TryOpen(FileInfo fi, out ExcelPersonReader xlsReader)
	{
		try
		{
			OPCPackage pkg = OPCPackage.Open(fi, PackageAccess.READ);
			XSSFWorkbook wb = new XSSFWorkbook(pkg);
			xlsReader = new ExcelPersonReader(fi, wb);
			return true;
		}
		catch (Exception ex)
		{
			s_log.Error(fi.FullName, ex);
			xlsReader = null;
			return false;
		}
	}

	public IEnumerable<IPerson> Get()
	{
		try
		{
			return DoGet();
		}
		catch (Exception ex)
		{
			s_log.Error(m_path, ex);
			throw;
		}
	}

	private IEnumerable<IPerson> DoGet()
	{
		Dictionary<PersonDataColumn, int> nameMap = GetColumns();
		PersonDataColumn[] requiredCols = null;
		PersonDataColumn[][] requiredColSets = new PersonDataColumn[2][]
		{
			new PersonDataColumn[2]
			{
				PersonDataColumn.FirstName,
				PersonDataColumn.Surname
			},
			new PersonDataColumn[1]
		};
		foreach (PersonDataColumn[] cols in requiredColSets)
		{
			if (HasRequiredColumns(nameMap, cols))
			{
				requiredCols = cols;
				break;
			}
		}
		if (requiredCols != null)
		{
			ISheet sheet = m_wb.GetSheetAt(0);
			int count = sheet.LastRowNum;
			s_log.Info($"Rows: {count}");
			for (int i = 1; i <= count; i++)
			{
				IPerson p = ReadPerson(sheet, i, nameMap, requiredCols);
				if (p != null)
				{
					yield return p;
				}
				else
				{
					s_log.Error($"Row{i} has no data");
				}
			}
		}
		else
		{
			s_log.Error("Missing required columns");
		}
	}

	private IPerson ReadPerson(ISheet sheet, int rowNo, Dictionary<PersonDataColumn, int> nameMap, PersonDataColumn[] requiredCols)
	{
		try
		{
			IRow row = sheet.GetRow(rowNo);
			Dictionary<PersonDataColumn, object> dic = new Dictionary<PersonDataColumn, object>();
			foreach (KeyValuePair<PersonDataColumn, int> pair in nameMap)
			{
				ICell cell = row.GetCell(pair.Value);
				object value = GetValue(pair.Key, cell);
				if (value == null)
				{
					if (Array.IndexOf(requiredCols, pair.Key) > -1)
					{
						s_log.Error($"Row{rowNo}: Missing required {pair.Key}");
						return null;
					}
				}
				else
				{
					dic[pair.Key] = value;
				}
			}
			PersonData p = new PersonData(dic);
			s_log.Info($"Row{rowNo} = {p.ID}, {p.FirstName} {p.Surname}");
			return p;
		}
		catch (Exception ex)
		{
			throw new ApplicationException($"Row: {rowNo}", ex);
		}
	}

	private object GetValue(PersonDataColumn col, ICell cell)
	{
		if (cell.CellType == CellType.Blank)
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
		case PersonDataColumn.Sex:
		case PersonDataColumn.Nation:
		case PersonDataColumn.Organization:
		case PersonDataColumn.Class:
			return cell.StringCellValue?.Trim();
		case PersonDataColumn.DateOfBirth:
			if (cell.CellType == CellType.String)
			{
				string stringValue = cell.StringCellValue;
				return DateOfBirth.Parse(stringValue);
			}
			if (cell.CellType == CellType.Numeric)
			{
				DateTime dt = cell.DateCellValue;
				return new DateOfBirth(dt);
			}
			throw new NotSupportedException($"[{col}] {cell.CellType}");
		default:
			throw new NotImplementedException(col.ToString());
		}
	}

	private bool HasRequiredColumns(Dictionary<PersonDataColumn, int> nameMap, params PersonDataColumn[] columns)
	{
		bool valid = true;
		foreach (PersonDataColumn c in columns)
		{
			if (!nameMap.ContainsKey(c))
			{
				valid = false;
				break;
			}
		}
		return valid;
	}

	private Dictionary<PersonDataColumn, int> GetColumns()
	{
		Dictionary<PersonDataColumn, int> dic = new Dictionary<PersonDataColumn, int>();
		ISheet sheet = m_wb.GetSheetAt(0);
		IRow row = sheet.GetRow(0);
		short colCount = row.LastCellNum;
		for (int i = 0; i < colCount; i++)
		{
			ICell cell = row.GetCell(i);
			string name = cell.StringCellValue;
			PersonDataColumn? pCol = NameMapping.GetColumn(name);
			if (pCol.HasValue)
			{
				s_log.Debug($"Col{i} = {pCol}");
				dic[pCol.Value] = i;
			}
		}
		return dic;
	}

	public void Dispose()
	{
	}
}
