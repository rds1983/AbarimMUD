﻿using AbarimMUD.Data;

namespace AbarimMUD.Commands.AreaBuilder
{
	public sealed class MCreate : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			// Create new mobile
			var newMobileInfo = new Mobile
			{
				Area = context.CurrentRoom.Area,
				Name = "unset",
				ShortDescription = "Unset",
				LongDescription = "A mobile with 'unset' name is standing here.",
				Description = "Unset."
			};

			Database.CreateMobile(newMobileInfo);

			context.SendTextLine(string.Format("New mobile info (#{0}) had been created for the area {1} (#{2})",
				newMobileInfo.Id,
				context.CurrentRoom.Area.Name,
				context.CurrentRoom.Area.Id));

			new MSpawn().Execute(context, newMobileInfo.Id.ToString());
		}
	}
}