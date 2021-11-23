using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using System.IO;
using System.Net;
using System.Text;

public static class GiftManager
{
    private static string giftJson;
    public const string GiftFile = "Gift List";
    public static GiftPool giftPool;    //外界调用数据池的入口
    public static void Launch() { }  //触发static构造

    static GiftManager()    //反序列化、索引化、自检
    {
        //反序列化
        Debug.Log("Gift Manager Constructing...");
        giftJson = FileToJSON(GiftFile);
        JSONtoPools();
        //索引化
        giftPool.giftList.Sort();
        //自检——索引是否与ID相同匹配
        CheckIDs();
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
        giftPool = JsonUtility.FromJson<GiftPool>(giftJson);
    }

    public static void UpdateAbilities(int giftID)   //更新人物能力
    {
        GiftInfluence influence = giftPool.giftList[giftID].influence;
        RealizeInfluence(influence.infAge, ref Abilities.Age);
        RealizeInfluence(influence.infEq, ref Abilities.Eq);
        RealizeInfluence(influence.infHp, ref Abilities.Health);
        RealizeInfluence(influence.infSp, ref Abilities.Spirit);
        RealizeInfluence(influence.infIq, ref Abilities.Iq);
        RealizeInfluence(influence.infKindness, ref Abilities.Kindness);
        RealizeInfluence(influence.infLifespan, ref Abilities.Lifespan);
        RealizeInfluence(influence.infLuck, ref Abilities.Luck);
        RealizeInfluence(influence.infProp, ref Abilities.Property);
        RealizeInfluence(influence.infSan, ref Abilities.Sanity);

        RealizeInfluenceSpeed(influence.speedAgeUp, ref Abilities.SpeedAgeUp);
        RealizeInfluenceSpeed(influence.speedEqUp, ref Abilities.SpeedEqUp);
        RealizeInfluenceSpeed(influence.speedHpUp, ref Abilities.SpeedHealthUp);
        RealizeInfluenceSpeed(influence.speedSpUp, ref Abilities.SpeedSpiritUp);
        RealizeInfluenceSpeed(influence.speedIqUp, ref Abilities.SpeedIqUp);
        RealizeInfluenceSpeed(influence.speedKindnessUp, ref Abilities.SpeedKindnessUp);
        RealizeInfluenceSpeed(influence.speedLuckUp, ref Abilities.SpeedLuckUp);
        RealizeInfluenceSpeed(influence.speedPropUp, ref Abilities.SpeedPropertyUp);
        RealizeInfluenceSpeed(influence.speedSanUp, ref Abilities.SpeedSanityUp);

        RealizeInfluenceSpeed(influence.speedAgeDown, ref Abilities.SpeedAgeDown);
        RealizeInfluenceSpeed(influence.speedEqDown, ref Abilities.SpeedEqDown);
        RealizeInfluenceSpeed(influence.speedHpDown, ref Abilities.SpeedHealthDown);
        RealizeInfluenceSpeed(influence.speedSpDown, ref Abilities.SpeedSpiritDown);
        RealizeInfluenceSpeed(influence.speedIqDown, ref Abilities.SpeedIqDown);
        RealizeInfluenceSpeed(influence.speedKindnessDown, ref Abilities.SpeedKindnessDown);
        RealizeInfluenceSpeed(influence.speedLuckDown, ref Abilities.SpeedLuckDown);
        RealizeInfluenceSpeed(influence.speedPropDown, ref Abilities.SpeedPropertyDown);
        RealizeInfluenceSpeed(influence.speedSanDown, ref Abilities.SpeedSanityDown);
    }

    private static void RealizeInfluence(string influence, ref float target)
    {
        if (influence == null || influence.Length <= 1)
        {
            Debug.LogError("Illegal Influence: " + influence);
            return;
        }
        switch (influence[0])
        {
            case '+':
                target += Int32.Parse(influence.Substring(1));
                break;
            case '-':
                target -= Int32.Parse(influence.Substring(1));
                break;
            case '*':
                target *= Int32.Parse(influence.Substring(1));
                break;
            case '/':
                target /= Int32.Parse(influence.Substring(1));
                break;
            case '=':
                target = Int32.Parse(influence.Substring(1));
                break;
            default:
                Debug.LogError("Syntax error in influence: " + influence);  //数据池语法错误
                break;
        }
    }

    private static void RealizeInfluenceSpeed(string influence, ref float target)
    {
        if (influence == null || influence.Length <= 1)
        {
            Debug.LogError("Illegal Influence: " + influence);
            return;
        }
        switch (influence[0])
        {
            case '+':
                target += float.Parse(influence.Substring(1));
                break;
            case '-':
                target -= float.Parse(influence.Substring(1));
                break;
            case '*':
                target *= float.Parse(influence.Substring(1));
                break;
            case '/':
                target /= float.Parse(influence.Substring(1));
                break;
            case '=':
                target = float.Parse(influence.Substring(1));
                break;
            default:
                Debug.LogError("Syntax error in influenceSpeed: " + influence);  //数据池语法错误
                break;
        }
    }
    
    private static void CheckIDs()
    {
        Debug.Log("Checking Gift IDs...");
        try
        {
            List<SingleGift> checkList = giftPool.giftList;
            for (int i = 0; i < checkList.Capacity; ++i)
            {
                if (checkList[i].id != i)
                {
                    throw new Exception("Illegal Gift ID Registered! at: " + i);
                }
            }
        }
        catch (Exception e) { Debug.LogError(e); }
    }
}

[Serializable]
public class GiftPool //天赋列表、临时概率表、天赋获取
{
    public List<SingleGift> giftList;
    [NonSerialized] private List<float> _weightList; //概率表（和为1）

    public List<int> FetchGifts(int num) //从池中随机获取指定数量的天赋
    {
        getWeightList();   //O(n)
        bool[] pickList = new bool[_weightList.Count];   //已选为false
        List<int> randomGifts = new List<int>();  //挑选好的gift列表
        for (int i = 0; i < Math.Min(num, _weightList.Count); ++i)
        {
            int id;
            do
            {
                id = RandomWithWeight.GetRandomWithWeight(_weightList);
            } while (pickList[id]);
            pickList[id] = true;
            randomGifts.Add(id);
        }
        return randomGifts;
    }
    private void getWeightList()
    {
        if (giftList == null)
        {
            Debug.LogError("Uninitialized giftList!");
            return;
        }
        int weightSum = 0;
        for (int i = 0; i < giftList.Count; ++i)
        {
            weightSum += giftList[i].possibility;
        }

        _weightList = new List<float>();
        for (int i = 0; i < giftList.Count; ++i)
        {
            _weightList.Add((float)giftList[i].possibility / weightSum);
        }
    }
}

[Serializable]
public class SingleGift : IComparable
{
    public int id;
    public string name;
    public int possibility;
    public GiftInfluence influence;
    public int CompareTo(object obj)
    {
        int result;
        SingleGift gift = obj as SingleGift;
        if (this.id > gift.id)
        {
            result = 1;
        }
        else result = 0;
        return result;
    }
}

[Serializable]
public class GiftInfluence
{
    public string infAge, infEq, infHp, infIq, infKindness, infLifespan, infLuck, infProp, infSan, infSp;
    public string speedAgeUp, speedEqUp, speedHpUp, speedIqUp, speedKindnessUp, speedLuckUp, speedPropUp, speedSanUp, speedSpUp;
    public string speedAgeDown, speedEqDown, speedHpDown, speedIqDown, speedKindnessDown, speedLuckDown, speedPropDown, speedSanDown, speedSpDown;
}