using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abilities : MonoBehaviour  //封装人物属性
{
    public static List<SingleGift> gifts;   //当前拥有的天赋
    public static List<SingleEvent> events; //当前已经发生的事件
    //public static Gesture gesture;  //手势曲线，影响生涯选择

    public static string myName;
    public static string myGender;
    public static int Lifespan = 5; //寿命
    public static int Age = 0;  //年龄
    public static int Property = 0;    //家境
    public static int Health = 0;  //身体状况
    public static int Sanity = 0;  //精神状况（心情)
    public static int Luck = 0; //运气（影响稀有事件发生概率）
    public static int Iq = 0;  //智商（影响成绩、业绩）
    public static int Eq = 0;  //情商（影响人际关系、情感关系）
    public static int Spirit = 0;    //灵力(隐藏属性)
    public static int Kindness = 0;    //善恶值，正善负恶，影响人物选择(隐藏属性)
    
    //得分系统：取决于事件稀有度，连续出发高稀有度事件（COMBO）会有累次增益
    public static long FinalScore = 0; //最终总分
    private int scoreMagnifier = 1;    //稀有度*该倍数=事件得分
    private float comboRate = 0.1f;     //
    
    //以下为成长倍率（受天赋影响）
    public static float SpeedLifespan = 1; //寿命
    public static float SpeedAge = 1;  //年龄
    public static float SpeedProperty = 1;    //家境
    public static float SpeedHealth = 1;  //身体状况
    public static float SpeedSanity = 1;  //精神状况（心情)
    public static float SpeedIq = 1;  //智商（影响成绩、业绩）
    public static float SpeedEq = 1;  //情商（影响人际关系、情感关系）
    public static float SpeedSpirit = 1;    //灵力(隐藏属性)
    public static float SpeedKindness = 1;    //善恶值，正善负恶，影响人物选择(隐藏属性)
    public static float SpeedLuck = 1;  //运气，影响事件发生稀有度

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
