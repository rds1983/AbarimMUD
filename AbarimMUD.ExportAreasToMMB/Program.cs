﻿using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;
using MUDMapBuilder;
using AbarimMUD.Storage;
using System.Linq;

namespace AbarimMUD.ExportAreasToMMB
{
	internal static class Program
	{
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

		static void Log(string message) => Console.WriteLine(message);

		static void Process()
		{
			var dataDir = Path.Combine(ExecutingAssemblyDirectory, "../../../../Data");
			var db = new DataContext(dataDir, Log);

			var mmbAreas = new List<MMBArea>();
			foreach (var area in db.Areas)
			{
				Log($"Processing area '{area.Name}'");

				var mmbArea = new MMBArea
				{
					Name = area.Name
				};

				foreach(var room in area.Rooms)
				{
					var mmbRoom = new MMBRoom(room.Id, room.Name, false);
					Log($"{mmbRoom}");
					mmbArea.Add(mmbRoom);

					foreach (var exit in room.Exits)
					{
						if (exit.Value == null || exit.Value.TargetRoom == null)
						{
							continue;
						}

						var dir = (MMBDirection)exit.Key;
						var targetRoomId = exit.Value.TargetRoom.Id;
						mmbRoom.Connections[dir] = new MMBRoomConnection(dir, targetRoomId);
					}
				}

				mmbAreas.Add(mmbArea);
			}

			// Build complete dictionary of rooms
			var allRooms = new Dictionary<int, MMBRoom>();
			foreach (var area in mmbAreas)
			{
				foreach (var room in area.Rooms)
				{
					if (allRooms.ContainsKey(room.Id))
					{
						throw new Exception($"Dublicate room id: {room.Id}");
					}

					var areaExit = room.Clone();
					areaExit.Name = $"To {area.Name}";
					areaExit.IsExitToOtherArea = true;

					allRooms[room.Id] = areaExit;
				}
			}

			// Now add areas exits
			foreach (var area in mmbAreas)
			{
				var areaExits = new Dictionary<int, MMBRoom>();
				foreach (var room in area.Rooms)
				{
					var toDelete = new List<MMBDirection>();
					foreach (var exit in room.Connections)
					{
						if (!allRooms.ContainsKey(exit.Value.RoomId))
						{
							toDelete.Add(exit.Key);
						}
					}

					foreach (var d in toDelete)
					{
						room.Connections.Remove(d);
					}

					foreach (var exit in room.Connections)
					{
						MMBRoom inAreaRoom;
						inAreaRoom = (from r in area.Rooms where r.Id == exit.Value.RoomId select r).FirstOrDefault();
						if (inAreaRoom != null)
						{
							continue;
						}

						areaExits[exit.Value.RoomId] = allRooms[exit.Value.RoomId];
					}
				}

				foreach (var pair in areaExits)
				{
					area.Add(pair.Value);
				}
			}

			// Save all areas and generate conversion script
			if (!Directory.Exists("output"))
			{
				Directory.CreateDirectory("output");
			}
			foreach (var area in mmbAreas)
			{
				var fileName = $"{area.Name}.json";
				Log($"Saving {fileName}...");

				// Copy build options
				var project = new MMBProject(area, new BuildOptions());
				var data = project.ToJson();
				File.WriteAllText(Path.Combine(@"output", fileName), data);
			}
		}

		static void Main(string[] args)
		{
			try
			{
				Process();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}

}
