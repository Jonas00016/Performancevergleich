using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(PlayerRotateSystem))]
public partial class PlayerMovementSystem : SystemBase
{
    private float3 FORWARD = new float3(0f, 0f, 1f);
    private float3 SIDEWAYS = new float3(1f, 0f, 0f);
    private const float MOVEMENTSPEED = 1000f;

    public JobHandle movementHandle { get; private set; }

    [BurstCompile]
    protected override void OnUpdate()
    {
        float3 vertical = FORWARD * (Input.GetKey("w") ? 1f : (Input.GetKey("s") ? -1f : 0f ));
        float3 horizontal = SIDEWAYS * (Input.GetKey("d") ? 1f : (Input.GetKey("a") ? -1f : 0f));

        float deltaTime = Time.DeltaTime;
        float speed = MOVEMENTSPEED;

        movementHandle = Entities.WithAll<PlayerTag>().ForEach((ref PhysicsVelocity velocity, in Rotation rotation) => {
            velocity.Linear = math.mul(rotation.Value, math.normalizesafe(vertical + horizontal, 0f) * speed * deltaTime);
        }).Schedule(Dependency);

        Dependency = JobHandle.CombineDependencies(Dependency, movementHandle);
    }
}
