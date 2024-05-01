
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SaccFlightAndVehicles
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DFUNC_DummySystem : UdonSharpBehaviour
    {
        public UdonSharpBehaviour SAVControl;
        public GameObject HudDummy;
        public GameObject Dial_Funcon;
        [Header("Cruise = Helicopter Throttle")]
        public bool HelicopterMode;
        private bool Cruise;
        private bool CruiseThrottleOverridden;
        private SaccEntity EntityControl;
        private bool UseLeftTrigger = false;
        private bool TriggerLastFrame;
        [System.NonSerializedAttribute] public bool AltHold;
        private Rigidbody VehicleRigidbody;
        private Transform VehicleTransform;
        private Vector3 RotationInputs;
        private bool EngineOn;
        private bool IsOwner;
        private bool InVR;
        private bool Selected;
        private bool JoyStickOveridden;
        private bool StickHeld;
        private bool Piloting;

        //PID制御の変数。UnityのInspectorを見ながら調整するためにpublicにしておく。
        [Header("PITCH")]
        public float TargetSpeed = 30.8667f;
        public float CurrentSpeed;
        public float SpeedError;
        public float SpeedErrorLast = 0;
        public float SpeedPGain = -1.8f;
        public float SpeedPTerm;
        public float SpeedIError = 0;
        public float SpeedIErrorLimit = 20;
        public float SpeedIGain = -0.6f;
        public float SpeedITerm;
        public float SpeedDError;
        public float SpeedDGain = 0;
        public float SpeedDTerm;
        public float TargetPitch;
        public float TargetPitchLimit = 15;
        public float CurrentPitch;
        public float PitchError;
        public float PitchErrorLast = 0;
        public float PitchPGain = -0.08f;
        public float PitchPTerm;
        public float PitchIError = 0;
        public float PitchIErrorLimit = 40;
        public float PitchIGain = -0.0005f;
        public float PitchITerm;
        public float PitchDError;
        public float PitchDGain = -0.04f;
        public float PitchDTerm;
        public float CyclicUPitch;
        [Header("ROLL")]
        public Vector2[] CourseTargetPositions = {new Vector2(600, -600), new Vector2(-600, 200)};
        public int CourseTargetPositionIndex = 0;
        public bool RightTurn = false;
        public float CourseTargetDistance;
        public float CourseError;
        public float CourseErrorLast = 0;
        public float CoursePGain = -0.8f;
        public float CoursePTerm;
        public float CourseIError;
        public float CourseIErrorLimit = 0;
        public float CourseIGain = 0;
        public float CourseITerm;
        public float CourseDError;
        public float CourseDGain = 0;
        public float CourseDTerm;
        public float TargetRoll;
        public float TargetRollLimit = 15;
        public float CurrentRoll;
        public float RollError;
        public float RollErrorLast = 0;
        public float RollPGain = 0.003f;
        public float RollPTerm;
        public float RollIError;
        public float RollIErrorLimit = 0;
        public float RollIGain = 0.0003f;
        public float RollITerm;
        public float RollDError;
        public float RollDGain = 0.002f;
        public float RollDTerm;
        public float CyclicURoll;
        [Header("YAW")]
        public Vector2 TargetHeading;
        public Vector2 CurrentHeading;
        public float HeadingErrorScale = 1f;
        public float HeadingError;
        public float HeadingErrorLast = 0;
        public float HeadingPGain = 0.018f;
        public float HeadingPTerm;
        public float HeadingIError;
        public float HeadingIErrorLimit = 0;
        public float HeadingIGain = 0;
        public float HeadingITerm;
        public float HeadingDError;
        public float HeadingDGain = 0.016f;
        public float HeadingDTerm;
        public float CyclicUYaw;
        [Header("THRUST")]
        public float TargetAltitude = 213.36f;
        public float CurrentAltitude;
        public float AltitudeDistance;
        public float AltitudeError;
        public float AltitudeErrorLast = 0;
        public float AltitudePGain = 10f;
        public float AltitudePTerm;
        public float AltitudeIError;
        public float AltitudeIErrorLimit = 0;
        public float AltitudeIGain = 0;
        public float AltitudeITerm;
        public float AltitudeDError;
        public float AltitudeDGain = 0;
        public float AltitudeDTerm;
        public float TargetVerticalSpeed;
        public float TargetVerticalSpeedLimit = 152.4f;
        public float CurrentVerticalSpeed;
        public float VerticalSpeedError;
        public float VerticalSpeedErrorLast = 0;
        public float VerticalSpeedPGain = 0.005f;
        public float VerticalSpeedPTerm;
        public float VerticalSpeedIError;
        public float VerticalSpeedIErrorLimit = 0;
        public float VerticalSpeedIGain = 0.001f;
        public float VerticalSpeedITerm;
        public float VerticalSpeedDError;
        public float VerticalSpeedDGain = 0.004f;
        public float VerticalSpeedDTerm;
        public float CollectiveU;

        public void DFUNC_LeftDial() { UseLeftTrigger = true; }
        public void DFUNC_RightDial() { UseLeftTrigger = false; }
        public void SFEXT_L_EntityStart()
        {
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (localPlayer != null)
            { InVR = localPlayer.IsUserInVR(); }
            EntityControl = (SaccEntity)SAVControl.GetProgramVariable("EntityControl");
            VehicleRigidbody = (Rigidbody)SAVControl.GetProgramVariable("VehicleRigidbody");
            VehicleTransform = EntityControl.transform;
            if (Dial_Funcon) { Dial_Funcon.SetActive(false); }
            IsOwner = (bool)SAVControl.GetProgramVariable("IsOwner");
        }
        public void DFUNC_Selected()
        {
            TriggerLastFrame = true;
            gameObject.SetActive(true);
            Selected = true;
        }
        public void DFUNC_Deselected()
        {
            if (!AltHold) { gameObject.SetActive(false); }
            Selected = false;
        }
        public void SFEXT_O_PilotEnter()
        {
            Piloting = true;
            if (!AltHold) { gameObject.SetActive(false); }
            if (Dial_Funcon) Dial_Funcon.SetActive(AltHold);
        }
        public void SFEXT_O_PilotExit()
        {
            Piloting = false;
            Selected = false;
            StickHeld = false;
        }
        public void SFEXT_L_PassengerEnter()
        {
            if (Dial_Funcon) Dial_Funcon.SetActive(AltHold);
        }
        public void SFEXT_G_EngineOn()
        {
            EngineOn = true;
            ResetCourse();
        }
        public void SFEXT_G_EngineOff()
        {
            gameObject.SetActive(false);
            Selected = false;
            EngineOn = false;
            if (AltHold)
            { DeactivateAltHold(); }
        }
        public void SFEXT_G_TouchDown()
        {
            if (AltHold)
            { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(DeactivateAltHold)); }
        }
        public void SFEXT_O_EnterVTOL()
        {
            if (AltHold)
            { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(DeactivateAltHold)); }
        }
        public void SFEXT_O_OnPlayerJoined()
        {
            if (AltHold)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ActivateAltHold));
            }
        }
        public void SFEXT_O_JoystickGrabbed()
        {
            ResetIntegrators();

            StickHeld = true;
            if (JoyStickOveridden)
            {
                SAVControl.SetProgramVariable("JoystickOverridden", (int)SAVControl.GetProgramVariable("JoystickOverridden") - 1);
                JoyStickOveridden = false;
            }
        }
        public void SFEXT_O_JoystickDropped()
        {
            ResetIntegrators();

            StickHeld = false;
            if (!JoyStickOveridden && AltHold)
            {
                SAVControl.SetProgramVariable("JoystickOverridden", (int)SAVControl.GetProgramVariable("JoystickOverridden") + 1);
                JoyStickOveridden = true;
            }
        }
        public void ActivateAltHold()
        {
            if (AltHold) { return; }
            AltHold = true;
            if (!JoyStickOveridden && !StickHeld)
            {
                SAVControl.SetProgramVariable("JoystickOverridden", (int)SAVControl.GetProgramVariable("JoystickOverridden") + 1);
                JoyStickOveridden = true;
            }
            if (Dial_Funcon) { Dial_Funcon.SetActive(AltHold); }
            if (HudDummy) { HudDummy.SetActive(AltHold); }
            EntityControl.SendEventToExtensions("SFEXT_G_AltHoldOn");
            if (HelicopterMode)
            { SetCruiseOn(); }
        }
        public void DeactivateAltHold()
        {
            if (!AltHold) { return; }
            if (!InVR || !Selected) { gameObject.SetActive(false); }
            AltHold = false;
            if (Dial_Funcon) { Dial_Funcon.SetActive(AltHold); }
            if (HudDummy) { HudDummy.SetActive(AltHold); }
            if (JoyStickOveridden)
            {
                SAVControl.SetProgramVariable("JoystickOverridden", (int)SAVControl.GetProgramVariable("JoystickOverridden") - 1);
                JoyStickOveridden = false;
            }
            SAVControl.SetProgramVariable("JoystickOverride", Vector3.zero);
            RotationInputs = Vector3.zero;
            ResetIntegrators();

            EntityControl.SendEventToExtensions("SFEXT_G_AltHoldOff");
            if (HelicopterMode)
            { SetCruiseOff(); }
        }
        private void FixedUpdate()
        {
            if (Selected)
            {
                if (InVR)
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
                            ToggleAltHold();
                        }
                        TriggerLastFrame = true;
                    }
                    else { TriggerLastFrame = false; }
                }
            }

            if (AltHold && IsOwner)
            {
                float DeltaTime = Time.deltaTime;

                //Pitch

                //目標速度のためのピッチ
                CurrentSpeed = (float)SAVControl.GetProgramVariable("AirSpeed");
                SpeedError = TargetSpeed - CurrentSpeed;
                TargetPitch = PIDControl(SpeedError, ref SpeedErrorLast, SpeedPGain, ref SpeedPTerm, ref SpeedIError, SpeedIErrorLimit, SpeedIGain, ref SpeedITerm, ref SpeedDError, SpeedDGain, ref SpeedDTerm, DeltaTime, TargetPitchLimit);

                //目標ピッチのための操作量
                CurrentPitch = Vector3.SignedAngle(VehicleTransform.forward, Vector3.ProjectOnPlane(VehicleTransform.forward, Vector3.up), VehicleTransform.right);
                PitchError = TargetPitch - CurrentPitch;
                CyclicUPitch = PIDControl(PitchError, ref PitchErrorLast, PitchPGain, ref PitchPTerm, ref PitchIError, PitchIErrorLimit, PitchIGain, ref PitchITerm, ref PitchDError, PitchDGain, ref PitchDTerm, DeltaTime, 1);
                RotationInputs.x = CyclicUPitch;

                //Roll

                Vector2 CurrentPosition = new Vector2(VehicleTransform.position.x, VehicleTransform.position.z);
                Vector2 CourseTargetPosition = CourseTargetPositions[CourseTargetPositionIndex];

                //目標地点に近付いたら次の目標へ
                CourseTargetDistance = Vector2.Distance(CourseTargetPosition, CurrentPosition);
                if (CourseTargetDistance < 200)
                {
                    CourseTargetPositionIndex = 1 - CourseTargetPositionIndex; //0と1を切り替え
                    CourseTargetPosition = CourseTargetPositions[CourseTargetPositionIndex];
                }

                //目標地点が2番目のとき右旋回
                RightTurn = (CourseTargetPositionIndex == 1);

                //目標方位 = 自分から見た目標地点の方位
                Vector2 TargetCourse = (CourseTargetPosition - CurrentPosition);

                Vector3 CurrentCourse3 = ((Vector3)SAVControl.GetProgramVariable("CurrentVel"));
                Vector2 CurrentCourse = new Vector2(CurrentCourse3.x, CurrentCourse3.z);

                //コース（方位）の偏差
                CourseError = Vector2.SignedAngle(TargetCourse, CurrentCourse); //-180から180
                //20度よりずれていたら旋回方向を一定にする
                if (RightTurn && CourseError < -20)
                {
                    CourseError += 360;
                }
                if (!RightTurn && CourseError > 20)
                {
                    CourseError -= 360;
                }

                //目標方位のためのロール
                TargetRoll = PIDControl(CourseError, ref CourseErrorLast, CoursePGain, ref CoursePTerm, ref CourseIError, CourseIErrorLimit, CourseIGain, ref CourseITerm, ref CourseDError, CourseDGain, ref CourseDTerm, DeltaTime, TargetRollLimit);

                //目標ロールのための操作量
                CurrentRoll = VehicleTransform.localEulerAngles.z; //0から360 機内(ベクトルの後ろ)から見ると反時計回り
                if (CurrentRoll > 180)
                {
                    CurrentRoll -= 360;
                }
                RollError = TargetRoll - CurrentRoll;
                CyclicURoll = PIDControl(RollError, ref RollErrorLast, RollPGain, ref RollPTerm, ref RollIError, RollIErrorLimit, RollIGain, ref RollITerm, ref RollDError, RollDGain, ref RollDTerm, DeltaTime, 1);
                RotationInputs.z = CyclicURoll;

                //Yaw
                TargetHeading = CurrentCourse;
                CurrentHeading = new Vector2(VehicleTransform.forward.x, VehicleTransform.forward.z);
                HeadingError = Vector2.SignedAngle(TargetHeading, CurrentHeading); //-180から180
                HeadingError *= HeadingErrorScale;
                CyclicUYaw = PIDControl(HeadingError, ref HeadingErrorLast, HeadingPGain, ref HeadingPTerm, ref HeadingIError, HeadingIErrorLimit, HeadingIGain, ref HeadingITerm, ref HeadingDError, HeadingDGain, ref HeadingDTerm, DeltaTime, 1);
                RotationInputs.y = CyclicUYaw;

                SAVControl.SetProgramVariable("JoystickOverride", RotationInputs);

                if (HelicopterMode)
                {
                    if (EngineOn)
                    {
                        if (!InVR && Piloting)
                        {
                            bool ShiftCtrl = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl);
                            if (ShiftCtrl)
                            {
                                if (CruiseThrottleOverridden)
                                {
                                    SAVControl.SetProgramVariable("ThrottleOverridden", (int)SAVControl.GetProgramVariable("ThrottleOverridden") - 1);
                                    CruiseThrottleOverridden = false;
                                }
                            }
                            else
                            {
                                if (Cruise)
                                {
                                    if (!CruiseThrottleOverridden)
                                    {
                                        SAVControl.SetProgramVariable("ThrottleOverridden", (int)SAVControl.GetProgramVariable("ThrottleOverridden") + 1);
                                        CruiseThrottleOverridden = true;
                                    }
                                }
                            }
                        }

                        //目標高度のための垂直速度
                        CurrentAltitude = ((Transform)SAVControl.GetProgramVariable("CenterOfMass")).position.y - (float)SAVControl.GetProgramVariable("SeaLevel");
                        AltitudeError = TargetAltitude - CurrentAltitude;
                        TargetVerticalSpeed = PIDControl(AltitudeError, ref AltitudeErrorLast, AltitudePGain, ref AltitudePTerm, ref AltitudeIError, AltitudeIErrorLimit, AltitudeIGain, ref AltitudeITerm, ref AltitudeDError, AltitudeDGain, ref AltitudeDTerm, DeltaTime, TargetVerticalSpeedLimit);

                        //目標垂直速度のための操作量
                        CurrentVerticalSpeed = ((Vector3)SAVControl.GetProgramVariable("CurrentVel")).y * 60;
                        VerticalSpeedError = TargetVerticalSpeed - CurrentVerticalSpeed;
                        CollectiveU = PIDControl(VerticalSpeedError, ref VerticalSpeedErrorLast, VerticalSpeedPGain, ref VerticalSpeedPTerm, ref VerticalSpeedIError, VerticalSpeedIErrorLimit, VerticalSpeedIGain, ref VerticalSpeedITerm, ref VerticalSpeedDError, VerticalSpeedDGain, ref VerticalSpeedDTerm, DeltaTime, 1);
                        SAVControl.SetProgramVariable("ThrottleOverride", CollectiveU);
                    }
                }
            }
        }

        private float PIDControl(float error, ref float errorLast, float pGain, ref float pTerm, ref float iError, float iErrorLimit, float iGain, ref float iTerm, ref float dError, float dGain, ref float dTerm, float deltaTime, float uLimit)
        {
            //proportional
            pTerm = pGain * error;

            //integral
            iError += error * deltaTime;
            if (iErrorLimit > 0)
            {
                iError = Mathf.Clamp(iError, -iErrorLimit, iErrorLimit);
            }
            iTerm = iGain * iError;

            //derivative
            dError = (error - errorLast) / deltaTime;
            errorLast = error;
            dTerm = dGain * dError;

            return Mathf.Clamp(pTerm + iTerm + dTerm, -uLimit, uLimit);
        }

        private void ResetIntegrators()
        {
            SpeedIError = 0;
            PitchIError = 0;
            CourseIError = 0;
            RollIError = 0;
            HeadingIError = 0;
            AltitudeIError = 0;
            VerticalSpeedIError = 0;
        }
        private void ResetCourse()
        {
            CourseTargetPositionIndex = 0;
        }
        public void SetCruiseOn()
        {
            if (Cruise) { return; }
            if (!CruiseThrottleOverridden)
            {
                SAVControl.SetProgramVariable("ThrottleOverridden", (int)SAVControl.GetProgramVariable("ThrottleOverridden") + 1);
                CruiseThrottleOverridden = true;
            }
            //SetSpeed = (float)SAVControl.GetProgramVariable("AirSpeed");
            Cruise = true;
            if (Dial_Funcon) { Dial_Funcon.SetActive(true); }
            EntityControl.SendEventToExtensions("SFEXT_O_CruiseEnabled");
        }
        public void SetCruiseOff()
        {
            if (!Cruise) { return; }
            if (CruiseThrottleOverridden)
            {
                SAVControl.SetProgramVariable("ThrottleOverridden", (int)SAVControl.GetProgramVariable("ThrottleOverridden") - 1);
                CruiseThrottleOverridden = false;
            }
            SAVControl.SetProgramVariable("PlayerThrottle", (float)SAVControl.GetProgramVariable("ThrottleInput"));
            Cruise = false;
            if (Dial_Funcon) { Dial_Funcon.SetActive(false); }
            EntityControl.SendEventToExtensions("SFEXT_O_CruiseDisabled");
        }
        public void SFEXT_O_ThrottleDropped()
        {
            if (!CruiseThrottleOverridden && Cruise)
            {
                SAVControl.SetProgramVariable("ThrottleOverridden", (int)SAVControl.GetProgramVariable("ThrottleOverridden") + 1);
                CruiseThrottleOverridden = true;
            }
        }
        public void SFEXT_O_ThrottleGrabbed()
        {
            if (CruiseThrottleOverridden)
            {
                SAVControl.SetProgramVariable("ThrottleOverridden", (int)SAVControl.GetProgramVariable("ThrottleOverridden") - 1);
                CruiseThrottleOverridden = false;
            }
        }
        public void KeyboardInput()
        {
            ToggleAltHold();
        }
        private void ToggleAltHold()
        {
            if (AltHold)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(DeactivateAltHold));
            }
            else
            {
                //if (((bool)SAVControl.GetProgramVariable("InVTOL") && !HelicopterMode) || (bool)SAVControl.GetProgramVariable("Taxiing") || !EngineOn) { return; }
                if (((bool)SAVControl.GetProgramVariable("InVTOL") && !HelicopterMode) || !EngineOn) { return; }
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ActivateAltHold));
                gameObject.SetActive(true);
            }
        }
        public void SFEXT_G_Explode()
        {
            gameObject.SetActive(false);
        }
        public void SFEXT_O_TakeOwnership()
        {
            IsOwner = true;
            if (AltHold)
            { gameObject.SetActive(true); }
        }
        public void SFEXT_O_LoseOwnership()
        {
            IsOwner = false;
            gameObject.SetActive(false);
        }
    }
}
