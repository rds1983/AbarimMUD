﻿using AbarimMUD.Data;
using System.Linq;

namespace AbarimMUD.Commands.AreaBuilder
{
	internal class ISpawn : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var pat = data.Trim().ToLower();
			int id;
			if (string.IsNullOrEmpty(pat) || !int.TryParse(pat, out id))
			{
				context.SendTextLine("Usage: ispawn _id_");
				return;
			}
			else
			{
				var item = (from i in Item.Storage where i.Id == id select i).FirstOrDefault();
				if (item == null)
				{
					context.SendTextLine($"Unable to find an item with id {id}");
				}
				else
				{
					context.Creature.Inventory.AddItem(new ItemInstance(item) { Quantity = 1 });
					context.SendTextLine($"{item} appeared in your inventory");

					var asPlayer = context.Creature as Character;
					asPlayer?.Save();
				}
			}
		}
	}
}