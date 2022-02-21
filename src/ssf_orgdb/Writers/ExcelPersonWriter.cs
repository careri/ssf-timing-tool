using System;
using System.Collections.Generic;
using System.IO;
using log4net;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SSF.Timing.Tools.OrganizationDb.Utils;

namespace SSF.Timing.Tools.OrganizationDb.Writers;

internal class ExcelPersonWriter : IPersonWriter, IReaderWriter, IDisposable
{
	public class Factory : IPersonWriterFactory, IReaderWriterFactory<IPersonWriter>
	{
		public bool TryGet(FileInfo fi, out IPersonWriter writer)
		{
			if (fi.Extension.StartsWith(".xls", StringComparison.InvariantCultureIgnoreCase) && TryCreate(fi, out var xlsWriter))
			{
				writer = xlsWriter;
				return true;
			}
			writer = null;
			return false;
		}
	}

	private static readonly ILog s_log = LogManager.GetLogger(typeof(ExcelPersonWriter).Name);

	private readonly string m_path;

	private readonly XSSFWorkbook m_wb;

	private readonly Lazy<ICellStyle> m_defaultStyle;

	private readonly Lazy<ICellStyle> m_dateStyle;

	private readonly Dictionary<PersonDataColumn, Lazy<ICellStyle>> m_customStyles;

	private readonly Lazy<IReadOnlyCollection<PersonDataColumn>> m_dataColumns;

	public FlagsCollection Flags { get; set; }

	private ExcelPersonWriter(FileInfo fi, XSSFWorkbook wb)
	{
		m_path = fi.FullName;
		m_wb = wb;
		m_dataColumns = new Lazy<IReadOnlyCollection<PersonDataColumn>>(WriteHeaders, isThreadSafe: true);
		m_dateStyle = new Lazy<ICellStyle>(InitDateStyle, isThreadSafe: true);
		m_defaultStyle = new Lazy<ICellStyle>(InitDefaultStyle, isThreadSafe: true);
		m_customStyles = new Dictionary<PersonDataColumn, Lazy<ICellStyle>> { 
		{
			PersonDataColumn.DateOfBirth,
			m_dateStyle
		} };
	}

	private IReadOnlyCollection<PersonDataColumn> WriteHeaders()
	{
		IReadOnlyCollection<PersonDataColumn> cols = WellKnown.Columns;
		ICellStyle style = InitDefaultStyle();
		ISheet sheet = m_wb.GetSheetAt(0);
		style.FillBackgroundColor = IndexedColors.BrightGreen.Index;
		int i = 0;
		IRow row = sheet.CreateRow(0);
		foreach (PersonDataColumn col in cols)
		{
			ICell cell = row.CreateCell(i++);
			cell.SetCellValue(col.ToString());
			cell.CellStyle = style;
		}
		return cols;
	}

	private ICellStyle InitDefaultStyle()
	{
		ICellStyle style = m_wb.CreateCellStyle();
		style.DataFormat = m_wb.CreateDataFormat().GetFormat("yyyy-MM-dd");
		style.BorderBottom = BorderStyle.Thin;
		style.BorderLeft = BorderStyle.Thin;
		style.BorderRight = BorderStyle.Thin;
		style.BorderBottom = BorderStyle.Thin;
		return style;
	}

	private ICellStyle InitDateStyle()
	{
		ICellStyle style = InitDefaultStyle();
		style.DataFormat = m_wb.CreateDataFormat().GetFormat("yyyy-MM-dd");
		return style;
	}

	public void Dispose()
	{
		Save();
	}

	private void Save()
	{
		ISheet sheet = m_wb.GetSheetAt(0);
		for (int i = 0; i < m_dataColumns.Value.Count; i++)
		{
			sheet.AutoSizeColumn(i);
		}
		FileInfo fi = new FileInfo(m_path);
		fi.Directory.Create();
		using FileStream stream = fi.Create();
		m_wb.Write(stream);
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
		ISheet sheet = m_wb.GetSheetAt(0);
		int rows = sheet.LastRowNum;
		IReadOnlyCollection<PersonDataColumn> cols = m_dataColumns.Value;
		foreach (IPerson p in persons)
		{
			s_log.Debug("Write: " + p.FirstName + " " + p.Surname);
			Dictionary<PersonDataColumn, object> dic = p.ToDictionary();
			if (dic.Count <= 0)
			{
				continue;
			}
			IRow row = sheet.CreateRow(++rows);
			int i = 0;
			foreach (PersonDataColumn col in cols)
			{
				ICell cell = row.CreateCell(i++);
				if (dic.TryGetValue(col, out var o))
				{
					WriteValue(cell, col, o);
				}
				if (!m_customStyles.TryGetValue(col, out var style))
				{
					style = m_defaultStyle;
				}
				cell.CellStyle = style.Value;
			}
		}
	}

	private void WriteValue(ICell cell, PersonDataColumn col, object o)
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
			cell.SetCellValue(o?.ToString());
			break;
		case PersonDataColumn.DateOfBirth:
		{
			DateOfBirth dt = (DateOfBirth)o;
			DateOfBirth.DateType type = dt.Type;
			if ((uint)type <= 1u)
			{
				DateTime dob = dt.Date;
				DateTime year = new DateTime(dob.Year, dob.Month, dob.Day);
				cell.SetCellValue(year);
				break;
			}
			throw new NotImplementedException(dt.Type.ToString());
		}
		case PersonDataColumn.Nation:
		case PersonDataColumn.Organization:
		{
			IEntityNameID e = (IEntityNameID)o;
			cell.SetCellValue(e.ID ?? e.Name);
			break;
		}
		default:
			throw new NotImplementedException(col.ToString());
		}
	}

	public void Write(IPerson p)
	{
		Write(new IPerson[1] { p });
	}

	private static bool TryCreate(FileInfo fi, out ExcelPersonWriter xlsWriter)
	{
		try
		{
			fi.Directory.Create();
			XSSFWorkbook wb = new XSSFWorkbook();
			int sheets = wb.NumberOfSheets;
			ISheet sheet = wb.CreateSheet("Medlemsregister");
			xlsWriter = new ExcelPersonWriter(fi, wb);
			return true;
		}
		catch (Exception ex)
		{
			s_log.Error(fi.FullName, ex);
			xlsWriter = null;
			return false;
		}
	}
}
