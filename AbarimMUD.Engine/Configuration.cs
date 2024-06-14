﻿namespace AbarimMUD
{
	public static class Configuration
	{
		public static int ServerPort => 6101;
		public static string WebServiceUrl => "http://localhost:8080/AbarimMUD/";
		public static string SplashFile => string.Empty;
		public static int StartRoomId { get; set; } = 3001;
		// public static int StartRoomId => 1455;
		// public static int StartRoomId => 5135;
		public static string DataFolder;
		public static string DefaultCharacter = "Yang";
		public static string DefaultRace = "human";
		public static string DefaultClass = "warrior";

		public static int PauseBetweenFightRoundsInMs { get; set; } = 3000;
	}
}
