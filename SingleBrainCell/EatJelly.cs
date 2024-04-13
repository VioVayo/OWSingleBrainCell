using System.Collections;
using UnityEngine;

namespace SingleBrainCell
{
    public class EatJelly
    {
        private static InteractReceiver noteReceiver, eatReceiver;

        public static IEnumerator OnStart(Transform parent)
        {
            noteReceiver = parent.GetComponentInChildren<InteractReceiver>();
            noteReceiver.OnPressInteract += EnableEat;

            yield return null;
            var go = new GameObject("EatReceiver");
            go.transform.SetParent(parent);
            go.transform.localPosition = new Vector3(0, 1, -0.1f);
            go.transform.localRotation = Quaternion.identity;
            go.AddComponent<BoxCollider>().size = new Vector3(2, 2, 0.1f);

            eatReceiver = go.AddComponent<InteractReceiver>();
            eatReceiver.SetPromptText(UITextType.RoastingEatPrompt);
            eatReceiver.DisableInteraction();
            eatReceiver.OnPressInteract += Eat;
        }

        private static void EnableEat()
        {
            eatReceiver.EnableInteraction();
            noteReceiver.OnPressInteract -= EnableEat;
        }

        private static void Eat()
        {
            if (Locator.GetPlayerSuit().IsWearingSuit(true))
            {
                Locator.GetPlayerSuit().RemoveHelmet();
                eatReceiver.ChangePrompt("Put Helmet Back On");
                eatReceiver.OnPressInteract -= Eat;
                eatReceiver.OnPressInteract += ReplaceHelmet;
                eatReceiver.ResetInteraction();
            }
            Locator.GetPlayerTransform().GetComponentInChildren<PlayerAudioController>().PlayMarshmallowEatBurnt();
        }

        private static void ReplaceHelmet()
        {
            if (Locator.GetPlayerSuit().IsWearingSuit(true)) { Locator.GetPlayerSuit().PutOnHelmet(); }
            eatReceiver.SetPromptText(UITextType.RoastingEatPrompt);
            eatReceiver.OnPressInteract -= ReplaceHelmet;
            eatReceiver.OnPressInteract += Eat;
        }
    }
}
