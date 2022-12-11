using System;
using System.Collections.Generic;
using HarmonyLib;
using CoOpSpRpG;
using Microsoft.Xna.Framework;
using WTFModLoader;
using WTFModLoader.Manager;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace SpeedUp
{
    public class SpeedUp : IWTFMod
    {
        public ModLoadPriority Priority => ModLoadPriority.Normal;
        public void Initialize()
        {
            Harmony harmony = new Harmony("blacktea.speedup");
            harmony.PatchAll();
        }
    }

	[HarmonyPatch(typeof(SpaceTerrain), "doCollision")]
	public class SpaceTerrain_doCollision
	{
		[HarmonyPrefix]
		private static bool Prefix(Ship ship)
		{
			if (ship.cosm == null || ship.crewCount == 0 || ship.velocity == Vector2.Zero || ship.velocity.Length() < 30f)
			{
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Shard), "findHitbox")]
	public class Shard_findHitbox
	{

		[HarmonyPrefix]
		private static bool Prefix(Shard __instance)
		{
			if (__instance.bBox == null || __instance.bBox == Rectangle.Empty || Squirrel3RNG.Next(1000) == 0)
			{
				return true;
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(Ship), "buildBoardingList")]
	public class Ship_buildBoardingList
	{

		[HarmonyPrefix]
		private static bool Prefix(Ship __instance)
		{
			int num = __instance.botD.Length;
			if (__instance.boardable == null || __instance.boardable.Length != num || Squirrel3RNG.Next(1000) == 0)
			{
				if (__instance.boardable == null || __instance.boardable.Length != num)
				{
					__instance.boardable = new byte[num];
				}
				for (int i = 0; i < num; i++)
				{
					CoOpSpRpG.Module module = TILEBAG.getModule(__instance.botD[i]);
					if (module != null)
					{
						for (int j = 0; j < module.tiles.Count<ModTile>(); j++)
						{
							int num2 = module.tiles[j].X / 16;
							int num3 = module.tiles[j].Y / 16 * __instance.Width;
							int num4 = num2 + num3 + i;
							if (num4 > 0 && num4 < num && i % __instance.Width + num2 < __instance.Width && i % __instance.Width + num2 >= 0 && !module.tiles[j].blocking && __instance.botD[num4].A >= 100)
							{
								if (module.type == ModuleType.airlock || __instance.topD[num4].A < 50)
								{
									__instance.boardable[num4] = byte.MaxValue;
								}
								else
								{
									__instance.boardable[num4] = 50;
								}
							}
						}
					}
				}
				for (int k = 0; k < num; k++)
				{
					if (__instance.boardable[k] > 0)
					{
						int num5 = 0;
						int num6 = k - 1;
						if (num6 >= 0 && num6 < num && k % __instance.Width < __instance.Width && k % __instance.Width >= 0 && __instance.boardable[num6] > 0)
						{
							num5++;
						}
						num6 = k + 1;
						if (num6 >= 0 && num6 < num && k % __instance.Width < __instance.Width && k % __instance.Width >= 0 && __instance.boardable[num6] > 0)
						{
							num5++;
						}
						num6 = k - __instance.Width;
						if (num6 >= 0 && num6 < num && k % __instance.Width < __instance.Width && k % __instance.Width >= 0 && __instance.boardable[num6] > 0)
						{
							num5++;
						}
						num6 = k + __instance.Width;
						if (num6 >= 0 && num6 < num && k % __instance.Width < __instance.Width && k % __instance.Width >= 0 && __instance.boardable[num6] > 0)
						{
							num5++;
						}
						if (num5 < 2)
						{
							__instance.boardable[k] = 0;
						}
					}
				}
				for (int l = 0; l < num; l++)
				{
					if (__instance.boardable[l] == 50)
					{
						int num7 = l - 1;
						if (num7 >= 0 && num7 < num && l % __instance.Width < __instance.Width && l % __instance.Width >= 0 && (__instance.botD[num7].A < 100 || __instance.topD[num7].A < 50))
						{
							__instance.boardable[l] = byte.MaxValue;
						}
						else
						{
							num7 = l + 1;
							if (num7 >= 0 && num7 < num && l % __instance.Width < __instance.Width && l % __instance.Width >= 0 && (__instance.botD[num7].A < 100 || __instance.topD[num7].A < 50))
							{
								__instance.boardable[l] = byte.MaxValue;
							}
							else
							{
								num7 = l - __instance.Width;
								if (num7 >= 0 && num7 < num && l % __instance.Width < __instance.Width && l % __instance.Width >= 0 && (__instance.botD[num7].A < 100 || __instance.topD[num7].A < 50))
								{
									__instance.boardable[l] = byte.MaxValue;
								}
								else
								{
									num7 = l + __instance.Width;
									if (num7 >= 0 && num7 < num && l % __instance.Width < __instance.Width && l % __instance.Width >= 0 && (__instance.botD[num7].A < 100 || __instance.topD[num7].A < 50))
									{
										__instance.boardable[l] = byte.MaxValue;
									}
									else
									{
										__instance.boardable[l] = 0;
									}
								}
							}
						}
					}
				}
				if (__instance.boardingMask != null)
				{
					__instance.boardingMask.Dispose();
				}
				__instance.boardingMask = new Texture2D(SCREEN_MANAGER.Device, __instance.Width, __instance.Height, false, SurfaceFormat.Alpha8);
				__instance.boardingMask.SetData<byte>(__instance.boardable);
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(FreelancerFactionRev2), "updateFlotillas")]
	public class FreelancerFactionRev2_updateFlotillas
	{
		[HarmonyPrefix]
		private static bool Prefix(FreelancerFactionRev2 __instance, Dictionary<ulong, List<Point>> ___specialFlotillas, List<ulong> ___flotillas, List<Point> ___redZone, List<Point> ___yellowZone, List<Point> ___greyZone)
		{
			foreach (ulong key in ___specialFlotillas.Keys)
			{
				if (!__instance.ships.ContainsKey(key))
				{
					___specialFlotillas.Remove(key);
					break;
				}
				else if (__instance.ships[key].isAtDestination)
				{
					List<Point> list = ___specialFlotillas[key];
					Point g = list[RANDOM.Next(list.Count)];
					__instance.ships[key].patrolToGridRandom(g);
				}
			}
			if (Squirrel3RNG.Next(1000) == 0)
			{
				for (int i = 0; i < ___flotillas.Count(); i++)
				{
					ulong num = ___flotillas[i];
					if (__instance.ships.ContainsKey(num))
					{
						if (__instance.ships[num].isAtDestination)
						{
							Point grid = __instance.ships[num].grid;
							Point g2 = grid;
							if (RANDOM.Next(5) == 0)
							{
								int num2 = RANDOM.Next(2);
								if (num2 != 0)
								{
									if (num2 == 1)
									{
										g2 = ___yellowZone[RANDOM.Next(___yellowZone.Count)];
									}
								}
								else
								{
									g2 = ___greyZone[RANDOM.Next(___greyZone.Count)];
								}
							}
							else
							{
								if (___greyZone.Contains(grid))
								{
									g2 = ___greyZone[RANDOM.Next(___greyZone.Count)];
								}
								if (___yellowZone.Contains(grid))
								{
									g2 = ___yellowZone[RANDOM.Next(___yellowZone.Count)];
								}
								if (___redZone.Contains(grid))
								{
									g2 = ___redZone[RANDOM.Next(___redZone.Count)];
								}
								g2.X -= 8;
								g2.X += RANDOM.Next(17);
								g2.Y -= 8;
								g2.Y += RANDOM.Next(17);
							}
							__instance.ships[num].patrolToGridRandom(g2);
						}
					}
					else
					{
						___flotillas.Remove(num);
						break;
					}
				}
			}
			return false; //instruction for harmony to supress executing the original method
		}
	}

}
