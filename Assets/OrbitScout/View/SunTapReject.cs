using OrbitScout.Core;
using UnityEngine;

namespace OrbitScout.View
{
    /// <summary>
    /// Sun taps give a silly rejection — no collider on planets' parent sun for quiz answers.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class SunTapReject : MonoBehaviour
    {
        public static event System.Action<string> OnSunTapMessage;

        void Awake()
        {
            SphereCollider col = GetComponent<SphereCollider>();
            col.isTrigger = false;
        }

        public void NotifyTapped()
        {
            OnSunTapMessage?.Invoke(MissionBanter.GetSunTapReaction());
        }
    }
}
