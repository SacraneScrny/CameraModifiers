using System.Collections.Generic;

using ModifiableVariable;

using UnityEngine;

namespace CameraModifiers
{
    /// <summary>Drives shake on a scalar <see cref="Modifiable{T}"/> by registering a single permanent additive offset.</summary>
    public sealed class FloatShakeChannel
    {
        readonly List<ShakeEntity> _queue = new();
        float _current;

        public FloatShakeChannel(Modifiable<float> target)
        {
            target.Add(() => _current);
        }

        /// <summary>Queues <paramref name="count"/> shake pulses, <paramref name="delay"/> seconds apart, each lasting <paramref name="duration"/> seconds at <paramref name="strength"/>.</summary>
        public void Push(float duration, float strength, int count = 1, float delay = 0f)
        {
            for (var i = 0; i < count; i++)
            {
                var direction = Random.Range(-1f, 1f) * strength;

                _queue.Add(new ShakeEntity
                {
                    Direction = new Vector3(direction, 0f, 0f),
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

                _current = Mathf.Lerp(_current, _queue[i].GetDirectionOffset().x, 15f * deltaTime);
            }

            if (_queue.Count == 0)
                _current = Mathf.Lerp(_current, 0f, 15f * deltaTime);
        }
    }
}
