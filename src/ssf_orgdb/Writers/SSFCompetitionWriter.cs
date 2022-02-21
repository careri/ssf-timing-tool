using System;
using System.IO;
using System.Xml.Linq;
using log4net;
using SSF.Timing.Tools.OrganizationDb.Utils;

namespace SSF.Timing.Tools.OrganizationDb.Writers;

internal class SSFCompetitionWriter : ICompetitionWriter, IReaderWriter, IDisposable
{
	public class Factory : ICompetitionWriterFactory, IReaderWriterFactory<ICompetitionWriter>
	{
		public bool TryGet(FileInfo fi, out ICompetitionWriter writer)
		{
			if (fi.Name.StartsWith("SSFEntry_") && fi.Extension.StartsWith(".xml", StringComparison.InvariantCultureIgnoreCase) && TryCreate(fi, out var ssfWriter))
			{
				writer = ssfWriter;
				return true;
			}
			writer = null;
			return false;
		}
	}

	private abstract class Template
	{
		private const string c_xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<SSFEntry Version=\"1.4.0\" xsi:noNamespaceSchemaLocation=\"SSFEntries.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\r\n  <EventHeader Id=\"10025\">\r\n    <EventName>Korsnäsloppet</EventName>\r\n    <Sector>CC</Sector>\r\n    <Season Code=\"2019\">2018/2019</Season>\r\n    <GeographicalScope>Nationell</GeographicalScope>\r\n    <EventType>Tävling</EventType>\r\n    <Place>Pumpen, Hosjö, Falun</Place>\r\n    <Website>http://www.korsnasifsk.se/contentassets/1997813bd3b3401099d58c1d3ed622f3/korsnasloppet-2019-inbjudan.pdf</Website>\r\n    <Organizer Id=\"12563\">Korsnäs IF SK</Organizer>\r\n  </EventHeader>\r\n  <RaceHeader Id=\"16935\">\r\n    <RaceName>Korsnäsloppet</RaceName>\r\n    <RaceDate>2019-02-17</RaceDate>\r\n  </RaceHeader>\r\n  <Entry Id=\"143465\">\r\n    <Class Id=\"97\" FromAge=\"0\" ToAge=\"8\" Sex=\"L\">D 0-8</Class>\r\n    <Discipline Id=\"29\" Code=\"L\">Intervallstart klassisk stil</Discipline>\r\n    <RaceCategory Code=\"Ung\">Ungdom</RaceCategory>\r\n    <EntryFees>\r\n      <DeadlineEntryFee Deadline=\"2019-02-12\">0</DeadlineEntryFee>\r\n    </EntryFees>\r\n    <SingleEntry CreateDate=\"2019-01-27 19:25:26\" ModifyDate=\"2019-01-27 19:25:26\">\r\n      <Person Id=\"IID3262187\">\r\n        <Lastname>Kallur</Lastname>\r\n        <Firstname>Tyra</Firstname>\r\n        <Sex>L</Sex>\r\n        <Nation>SWE</Nation>\r\n        <BirthDate>2011-09-03</BirthDate>\r\n        <Organization Id=\"12563\">Korsnäs IF SK</Organization>\r\n        <District Id=\"536\">Dalarnas Skidförbund</District>\r\n      </Person>\r\n    </SingleEntry>\r\n  </Entry>\r\n</SSFEntry>";

