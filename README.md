# CameraModifiers

`CameraModifiersController` drives a `Camera`'s position, rotation, FOV and orthographic size
entirely through `ModifiableVariable` values — the transform is only ever written from
`Position`/`Rotation` in `LateUpdate`, never set directly anywhere else.

## Setup

Add `CameraModifiersController` to the same GameObject as the `Camera`. If the camera has a
parent at `Awake`, `Position`/`Rotation` operate in local space (`transform.localPosition`/
`localRotation`); otherwise they operate in world space.

`Cache Default Position And Rotation` (on by default) captures the transform's current
position/rotation as the base value at `Awake`. Turn it off to start `Position`/`Rotation`
at zero/identity instead.

## Persistent modifiers

```csharp
var handler = controller.Position.Add(() => new Vector3(0, 0.2f, 0), Position.Offset);
var zoomBoost = controller.Fov.Add(() => 1.2f, General.Multiply);
handler.Dispose();
zoomBoost.Dispose();
```

`Position` is a `PositionModifiable<Vector3>` (`Offset` +, `Scale` ×, `Override`).
`Rotation` is a `RotationModifiable<Quaternion>` (`Multiply` ×, `Override`).
`Fov`/`Orthographic` are plain `Modifiable<float>` (`Flat` +, `Multiply` ×).

## Passive idle sway

`controller.PassiveShake` exposes four independent strengths (rotation, position, FOV,
orthographic) plus a shared `Speed`, all `Modifiable<float>` — set their default values in
the inspector, or add/remove modifiers at runtime the same way as any other `Modifiable<float>`:

```csharp
var boost = controller.PassiveShake.RotationStrength.Add(() => 2f);
boost.Dispose(); // settle back down
```

## Push shake

Fire-and-forget shake on any of the four channels, from anywhere in code (e.g. on every gunshot):

```csharp
controller.PushPositionShake(duration: 0.2f, strength: 0.3f);
controller.PushRotationShake(duration: 0.2f, strength: 1.5f, count: 3, delay: 0.05f);
```

Each pulse picks a random target and lerps toward it while active, lerping back toward
neutral once its queue empties — the same behavior the original `CameraShake` had, just
exposed per-axis with `count`/`delay` for firing a quick burst of pulses.

## Dependencies

Requires `ModifiableVariable` (see its own README).
