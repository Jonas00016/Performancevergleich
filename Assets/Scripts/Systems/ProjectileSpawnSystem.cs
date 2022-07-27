using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;

public partial class ProjectileSpawnSystem : SystemBase
{
    private const float MOVEMENTSPEED = 100f;

    private BeginSimulationEntityCommandBufferSystem beginSimulationECB;
    private Entity prefab;

    private float perSecond = 10f;
    private float nextTime = 0f;

    protected override void OnCreate()
    {
        beginSimulationECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        if (prefab == Entity.Null)
        {
            prefab = GetSingleton<ProjectileAuthoringComponent>().prefab;
            return;
        }

        if (!Input.GetKey("space") || UnityEngine.Time.time < nextTime) return;
        nextTime = Time.DeltaTime + 1f / perSecond;

        EntityCommandBuffer.ParallelWriter commandBuffer = beginSimulationECB.CreateCommandBuffer().AsParallelWriter();
        Entity projectilePrefab = prefab;
        float movementSpeed = MOVEMENTSPEED;

        Entities
            .WithAll<PlayerTag>()
            .ForEach((Entity e, int entityInQueryIndex, in Translation translation, in Rotation rotation, in PhysicsVelocity velocity) =>
            {
                Entity projectileEntity = commandBuffer.Instantiate(entityInQueryIndex, projectilePrefab);

                Translation spawnPosition = new Translation { Value = translation.Value + math.mul(rotation.Value, float3.zero) };
                commandBuffer.SetComponent(entityInQueryIndex, projectileEntity, spawnPosition);

                PhysicsVelocity spawnVelocity = new PhysicsVelocity { Linear = (movementSpeed * math.mul(rotation.Value, new float3(0, 0, 1)).xyz) + velocity.Linear };
                commandBuffer.AddComponent(entityInQueryIndex, projectileEntity, spawnVelocity);
            }
        ).ScheduleParallel();

        beginSimulationECB.AddJobHandleForProducer(Dependency);
    }
}
