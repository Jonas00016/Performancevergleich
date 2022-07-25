using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateBefore(typeof(DestroySystem))]
public partial class ProjectileDisappearSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem endSimulationECB;

    protected override void OnCreate()
    {
        endSimulationECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter commandBuffer = endSimulationECB.CreateCommandBuffer().AsParallelWriter();
        float deltaTime = Time.DeltaTime;

        Entities
            .WithAll<ProjectileTag>()
            .ForEach((Entity e, int entityInQueryIndex, ref ProjectileLifetime projectileLifetime) =>
            {
                projectileLifetime.lifetime += deltaTime;

                if (projectileLifetime.lifetime >= projectileLifetime.maxLifetime)
                {
                    commandBuffer.AddComponent(entityInQueryIndex, e, new DestroyTag { });
                }
            }
        ).ScheduleParallel();

        endSimulationECB.AddJobHandleForProducer(Dependency);
    }
}
