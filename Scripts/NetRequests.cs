using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using System.Net;
using Newtonsoft.Json;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Networking;

//事件编织有两个维度——时间（相对寿命比例的若干区间），空间（多个职业路线，要注意多路线兼容的可能，暂不考虑互克路线）

public static class NetRequests //与网络数据库的连接，并且使用JSON Utility的接口存储json到Streaming Assets中
{
    private static DateTime lastUpdateTime;
    public static int updateInterval = 1;   //自动更新的间隔(天数)

    public static void AutomaticUpdate() //每次启动游戏会执行
    {
        if ((DateTime.Now - lastUpdateTime).Days > updateInterval)
        {
            QueryEvent();
            QueryGift();
            Debug.Log("Files updated！");
        }
    }
    
    public static void QueryEvent() 
    {
        //HTTP请求
        WWWForm form = new WWWForm();   //请求体
        form.AddField("name", "Feli");  //示例
        //
        /*
         * http上的事件库：多个事件间由^连接，`连接同一事件的不同属性（依次是importance、condition、），英文逗号连接同种属性
         * rarity语法：一位数字，决定卡片颜色与出现概率，9神明8传说7史诗6珍奇5稀有4重要3一般2平淡无奇1流水账0出生/死亡
         * height语法：一位数，i = 0~9表示cardHeight = (i + 1) * 0.1 * screenHeight
         * width语法：一位数，i = 0~9表示cardWidth = (i + 1) * 0.1 * screenWidth
         * content语法：内容介绍，由TTS朗读，注意不能使用英文逗号！
         * condition语法：min,max描述条件区间，用英文逗号隔开，content中一定用中文逗号, age, eq, hp, iq, kindness, prop, san, spirit
         * influence语法：同上，注意是对值影响，每个逗号分割的子串第一个字符是运算符+-*,还有等于（重新设定值）和恒等于（锁定数值）
         * age change(一般事件都是+1，少数同年事件、时间跨越类事件除外), property change, health change, sanity range, iq range, eq range, spirit range, kindness range
         */
        
        //下载到JSON File
        string json = "testing...";    //来自请求返回的json字串，若非json而是对象，则使用以下语句
        //json = JsonUtility.ToJson(giftPool, true);
        string path = Application.streamingAssetsPath + "/" + EventManager.PersonalFile + ".json";
        if (!File.Exists(path)) //若无该文件，则生成一个
        {
            File.Create(path).Close();
        }
        File.WriteAllText(path, json, Encoding.UTF8);
        lastUpdateTime = DateTime.Now;
    }

    public static void QueryGift()
    {
        //HTTP请求
        WWWForm form = new WWWForm();   //请求体
        form.AddField("name", "Feli");  //示例
        //
        /*
         * http上的天赋库：事件库语法的简化版
         * rarity语法：一位数字，决定卡片颜色与出现概率，9神明8传说7史诗6珍奇5稀有4重要3一般2平淡无奇1流水账0出生/死亡
         * content
         * influence_type语法：影响类型，有能力值、生涯路线、阶段事件、来生能力、提高来生天赋稀有度等方面
         * influence_content语法：与influence_type对应相关
         * 能力值：加减乘除一个正整数
         * 生涯路线：@一定会走的路线、!一定不会走的路线
         * 阶段事件：
         * 提高来生天赋稀有度：
         */
        
        //下载到JSON File
        string json = "testing...";    //来自请求返回的json字串，若非json而是对象，则使用以下语句
        //json = JsonUtility.ToJson(giftPool, true);
        string path = Application.streamingAssetsPath + "/" + GiftManager.GiftFile + ".json";
        if (!File.Exists(path)) //若无该文件，则生成一个
        {
            File.Create(path).Close();
        }
        File.WriteAllText(path, json, Encoding.UTF8);
        lastUpdateTime = DateTime.Now;
    }
}
