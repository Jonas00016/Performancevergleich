using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;

public partial class EnemyMovementSystem : SystemBase
{
    private const float MOVEMENTSPEED = 300;

    private EntityQuery playerQuery;
    private EntityQuery enemyQuery;

    protected override void OnCreate()
    {
        playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>(), ComponentType.ReadOnly<Translation>());
        enemyQuery = GetEntityQuery(ComponentType.ReadOnly<EnemyTag>());
    }

    protected override void OnUpdate()
    {
        int enemyCount = enemyQuery.CalculateEntityCountWithoutFiltering();
        if (enemyCount <= 0) return;

        NativeArray<Translation> playerPosition = playerQuery.ToComponentDataArrayAsync<Translation>(Allocator.TempJob, out JobHandle getPositionHandle);
        float3 movementSpeed = MOVEMENTSPEED;
        float deltaTime = Time.DeltaTime;

        var moveEnemyHandle = Entities
            .WithAll<EnemyTag>()
            .WithReadOnly(playerPosition)
            .WithDisposeOnCompletion(playerPosition)
            .ForEach((ref PhysicsVelocity velocity, ref Rotation rotation, in Translation translation) =>
        {
            if (playerPosition.Length <= 0) return;

            float3 moveDirection = math.normalizesafe(playerPosition[0].Value - translation.Value, float3.zero);

            rotation.Value = quaternion.LookRotation(new float3(moveDirection.x, 0.0f, moveDirection.z), math.up());

            velocity.Linear = math.mul(rotation.Value, moveDirection * movementSpeed * deltaTime);

        }).ScheduleParallel(getPositionHandle);

        Dependency = moveEnemyHandle;
    }
}
