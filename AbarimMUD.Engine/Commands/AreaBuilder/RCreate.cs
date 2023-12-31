﻿using AbarimMUD.Data;

namespace AbarimMUD.Commands.AreaBuilder
{
	public sealed class RCreate : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			// Create new room
			var newRoom = new Room
			{
				Name = "Empty",
				Description = "Empty"
			};

			Database.CreateRoom(context.CurrentRoom.Area, newRoom);

			context.SendTextLine(string.Format("New room (#{0}) had been created for the area {1} (#{2})",
				newRoom.Id,
				context.CurrentRoom.Area.Name,
				context.CurrentRoom.Area.Id));

			new Goto().Execute(context, newRoom.Id.ToString());
		}
	}
}