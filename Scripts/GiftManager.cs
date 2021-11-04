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
    private static string json;
    public const string FileName = "Gift List";
    public static GiftPool giftpool;    //外界调用数据池的入口

    static GiftManager()
    {
        FileToJSON();
        JSONtoObject();
    }

    public static void LaunchTest() { }  //在TestScript中调用，触发static构造

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
        giftpool = new GiftPool();
        giftpool = JsonUtility.FromJson<GiftPool>(json);
    }

    public static void UpdateAbilities(SingleGift selectedGift)   //更新人物能力
    {
        GiftInfluence influence = selectedGift.influence;
        realizeInfluence(influence.infAge, ref Abilities.Age);
        realizeInfluence(influence.infEq, ref Abilities.Eq);
        realizeInfluence(influence.infHp, ref Abilities.Health);
        realizeInfluence(influence.infSp, ref Abilities.Spirit);
        realizeInfluence(influence.infIq, ref Abilities.Iq);
        realizeInfluence(influence.infKindness, ref Abilities.Kindness);
        realizeInfluence(influence.infLifespan, ref Abilities.Lifespan);
        realizeInfluence(influence.infLuck, ref Abilities.Luck);
        realizeInfluence(influence.infProp, ref Abilities.Property);
        realizeInfluence(influence.infSan, ref Abilities.Sanity);

        realizeInfluenceSpeed(influence.speedAgeUp, ref Abilities.SpeedAgeUp);
        realizeInfluenceSpeed(influence.speedEqUp, ref Abilities.SpeedEqUp);
        realizeInfluenceSpeed(influence.speedHpUp, ref Abilities.SpeedHealthUp);
        realizeInfluenceSpeed(influence.speedSpUp, ref Abilities.SpeedSpiritUp);
        realizeInfluenceSpeed(influence.speedIqUp, ref Abilities.SpeedIqUp);
        realizeInfluenceSpeed(influence.speedKindnessUp, ref Abilities.SpeedKindnessUp);
        realizeInfluenceSpeed(influence.speedLuckUp, ref Abilities.SpeedLuckUp);
        realizeInfluenceSpeed(influence.speedPropUp, ref Abilities.SpeedPropertyUp);
        realizeInfluenceSpeed(influence.speedSanUp, ref Abilities.SpeedSanityUp);

        realizeInfluenceSpeed(influence.speedAgeDown, ref Abilities.SpeedAgeDown);
        realizeInfluenceSpeed(influence.speedEqDown, ref Abilities.SpeedEqDown);
        realizeInfluenceSpeed(influence.speedHpDown, ref Abilities.SpeedHealthDown);
        realizeInfluenceSpeed(influence.speedSpDown, ref Abilities.SpeedSpiritDown);
        realizeInfluenceSpeed(influence.speedIqDown, ref Abilities.SpeedIqDown);
        realizeInfluenceSpeed(influence.speedKindnessDown, ref Abilities.SpeedKindnessDown);
        realizeInfluenceSpeed(influence.speedLuckDown, ref Abilities.SpeedLuckDown);
        realizeInfluenceSpeed(influence.speedPropDown, ref Abilities.SpeedPropertyDown);
        realizeInfluenceSpeed(influence.speedSanDown, ref Abilities.SpeedSanityDown);
    }

    private static void realizeInfluence(string influence, ref int target)
    {
        if (influence == null || influence.Length <= 1) return;    //空字串表示没有影响
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

    private static void realizeInfluenceSpeed(string influence, ref float target)
    {
        if (influence == null || influence.Length <= 1) return;    //空字串表示没有影响
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

}
[Serializable]
public class GiftPool
{
    public int poolCapacity;
    public List<SingleGift> giftList;
    [NonSerialized] private List<float> _weightList; //概率表（和为1）

    public List<SingleGift> FetchGifts(int num, bool inheriting = false) //从池中随机获取指定数量的天赋
    {
        getWeightList(inheriting);   //O(n)
        bool[] pickList = new bool[_weightList.Count];   //已选为false
        List<SingleGift> randomGifts = new List<SingleGift>();  //挑选好的gift列表
        for (int i = 0; i < Math.Min(num, _weightList.Count); ++i)
        {
            int id;
            do
            {
                id = RandomWithWeight.GetRandomWithWeight(_weightList);
            } while (pickList[id]);
            pickList[id] = true;
            randomGifts.Add(giftList[id]);
        }
        return randomGifts;
    }
    private void getWeightList(bool inheriting = false)
    {
        if (giftList == null)
        {
            Debug.LogError("Uninitialized giftList!");
            return;
        }
        if (!inheriting)
        {
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
        else
        {
            int weightSum = 0;
            for (int i = 0; i < giftList.Count && !UIGoodGame.inheritedGifts.Contains(giftList[i]) && !Abilities.gifts.Contains(giftList[i]); ++i)
            {
                weightSum += giftList[i].possibility;
            }

            _weightList = new List<float>();
            for (int i = 0; i < giftList.Count && !UIGoodGame.inheritedGifts.Contains(giftList[i]) && !Abilities.gifts.Contains(giftList[i]); ++i)
            {
                _weightList.Add((float)giftList[i].possibility / weightSum);
            }
        }
    }
}

[Serializable]
public class SingleGift
{
    public int id;
    public string name;
    public int possibility;
    public GiftInfluence influence;
}

[Serializable]
public class GiftInfluence
{
    public string infAge, infEq, infHp, infIq, infKindness, infLifespan, infLuck, infProp, infSan, infSp;
    public string speedAgeUp, speedEqUp, speedHpUp, speedIqUp, speedKindnessUp, speedLuckUp, speedPropUp, speedSanUp, speedSpUp;
    public string speedAgeDown, speedEqDown, speedHpDown, speedIqDown, speedKindnessDown, speedLuckDown, speedPropDown, speedSanDown, speedSpDown;
}
