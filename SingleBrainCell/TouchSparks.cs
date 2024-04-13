using System.Linq;
using UnityEngine;

namespace SingleBrainCell
{
    public class TouchSparks : MonoBehaviour
    {
        private static ScreenPrompt touchPrompt = new(InputLibrary.interactSecondary, "<CMD> " + "Touch");

        private FirstPersonManipulator firstPerson;
        private HazardDetector player;
        private ElectricityVolume electricity; //just a proxy to do things through

        private RepairReceiver[] receivers;
        private int damage = 60;

        private void Start()
        {
            Locator.GetPromptManager().AddScreenPrompt(touchPrompt, PromptPosition.Center);
            firstPerson = GetComponent<FirstPersonManipulator>();
            player = Locator.GetPlayerDetector().GetComponent<HazardDetector>();
            electricity = GameObject.Find("ElectricBarrier").GetComponent<ElectricityVolume>();

            receivers = Resources.FindObjectsOfTypeAll<RepairReceiver>().Where(obj =>
            (obj._targetComponent?._damageEffect?._particleSystem?.name.Contains("HEA_Sparks") ?? false) ||
            (obj._targetSatNode?._damageEffect?._particleSystem?.name.Contains("HEA_Sparks") ?? false)).ToArray();
        }

        private void Update()
        {
            if (firstPerson._repairScreenPrompt._isVisible != touchPrompt._isVisible && (firstPerson._repairScreenPrompt._isVisible ? receivers.Contains(firstPerson._focusedRepairReceiver) : true))
            {
                touchPrompt.SetVisibility(firstPerson._repairScreenPrompt._isVisible);
            }

            if (OWInput.IsNewlyPressed(InputLibrary.interactSecondary, InputMode.Character) && touchPrompt._isVisible) Shock();
        }

        private void Shock()
        {
            player.GetAttachedOWRigidbody().GetComponent<PlayerResources>().ApplyInstantDamage(damage, InstantDamageType.Electrical);
            electricity.ApplyShock(player);
        }
    }
}
