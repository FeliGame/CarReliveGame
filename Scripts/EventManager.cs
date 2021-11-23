using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.IO;
using System.Text;
using Debug = UnityEngine.Debug;
public static class EventManager    //管理各类事件
{
    private static string personalJson;
    private static string newsJson;
    private static string careerJson;
    public const string PersonalFile = "Personal Events";
    public const string NewsFile = "News Events";
    public const string CareerFile = "Career Events";
    public static PersonalPool personalPool;
    public static NewsPool newsPool;
    public static CareerPool careerPool;
    public static void Launch() { }  //触发static构造
    
    static EventManager()   //反序列化、索引化、自检
    {
        //反序列化
        Debug.Log("Event Manager Constructing...");
        personalJson = FileToJSON(PersonalFile);
        newsJson = FileToJSON(NewsFile);
        careerJson = FileToJSON(CareerFile);
        JSONtoPools();
        //索引化
        personalPool.personalEventList.Sort();
        newsPool.newsEventList.Sort();
        careerPool.careerTreeList.Sort();
        foreach (var item in careerPool.careerTreeList)
        {
            item.branchList.Sort();
        }
        //自检——索引是否与ID相同匹配
        CheckIDs();
    }
    
    public static void UpdateAbilities(int latestEvent)   //更新人物能力
    {
        Influence influence = EventManager.personalPool.personalEventList[latestEvent].influence;
        RealizeInfluence(influence.infAge, ref Abilities.Age, Abilities.SpeedAgeUp, Abilities.SpeedAgeDown);
        RealizeInfluence(influence.infEq, ref Abilities.Eq, Abilities.SpeedEqUp, Abilities.SpeedEqDown);
        RealizeInfluence(influence.infHp, ref Abilities.Health, Abilities.SpeedHealthUp, Abilities.SpeedHealthDown);
        RealizeInfluence(influence.infProp, ref Abilities.Property, Abilities.SpeedPropertyUp, Abilities.SpeedPropertyDown);
        RealizeInfluence(influence.infSan, ref Abilities.Sanity, Abilities.SpeedSanityUp, Abilities.SpeedSanityDown);
        RealizeInfluence(influence.infSp, ref Abilities.Spirit, Abilities.SpeedSpiritUp, Abilities.SpeedSpiritDown);
        RealizeInfluence(influence.infIq, ref Abilities.Iq, Abilities.SpeedIqUp, Abilities.SpeedIqDown);
        RealizeInfluence(influence.infKindness, ref Abilities.Kindness, Abilities.SpeedKindnessUp, Abilities.SpeedKindnessDown);
        RealizeInfluence(influence.infLifespan, ref Abilities.Lifespan, Abilities.SpeedLifespanUp, Abilities.SpeedLifespanDown);
        RealizeInfluence(influence.infLuck, ref Abilities.Luck, Abilities.SpeedLuckUp, Abilities.SpeedLuckDown);
    }
    
    private static string FileToJSON(string fileName)  //从Streaming Assets中加载指定json
    {
        string path = Application.streamingAssetsPath + "/" + fileName + ".json";
        if (!File.Exists(path))
        {
            Debug.LogError("File not found: " + path);
            //尝试NetRequests获取数据
        }
        return File.ReadAllText(path);
    }

    private static void JSONtoPools()
    {
        personalPool = JsonUtility.FromJson<PersonalPool>(personalJson);
        newsPool = JsonUtility.FromJson<NewsPool>(newsJson);
        careerPool = JsonUtility.FromJson<CareerPool>(careerJson);
    }

