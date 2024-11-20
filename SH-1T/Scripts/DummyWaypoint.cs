
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SaccFlightAndVehicles
{
    // ダミーシステムの経由地
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DummyWaypoint : UdonSharpBehaviour
    {
        [Tooltip("経由地の位置")]
        public Vector3 Position;
        [Tooltip("この経由地に向かう際の速度")]
        public float Speed = 30.8667f;
        [Tooltip("この経由地にこの距離まで近付いたら次の経由地へ切り替える")]
        public float Lead = 200f;
        [Tooltip("この経由地に向かうとき、trueなら右旋回、falseなら左旋回")]
        public bool RightTurn = false;

        private void Start()
        {
            if(Position == Vector3.zero)
            {
                Position = transform.position;
            }
        }
    }
}