		internal static XDocument Create()
		{
			return XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<SSFEntry Version=\"1.4.0\" xsi:noNamespaceSchemaLocation=\"SSFEntries.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\r\n  <EventHeader Id=\"10025\">\r\n    <EventName>Korsnäsloppet</EventName>\r\n    <Sector>CC</Sector>\r\n    <Season Code=\"2019\">2018/2019</Season>\r\n    <GeographicalScope>Nationell</GeographicalScope>\r\n    <EventType>Tävling</EventType>\r\n    <Place>Pumpen, Hosjö, Falun</Place>\r\n    <Website>http://www.korsnasifsk.se/contentassets/1997813bd3b3401099d58c1d3ed622f3/korsnasloppet-2019-inbjudan.pdf</Website>\r\n    <Organizer Id=\"12563\">Korsnäs IF SK</Organizer>\r\n  </EventHeader>\r\n  <RaceHeader Id=\"16935\">\r\n    <RaceName>Korsnäsloppet</RaceName>\r\n    <RaceDate>2019-02-17</RaceDate>\r\n  </RaceHeader>\r\n  <Entry Id=\"143465\">\r\n    <Class Id=\"97\" FromAge=\"0\" ToAge=\"8\" Sex=\"L\">D 0-8</Class>\r\n    <Discipline Id=\"29\" Code=\"L\">Intervallstart klassisk stil</Discipline>\r\n    <RaceCategory Code=\"Ung\">Ungdom</RaceCategory>\r\n    <EntryFees>\r\n      <DeadlineEntryFee Deadline=\"2019-02-12\">0</DeadlineEntryFee>\r\n    </EntryFees>\r\n    <SingleEntry CreateDate=\"2019-01-27 19:25:26\" ModifyDate=\"2019-01-27 19:25:26\">\r\n      <Person Id=\"IID3262187\">\r\n        <Lastname>Kallur</Lastname>\r\n        <Firstname>Tyra</Firstname>\r\n        <Sex>L</Sex>\r\n        <Nation>SWE</Nation>\r\n        <BirthDate>2011-09-03</BirthDate>\r\n        <Organization Id=\"12563\">Korsnäs IF SK</Organization>\r\n        <District Id=\"536\">Dalarnas Skidförbund</District>\r\n      </Person>\r\n    </SingleEntry>\r\n  </Entry>\r\n</SSFEntry>");
		}
	}

	private static readonly ILog s_log = LogManager.GetLogger(typeof(SSFCompetitionWriter).Name);

	private readonly string m_path;

	public FlagsCollection Flags { get; set; }

	private SSFCompetitionWriter(FileInfo fi)
	{
		m_path = fi.FullName;
	}

	public void Dispose()
	{
	}

	public void Write(ICompetition competition)
	{
		try
		{
			s_log.Debug("Writing: " + m_path);
			DoWrite(competition);
		}
		catch (Exception ex)
		{
			s_log.Error("Write Failed: " + m_path, ex);
			throw;
		}
	}

	private void DoWrite(ICompetition competition)
	{
		long compID = long.Parse(competition.Id);
		FileInfo fi = new FileInfo(m_path);
		fi.Directory.Create();
		XDocument xDoc = Template.Create();
		XElement root = xDoc.Root;
		WriteHeader(root, competition, compID);
		WriteRaceHeader(root, competition, compID);
		WriteRaceGroups(root, competition, compID);
		fi.Directory.Create();
		xDoc.Save(fi.FullName);
	}

	private void WriteRaceGroups(XElement root, ICompetition competition, long compID)
	{
		XElement xEntry = root.Element("Entry");
		int i = 2;
		foreach (ICompetitionGroup grp in competition.Groups)
		{
			XElement xGrp = new XElement(xEntry);
			WriteRaceGroup(xGrp, grp, compID + i++);
			root.Add(xGrp);
		}
		xEntry.Remove();
	}

	private void WriteRaceGroup(XElement xGrp, ICompetitionGroup grp, long id)
	{
		XElement xClass = xGrp.Element("Class");
		XElement xDiscipline = xGrp.Element("Discipline");
		XElement xCategory = xGrp.Element("RaceCategory");
		XElement xWriteInfoTemplate = xGrp.Element("SingleEntry");
		string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
		CompetitionClass compClass = CompetitionClassFormatter.Swedish.Parse(grp.Class.Name);
		xGrp.SetAttributeValue("Id", id);
		xClass.SetAttributeValue("FromAge", compClass.AgeFrom);
		xClass.SetAttributeValue("ToAge", compClass.AgeFrom);
		xClass.SetAttributeValue("Sex", GenderFormatter.SSF.GetName(compClass.Sex));
		xClass.Value = compClass.Name;
		foreach (IPerson p in grp.Competitors)
		{
			XElement xWriteInfo = new XElement(xWriteInfoTemplate);
			XElement xPerson = xWriteInfo.Element("Person");
			xWriteInfo.SetAttributeValue("CreateDate", now);
			xWriteInfo.SetAttributeValue("ModifyDate", now);
			XElement xOrg = xPerson.Element("Organization");
			XElement xDis = xPerson.Element("District");
			IEntityNameID org = p.Organization;
			xPerson.SetAttributeValue("Id", p.ID);
			xPerson.Element("Lastname").Value = p.Surname;
			xPerson.Element("Firstname").Value = p.FirstName;
			xPerson.Element("Sex").Value = ((p.Sex == Gender.Female) ? "L" : "M");
			xPerson.Element("Nation").Value = p.Nation.Name ?? p.Nation.ID ?? "SWE";
			xPerson.Element("BirthDate").Value = p.DateOfBirth.Date.ToString("yyyy-MM-dd");
			xGrp.Add(xWriteInfo);
		}
		xWriteInfoTemplate.Remove();
	}

	private void WriteRaceHeader(XElement root, ICompetition competition, long compID)
	{
		XElement header = root.Element("RaceHeader");
		header.SetAttributeValue("Id", compID + 2);
		header.Element("RaceName").Value = competition.Name;
		header.Element("RaceDate").Value = competition.RaceDate.ToString("yyyy-MM-dd");
	}

	private void WriteHeader(XElement root, ICompetition competition, long compID)
	{
		XElement header = root.Element("EventHeader");
		XElement xOrg = header.Element("Organizer");
		XElement xSeason = header.Element("Season");
		IEntityNameID org = competition.Organizer;
		CompetitionSeason season = CompetitionSeason.Parse(competition.Season) ?? new CompetitionSeason();
		header.SetAttributeValue("Id", compID);
		header.Element("EventName").Value = competition.Name;
		header.Element("Sector").Value = competition.Sector;
		header.Element("Place").Value = competition.Place;
		header.Element("EventType").Value = competition.EventType;
		header.Element("GeographicalScope").Value = competition.GeographicalScope;
		header.Element("Website").Value = competition.Website;
		xOrg.Value = org.Name;
		xOrg.SetAttributeValue("Id", org.ID);
		xSeason.Value = season.Name;
		xSeason.SetAttributeValue("Code", season.ToYear);
	}

	private static bool TryCreate(FileInfo fi, out SSFCompetitionWriter ssfWriter)
	{
		try
		{
			ssfWriter = new SSFCompetitionWriter(fi);
			return true;
		}
		catch (Exception ex)
		{
			s_log.Error(fi.FullName, ex);
			ssfWriter = null;
			return false;
		}
	}
}
