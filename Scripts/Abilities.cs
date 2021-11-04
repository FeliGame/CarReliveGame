using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abilities : MonoBehaviour  //封装人物属性
{
    public static List<SingleGift> gifts;   //当前拥有的天赋
    public static List<SingleEvent> events; //当前已经发生的事件
    //public static Gesture gesture;  //手势曲线，影响生涯选择

    public static string Name;
    public static string Gender;
    public static int Lifespan = 100; //寿命
    public static float Age = 0;  //年龄
    public static float Property = 0;    //家境
    public static float Health = 0;  //身体状况
    public static float Sanity = 0;  //精神状况（心情)
    public static float Luck = 0; //运气（影响稀有事件发生概率）
    public static float Iq = 0;  //智商（影响成绩、业绩）
    public static float Eq = 0;  //情商（影响人际关系、情感关系）
    public static float Spirit = 0;    //灵力(隐藏属性)
    public static float Kindness = 0;    //善恶值，正善负恶，影响人物选择(隐藏属性)

    //额外的事件机制
    public static bool isEqualized = false; //能力值平均
    
    //动态概率系统
    public static Dictionary<int, string> possibilityFluctuation; //对某个稀有度的事件有指定的概率增益或减益

    //Stage系统
    public static int stageOfPerson {
        get {
            if(Age < 4) {
                return 0;
            } else if(Age < 12) {
                return 1;
            } else if(Age < 21) {
                return 2;
            } else if(Age < 41) {
                return 3;
            } else if(Age < 66) {
                return 4;
            } else return 5;
        }
    }   //(0婴幼儿、1童年、2青少年、3青年、4中年、5老年)，不用
    public static Dictionary<string, int> stageOfEvents;   //新闻或环境发展阶段（左边是事物描述，右边是阶段，一概分为0,1,2,3,4,5，对于某一个键，想达到更高阶段必须经历过至少一件较低一阶段的事件）
    public static Dictionary<string, int> stageOfMates;    //亲友发展阶段（左边是人物对象，其余同上）
    //事件Career系统
    public static List<string> currentCareers; //当前参与的career列表，需要设计好进入career和退出career的系统
    public static int possibilityOfPlayer;   //个人事件概率
    public static int possibilityOfEnvironment;   //环境事件概率
    public static int possibilityOfMates;  //伙伴、家庭、朋友事件概率
    
    //得分系统：取决于事件稀有度，连续出发高稀有度事件（COMBO）会有累次增益
    public static long FinalScore = 0; //最终总分
    private static int scoreMagnifier = 1;    //稀有度*该倍数=事件得分
    private static float comboRate = 0.1f;     //连续出现稀有事件会有增益
    
    //以下为成长倍率（受天赋影响）
    //public static float SpeedLifespanUp = 1; //寿命，不用
    public static float SpeedAgeUp = 1;  //年龄
    public static float SpeedPropertyUp = 1;    //家境
    public static float SpeedHealthUp = 1;  //身体状况
    public static float SpeedSanityUp = 1;  //精神状况（心情)
    public static float SpeedIqUp = 1;  //智商（影响成绩、业绩）
    public static float SpeedEqUp = 1;  //情商（影响人际关系、情感关系）
    public static float SpeedSpiritUp = 1;    //灵力(隐藏属性)
    public static float SpeedKindnessUp = 1;    //善恶值，正善负恶，影响人物选择(隐藏属性)
    public static float SpeedLuckUp = 1;  //运气，影响事件发生稀有度

    //public static float SpeedLifespanDown = 1; //寿命，不用
    public static float SpeedAgeDown = 1;  //年龄
    public static float SpeedPropertyDown = 1;    //家境
    public static float SpeedHealthDown = 1;  //身体状况
    public static float SpeedSanityDown = 1;  //精神状况（心情)
    public static float SpeedIqDown = 1;  //智商（影响成绩、业绩）
    public static float SpeedEqDown = 1;  //情商（影响人际关系、情感关系）
    public static float SpeedSpiritDown = 1;    //灵力(隐藏属性)
    public static float SpeedKindnessDown = 1;    //善恶值，正善负恶，影响人物选择(隐藏属性)
    public static float SpeedLuckDown = 1;  //运气，影响事件发生稀有度

    public static void Bear()   //出生事件
    {
        foreach(var item in gifts)
        {
            GiftManager.UpdateAbilities(item);
        }
        Age = 0;
        Health = 5;
        Abilities.events = new List<SingleEvent>(); //重置事件列表
        //重置属性值
        //。。。
    }
}
