using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace Jobs.DOD
{
    struct JobParallelForTransformImplement : IJobParallelForTransform
    {
        [NativeDisableParallelForRestriction]
        public NativeList<int> vs;
        public void Execute(int index, TransformAccess transform)
        {
            //
        }
    }

    struct JobImplement : IJob
    {
        public void Execute()
        {
            throw new System.NotImplementedException();
        }
    }

    struct JobForImplement : IJobFor
    {
        public void Execute(int index)
        {
            throw new System.NotImplementedException();
        }
    }

    struct JobParallelForImplement : IJobParallelFor
    {
        public void Execute(int index)
        {
            throw new System.NotImplementedException();
        }
    }

    struct JobParallelForBatchImplement : IJobParallelForBatch
    {
        public void Execute(int startIndex, int count)
        {
            throw new System.NotImplementedException();
        }
    }

    struct JobParallelForDeferImplement : IJobParallelForDefer
    {
        public void Execute(int index)
        {
            throw new System.NotImplementedException();
        }
    }

    struct JobFilterImplement : IJobFilter
    {
        public bool Execute(int index)
        {
            throw new System.NotImplementedException();
        }
    }
    public class JobSchedule : MonoBehaviour
    {
        NativeList<int> executeTarget;
        private TransformAccessArray cubes;
        // Start is called before the first frame update
        void Start()
        {
            executeTarget = new NativeList<int>(100, Allocator.Persistent);
            for (int i = 0; i < 100; i++)
            {
                var randomValue = Random.Range(0, 100);
                executeTarget.Add(randomValue);
                var instance = new GameObject();
                instance.transform.position = Vector3.one * randomValue;
                cubes.Add(instance.transform);
            }
        }

        // Update is called once per frame
        void Update()
        {
            var jobParallelForTransform = new JobParallelForTransformImplement();
            var transformHandle = jobParallelForTransform.Schedule(cubes);

            var job = new JobImplement();
            job.Run();
            job.Schedule(transformHandle);

            var jobFor = new JobForImplement();
            jobFor.Run(100);

            var parallelFor = new JobParallelForImplement();
            parallelFor.Run(100);
            parallelFor.Schedule(100, 10, transformHandle);

            var parallelForBatch = new JobParallelForBatchImplement();
            parallelForBatch.Run(100,10);
            parallelForBatch.RunBatch(10);
            parallelForBatch.Run(100, 10);
            parallelForBatch.Schedule(100, 10, transformHandle);//并行调度100个JobBatch，每个并行批次索引数量为10
            parallelForBatch.ScheduleBatch(100, 10, transformHandle);//功能和上面的调度方法一样
            parallelForBatch.ScheduleParallel(100, 10, transformHandle);//功能和上面的调度方法一样

            var parallelForDefer = new JobParallelForDeferImplement();
            var deferHandle = parallelForDefer.Schedule(executeTarget, 16, transformHandle);//调度executeTarget长度的Job，16为每次迭代的数量

            var jobFilter = new JobFilterImplement();
            jobFilter.RunAppend(executeTarget, 100);
            jobFilter.RunFilter(executeTarget);//在主线程上过滤
            jobFilter.ScheduleFilter(executeTarget, transformHandle);//Execute返回false的索引会被从ScheduleFilter移除
            NativeList<int> result = new();
            var combineDepends = JobHandle.CombineDependencies(deferHandle, transformHandle);
            jobFilter.ScheduleAppend(result, 100, combineDepends);//Execute返回true的索引会附加到result列表内，当前调度依赖于deferHandle和transformHandle

        }

        private void OnDestroy()
        {
            executeTarget.Dispose();
        }
    }
}