using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial class DestroySystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem endSimulationECB;

    protected override void OnCreate()
    {
        endSimulationECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        RequireSingletonForUpdate<DestroyTag>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter commandBuffer = endSimulationECB.CreateCommandBuffer().AsParallelWriter();

        Entities
            .WithAll<DestroyTag>()
            .ForEach((Entity e, int entityInQueryIndex) => 
            {
                commandBuffer.DestroyEntity(entityInQueryIndex, e);
            }
        ).ScheduleParallel();

        endSimulationECB.AddJobHandleForProducer(Dependency);
    }
}
