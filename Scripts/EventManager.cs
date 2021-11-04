using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.IO;
using System.Text;
using Debug = UnityEngine.Debug;

public static class EventManager
{
    private static string json;
    public const string FileName = "Event List";
    public static EventPool eventPool;
    
    static EventManager()
    {
        FileToJSON();
        JSONtoObject();
    }
    
    public static void LaunchTest() {}  //在TestScript中调用，触发static构造
    
    private static void FileToJSON()  //从Streaming Assets中加载指定json
    {
        string path = Application.streamingAssetsPath + "/" + FileName + ".json";
        if (!File.Exists(path))
        {
            Debug.LogError("File not found: " + path);
            //尝试NetRequests获取数据
        }
        json = File.ReadAllText(path);
    }

    private static void JSONtoObject()
    {
        eventPool = new EventPool();
        eventPool = JsonUtility.FromJson<EventPool>(json);
    }

    public static void UpdateAbilities(SingleEvent latestEvent)   //更新人物能力
    {
        EventInfluence influence = latestEvent.influence;
        realizeInfluence(influence.infAge, ref Abilities.Age, Abilities.SpeedAgeUp, Abilities.SpeedAgeDown);
        realizeInfluence(influence.infEq, ref Abilities.Eq, Abilities.SpeedEqUp, Abilities.SpeedEqDown);
        realizeInfluence(influence.infHp, ref Abilities.Health, Abilities.SpeedHealthUp, Abilities.SpeedHealthDown);
        realizeInfluence(influence.infProp, ref Abilities.Property, Abilities.SpeedPropertyUp, Abilities.SpeedPropertyDown);
        realizeInfluence(influence.infSan, ref Abilities.Sanity, Abilities.SpeedSanityUp, Abilities.SpeedSanityDown);
        realizeInfluence(influence.infSp, ref Abilities.Spirit, Abilities.SpeedSpiritUp, Abilities.SpeedSpiritDown);
        realizeInfluence(influence.infIq, ref Abilities.Iq, Abilities.SpeedIqUp, Abilities.SpeedIqDown);
        realizeInfluence(influence.infKindness, ref Abilities.Kindness, Abilities.SpeedKindnessUp, Abilities.SpeedKindnessDown);
        //realizeInfluence(influence.infLifespan, ref Abilities.Lifespan, Abilities.SpeedLifespanUp, Abilities.SpeedLifespanDown);
        realizeInfluence(influence.infLuck, ref Abilities.Luck, Abilities.SpeedLuckUp, Abilities.SpeedLuckDown);
    }

    private static void realizeInfluence(string influence, ref float target, float increase, float decrease)
    {
        if (influence == null || influence.Length <= 1) return;
        
        switch (influence[0])
        {
            case '+': target += (Int32.Parse(influence.Substring(1)) * increase);
                break;
            case '-': target -= (Int32.Parse(influence.Substring(1)) * decrease);
                break;
            case '*': target *= (Int32.Parse(influence.Substring(1)) * increase);
                break;
            case '/': target /= (Int32.Parse(influence.Substring(1)) * decrease);
                break;
            case '=': target = Int32.Parse(influence.Substring(1));
                break;
            default:
                Debug.LogError("Syntax error in influence: " + influence);  //数据池语法错误
                break;
        }
    }

}

[Serializable]
public class EventPool
{
    public int poolCapacity;
    public List<SingleEvent> eventList;
    [NonSerialized] private List<float> _weightList; //概率表（和为1）
    //[NonSerialized] private List<SingleEvent> _prepareForPick; //该代码旨在移除不满足条件的事件，减少随机数据量

    public List<SingleEvent> FetchEvents(int num) //从池中随机获取指定数量的事件(经典玩法是每次获取一个事件)
    {
        if(Abilities.Age == 0) {    //出生事件
            List<SingleEvent> birthEvents = new List<SingleEvent>();
            birthEvents.Add(new SingleEvent());
            birthEvents[0].content = Abilities.Name + "出生了，是" + Abilities.Gender + "孩子";
            birthEvents[0].heightRatio = 0.5f;
            birthEvents[0].widthRatio = 0.5f;
            birthEvents[0].influence = new EventInfluence();
            birthEvents[0].influence.infAge = "+1";
            return birthEvents;
        }
        generateWeight(); //O(n)，已过滤了不满足条件的事件
        Debug.Log("the number of conditional events: " + _weightList.Count);
        List<SingleEvent> randomEvents = new List<SingleEvent>(); //挑选好的event列表
        for (int i = 0; i < Math.Min(num, _weightList.Count); ++i)
        {
            int id = RandomWithWeight.GetRandomWithWeight(_weightList);
            randomEvents.Add(eventList[id]);
        }
        
        return randomEvents;
    }

    private bool satisfyCondition(int i) //判断是否满足条件
    {
        return eventList[i].condition.ageMax >= Abilities.Age &&
               eventList[i].condition.ageMin <= Abilities.Age &&
               eventList[i].condition.eqMax >= Abilities.Eq &&
               eventList[i].condition.eqMin <= Abilities.Eq &&
               eventList[i].condition.iqMax >= Abilities.Iq &&
               eventList[i].condition.iqMin <= Abilities.Iq &&
               eventList[i].condition.hpMax >= Abilities.Health &&
               eventList[i].condition.hpMin <= Abilities.Health &&
               eventList[i].condition.luckMax >= Abilities.Luck &&
               eventList[i].condition.luckMin <= Abilities.Luck &&
               eventList[i].condition.propMax >= Abilities.Property &&
               eventList[i].condition.propMin <= Abilities.Property &&
               eventList[i].condition.sanMax >= Abilities.Sanity &&
               eventList[i].condition.sanMin <= Abilities.Sanity &&
               eventList[i].condition.spMax >= Abilities.Spirit &&
               eventList[i].condition.spMin <= Abilities.Spirit &&
               eventList[i].condition.kindnessMax >= Abilities.Kindness &&
               eventList[i].condition.kindnessMin <= Abilities.Kindness;
    }


    private void generateWeight()
    {
        if (eventList == null)
        {
            Debug.Log("Uninitialized eventList!");
            return;
        }

        int weightSum = 0;
        for (int i = 0; i < eventList.Count && satisfyCondition(i); ++i)    //将所有满足条件的事件放入
        {
            weightSum += eventList[i].possibility;
        }

        _weightList = new List<float>();
        for (int i = 0; i < eventList.Count && satisfyCondition(i); ++i)
        {
            _weightList.Add( (float)eventList[i].possibility / weightSum);
        }
    }
}
[Serializable]
public class SingleEvent
{
    public int id;
    public string content;
    public int possibility;
    public string career;
    public string stage;
    
    public float heightRatio;
    public float widthRatio;
    
    public Condition condition;
    public EventInfluence influence;
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
public class EventInfluence
{
    public string infAge, infEq, infHp, infIq, infKindness, infLifespan, infLuck, infProp, infSan, infSp;
}
