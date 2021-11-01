using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using System.Text;


public static class TestJSONProvider    //网络数据库未成形的阶段暂时替代
{
    public static List<SingleGift> giftList;
    public static List<SingleEvent> eventList;
    public static string giftPath = Application.streamingAssetsPath + "/Gift List.json";
    public static string eventPath = Application.streamingAssetsPath + "/Event List.json";
    private static GiftPool giftPool;   //用于手工赋值的天赋池
    private static EventPool eventPool; 

    static TestJSONProvider()
    {
        giftPool = new GiftPool();
        giftPool.giftList = new List<SingleGift>(); //重置了所有事件
        giftPool.poolCapacity = 0;
        eventPool = new EventPool();
        eventPool.eventList = new List<SingleEvent>();//重置了所有事件
        eventPool.poolCapacity = 0;
        
        AddSingleGift(0,"试试就逝世","运气+1，智力-1",1,"","","","-1","","+1");
        AddSingleGift(1,"智力超常","智力+10，增长倍率+0.5",1,
            "","","","+10","","","","","",
            "","","","+0.5");
        AddSingleGift(2,"夭折","活不过10岁",1,"","","","","","=10"); //寿命控制

        //AddSingleEvent(1,"你死了",4,"","",0.4f,0.6f,"","","=0");   //死亡类事件
        AddSingleEvent(2,"平凡地活着",3,"","",0.4f,0.6f,"+1");
        //AddSingleEvent(3,"");

        RewriteFile();  //清空并使用上面添加的数据重写json
    }

    private static void AddSingleGift(int id, string name, string detail, int rarity, 
        string infAge = "", string infEq = "", string infHp = "", string infIq = "", 
        string infKindness = "", string infLifespan = "", string infLuck = "", string infProp = "", string infSan = "", string infSp = "",
        string speedAge = "", string speedEq = "", string speedHp = "", string speedIq = "", 
        string speedKindness = "", string speedLuck = "", string speedProp = "", string speedSan = "", string speedSp = "",
        string inheritAge = "", string inheritEq = "", string inheritHp = "", string inheritIq = "", 
        string inheritKindness = "", string inheritLuck = "", string inheritProp = "", string inheritSan = "", string inheritSp = "")
    {
        giftPool.poolCapacity += 1;
        SingleGift singleGift = new SingleGift();

        singleGift.id = id;
        singleGift.name = name;
        singleGift.detail = detail;
        singleGift.possibility = rarity;
        singleGift.influence = new GiftInfluence();
        singleGift.influence.infAge = infAge;
        singleGift.influence.infEq = infEq;
        singleGift.influence.infHp = infHp;
        singleGift.influence.infIq = infIq;
        singleGift.influence.infKindness = infKindness;
        singleGift.influence.infLifespan = infLifespan;
        singleGift.influence.infLuck = infLuck;
        singleGift.influence.infProp = infProp;
        singleGift.influence.infSan = infSan;
        singleGift.influence.infSp = infSp;
        singleGift.influence.speedAge = speedAge;
        singleGift.influence.speedEq = speedEq;
        singleGift.influence.speedHp = speedHp;
        singleGift.influence.speedIq = speedIq;
        singleGift.influence.speedKindness = speedKindness;
        singleGift.influence.speedLuck = speedLuck;
        singleGift.influence.speedProp = speedProp;
        singleGift.influence.speedSan = speedSan;
        singleGift.influence.speedSp = speedSp;
        singleGift.influence.inheritAge = inheritAge;
        singleGift.influence.inheritEq = inheritEq;
        singleGift.influence.inheritHp = inheritHp;
        singleGift.influence.inheritIq = inheritIq;
        singleGift.influence.inheritKindness = inheritKindness;
        singleGift.influence.inheritLuck = inheritLuck;
        singleGift.influence.inheritProp = inheritProp;
        singleGift.influence.inheritSan = inheritSan;
        singleGift.influence.inheritSp = inheritSp;
        
        giftPool.giftList.Add(singleGift);
    }

    private static void AddSingleEvent(int id , string content, int rarity,
        string career = "", string stage = "", float heightRatio = 0.5f, float widthRatio = 0.5f,
        string infAge = "", string infEq = "", string infHp = "", string infIq = "", 
        string infKindness = "", string infLifespan = "", string infLuck = "", string infProp = "", string infSan = "", string infSp = "",
        int maxAge = Int32.MaxValue, int maxEq = Int32.MaxValue, int maxHp = Int32.MaxValue, int maxIq = Int32.MaxValue, int maxKindness = Int32.MaxValue, int maxLuck = Int32.MaxValue, int maxProp = Int32.MaxValue, int maxSan = Int32.MaxValue, int maxSp = Int32.MaxValue,
        int minAge = Int32.MinValue, int minEq = Int32.MinValue, int minHp = Int32.MinValue, int minIq = Int32.MinValue, int minKindness = Int32.MinValue, int minLuck = Int32.MinValue, int minProp = Int32.MinValue, int minSan = Int32.MinValue, int minSp = Int32.MinValue) 
    {
        eventPool.poolCapacity += 1;
        SingleEvent singleEvent = new SingleEvent();
        
        singleEvent.id = id;
        singleEvent.content = content;
        singleEvent.possibility = rarity;
        singleEvent.career = career;
        singleEvent.stage = stage;
        singleEvent.heightRatio = heightRatio;
        singleEvent.widthRatio = widthRatio;
        singleEvent.influence = new EventInfluence();
        singleEvent.influence.infAge = infAge;
        singleEvent.influence.infEq = infEq;
        singleEvent.influence.infHp = infHp;
        singleEvent.influence.infIq = infIq;
        singleEvent.influence.infKindness = infKindness;
        singleEvent.influence.infLuck = infLuck;
        singleEvent.influence.infProp = infProp;
        singleEvent.influence.infSan = infSan;
        singleEvent.influence.infSp = infSp;
        singleEvent.condition = new Condition();
        singleEvent.condition.ageMax = maxAge;
        singleEvent.condition.ageMin = minAge;
        singleEvent.condition.eqMax = maxEq;
        singleEvent.condition.eqMin = minEq;
        singleEvent.condition.hpMax = maxHp;
        singleEvent.condition.hpMin = minHp;
        singleEvent.condition.iqMax = maxIq;
        singleEvent.condition.iqMin = minIq;
        singleEvent.condition.kindnessMax = maxKindness;
        singleEvent.condition.kindnessMin = minKindness;
        singleEvent.condition.luckMax = maxLuck;
        singleEvent.condition.luckMin = minLuck;
        singleEvent.condition.propMax = maxProp;
        singleEvent.condition.propMin = minProp;
        singleEvent.condition.sanMax = maxSan;
        singleEvent.condition.sanMin = minSan;
        singleEvent.condition.spMax = maxSp;
        singleEvent.condition.spMin = minSp;
        
        eventPool.eventList.Add(singleEvent);
    }

    private static void RewriteFile()
    {
        File.Create(giftPath).Close();  //创建后须使用Close解除文件流引用，否则会抛出错误
        string giftJson = JsonUtility.ToJson(giftPool, true);
        File.WriteAllText(giftPath, giftJson);
        File.Create(eventPath).Close();  //创建后须使用Close解除文件流引用，否则会抛出错误
        string eventJson = JsonUtility.ToJson(eventPool, true);
        File.WriteAllText(eventPath, eventJson);
    }

    public static void Stimulate() {} //激发构造函数
}
