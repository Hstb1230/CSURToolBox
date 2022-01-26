﻿using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using CSURToolBox.UI;
using CSURToolBox.Util;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace CSURToolBox.Patch
{
    [HarmonyPatch]
    public static class NetManagerCreateSegmentPatch
	{
		public static MethodBase TargetMethod()
        {
			// public bool CreateSegment(out ushort, ref Randomizer, NetInfo, TreeInfo, ushort, ushort, Vector3, Vector3, uint, uint, bool)
			Type[] argTypes = new Type[]
			{
				typeof(ushort).MakeByRefType(),
				typeof(Randomizer).MakeByRefType(),
				typeof(NetInfo),
				typeof(TreeInfo),
				typeof(ushort),
				typeof(ushort),
				typeof(Vector3),
				typeof(Vector3),
				typeof(uint),
				typeof(uint),
				typeof(bool)
			};
			return typeof(NetManager).GetMethod("CreateSegment", argTypes);
        }
        public static void Postfix(ref NetManager __instance, ref ushort segment, ref bool __result)
        {
			if (OptionUI.fixLargeJunction)
			{
				if (__result)
				{
					if (__instance.m_segments.m_buffer[segment].Info.m_vehicleTypes.IsFlagSet(VehicleInfo.VehicleType.Car))
					{
						//DebugLog.LogToFileOnly($"Update segment to fix a wierd issue = {segment}");
						__instance.m_segments.m_buffer[segment].CalculateSegment(segment);
						__instance.m_segments.m_buffer[segment].UpdateBounds(segment);
						__instance.m_segments.m_buffer[segment].UpdateLanes(segment, false);
						__instance.m_segments.m_buffer[segment].UpdateSegment(segment);
						__instance.UpdateSegmentFlags(segment);
						__instance.UpdateSegmentRenderer(segment, true);
					}
				}
			}
        }

		public static void CalculateSegmentDirections(ref NetSegment segment, ushort segmentID)
		{
			if (segment.m_flags != NetSegment.Flags.None)
			{
				segment.m_startDirection.y = 0;
				segment.m_endDirection.y = 0;

				segment.m_startDirection.Normalize();
				segment.m_endDirection.Normalize();

				segment.m_startDirection = segment.FindDirection(segmentID, segment.m_startNode);
				segment.m_endDirection = segment.FindDirection(segmentID, segment.m_endNode);
			}
		}
	}
}
