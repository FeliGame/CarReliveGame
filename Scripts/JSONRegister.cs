using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using System.Text;


public static class JSONRegister    //网络数据库未成形的阶段暂时替代
{
    //注意一下四个字段会使得Manager先初始化，以至于本次该类注入的事件无法被读取到程序内存
    public static string giftPath = Application.streamingAssetsPath + "/" + GiftManager.GiftFile + ".json";
    public static string personalPath = Application.streamingAssetsPath + "/" + EventManager.PersonalFile + ".json";
    public static string newsPath = Application.streamingAssetsPath + "/" + EventManager.NewsFile + ".json";
    public static string careerPath = Application.streamingAssetsPath + "/" + EventManager.CareerFile + ".json";
    private static GiftPool giftPool;   //用于手工赋值的天赋池
    private static PersonalPool personalPool;
    private static NewsPool newsPool;
    private static CareerPool careerPool;

    public static void Launch() { } //触发static构造

    static JSONRegister()   //录入事件、序列化
    {
        //重置
        giftPool = new GiftPool();
        giftPool.giftList = new List<SingleGift>();
        personalPool = new PersonalPool();
        personalPool.personalEventList = new List<SinglePersonal>();
        newsPool = new NewsPool();
        newsPool.newsEventList = new List<SingleNews>();
        careerPool = new CareerPool();
        careerPool.careerTreeList = new List<CareerTree>();
        //注入内容
        AddGift(0, "试试就逝世", "运气+1，智力-1", 1, "+1", "", "", "-1", "", "+1");
        AddGift(1, "智力超常", "智力+10，增长倍率+0.5", 1,
            "", "", "", "+10", "", "", "", "", "",
            "", "", "", "+0.5");
        AddGift(2, "夭折", "活不过10岁", 1, "", "", "", "", "", "=10"); //寿命控制

        AddPersonalEvent(0, "你死了", 4, "", 0.4f, 0.6f, "", "", "=0");   //死亡类事件
        AddPersonalEvent(1, "平凡地活着", 3, "", 0.4f, 0.6f, "+1");

        AddNewsEvent(0, "哈雷彗星光临地球", 1, "", "", "", "", "", "", "", "", "", "", 10, 60);
        AddNewsEvent(1, "全球多处地下水涌出，预计海拔上涨4米", 1);

        //下面这行不要动
        List<CareerBranch> branchList;
        //上面这行不要动

        //同一棵树的Branch写在一段，每段的第一行清空branchList，中间行添加branches，最后一行将branches合并到一个tree中
        branchList = new List<CareerBranch>();
        AddCareerBranch(branchList, 0, "你加入了篮球校队", 1, new List<int>{}, new List<int>{1});
        AddCareerBranch(branchList, 1, "你成为CBA新秀", 1, new List<int>{0}, new List<int>{});
        AddCareerTree(0, "篮球运动员", branchList, null, 1);

        branchList = new List<CareerBranch>();
        AddCareerBranch(branchList, 0, "你加入了校足球队", 1, new List<int>{}, new List<int>{1});
        AddCareerBranch(branchList, 1, "你成为国足希望", 1, new List<int>{0}, new List<int>{});
        AddCareerTree(2, "足球运动员", branchList, null, 1);

        //序列化
        Serialize(giftPool, giftPath);
        Serialize(personalPool, personalPath);
        Serialize(newsPool, newsPath);
        Serialize(careerPool, careerPath);
    }

    private static void AddGift(int id, string name, string detail, int possibility,
        string infAge = "", string infEq = "", string infHp = "", string infIq = "",
        string infKindness = "", string infLifespan = "", string infLuck = "", string infProp = "", string infSan = "", string infSp = "",
        string speedAge = "", string speedEq = "", string speedHp = "", string speedIq = "",
        string speedKindness = "", string speedLuck = "", string speedProp = "", string speedSan = "", string speedSp = "")
    {
        SingleGift singleGift = new SingleGift();

        singleGift.id = id;
        singleGift.name = name;
        singleGift.possibility = possibility;
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
        singleGift.influence.speedAgeUp = speedAge;
        singleGift.influence.speedEqUp = speedEq;
        singleGift.influence.speedHpUp = speedHp;
        singleGift.influence.speedIqUp = speedIq;
        singleGift.influence.speedKindnessUp = speedKindness;
        singleGift.influence.speedLuckUp = speedLuck;
        singleGift.influence.speedPropUp = speedProp;
        singleGift.influence.speedSanUp = speedSan;
        singleGift.influence.speedSpUp = speedSp;

        giftPool.giftList.Add(singleGift);
    }