    private static void CheckIDs()
    {
        Debug.Log("Checking Personal Event IDs...");
        try
        {
            List<SinglePersonal> checkList = personalPool.personalEventList;
            for (int i = 0; i < checkList.Capacity; ++i)
            {
                if (checkList[i].id != i)
                {
                    throw new Exception("Illegal Personal Event ID Registered! at: " + i);
                }
            }
        }
        catch (Exception e) { Debug.LogError(e); }

        Debug.Log("Checking News Event IDs...");
        try
        {
            List<SingleNews> checkList = newsPool.newsEventList;
            for (int i = 0; i < checkList.Capacity; ++i)
            {
                if (checkList[i].id != i)
                {
                    throw new Exception("Illegal News Event ID Registered! at: " + i);
                }
            }
        }
        catch (Exception e) { Debug.LogError(e); }

        Debug.Log("Checking Career Tree IDs...");
        try
        {
            List<CareerTree> checkList = careerPool.careerTreeList;
            for (int i = 0; i < checkList.Capacity; ++i)
            {
                if (checkList[i].id != i)
                {
                    throw new Exception("Illegal Career Tree ID Registered! at: " + i);
                }
            }
        }
        catch (Exception e) { Debug.LogError(e); }

        foreach (CareerTree tree in careerPool.careerTreeList)
        {
            try
            {
                Debug.Log("Checking Career " + tree.content + ", ID is: " + tree.id);
                for (int i = 0; i < tree.branchList.Capacity; ++i)
                {
                    if (tree.branchList[i].id != i)
                    {
                        throw new Exception("Illegal Career Branch ID Registered! at: " + i);
                    }
                }
            }
            catch (Exception e) { Debug.LogError(e); }
        }
    }
    
    private static void RealizeInfluence(string influence, ref float target, float increase, float decrease)
    {
        if (influence == null || influence.Length <= 1)
        {
            Debug.LogError("Illegal Influence: " + influence);
            return;
        }
        switch (influence[0])
        {
            case '+':
                target += (Int32.Parse(influence.Substring(1)) * increase);
                break;
            case '-':
                target -= (Int32.Parse(influence.Substring(1)) * decrease);
                break;
            case '*':
                target *= (Int32.Parse(influence.Substring(1)) * increase);
                break;
            case '/':
                target /= (Int32.Parse(influence.Substring(1)) * decrease);
                break;
            case '=':
                target = Int32.Parse(influence.Substring(1));
                break;
            default:
                Debug.LogError("Syntax error in influence: " + influence);  //数据池语法错误
                break;
        }
    }

}



[Serializable]
public class PersonalPool   //事件列表、临时概率表、条件判断、事件获取
{
    public List<SinglePersonal> personalEventList;
    [NonSerialized] private List<float> _weightList; //概率表（和为1）

    public List<int> FetchEvents(int num) //从池中随机获取指定数量的事件
    {
        //先从CareerPool抽取事件，再从PersonalPool中抽取事件
        generateWeight(); //O(n)，已过滤了不满足条件的事件
        Debug.Log("the number of conditional events: " + _weightList.Count);
        List<int> randomEvents = new List<int>(); //挑选好的event列表
        for (int i = 0; i < Math.Min(num, _weightList.Count); ++i)
        {
            int id = RandomWithWeight.GetRandomWithWeight(_weightList);
            randomEvents.Add(id);
        }
        return randomEvents;
    }

    private bool satisfyCondition(int i) //判断是否满足条件
    {
        return personalEventList[i].condition.ageMax >= Abilities.Age &&
               personalEventList[i].condition.ageMin <= Abilities.Age &&
               personalEventList[i].condition.eqMax >= Abilities.Eq &&
               personalEventList[i].condition.eqMin <= Abilities.Eq &&
               personalEventList[i].condition.iqMax >= Abilities.Iq &&
               personalEventList[i].condition.iqMin <= Abilities.Iq &&
               personalEventList[i].condition.hpMax >= Abilities.Health &&
               personalEventList[i].condition.hpMin <= Abilities.Health &&
               personalEventList[i].condition.luckMax >= Abilities.Luck &&
               personalEventList[i].condition.luckMin <= Abilities.Luck &&
               personalEventList[i].condition.propMax >= Abilities.Property &&
               personalEventList[i].condition.propMin <= Abilities.Property &&
               personalEventList[i].condition.sanMax >= Abilities.Sanity &&
               personalEventList[i].condition.sanMin <= Abilities.Sanity &&
               personalEventList[i].condition.spMax >= Abilities.Spirit &&
               personalEventList[i].condition.spMin <= Abilities.Spirit &&
               personalEventList[i].condition.kindnessMax >= Abilities.Kindness &&
               personalEventList[i].condition.kindnessMin <= Abilities.Kindness;
    }


    private void generateWeight()
    {
        if (personalEventList == null)
        {
            Debug.Log("Uninitialized eventList!");
            return;
        }

        int weightSum = 0;
        for (int i = 0; i < personalEventList.Count && satisfyCondition(i); ++i)    //将所有满足条件的事件放入
        {
            weightSum += personalEventList[i].possibility;
        }

        _weightList = new List<float>();
        for (int i = 0; i < personalEventList.Count && satisfyCondition(i); ++i)
        {
            _weightList.Add((float)personalEventList[i].possibility / weightSum);
        }
    }


}

