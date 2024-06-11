﻿using AbarimMUD.Storage;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public enum ModifierType
	{
		AttacksCount
	}

	public class Skill
	{
		public static readonly MultipleFilesStorageString<Skill> Storage = new MultipleFilesStorageString<Skill>(r => r.Id, "skills");

		public string Id { get; set; }
		public string Name { get; set; }
		public Dictionary<ModifierType, int> Modifiers { get; set; }

		public static Skill GetSkillById(string name) => Storage.GetByKey(name);
		public static Skill EnsureSkillById(string name) => Storage.EnsureByKey(name);
	}
}
