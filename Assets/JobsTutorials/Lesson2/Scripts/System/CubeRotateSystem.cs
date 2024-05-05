using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
namespace Jobs.DOD
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateAfter(typeof(CubeGenerateByPrefabSystem))]//该System需要在Generate完成后执行
    [UpdateInGroup(typeof(CreateEntitiesByPrefabSystemGroup))]
    public partial struct CubeRotateSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CubeGeneratorByPrefab>();
        }
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            //该Query只能在foreach内，foreach已被重载，为了避免修改到数据的副本，这里的RefRW和RefRO都是Native指针
            foreach (var item in SystemAPI.Query<RefRW<LocalTransform>, RefRO<RotateSpeed>>())
            {
                item.Item1.ValueRW = item.Item1.ValueRW.RotateY(item.Item2.ValueRO.rotateSpeed * deltaTime);
            }
        }
    }

}