[Serializable]
public class NewsPool   //事件列表、临时概率表、条件判断、事件获取
{
    public List<SingleNews> newsEventList;
    [NonSerialized] private List<float> _weightList; //概率表（和为1）
    private void generateWeight()
    {
        if (newsEventList == null)
        {
            Debug.Log("Uninitialized eventList!");
            return;
        }

        int weightSum = 0;
        for (int i = 0; i < newsEventList.Count && satisfyCondition(i); ++i)    //将所有满足条件的事件放入
        {
            weightSum += newsEventList[i].possibility;
        }

        _weightList = new List<float>();
        for (int i = 0; i < newsEventList.Count && satisfyCondition(i); ++i)
        {
            _weightList.Add((float)newsEventList[i].possibility / weightSum);
        }
    }
    private bool satisfyCondition(int i) //判断是否满足条件
    {
        return newsEventList[i].condition.ageMax >= Abilities.Age &&
               newsEventList[i].condition.ageMin <= Abilities.Age;
    }
}

[Serializable]
public class CareerPool //事件列表、临时概率表、条件判断、事件获取
{
    public List<CareerTree> careerTreeList;
    [NonSerialized] private List<float> _weightList; //概率表（和为1）

}

[Serializable]
public class SinglePersonal : IComparable
{
    public int id;
    public string content;
    public int possibility;
    public float heightRatio;
    public float widthRatio;

    public Condition condition;
    public Influence influence;
    public int CompareTo(object obj)
    {
        int result;
        SinglePersonal personal = obj as SinglePersonal;
        if (this.id > personal.id)
        {
            result = 1;
        }
        else result = 0;
        return result;
    }
}

[Serializable]
public class SingleNews : IComparable
{
    public int id;
    public string content;
    public int possibility;
    public Condition condition;
    public Influence influence;
    public int CompareTo(object obj)
    {
        int result;
        SingleNews news = obj as SingleNews;
        if (this.id > news.id)
        {
            result = 1;
        }
        else result = 0;
        return result;
    }
}
[Serializable]
public class CareerTree : IComparable //career树
{
    public int id;  //从0开始，初始化时对应CareerPool的索引
    public string content;    //梦境、学业、运动员等抽象的概括性职业
    public int possibility;
    public List<CareerBranch> branchList; //存储同一个事件的多个分支
    public CareerEntry entry;   //开始该事件所需条件
    public int CompareTo(object obj)
    {
        int result;
        CareerTree tree = obj as CareerTree;
        if (this.id > tree.id)
        {
            result = 1;
        }
        else result = 0;
        return result;
    }
}

[Serializable]
public class CareerBranch : IComparable   //career的不同分支
{
    public int id;    //用于区分，从0开始且对于同career的每一个分支来说都是唯一的，初始化时排序使之与List索引对应
    public string content;  //分支内容
    public int possibility;  //同样满足条件时，先被抽取的概率
   
    public float heightRatio;
    public float widthRatio;
    public Condition condition;
    public Influence influence;
    public List<int> fromList;  //触发该事件的前置id列表
    public List<int> toList;    //该事件能触发的id列表
    
    public int CompareTo(object obj)
    {
        int result;
        CareerBranch branch = obj as CareerBranch;
        if (this.id > branch.id)
        {
            result = 1;
        }
        else result = 0;
        return result;
    }
}

[Serializable]
public class CareerEntry    //开始该职业的条件
{
    public int ageMax, ageMin;
    public int eqMax, eqMin;
    public int hpMax, hpMin;
    public int iqMax, iqMin;
    public int kindnessMax, kindnessMin;
    public int luckMax, luckMin;
    public int propMax, propMin;
    public int sanMax, sanMin;
    public int spMax, spMin;
    //特定事件触发
}

[Serializable]
public class Condition
{
    public int ageMax, ageMin;
    public int eqMax, eqMin;
    public int hpMax, hpMin;
    public int iqMax, iqMin;
    public int kindnessMax, kindnessMin;
    public int luckMax, luckMin;
    public int propMax, propMin;
    public int sanMax, sanMin;
    public int spMax, spMin;
}

[Serializable]
public class Influence
{
    public string infAge, infEq, infHp, infIq, infKindness, infLifespan, infLuck, infProp, infSan, infSp;
}
