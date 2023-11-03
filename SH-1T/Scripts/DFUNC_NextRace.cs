using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SaccFlightAndVehicles
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DFUNC_NextRace : UdonSharpBehaviour
    {
        public SaccRaceToggleButton RaceToggler;
        public AudioSource SwitchFunctionSound;

        private bool Selected;
        private bool TriggerLastFrame;
        private bool UseLeftTrigger = false;

        public void DFUNC_LeftDial() { UseLeftTrigger = true; }
        public void DFUNC_RightDial() { UseLeftTrigger = false; }
        public void SFEXT_L_EntityStart()
        {
            ;
        }

        public void DFUNC_Selected()
        {
            TriggerLastFrame = true;
            Selected = true;
        }
        public void DFUNC_Deselected()
        {
            Selected = false;
        }

        public void SFEXT_O_PilotEnter()
        {
            gameObject.SetActive(true);
        }
        public void SFEXT_O_PilotExit()
        {
            gameObject.SetActive(false);
            Selected = false;
        }

        private void Update()
        {
            if (Selected)
            {
                float Trigger;
                if (UseLeftTrigger)
                { Trigger = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger"); }
                else
                { Trigger = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger"); }

                if (Trigger > 0.75)
                {
                    if (!TriggerLastFrame)
                    {
                        NextRace();
                    }
                    TriggerLastFrame = true;
                }
                else
                {
                    TriggerLastFrame = false;
                }
            }

        }

        private void NextRace()
        {
            RaceToggler.NextRace();
        }

        public void KeyboardInput()
        {
            NextRace();
            if (SwitchFunctionSound) { SwitchFunctionSound.Play(); }
        }

    }
}