    private static void AddPersonalEvent(int id, string content, int possibility,
        string stage = "", float heightRatio = 0.5f, float widthRatio = 0.5f,
        string infAge = "", string infEq = "", string infHp = "", string infIq = "",
        string infKindness = "", string infLifespan = "", string infLuck = "", string infProp = "", string infSan = "", string infSp = "", 
        int minAge = Int32.MinValue, int minEq = Int32.MinValue, int minHp = Int32.MinValue, int minIq = Int32.MinValue, int minKindness = Int32.MinValue, int minLuck = Int32.MinValue, int minProp = Int32.MinValue, int minSan = Int32.MinValue, int minSp = Int32.MinValue,
        int maxAge = Int32.MaxValue, int maxEq = Int32.MaxValue, int maxHp = Int32.MaxValue, int maxIq = Int32.MaxValue, int maxKindness = Int32.MaxValue, int maxLuck = Int32.MaxValue, int maxProp = Int32.MaxValue, int maxSan = Int32.MaxValue, int maxSp = Int32.MaxValue)
    {
        SinglePersonal personalEvent = new SinglePersonal();

        personalEvent.id = id;
        personalEvent.content = content;
        personalEvent.possibility = possibility;
        personalEvent.heightRatio = heightRatio;
        personalEvent.widthRatio = widthRatio;

        personalEvent.influence = new Influence();
        personalEvent.influence.infAge = infAge;
        personalEvent.influence.infEq = infEq;
        personalEvent.influence.infHp = infHp;
        personalEvent.influence.infIq = infIq;
        personalEvent.influence.infKindness = infKindness;
        personalEvent.influence.infLuck = infLuck;
        personalEvent.influence.infProp = infProp;
        personalEvent.influence.infSan = infSan;
        personalEvent.influence.infSp = infSp;

        personalEvent.condition = new Condition();
        personalEvent.condition.ageMax = maxAge;
        personalEvent.condition.ageMin = minAge;
        personalEvent.condition.eqMax = maxEq;
        personalEvent.condition.eqMin = minEq;
        personalEvent.condition.hpMax = maxHp;
        personalEvent.condition.hpMin = minHp;
        personalEvent.condition.iqMax = maxIq;
        personalEvent.condition.iqMin = minIq;
        personalEvent.condition.kindnessMax = maxKindness;
        personalEvent.condition.kindnessMin = minKindness;
        personalEvent.condition.luckMax = maxLuck;
        personalEvent.condition.luckMin = minLuck;
        personalEvent.condition.propMax = maxProp;
        personalEvent.condition.propMin = minProp;
        personalEvent.condition.sanMax = maxSan;
        personalEvent.condition.sanMin = minSan;
        personalEvent.condition.spMax = maxSp;
        personalEvent.condition.spMin = minSp;

        personalPool.personalEventList.Add(personalEvent);
    }

    private static void AddNewsEvent(int id, string content, int possibility,
        string infAge = "", string infEq = "", string infHp = "", string infIq = "",
        string infKindness = "", string infLifespan = "", string infLuck = "", string infProp = "", string infSan = "", string infSp = "",
        int minAge = Int32.MinValue, int maxAge = Int32.MaxValue)   //条件只留年龄，其余和AddPersonalEvent没有区别
    {
        SingleNews singleNews = new SingleNews();

        singleNews.id = id;
        singleNews.content = content;
        singleNews.possibility = possibility;

        singleNews.condition = new Condition();
        singleNews.condition.ageMax = maxAge;
        singleNews.condition.ageMin = minAge;

        singleNews.influence = new Influence();
        singleNews.influence.infAge = infAge;
        singleNews.influence.infEq = infEq;
        singleNews.influence.infHp = infHp;
        singleNews.influence.infIq = infIq;
        singleNews.influence.infKindness = infKindness;
        singleNews.influence.infLuck = infLuck;
        singleNews.influence.infProp = infProp;
        singleNews.influence.infSan = infSan;
        singleNews.influence.infSp = infSp;

        newsPool.newsEventList.Add(singleNews);
    }

