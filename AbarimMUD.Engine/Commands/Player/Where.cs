﻿using System.Text;
using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public sealed class Where : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var area = context.CurrentRoom.Area;

			var sb = new StringBuilder();
			sb.Append(ConsoleCommand.ForeColorCyan);

			var name = area.Name;
			if (context.Type >= (Role.AreaBuilder))
			{
				name += string.Format(" (#{0})", area.Id);
			}

			sb.AddTextLine(name);

			context.Send(sb.ToString());
		}
	}
}