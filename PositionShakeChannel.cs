using System.Collections.Generic;

using ModifiableVariable;
using ModifiableVariable.Stages.StageFactory;

using UnityEngine;

namespace CameraModifiers
{
    /// <summary>Drives shake on a <see cref="PositionModifiable{T}"/> by registering a single permanent additive offset.</summary>
    public sealed class PositionShakeChannel
    {
        readonly List<ShakeEntity> _queue = new();
        Vector3 _current;

        public PositionShakeChannel(PositionModifiable<Vector3> target)
        {
            target.Add(() => _current, Position.Offset);
        }

        /// <summary>Queues <paramref name="count"/> shake pulses, <paramref name="delay"/> seconds apart, each lasting <paramref name="duration"/> seconds at <paramref name="strength"/> units.</summary>
        public void Push(float duration, float strength, int count = 1, float delay = 0f)
        {
            for (var i = 0; i < count; i++)
            {
                var direction = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f)) * strength;

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

                _current = Vector3.Lerp(_current, _queue[i].GetDirectionOffset(), 15f * deltaTime);
            }

            if (_queue.Count == 0)
                _current = Vector3.Lerp(_current, Vector3.zero, 15f * deltaTime);
        }
    }
}