    private static void AddCareerTree(int treeID, string content, List<CareerBranch> branchList, CareerEntry entry, int possibility = 1)
    {
        CareerTree tree = new CareerTree();
        tree.id = treeID;
        tree.content = content;
        tree.branchList = new List<CareerBranch>();
        tree.branchList = branchList;
        tree.entry = new CareerEntry();
        tree.entry = entry;
        tree.possibility = possibility;

        careerPool.careerTreeList.Add(tree);
    }

    private static void AddCareerBranch(List<CareerBranch> list, int branchID, string content, int possibility, List<int> fromIDList, List<int> toIDList, float heightRatio = 0.5f, float widthRatio = 0.5f,
        string infAge = "", string infEq = "", string infHp = "", string infIq = "",
        string infKindness = "", string infLifespan = "", string infLuck = "", string infProp = "", string infSan = "", string infSp = "",
        int maxAge = Int32.MaxValue, int maxEq = Int32.MaxValue, int maxHp = Int32.MaxValue, int maxIq = Int32.MaxValue, int maxKindness = Int32.MaxValue, int maxLuck = Int32.MaxValue, int maxProp = Int32.MaxValue, int maxSan = Int32.MaxValue, int maxSp = Int32.MaxValue,
        int minAge = Int32.MinValue, int minEq = Int32.MinValue, int minHp = Int32.MinValue, int minIq = Int32.MinValue, int minKindness = Int32.MinValue, int minLuck = Int32.MinValue, int minProp = Int32.MinValue, int minSan = Int32.MinValue, int minSp = Int32.MinValue)
    {
        CareerBranch branch = new CareerBranch();
        branch.id = branchID;
        branch.content = content;
        branch.possibility = possibility;
        branch.fromList = new List<int>();
        branch.fromList = fromIDList;
        branch.toList = new List<int>();
        branch.toList = toIDList;
        branch.heightRatio = heightRatio;
        branch.widthRatio = widthRatio;

        branch.influence = new Influence();
        branch.influence.infAge = infAge;
        branch.influence.infEq = infEq;
        branch.influence.infHp = infHp;
        branch.influence.infIq = infIq;
        branch.influence.infKindness = infKindness;
        branch.influence.infLuck = infLuck;
        branch.influence.infProp = infProp;
        branch.influence.infSan = infSan;
        branch.influence.infSp = infSp;

        branch.condition = new Condition();
        branch.condition.ageMax = maxAge;
        branch.condition.ageMin = minAge;
        branch.condition.eqMax = maxEq;
        branch.condition.eqMin = minEq;
        branch.condition.hpMax = maxHp;
        branch.condition.hpMin = minHp;
        branch.condition.iqMax = maxIq;
        branch.condition.iqMin = minIq;
        branch.condition.kindnessMax = maxKindness;
        branch.condition.kindnessMin = minKindness;
        branch.condition.luckMax = maxLuck;
        branch.condition.luckMin = minLuck;
        branch.condition.propMax = maxProp;
        branch.condition.propMin = minProp;
        branch.condition.sanMax = maxSan;
        branch.condition.sanMin = minSan;
        branch.condition.spMax = maxSp;
        branch.condition.spMin = minSp;

        list.Add(branch);
    }

    private static void Serialize(System.Object from, string path)    //将程序内存中数据写入指定路径
    {
        File.Create(path).Close();  //创建后须使用Close解除文件流引用，否则会抛出错误
        string json = JsonUtility.ToJson(from, true);
        File.WriteAllText(path, json);
    }

}
