﻿using AbarimMUD.Data;
using AbarimMUD.Utils;
using System;

namespace AbarimMUD.Commands.AreaBuilder
{
	internal class MAddAttack : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			int id, accuracy, minDamage, maxDamage;
			string typeStr;
			if (!data.Parse(out id, out typeStr, out accuracy, out minDamage, out maxDamage))
			{
				context.Send("Usage: maddattack _mobileId_ _type_ _accuracy_ _minDamage_ _maxDamage_");
				return;
			}

			AttackType attackType;
			if (!typeStr.TryParseEnumUncased(out attackType))
			{
				context.Send($"Unable to parse attack type {typeStr}");
				return;
			}

			if (minDamage < 0)
			{
				context.Send($"minDamage couldn't be negative");
				return;
			}

			if (maxDamage < 0)
			{
				context.Send($"maxDamage couldn't be negative");
				return;
			}

			if (minDamage > maxDamage)
			{
				context.Send($"minDamage couldn't be bigger than maxDamage");
				return;
			}

			var area = context.CurrentArea;
			var mobileInfo = area.GetMobileById(id);
			if (mobileInfo == null)
			{
				context.Send($"Unable to find mobile info with id {id}");
				return;
			}

			var newAttack = new Attack
			{
				AttackType = attackType,
				Accuracy = accuracy,
				MinimumDamage = minDamage,
				MaximumDamage = maxDamage
			};

			mobileInfo.Attacks.Add(newAttack);
			Database.Areas.Update(area);
			context.Send($"Added #{mobileInfo.Attacks.Count - 1} attack ({newAttack}) to {mobileInfo.ShortDescription} (#{mobileInfo.Id})");
		}
	}
}
