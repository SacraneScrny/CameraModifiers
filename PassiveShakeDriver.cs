using ModifiableVariable;
using ModifiableVariable.Stages.StageFactory;

using UnityEngine;

namespace CameraModifiers
{
    /// <summary>Continuous Perlin-noise idle sway for rotation, position, FOV and orthographic size, each with its own adjustable strength.</summary>
    public sealed class PassiveShakeDriver
    {
        /// <summary>Speed at which the underlying noise is sampled.</summary>
        public Modifiable<float> Speed { get; private set; }

        /// <summary>Strength of the passive rotation sway, in degrees.</summary>
        public Modifiable<float> RotationStrength { get; private set; }

        /// <summary>Strength of the passive position sway, in units.</summary>
        public Modifiable<float> PositionStrength { get; private set; }

        /// <summary>Strength of the passive FOV sway.</summary>
        public Modifiable<float> FovStrength { get; private set; }

        /// <summary>Strength of the passive orthographic size sway.</summary>
        public Modifiable<float> OrthoStrength { get; private set; }

        Quaternion _rotation = Quaternion.identity;
        Vector3 _position;
        float _fov;
        float _ortho;
        float _timer;

        public PassiveShakeDriver(
            float speed,
            float rotationStrength,
            float positionStrength,
            float fovStrength,
            float orthoStrength,
            RotationModifiable<Quaternion> rotationTarget,
            PositionModifiable<Vector3> positionTarget,
            Modifiable<float> fovTarget,
            Modifiable<float> orthoTarget)
        {
            Speed = speed;
            RotationStrength = rotationStrength;
            PositionStrength = positionStrength;
            FovStrength = fovStrength;
            OrthoStrength = orthoStrength;

            rotationTarget.Add(() => _rotation);
            positionTarget.Add(() => _position, Position.Offset);
            fovTarget.Add(() => _fov);
            orthoTarget.Add(() => _ortho);
        }

        public void Tick(float deltaTime)
        {
            _timer += deltaTime * Speed;
            var t = _timer / 10f * Mathf.PI * 2f;
            var c = new Vector2(Mathf.Cos(t), Mathf.Sin(t));

            var nx = ToSigned(Mathf.PerlinNoise(c.x + 0.1f, c.y + 0.7f));
            var ny = ToSigned(Mathf.PerlinNoise(c.x + 2.3f, c.y + 3.4f));
            var nz = ToSigned(Mathf.PerlinNoise(c.x + 2.2f, c.y + 2.1f));

            var direction = new Vector3(nx, ny, nz);
            if (direction.sqrMagnitude > 0.0001f)
                direction.Normalize();

            var rotationTarget = Quaternion.AngleAxis(RotationStrength, direction);
            _rotation = Quaternion.Slerp(_rotation, rotationTarget, deltaTime * 5f);

            var positionTarget = new Vector3(nx, ny, nz) * PositionStrength;
            _position = Vector3.Lerp(_position, positionTarget, deltaTime * 5f);

            var fovNoise = ToSigned(Mathf.PerlinNoise(_timer, 42.42f));
            _fov = Mathf.Lerp(_fov, fovNoise * FovStrength, deltaTime * 5f);

            var orthoNoise = ToSigned(Mathf.PerlinNoise(_timer, 69.69f));
            _ortho = Mathf.Lerp(_ortho, orthoNoise * OrthoStrength, deltaTime * 5f);
        }

        static float ToSigned(float perlin01) => perlin01 * 2f - 1f;
    }
}
