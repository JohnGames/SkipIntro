using System;
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
using UnityEngine.Video;
using System.Reflection.Emit;

namespace SkipIntro
{
	static class Main
	{

		// Send a response to the mod manager about the launch status, success or not.
		static bool Load(ModEntry modEntry)
		{
			var harmony = HarmonyInstance.Create(modEntry.Info.Id);

			if (modEntry.GameVersion != gameVersion)
			{
				UnityModManager.Logger.Log($"Skip Intro expects {modEntry.GameVersion} but found {gameVersion}.");
			}

			harmony.PatchAll(Assembly.GetExecutingAssembly());
			return true; // If false the mod will show an error.
		}

	}

	static class SkipIntroParent
	{
		[HarmonyPatch(typeof(Boot))]
		[HarmonyPatch("Start")]
		public static class SkipIntro
		{

			static void Postfix(Boot __instance, ref int ___m_Stage)
			{
				Traverse traverse = Traverse.Create(__instance);
				traverse.Field<bool>("m_SkipVideo").Value = true;
				traverse.Field<VideoPlayer>("m_Video").Value.gameObject.SetActive(false);
			}

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var ins = instructions.Last<CodeInstruction>(instruction => instruction.opcode == OpCodes.Ldarg_0);
				var codes = instructions.ToList<CodeInstruction>();
				var location = codes.IndexOf(ins);

				for (int i = 0; i <= 2; i++)
				{
					instructions.ElementAt(location+i).opcode = OpCodes.Nop;
				}
				return codes;
			}

		}

		[HarmonyPatch(typeof(Boot))]
		[HarmonyPatch("LoadManager")]
		public static class LoadManager
		{
			static bool Prefix(string Name)
			{
				UnityModManager.Logger.Log($"Loading {Name}");
				return true;
			}
		}

		[HarmonyPatch(typeof(Boot))]
		[HarmonyPatch("Update")]
		public static class UpdateSkipintro
		{
			static bool Prefix(Boot __instance, ref int ___m_Stage)
			{

				Traverse traverse = Traverse.Create(__instance);
				int limit = traverse.Field<string[]>("m_Names").Value.Length;
				if (___m_Stage != limit)
				{
					LoadManagerPass(traverse, ___m_Stage);
					___m_Stage++;

					if (___m_Stage == limit)
					{
						UnityModManager.Logger.Log($"Calling AllLoaded");
						traverse.Method("AllLoaded").GetValue();
					}

				}

				return false;

				
			}

			static void LoadManagerPass(Traverse traverse, int i)
			{
				string Name = traverse.Field<string[]>("m_Names").Value[i];
				Type[] paramsTypes = new Type[]
				{
					typeof(string)
				};
				object[] arguments = new object[]
				{
					Name
				};
				var methods = traverse.Methods();
				traverse.Method("LoadManager", paramsTypes, arguments).GetValue();
			}


		}
	}

}

