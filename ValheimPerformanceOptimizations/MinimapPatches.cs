﻿using System.IO;
using HarmonyLib;
using UnityEngine;

namespace ValheimPerformanceOptimizations
{
    /// <summary>
    /// Generating the minimap takes a lot of time at startup and is generated from WorldGenerator. Therefore the
    /// minimap never changes in a given world.
    /// Now it is being saved inside the world folder as PNGs (there are multiple for different masks). The file names
    /// are worldName_worldSeed_gameVersion_textureType.png
    /// </summary>
    [HarmonyPatch(typeof(Minimap), "GenerateWorldMap")]
    public static class MinimapPatches
    {
        private static bool Prefix(Minimap __instance)
        {
            // try to load existing textures
            if (ImageFilesExists())
            {
                __instance.m_forestMaskTexture.LoadImage(File.ReadAllBytes(ForestMaskTexturePath()));
                __instance.m_mapTexture.LoadImage(File.ReadAllBytes(MapTexturePath()));
                __instance.m_heightTexture.LoadRawTextureData(File.ReadAllBytes(HeightTexturePath()));
                return false;
            }

            // compute textures normally
            return true;
        }

        private static void Postfix(Minimap __instance)
        {
            if (!ImageFilesExists())
            {
                // write computed files to file
                Directory.CreateDirectory(GetMinimapSavePath());
                File.WriteAllBytes(ForestMaskTexturePath(), __instance.m_forestMaskTexture.EncodeToPNG());
                File.WriteAllBytes(MapTexturePath(), __instance.m_mapTexture.EncodeToPNG());
                File.WriteAllBytes(HeightTexturePath(), __instance.m_heightTexture.GetRawTextureData());
            }
        }

        public static bool ImageFilesExists()
        {
            return File.Exists(ForestMaskTexturePath()) &&
                   File.Exists(MapTexturePath()) &&
                   File.Exists(HeightTexturePath());
        }

        public static string GetMinimapSavePath()
        {
            return World.GetWorldSavePath() + "/minimap";
        }

        public static string GetBaseFileName()
        {
            return ZNet.m_world.m_name + "_" + ZNet.m_world.m_seed + "_" + Version.GetVersionString();
        }

        public static string ForestMaskTexturePath()
        {
            return GetMinimapSavePath() + "/" + GetBaseFileName() + "_forestMaskTexture.png";
        }

        public static string MapTexturePath()
        {
            return GetMinimapSavePath() + "/" + GetBaseFileName() + "_mapTexture.png";
        }

        public static string HeightTexturePath()
        {
            return GetMinimapSavePath() + "/" + GetBaseFileName() + "_heightTexture.raw";
        }
    }
}