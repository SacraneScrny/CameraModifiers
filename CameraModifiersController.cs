using ModifiableVariable;

using UnityEngine;

namespace CameraModifiers
{
    /// <summary>
    /// Drives a <see cref="Camera"/>'s position, rotation, field of view and orthographic size
    /// entirely through Modifiable values. The transform is only ever written from
    /// <see cref="Position"/>/<see cref="Rotation"/> in <c>LateUpdate</c> — never set directly elsewhere.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Camera/Camera Modifiers Controller")]
    public sealed class CameraModifiersController : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] bool _cacheDefaultPositionAndRotation = true;
        [SerializeField] bool _pixelPerfect;

        [Header("Passive Shake Defaults")]
        [SerializeField] float _defaultPassiveShakeSpeed = 1f;
        [SerializeField] float _defaultRotationPassiveShake;
        [SerializeField] float _defaultPositionPassiveShake;
        [SerializeField] float _defaultFovPassiveShake;
        [SerializeField] float _defaultOrthoPassiveShake;

        Camera _camera;
        bool _useLocalSpace;

        /// <summary>Camera field of view, in degrees. Clamped to [40, 130] on write.</summary>
        public Modifiable<float> Fov { get; private set; }

        /// <summary>Camera orthographic size. Clamped to [0.1, 100] on write.</summary>
        public Modifiable<float> Orthographic { get; private set; }

        /// <summary>Camera position — local space if the camera has a parent, world space otherwise.</summary>
        public PositionModifiable<Vector3> Position { get; private set; }

        /// <summary>Camera rotation — local space if the camera has a parent, world space otherwise.</summary>
        public RotationModifiable<Quaternion> Rotation { get; private set; }

        /// <summary>Continuous idle sway with independently adjustable per-axis strength.</summary>
        public PassiveShakeDriver PassiveShake { get; private set; }

        RotationShakeChannel _rotationShake;
        PositionShakeChannel _positionShake;
        FloatShakeChannel _fovShake;
        FloatShakeChannel _orthoShake;

        void Awake()
        {
            _camera = GetComponent<Camera>();
            _useLocalSpace = transform.parent != null;

            Fov = _camera.fieldOfView;
            Orthographic = _camera.orthographicSize;

            var basePosition = _useLocalSpace ? transform.localPosition : transform.position;
            var baseRotation = _useLocalSpace ? transform.localRotation : transform.rotation;

            if (_cacheDefaultPositionAndRotation)
            {
                Position = basePosition;
                Rotation = baseRotation;
            }
            else
            {
                Position = Vector3.zero;
                Rotation = Quaternion.identity;
            }

            _rotationShake = new RotationShakeChannel(Rotation);
            _positionShake = new PositionShakeChannel(Position);
            _fovShake = new FloatShakeChannel(Fov);
            _orthoShake = new FloatShakeChannel(Orthographic);

            PassiveShake = new PassiveShakeDriver(
                _defaultPassiveShakeSpeed,
                _defaultRotationPassiveShake,
                _defaultPositionPassiveShake,
                _defaultFovPassiveShake,
                _defaultOrthoPassiveShake,
                Rotation,
                Position,
                Fov,
                Orthographic);
        }

        void OnTransformParentChanged()
        {
            _useLocalSpace = transform.parent != null;
        }

        void LateUpdate()
        {
            var deltaTime = Time.deltaTime;
            PassiveShake.Tick(deltaTime);
            _rotationShake.Tick(deltaTime);
            _positionShake.Tick(deltaTime);
            _fovShake.Tick(deltaTime);
            _orthoShake.Tick(deltaTime);

            var targetPosition = Position.Value;
            if (_pixelPerfect)
                targetPosition = SnapToPixel(targetPosition);

            if (_useLocalSpace)
            {
                transform.localPosition = targetPosition;
                transform.localRotation = Rotation.Value;
            }
            else
            {
                transform.position = targetPosition;
                transform.rotation = Rotation.Value;
            }

            _camera.fieldOfView = Mathf.Clamp(Fov.Value, 40f, 130f);
            _camera.orthographicSize = Mathf.Clamp(Orthographic.Value, 0.1f, 100f);
        }

        Vector3 SnapToPixel(Vector3 worldPos)
        {
            var unitsPerPixel = _camera.orthographicSize * 2f / Screen.height;
            worldPos.x = Mathf.Round(worldPos.x / unitsPerPixel) * unitsPerPixel;
            worldPos.y = Mathf.Round(worldPos.y / unitsPerPixel) * unitsPerPixel;
            return worldPos;
        }

        /// <summary>Queues <paramref name="count"/> rotation shake pulses, <paramref name="delay"/> seconds apart.</summary>
        public void PushRotationShake(float duration, float strength, int count = 1, float delay = 0f)
            => _rotationShake.Push(duration, strength, count, delay);

        /// <summary>Queues <paramref name="count"/> position shake pulses, <paramref name="delay"/> seconds apart.</summary>
        public void PushPositionShake(float duration, float strength, int count = 1, float delay = 0f)
            => _positionShake.Push(duration, strength, count, delay);

        /// <summary>Queues <paramref name="count"/> FOV shake pulses, <paramref name="delay"/> seconds apart.</summary>
        public void PushFovShake(float duration, float strength, int count = 1, float delay = 0f)
            => _fovShake.Push(duration, strength, count, delay);

        /// <summary>Queues <paramref name="count"/> orthographic size shake pulses, <paramref name="delay"/> seconds apart.</summary>
        public void PushOrthoShake(float duration, float strength, int count = 1, float delay = 0f)
            => _orthoShake.Push(duration, strength, count, delay);
    }
}
