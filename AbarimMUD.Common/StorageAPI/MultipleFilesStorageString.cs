﻿using AbarimMUD.Data;
using System;

namespace AbarimMUD.Storage
{
	public class MultipleFilesStorageString<ItemType> : MultipleFilesStorage<string, ItemType> where ItemType : StoredInFile
	{
		public MultipleFilesStorageString(Func<ItemType, string> keyGetter, string subFolderName, bool ignoreCase = true) :
			base(keyGetter, subFolderName, ignoreCase ? (key => key.ToLower()) : null)
		{
		}
	}
}
