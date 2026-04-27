using System;
using System.Threading.Tasks;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using GameCreator.Runtime.Cameras;

[Version(1, 0, 0)]
[Title("Impact Camera Shake")]
[Description("Spring-based impact shake: sharp, short, directional. Feels like a real hit instead of soft Perlin noise.")]
[Category("Cameras/Shakes/Impact Camera Shake")]

[Parameter("Camera", "The camera that receives the impact shake effect")]
[Parameter("Intensity", "Strength of the impact (1 = normal hit, 2+ = heavy impact)")]
[Parameter("Source", "[Optional] Origin of the hit for directional shake (e.g. the attacker)")]

[Keywords("Camera", "Shake", "Impact", "Hit", "Punch", "Spring")]
[Image(typeof(IconCameraShake), ColorTheme.Type.Yellow)]

[Serializable]
public class InstructionImpactShake : Instruction
{
    // EXPOSED MEMBERS: -----------------------------------------------------------------------

    [SerializeField] private PropertyGetGameObject m_Camera = GetGameObjectCameraMain.Create;

    [Space]
    [SerializeField] private PropertyGetDecimal m_Intensity = new PropertyGetDecimal(1f);
    [SerializeField] private PropertyGetGameObject m_Source = new PropertyGetGameObject();

    // PROPERTIES: ----------------------------------------------------------------------------

    public override string Title => "Impact Camera Shake";

    // RUN: -----------------------------------------------------------------------------------

    protected override Task Run(Args args)
    {
        TCamera camera = this.m_Camera.Get<TCamera>(args);
        if (camera == null) return DefaultResult;

        float intensity = (float)this.m_Intensity.Get(args);
        if (intensity <= 0f) return DefaultResult;

        Vector3 direction = Vector3.zero;
        GameObject sourceObj = this.m_Source.Get(args);

        if (sourceObj != null)
        {
            direction = (camera.transform.position - sourceObj.transform.position).normalized;
        }

        ImpactShakeRunner runner = new ImpactShakeRunner(camera, intensity, direction);
        runner.Start();

        return DefaultResult;
    }

    // INNER CLASS: ---------------------------------------------------------------------------

    private class ImpactShakeRunner
    {
        // Spring tuning: hard hit, fast decay
        private const float Stiffness = 400f;
        private const float Damping = 22f;
        private const float MaxOffset = 0.05f;  // meters
        private const float MaxAngle = 4f;      // degrees
        private const float DeadZone = 0.0001f;

        private readonly TCamera _camera;

        private Vector3 _posVelocity;
        private Vector3 _posDisplacement;
        private Vector3 _rotVelocity;
        private Vector3 _rotDisplacement;

        public ImpactShakeRunner(TCamera camera, float intensity, Vector3 worldDirection)
        {
            _camera = camera;

            Vector3 localDir;

            if (worldDirection.sqrMagnitude > 0.001f)
            {
                // Convert hit direction to camera-local space (XY only)
                localDir = camera.transform.InverseTransformDirection(worldDirection);
                localDir.z = 0f;
                localDir = localDir.normalized;
            }
            else
            {
                // No source provided: use random direction
                localDir = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(-1f, 1f),
                    0f
                ).normalized;
            }

            // Immediate velocity kick: sharp onset, no fade-in
            float posKick = MaxOffset * intensity * Stiffness * 0.5f;
            float rotKick = MaxAngle * intensity * Stiffness * 0.3f;

            _posVelocity = new Vector3(
                localDir.x * posKick,
                localDir.y * posKick,
                0f
            );

            _rotVelocity = new Vector3(
                -localDir.y * rotKick,         // Pitch: hit from above -> head nods down
                 localDir.x * rotKick,          // Yaw: hit from right -> head turns left
                -localDir.x * rotKick * 0.5f    // Roll: slight tilt for added impact
            );
        }

        public void Start()
        {
            _camera.EventAfterUpdate += OnUpdate;
        }

        private void OnUpdate()
        {
            if (_camera == null)
            {
                Cleanup();
                return;
            }

            float dt = UnityEngine.Time.deltaTime;
            if (dt <= 0f) return;

            // Hooke's spring law + damping
            UpdateSpring(ref _posDisplacement, ref _posVelocity, dt);
            UpdateSpring(ref _rotDisplacement, ref _rotVelocity, dt);

            // Safety clamp to prevent extreme values
            _posDisplacement = Vector3.ClampMagnitude(_posDisplacement, MaxOffset * 3f);
            _rotDisplacement = Vector3.ClampMagnitude(_rotDisplacement, MaxAngle * 3f);

            // Apply additively to camera (same approach as GC2's internal shake system)
            Transform t = _camera.transform;
            t.localPosition += _posDisplacement;
            t.localEulerAngles += _rotDisplacement;

            // Dead zone: stop shake once fully settled
            bool positionDead = _posDisplacement.sqrMagnitude < DeadZone
                             && _posVelocity.sqrMagnitude < DeadZone;
            bool rotationDead = _rotDisplacement.sqrMagnitude < DeadZone
                             && _rotVelocity.sqrMagnitude < DeadZone;

            if (positionDead && rotationDead)
            {
                Cleanup();
            }
        }

        private void Cleanup()
        {
            if (_camera != null)
            {
                _camera.EventAfterUpdate -= OnUpdate;
            }
        }

        private static void UpdateSpring(ref Vector3 displacement, ref Vector3 velocity, float dt)
        {
            Vector3 force = -Stiffness * displacement - Damping * velocity;
            velocity += force * dt;
            displacement += velocity * dt;
        }
    }
}