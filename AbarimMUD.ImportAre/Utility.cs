﻿using AbarimMUD.Data;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace AbarimMUD.ImportAre
{
	internal static class Utility
	{
		private enum MobClass
		{
			Warrior,
			Thief,
			Mage,
			Cleric
		}

		private struct GuildThac0
		{
			public int Level1;
			public int Level32;

			public GuildThac0(int level1, int level32)
			{
				Level1 = level1;
				Level32 = level32;
			}
		}

		private static readonly int[][] _mobAttacksTable;
		private static readonly GuildThac0[] _guildThacs = new GuildThac0[]
		{
			new GuildThac0(20, -10),
			new GuildThac0(20, -4),
			new GuildThac0(20, 2),
			new GuildThac0(20, -2),
		};

		public static string ExecutingAssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().Location;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		static Utility()
		{
			_mobAttacksTable = new int[8][];

			_mobAttacksTable[(int)MobClass.Warrior] = new int[]
			{
				5,
				12,
				25
			};

			_mobAttacksTable[(int)MobClass.Thief] = new int[]
			{
				12,
				25
			};

			_mobAttacksTable[(int)MobClass.Mage] = new int[]
			{
				23
			};


			_mobAttacksTable[(int)MobClass.Cleric] = new int[]
			{
				20,
				30
			};
		}

		public static void RaiseError(this Stream stream, string message)
		{
			var line = 0;
			var linePos = 0;

			var lastPos = stream.Position;

			// Calculate the line and line pos
			stream.Seek(0, SeekOrigin.Begin);

			while (stream.Position < lastPos)
			{
				var b = stream.ReadByte();

				++linePos;

				if (b == '\n')
				{
					++line;
					linePos = 0;
				}
			}

			var asFileStream = stream as FileStream;
			if (asFileStream != null)
			{
				var fileName = asFileStream.Name;
				throw new Exception($"File: {fileName}, Line: {line}, LinePos: {linePos}, Error: {message}");
			}

			throw new Exception($"Line: {line}, LinePos: {linePos}, Error: {message}");
		}

		public static string RemoveTilda(this string str) => str.Replace("~", "");

		public static int ParseVnum(this string str) => int.Parse(str.Substring(1));


		public static bool EndOfStream(this Stream stream) => stream.Position >= stream.Length;

		public static void GoBackIfNotEOF(this Stream stream)
		{
			if (stream.Position == 0 || stream.EndOfStream())
			{
				return;
			}

			stream.Seek(-1, SeekOrigin.Current);
		}

		public static char ReadLetter(this Stream stream) => (char)stream.ReadByte();

		public static char ReadSpacedLetter(this Stream stream)
		{
			char c;
			do
			{
				c = stream.ReadLetter();
			}
			while (!stream.EndOfStream() && char.IsWhiteSpace(c));

			return c;
		}

		public static string ReadLine(this Stream stream)
		{
			var sb = new StringBuilder();

			var endOfLine = false;
			while (!stream.EndOfStream())
			{
				var c = stream.ReadLetter();

				var isNewLine = c == '\n' || c == '\r';
				if (!endOfLine)
				{
					if (!isNewLine)
					{
						sb.Append(c);
					}
					else
					{
						endOfLine = true;
					}
				}
				else if (!isNewLine)
				{
					stream.GoBackIfNotEOF();
					break;
				}

			}

			return sb.ToString();
		}


		public static string ReadId(this Stream stream)
		{
			var c = stream.ReadSpacedLetter();
			if (c != '#')
			{
				stream.RaiseError("# not found");
			}

			return stream.ReadLine().Trim();
		}

		public static int ReadFlag(this Stream stream)
		{
			int result = 0;
			var negative = false;
			var c = stream.ReadSpacedLetter();

			if (c == '-')
			{
				negative = true;
				c = stream.ReadLetter();
			}

			if (!char.IsDigit(c))
			{
				while (!stream.EndOfStream())
				{
					int bitsum = 0;
					if ('A' <= c && c <= 'Z')
					{
						bitsum = 1;
						for (int i = c; i > 'A'; i--)
						{
							bitsum *= 2;
						}
					}
					else if ('a' <= c && c <= 'z')
					{
						bitsum = 67108864;
						for (int i = c; i > 'a'; i--)
						{
							bitsum *= 2;
						}
					}
					else
					{
						break;
					}

					result += bitsum;
					c = stream.ReadLetter();
				}
			}
			else
			{
				var sb = new StringBuilder();

				while (!stream.EndOfStream() && char.IsDigit(c))
				{
					sb.Append(c);
					c = stream.ReadLetter();
				}

				result = int.Parse(sb.ToString());
			}

			if (c == '|')
			{
				result += stream.ReadFlag();
			}

			// Last symbol beint to the new data
			stream.GoBackIfNotEOF();

			return negative ? -result : result;
		}

		public static int ReadNumber(this Stream stream)
		{
			var negative = false;
			var c = stream.ReadSpacedLetter();

			if (c == '+')
			{
				c = stream.ReadLetter();
			}
			else if (c == '-')
			{
				negative = true;
				c = stream.ReadLetter();
			}

			if (!char.IsDigit(c))
			{
				stream.RaiseError($"Could not parse number {c}");
			}

			var sb = new StringBuilder();

			while (!stream.EndOfStream() && char.IsDigit(c))
			{
				sb.Append(c);
				c = stream.ReadLetter();
			}

			var result = int.Parse(sb.ToString());

			if (negative)
			{
				result = -result;
			}

			if (c == '|')
			{
				result += stream.ReadNumber();
			}

			// Last symbol beint to the new data
			stream.GoBackIfNotEOF();

			return result;
		}


		public static string ReadDikuString(this Stream stream)
		{
			var result = new StringBuilder();

			var c = stream.ReadSpacedLetter();
			while (!stream.EndOfStream())
			{
				if (c == '~')
				{
					break;
				}

				result.Append(c);
				c = stream.ReadLetter();
			}

			return result.ToString();
		}

		public static char EnsureChar(this Stream stream, char expected)
		{
			var c = stream.ReadLetter();
			if (c != expected)
			{
				stream.RaiseError($"Expected symbol '{expected}'");
			}

			return c;
		}

		public static string ReadDice(this Stream stream)
		{
			var sb = new StringBuilder();
			sb.Append(stream.ReadNumber());
			sb.Append(stream.EnsureChar('d'));
			sb.Append(stream.ReadNumber());
			sb.Append(stream.EnsureChar('+'));
			sb.Append(stream.ReadNumber());

			return sb.ToString();
		}

		public static string ReadWord(this Stream stream)
		{
			if (stream.EndOfStream())
			{
				return string.Empty;
			}

			var sb = new StringBuilder();
			var c = stream.ReadSpacedLetter();

			var startsWithQuote = c == '"' || c == '\'';

			if (startsWithQuote)
			{
				c = stream.ReadLetter();
			}

			while (!stream.EndOfStream())
			{
				if ((startsWithQuote && (c == '"' || c == '\'')) ||
					(!startsWithQuote && char.IsWhiteSpace(c)))
				{
					break;
				}

				sb.Append(c);
				c = stream.ReadLetter();
			}

			if (!startsWithQuote)
			{
				stream.GoBackIfNotEOF();
			}

			return sb.ToString();
		}

		public static T ToEnum<T>(this Stream stream, string value)
		{
			value = value.Replace("_", "").Replace(" ", "");

			// Parse the enum
			try
			{
				if (typeof(T) == typeof(AttackType))
				{
					if (value == "none" || value == "magic" || value == "divine" || value == "wrath")
					{
						value = "hit";
					}

					if (value == "peckb")
					{
						value = "peck";
					}

					if (value == "shbite" || value == "flbite" || value == "frbite" || value == "acbite" || value == "digestion")
					{
						value = "bite";
					}

					if (value == "thwack")
					{
						value = "hack";
					}

					if (value == "drain")
					{
						value = "slash";
					}

					if (value == "flame")
					{
						value = "pound";
					}
				}

				return (T)Enum.Parse(typeof(T), value, true);
			}
			catch (Exception)
			{
				stream.RaiseError($"Enum parse error: enum type={typeof(T).Name}, value={value}");
			}

			return default(T);
		}

		public static T ReadEnumFromDikuString<T>(this Stream stream)
		{
			var str = stream.ReadDikuString();
			return stream.ToEnum<T>(str);
		}

		public static T ReadEnumFromDikuStringWithDef<T>(this Stream stream, T def)
		{
			var str = stream.ReadDikuString();

			if (string.IsNullOrEmpty(str))
			{
				return def;
			}

			return stream.ToEnum<T>(str);
		}

		public static T ReadEnumFromWord<T>(this Stream stream)
		{
			var word = stream.ReadWord();
			return stream.ToEnum<T>(word);
		}

		public static T ReadEnumFromWordWithDef<T>(this Stream stream, T def)
		{
			var word = stream.ReadWord();

			if (string.IsNullOrEmpty(word))
			{
				return def;
			}

			return stream.ToEnum<T>(word);
		}

		public static bool ReadSocialString(this Stream stream, ref string s)
		{
			var temp = stream.ReadLine().Trim();
			if (temp == "$")
			{
				s = string.Empty;
			}
			else if (temp == "#")
			{
				return false;
			}
			else
			{
				s = temp;
			}

			return true;
		}

		private static MobClass GetMobClass(this Mobile mob)
		{
			var mc = MobClass.Warrior;
			if (mob.Flags.HasFlag(MobileFlags.Thief))
			{
				mc = MobClass.Thief;
			}
			else if (mob.Flags.HasFlag(MobileFlags.Mage))
			{
				mc = MobClass.Mage;
			}
			else if (mob.Flags.HasFlag(MobileFlags.Cleric))
			{
				mc = MobClass.Cleric;
			}

			return mc;
		}

		public static int GetAttacksCount(this Mobile mob)
		{
			var result = 1;

			// Area attack
			if (mob.OffenseFlags.HasFlag(MobileOffensiveFlags.AreaAttack))
			{
				++result;
			}

			var mc = mob.GetMobClass();
			var levelsTable = _mobAttacksTable[(int)mc];

			for (var i = 0; i < levelsTable.Length; i++)
			{
				if (mob.Level < levelsTable[i])
				{
					break;
				}

				++result;
			}

			return result;
		}

		public static int GetAccuracy(this Mobile mob)
		{
			var mc = mob.GetMobClass();

			var guildThac = _guildThacs[(int)mc];
			var thac0 = guildThac.Level1 + mob.Level * (guildThac.Level32 - guildThac.Level1) / 32;

			if (thac0 < 0)
				thac0 = thac0 / 2;

			if (thac0 < -5)
				thac0 = -5 + (thac0 + 5) / 2;

			return -(thac0 - 20) * 10;
		}
	}
}