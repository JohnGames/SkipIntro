﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityModManagerNet;
using Harmony;
using System.Reflection;
using static UnityModManagerNet.UnityModManager;
using System.IO;
using System.Collections;

namespace SkipIntro
{
	static class Main
	{
		public static ModEntry mod;

		// Send a response to the mod manager about the launch status, success or not.
		static bool Load(ModEntry modEntry)
		{
			var harmony = HarmonyInstance.Create(modEntry.Info.Id);
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			if(modEntry.GameVersion != gameVersion)
			{
				UnityModManager.Logger("Not same version!");
			}
			return true; // If false the mod will show an error.
		}

	}

	[HarmonyPatch(typeof(Boot))]
	[HarmonyPatch("Start")]
	static class SkipIntro
	{

		static void Postfix(Boot __instance, ref int ___m_Stage)
		{
			Traverse traverse = Traverse.Create(__instance);
			traverse.Field<bool>("m_SkipVideo").Value = true;
			int limit = traverse.Field<string[]>("m_Names").Value.Length;
			while (___m_Stage < limit)
			{
				___m_Stage++;
				LoadManager(___m_Stage);
			}
			traverse.Method("AllLoaded");
			

			void LoadManager(int i)
			{
				string Name = traverse.Field<string[]>("m_Names").Value[i];
				traverse.Method("LoadManager", new[] { Name });
			}
		}

	}

}

