using HarmonyLib;
using OWML.ModHelper;
using System.Reflection;
using UnityEngine;

namespace SingleBrainCell
{
    public class SingleBrainCell : ModBehaviour
    {
        public static SingleBrainCell ModInstance;

        private void Awake()
        {
            ModInstance = this;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        private void Start()
        {
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;
                GameObject.Find("Sun_Body/Sector_SUN/Volumes_SUN/ScaledVolumesRoot/DestructionFluidVolume").AddComponent<SolarRoasting>();
                GameObject.FindObjectOfType<FirstPersonManipulator>().gameObject.AddComponent<TouchSparks>();
                StartCoroutine(EatJelly.OnStart(GameObject.Find("JellyfishNote").transform));
            };
        }
    }
}