using System.Collections.Generic;

using ModifiableVariable;

using UnityEngine;

namespace CameraModifiers
{
    /// <summary>Drives shake on a <see cref="RotationModifiable{T}"/> by registering a single permanent multiplicative offset.</summary>
    public sealed class RotationShakeChannel
    {
        readonly List<ShakeEntity> _queue = new();
        Quaternion _current = Quaternion.identity;

        public RotationShakeChannel(RotationModifiable<Quaternion> target)
        {
            target.Add(() => _current);
        }

        /// <summary>Queues <paramref name="count"/> shake pulses, <paramref name="delay"/> seconds apart, each lasting <paramref name="duration"/> seconds at <paramref name="strength"/> degrees.</summary>
        public void Push(float duration, float strength, int count = 1, float delay = 0f)
        {
            for (var i = 0; i < count; i++)
            {
                var direction = new Vector3(
                    Random.Range(-90f, 90f),
                    Random.Range(-90f, 90f),
                    Random.Range(-90f, 90f)) * strength;

                _queue.Add(new ShakeEntity
                {
                    Direction = direction,
                    Duration = duration,
                    MaxDuration = duration,
                    Delay = delay * i,
                });
            }
        }

        public void Tick(float deltaTime)
        {
            for (var i = 0; i < _queue.Count; i++)
            {
                if (_queue[i].Delay > 0f)
                {
                    _queue[i].Delay -= deltaTime;
                    continue;
                }
                _queue[i].Duration -= deltaTime;
            }

            for (var i = _queue.Count - 1; i >= 0; i--)
                if (_queue[i].Duration <= 0f)
                    _queue.RemoveAt(i);

            for (var i = 0; i < _queue.Count; i++)
            {
                if (_queue[i].Delay > 0f)
                    continue;

                _current = Quaternion.Lerp(
                    _current,
                    Quaternion.Euler(_queue[i].GetDirectionOffset()),
                    15f * deltaTime);
            }

            if (_queue.Count == 0)
                _current = Quaternion.Lerp(_current, Quaternion.identity, 15f * deltaTime);
        }
    }
}
