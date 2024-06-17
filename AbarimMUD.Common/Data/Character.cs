﻿using AbarimMUD.Attributes;
using AbarimMUD.Storage;
using System;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public enum Role
	{
		Player,
		Builder,
		Administrator,
		Owner
	}

	public sealed class Character : Creature, IStoredInFile
	{
		public static readonly MultipleFilesStorage<Character> Storage = new Characters();

		private GameClass _class;
		private int _level;
		private long _experience;

		[JsonIgnore]
		public Account Account { get; set; }

		public Role Role { get; set; }

		[JsonIgnore]
		public bool IsStaff
		{
			get { return Role >= Role.Builder; }
		}

		[JsonIgnore]
		public string Id
		{
			get => Name;
			set => Name = value;
		}

		public string Name { get; set; }

		[OLCAlias("description")]
		public string PlayerDescription { get; set; }

		[OLCAlias("class")]

		public GameClass PlayerClass
		{
			get => _class;

			set
			{
				_class = value;
				InvalidateStats();
			}
		}

		[OLCAlias("level")]
		public int PlayerLevel
		{
			get => _level;
			set
			{
				if (value < 1)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				_level = value;
				InvalidateStats();
			}
		}

		[OLCAlias("sex")]
		public Sex PlayerSex { get; set; }

		public long Wealth { get; set; }

		public long Experience
		{
			get => _experience;

			set
			{
				if (value == _experience)
				{
					return;
				}

				_experience = value;

				// Process level ups
				UpdateLevel();
			}
		}

		public override string ShortDescription => Name;
		public override string Description => PlayerDescription;
		public override GameClass Class => PlayerClass;
		public override int Level => PlayerLevel;
		public override Sex Sex => PlayerSex;

		[JsonIgnore]
		public object Tag { get; set; }

		public Character()
		{
		}

		private void UpdateLevel()
		{
			if (_level == 0)
			{
				// Level not set
				return;
			}

			var changed = false;
			while (_level < Configuration.MaximumLevel)
			{
				var levelInfo = LevelInfo.GetLevelInfo(_level + 1);
				if (_experience < levelInfo.Experience)
				{
					break;
				}

				changed = true;
				_experience -= levelInfo.Experience;
				++_level;
			}

			if (changed)
			{
				InvalidateStats();
				Save();
			}
		}

		public override string ToString() => $"{Name}, {Role}, {Class.Name}, {Level}";

		public void Create() => Storage.Create(this);
		public void Save()
		{
			if (Account != null)
			{
				Storage.Save(this);
			}
		}

		public string BuildCharacterFolder()
		{
			var result = Account.BuildAccountFolder();

			// Add character name in the path
			result = Path.Combine(result, Name);

			return result;
		}

		public override bool MatchesKeyword(string keyword) => Name.StartsWith(keyword, StringComparison.OrdinalIgnoreCase);

		public static Character GetCharacterByName(string name) => Storage.GetByKey(name);
		public static Character EnsureCharacterByName(string name) => Storage.EnsureByKey(name);
		public static Character LookupCharacter(string name) => Storage.Lookup(name);
		public static Character[] GetCharactersByAccountName(string name)
		{
			var result = (from c in Storage where c.Account.Name.ToLower() == name.ToLower() select c).ToArray();

			return result;
		}
	}
}