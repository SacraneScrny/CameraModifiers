using UnityEngine;

namespace CameraModifiers
{
    /// <summary>A single queued shake pulse: a random target direction that decays in strength over its lifetime.</summary>
    public sealed class ShakeEntity
    {
        public Vector3 Direction;
        public float Duration;
        public float MaxDuration;
        public float Delay;

        public Vector3 GetDirectionOffset() => Direction * GetStrength();
        public float GetStrength() => Duration / MaxDuration;
    }
}
