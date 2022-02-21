using System.Linq;

namespace SSF.Timing.Tools.OrganizationDb.Models;

internal class EntityNameIDData : IEntityNameID, IAutoMapAble
{
	public string Name { get; set; }

	public string ID { get; set; }

	public EntityNameIDData(string name, string id)
	{
		Name = name;
		ID = id;
	}

	public static EntityNameIDData Parse(string str)
	{
		string s = str?.Trim();
		if (s != null && s.Length > 0)
		{
			bool charOnly = s.All(char.IsLetter);
			string name = null;
			string id = null;
			if (charOnly)
			{
				name = s;
			}
			else
			{
				id = s;
			}
			return new EntityNameIDData(name, id);
		}
		return null;
	}
}
