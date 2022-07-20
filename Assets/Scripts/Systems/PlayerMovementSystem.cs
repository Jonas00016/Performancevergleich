using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class PlayerMovementSystem : SystemBase
{
    private float3 FORWARD = new float3(0f, 0f, 1f);
    private float3 SIDEWAYS = new float3(1f, 0f, 0f);
    private const float MOVEMENTSPEED = 5;

    [BurstCompile]
    protected override void OnUpdate()
    {
        float3 vertical = FORWARD * Input.GetAxis("Vertical");
        float3 horizontal = SIDEWAYS * Input.GetAxis("Horizontal");

        if (vertical.Equals(horizontal)) return;

        float deltaTime = Time.DeltaTime;
        float speed = MOVEMENTSPEED;

        Entities.WithAll<PlayerTag>().ForEach((ref Translation translation, in Rotation rotation) => {
            translation.Value += math.mul(rotation.Value, math.normalize(vertical + horizontal) * speed * deltaTime);
        }).Schedule();
    }
}
