using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace DOTS.DOD.Lesson3
{
    public struct CubeRotateWithIJobChunk : IJobChunk
    {
        public float deltaTime;
        public ComponentTypeHandle<LocalTransform> localTransformTypeHandle;
        public ComponentTypeHandle<RotateSpeed> rotateTypeHandle;
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            var localTransformArr = chunk.GetNativeArray(ref localTransformTypeHandle);
            var rotateArr = chunk.GetNativeArray(ref rotateTypeHandle);
            //新建迭代器进行迭代
            var enumator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, rotateArr.Length);
            while (enumator.NextEntityIndex(out int index))
            {
                localTransformArr[index] = localTransformArr[index].RotateY(rotateArr[index].speed * deltaTime);
            }
        }
    }
    [BurstCompile]
    [UpdateInGroup(typeof(CubeRotateWithIJobChunkSystemGroup))]
    public partial struct CubeRotateWithIJobChunkSystem : ISystem
    {
        private EntityQuery allCubes;
        private ComponentTypeHandle<LocalTransform> localTransformTypeHandle;
        private ComponentTypeHandle<RotateSpeed> rotateTypeHandle;
        [BurstCompile]
        public void OnCreate(ref SystemState state) 
        {
            //预先生成查询和ComponentTypeHandle
            localTransformTypeHandle = state.GetComponentTypeHandle<LocalTransform>();
            rotateTypeHandle = state.GetComponentTypeHandle<RotateSpeed>(false);
            var entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, RotateSpeed>();
            allCubes = state.GetEntityQuery(in entityQueryBuilder);
        }
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //ComponentTypeHandle是在多个Update中共用的，
            //需要在System.OnUpdate内执行TypeHandle的Update
            localTransformTypeHandle.Update(ref state);
            rotateTypeHandle.Update(ref state);
            var jobChunk = new CubeRotateWithIJobChunk()
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                localTransformTypeHandle = localTransformTypeHandle,
                rotateTypeHandle = rotateTypeHandle
            };
            //IJobChunk的调度需要传递依赖，自身也会返回一个依赖
            //IJobChunk的Execute参数并不能自定义，因此这里调度时需要传递查询结果过去
            state.Dependency = jobChunk.ScheduleParallel(allCubes, state.Dependency);
        }
    }

}
