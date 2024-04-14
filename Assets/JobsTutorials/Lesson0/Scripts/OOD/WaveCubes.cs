using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class WaveCubes : MonoBehaviour
{
    public GameObject archGo;
    public int xRadio = 40;
    public int zRadio = 40;
    /// <summary>
    /// 幅度
    /// </summary>
    public float amplitude = 9;
    /// <summary>
    /// 频率
    /// </summary>
    public float frequency = 3;
    /// <summary>
    /// 波长
    /// </summary>
    public float waveLength = 0.2f;
    private List<Transform> cubes;
    private List<float> distance;
    /// <summary>
    /// 自定义性能分析字段
    /// </summary>
    static readonly ProfilerMarker<int> profilerMarker = new("WaveCubes UpdateTransform", "Objects Count");
    void Start()
    {
        cubes = new List<Transform>(xRadio * zRadio);
        distance = new List<float>(xRadio * zRadio);
        for (int i = -xRadio; i < xRadio; i++)
        {
            for (int j = -zRadio; j < zRadio; j++)
            {
                var instance = Instantiate(archGo).transform;
                instance.position = new Vector3(1.1f * i, 0, 1.1f * j);
                distance.Add(Vector3.Distance(Vector3.zero, instance.position));
                cubes.Add(instance);
            }
        }
        archGo.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        using(profilerMarker.Auto(cubes.Count))
        {
            var time = Time.time;
            for (int i = 0, length = cubes.Count; i < length; i++)
            {
                var perPos = cubes[i].position;
                perPos.y = Mathf.Sin(time * frequency + distance[i] * waveLength) * amplitude;
                cubes[i].localPosition = perPos;
            }
        }        
    }
}
