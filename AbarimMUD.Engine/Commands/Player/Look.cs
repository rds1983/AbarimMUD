﻿using System.Text;
using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public sealed class Look : PlayerCommand
	{
		private string BuildRoomDescription(ExecutionContext context, Room room)
		{
			var sb = new StringBuilder();
			sb.Append(ConsoleCommand.ForeColorCyan);

			var name = room.Name;
			if (context.IsStaff)
			{
				name += string.Format(" (#{0})", room.Id);
			}

			sb.AddTextLine(name);
			sb.Append(ConsoleCommand.ColorClear);
			sb.Append("   ");
			sb.AddTextLine(room.Description);
			sb.Append(ConsoleCommand.ForeColorCyan);
			sb.Append("Exits: ");

			var first = true;
			foreach (var pair in room.Exits)
			{
				var exit = pair.Value;
				if (!first)
				{
					sb.Append(ConsoleCommand.ColorClear);
					sb.Append(" ");
				}

				sb.Append(ConsoleCommand.ForeColorCyan);
				sb.Append(ConsoleCommand.Underline);
				sb.Append(exit.Direction.GetName());

				if (context.IsStaff)
				{
					sb.Append(string.Format("(#{0})", exit.TargetRoom.Id));
				}

				first = false;
			}

			sb.AddNewLine();
			sb.Append(ConsoleCommand.ColorClear);

			// Mobiles
			foreach (var mobile in room.Mobiles)
			{
				var desc = mobile.Info.LongDescription;
				if (context.IsStaff)
				{
					desc += string.Format(" (#{0})", mobile.Info.Id);
				}
				sb.AddTextLine(desc);
			}

			// Characters
			foreach (var character in room.Characters)
			{
				var asPlayer = context as PlayerExecutionContext;
				if (asPlayer != null && asPlayer.Session.Character == character)
				{
					continue;
				}

				sb.AddTextLine(string.Format("{0} is standing here.", character.Name));
			}

			return sb.ToString();
		}

		private string BuildMobileDescription(ExecutionContext context, Creature creature)
		{
			var sb = new StringBuilder();

			var mobile = creature as MobileInstance;
			if (mobile != null)
			{
				sb.AppendLine(mobile.Info.Description);
			}

			if (context.IsStaff)
			{
				sb.Append(ConsoleCommand.ForeColorCyan);

				if (mobile != null)
				{
					sb.AppendLine("Id: " + mobile.Info.Id);
					sb.AppendLine("Keywords: " + mobile.Info.Name);
					sb.AppendLine("Short: " + mobile.Info.ShortDescription);
					sb.AppendLine("Long: " + mobile.Info.LongDescription);
				}

				sb.AppendLine("Race: " + creature.Race.Name);
				sb.AppendLine("Class: " + creature.Class.Name);
				sb.AppendLine("Level: " + creature.Level);

				sb.AppendLine();

				var stats = creature.Stats;
				sb.AppendLine($"Hitpoints: {creature.State.Hitpoints}/{stats.MaxHitpoints}");
				sb.AppendLine("Armor Class: " + creature.Stats.Armor);
				for (var i = 0; i < stats.Attacks.Length; i++)
				{
					var attack = stats.Attacks[i];
					sb.AppendLine($"Attack {i + 1}: {attack}");
				}

				sb.Append(ConsoleCommand.ColorClear);
			}

			return sb.ToString();
		}

		private string BuildItemDescription(ExecutionContext context, ItemInstance item)
		{
			var sb = new StringBuilder();

			sb.AppendLine(item.Info.Description);
			if (context.IsStaff)
			{
				sb.Append(ConsoleCommand.ForeColorCyan);

				if (item != null)
				{
					sb.AppendLine("Id: " + item.Info.Id);
					sb.AppendLine("Keywords: " + item.Info.Name);
					sb.AppendLine("Short: " + item.Info.ShortDescription);
					sb.AppendLine("Long: " + item.Info.LongDescription);
				}

				sb.AppendLine("Type: " + item.Info.ItemType);
				sb.AppendLine("Value1: " + item.Info.Value1);
				sb.AppendLine("Value2: " + item.Info.Value2);
				sb.AppendLine("Value3: " + item.Info.Value3);
				sb.AppendLine("Value4: " + item.Info.Value4);

				sb.Append(ConsoleCommand.ColorClear);
			}

			return sb.ToString();
		}

		protected override void InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();

			do
			{
				if (string.IsNullOrEmpty(data))
				{
					// Look room
					var sd = BuildRoomDescription(context, context.CurrentRoom);
					context.Send(sd);
					break;
				}

				// Look for creature in room
				var lookContext = context.CurrentRoom.Find(data);
				if (lookContext != null)
				{
					context.Send($"You look at {lookContext.Name}.\n");

					var d = BuildMobileDescription(context, lookContext.Creature);
					context.Send(d);

					if (lookContext != context)
					{
						lookContext.SendTextLine(string.Format("{0} looks at you.", context.Name));
					}

					foreach (var t in context.AllExceptMeInRoom())
					{
						if (t != lookContext)
						{
							t.SendTextLine(string.Format("{0} looks at {1}.", context.Name, lookContext.Name));
						}
					}

					break;
				}

				// Look for an item in inv
				ItemInstance item = null;
				foreach (var i in context.Creature.Inventory.Items)
				{
					if (i.Keywords.StartsWithPattern(new[] { data }))
					{
						item = i;
						break;
					}
				}

				if (item != null)
				{
					var d = BuildItemDescription(context, item);
					context.Send(d);
					break;
				}

				context.SendTextLine(string.Format("There isnt '{0}' in this room", data));
			} while (false);
		}
	}
}
