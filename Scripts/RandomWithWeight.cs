using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomWithWeight
{
    private struct Section  //数值区间
    {
        public float minVal;
        public float maxVal;
    }
    //区间字典（每个权重占用了相应的区间）
    private static Dictionary<int, Section> valueSectionDic = new Dictionary<int, Section>();

    private static void DivideIntoSections(List<float> weightList)    //输入权重数组（和为1），静态构造函数在程序第一次调用相关该类的任意相关时调用
    {
        float startVal = 0f;
        valueSectionDic.Clear();
        for (int i = 0; i < weightList.Count; ++i)
        {
            Section section = new Section();
            section.minVal = startVal;
            section.maxVal = startVal + weightList[i];
            
            valueSectionDic.Add(i, section);
            startVal = section.maxVal;
        }
    }

    private static int GetSection(float randomVal)    //寻找随机值对应的区间
    {
        for (int i = 0; i < valueSectionDic.Count; ++i)
        {
            if (randomVal < valueSectionDic[i].maxVal)
            {
                return i;
            }
        }
        Debug.LogError("RandomWithWeightTypes not found!");
        return -1;
    }

    public static int GetRandomWithWeight(List<float> weightList)  //外界的唯一接口，返回按权重随机后的索引
    {
        DivideIntoSections(weightList);
        return GetSection(Random.Range(0, 1.0f));
    }
}

