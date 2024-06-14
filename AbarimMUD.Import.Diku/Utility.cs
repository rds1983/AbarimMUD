﻿using AbarimMUD.Data;
using System.Linq;

namespace AbarimMUD.Import.Diku
{
	internal static class Utility
	{
		public static Direction ToAMDirection(this DikuLoad.Data.Direction dir) => (Data.Direction)dir;

		public static Room ToAmRoom(this DikuLoad.Data.Room room)
		{
			var result = new Room
			{
				Id = room.VNum,
				Name = room.Name,
				Description = room.Description
			};

			foreach (var exit in room.Exits)
			{
				if (exit.Value == null || exit.Value.TargetRoom == null)
				{
					continue;
				}

				var roomExit = new RoomExit
				{
					Direction = exit.Key.ToAMDirection(),
					Tag = exit.Value.TargetRoom.VNum
				};

				result.Exits[roomExit.Direction] = roomExit;
			}

			return result;
		}

		public static Mobile ToAmMobile(this DikuLoad.Data.Mobile mobile)
		{
			var raceId = string.IsNullOrEmpty(mobile.Race) ? "human" : mobile.Race;
			var classId = "warrior";

			var result = new Mobile
			{
				Id = mobile.VNum,
				Keywords = mobile.Name.SplitByWhitespace().ToHashSet(),
				ShortDescription = mobile.ShortDescription,
				LongDescription = mobile.LongDescription,
				Description = mobile.Description,
				Race = Race.EnsureRaceById(raceId),
				Class = GameClass.EnsureClassById(classId),
				Level = mobile.Level,
			};

			return result;
		}

		public static Area ToAmArea(this DikuLoad.Data.Area area)
		{
			var result = new Area
			{
				Name = area.Name,
				Credits = area.Builders,
				MinimumLevel = area.MinimumLevel,
				MaximumLevel = area.MaximumLevel
			};

			foreach (var room in area.Rooms)
			{
				result.Rooms.Add(room.ToAmRoom());
			}

			foreach (var mobile in area.Mobiles)
			{
				result.Mobiles.Add(mobile.ToAmMobile());
			}

			return result;
		}
	}
}
