using HarmonyLib;
using UnityEngine;

namespace ValheimPerformanceOptimizations
{
    /// <summary>
    ///     The smoke particles can be rendered up to 8 times per instance due to its shader
    ///     doing per-pixel lighting for each light in the scene.
    ///     This patch sets the particle shader to use Lux instead,
    ///     which is vertex-lit by default and is rendered in one pass.
    ///     Ideally, the particles should be rendered in one drawcall (DrawMeshInstanced),
    ///     but I couldn't find a transparent vertex-lit shader that supported instancing.
    /// </summary>
    [HarmonyPatch]
    public class SmokePatches
    {
        private static bool _smokeShaderSet;

        [HarmonyPatch(typeof(Smoke), "Awake")]
        public static void Postfix(Smoke __instance)
        {
            if (_smokeShaderSet) return;

            var meshRenderer = __instance.m_mr;
            meshRenderer.sharedMaterial.shader = Shader.Find("Lux Lit Particles/ Bumped");

            _smokeShaderSet = true;
        }

        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        private static void Postfix(ZNetScene __instance)
        {
            var gameMain = GameObject.Find("_GameMain");
            gameMain.AddComponent<VPOSmokeRenderer>();
        }
    }

    /// <summary>
    ///     The Lux shader does not force quads to look at the camera,
    ///     so we do it ourselves
    /// </summary>
    public class VPOSmokeRenderer : MonoBehaviour
    {
        private void Update()
        {
            var camera = Utils.GetMainCamera();
            if (!camera) return;

            var camPosition = camera.transform.position;

            foreach (var smoke in Smoke.m_smoke)
            {
                smoke.transform.rotation = Quaternion.LookRotation(smoke.transform.position - camPosition);
            }
        }
    }
}