using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace SingleBrainCell
{
    public class SolarRoasting : MonoBehaviour
    {
        private RoastingStickController roastingSystem;
        private InteractReceiver sunReceiver;
        private Campfire fire;
        private bool isRoasting = false;


        private void Start()
        {
            RoastPatches.SolarRoasting = this;
            roastingSystem = GameObject.Find("RoastingSystem").GetComponent<RoastingStickController>();

            gameObject.layer = LayerMask.NameToLayer("Interactible");
            sunReceiver = gameObject.AddComponent<InteractReceiver>();
            sunReceiver.SetInteractRange(1000);
            sunReceiver.SetPromptText(UITextType.RoastingPrompt);
            sunReceiver.OnPressInteract += StartSunRoasting;

            fire = gameObject.AddComponent<Campfire>();
            RoastPatches.SunFire = fire;
        }

        private void LateUpdate()
        {
            var range = Locator.GetSunController().GetSurfaceRadius() + sunReceiver._interactRange;
            var camera = Locator.GetPlayerCamera();
            var dist = transform.position - camera.transform.position;
            if (dist.magnitude > range || Vector3.Dot(dist.normalized, camera.transform.forward) < 0 || !OWInput.IsInputMode(InputMode.Character))
            {
                if (sunReceiver._focused) sunReceiver.LoseFocus();
                return;
            }

            RaycastHit raycastHit;
            if (Physics.Raycast(camera.transform.position, camera.transform.forward, out raycastHit, range, OWLayerMask.blockableInteractMask))
            {
                if (raycastHit.collider != sunReceiver._owCollider._collider && sunReceiver._focused) sunReceiver.LoseFocus();
                if (raycastHit.collider == sunReceiver._owCollider._collider && !sunReceiver._focused) sunReceiver.Observe(raycastHit);
            }
            else if (sunReceiver._focused) sunReceiver.LoseFocus();

            if (sunReceiver._focused) sunReceiver.UpdateInteractVolume(); //apparently I have to do everything myself
        }

        private void StartSunRoasting()
        {
            Locator.GetToolModeSwapper().UnequipTool();
            Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().LockOn(gameObject.transform);
            isRoasting = true;
            StartCoroutine(RotateRoutine());
            GlobalMessenger<Campfire>.FireEvent("EnterRoastingMode", fire);
        }

        private IEnumerator RotateRoutine()
        {
            var rotA = roastingSystem.transform.localRotation;
            var rotB = fire.transform.localRotation;
            while (isRoasting)
            {
                roastingSystem.transform.rotation = Locator.GetPlayerCamera().transform.rotation;
                fire.transform.rotation = Locator.GetPlayerCamera().transform.rotation;
                var comp = transform.InverseTransformPoint(roastingSystem._stickPivotTransform.TransformPoint(new Vector3(0f, 0f, roastingSystem._stickMaxZ))).y;
                var diff = Vector3.Dot((Locator.GetPlayerCamera().transform.position - transform.position).normalized * 2, Locator.GetPlayerCamera().transform.up);
                fire._rockHeight = comp - diff;
                yield return null;
            }
            roastingSystem.transform.localRotation = rotA;
            fire.transform.localRotation = rotB;
        }

        public void StopSunRoasting()
        {
            if (!isRoasting) return;
            isRoasting = false;

            Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().BreakLock();
            if (PlayerState.InZeroG()) Locator.GetPlayerCamera().GetComponent<PlayerCameraController>().CenterCamera(50f, true);
            GlobalMessenger.FireEvent("ExitRoastingMode");
            if (Locator.GetPlayerSuit().IsWearingSuit(true)) { Locator.GetPlayerSuit().PutOnHelmet(); }
            sunReceiver.ResetInteraction();
        }
    }

    [HarmonyPatch]
    public class RoastPatches
    {
        public static SolarRoasting SolarRoasting;
        public static Campfire SunFire;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Marshmallow), nameof(Marshmallow.Eat))]
        public static void Marshmallow_Eat_Prefix()
        {
            if (Locator.GetPlayerSuit().IsWearingSuit(true)) Locator.GetPlayerSuit().RemoveHelmet();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Campfire), nameof(Campfire.GetHeatAtPosition))]
        public static bool Campfire_GetHeatAtPosition_Prefix(Campfire __instance, Vector3 worldPosition, ref float __result)
        {
            if (__instance == SunFire)
            {
                var dist = __instance.transform.InverseTransformPoint(worldPosition).magnitude;
                __result = 35 * 2000 * 2000 / (dist * dist); //not entirely physically accurate but who cares
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Campfire), nameof(Campfire.StopRoasting))]
        public static bool Campfire_StopRoasting_Prefix(Campfire __instance)
        {
            if (__instance == SunFire)
            {
                SolarRoasting.StopSunRoasting();
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Campfire), nameof(Campfire.CheckStickIntersection))]
        public static bool Campfire_CheckStickIntersection_Prefix(Campfire __instance, ref bool __result)
        {
            if (__instance == SunFire)
            {
                __result = false;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Campfire), nameof(Campfire.SetState))]
        public static bool Campfire_SetState_Prefix(Campfire __instance, Campfire.State newState)
        {
            if (__instance == SunFire)
            {
                __instance._state = newState;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Campfire), nameof(Campfire.SetLitFraction))]
        public static bool Campfire_SetLitFraction_Prefix(Campfire __instance, float fraction)
        {
            if (__instance == SunFire)
            {
                __instance._litFraction = fraction;
                return false;
            }
            return true;
        }
    }
}
