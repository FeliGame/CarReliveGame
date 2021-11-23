using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIMain : MonoBehaviour  //主游戏——上屏刷新事件
{
    //事件更新
    public int latestEvent; //最近事件
    public GameObject cardPrefab;   //卡片模版
    public GameObject latestEventCard;  //最近的事件卡片
    public List<GameObject> eventCardList;    //List是一个动态数组，事件卡片一个个右移
    private float updateInterval = 1f;   //新事件产生间隔（选项可改动）
    private float nextUpdate = 0.0f;    //下次更新的时间

    //屏幕分辨率
    private const float UpScreenHeight = 720;
    private const float UpScreenWidth = 4320;
    private const float DownScreenHeight = 1888;
    private const float DownScreenWidth = 1728;

    //属性栏（下屏）
    private Text ageText;      //年龄文本栏（此处不使用状态条）
    private Scrollbar propBar;
    private Scrollbar hpBar;
    private Scrollbar sanityBar;    //理智条有长短，同时会由当前长度影响颜色（正常为中间值）
    private Scrollbar iqBar;
    private Scrollbar eqBar;

    void Awake()
    {
        GameObject.Find("Name Text").GetComponent<Text>().text = "名字：" + Abilities.Name;
        ageText = GameObject.Find("Age Text").GetComponent<Text>();
        hpBar = GameObject.Find("HP Bar").GetComponent<Scrollbar>();
        propBar = GameObject.Find("Property Bar").GetComponent<Scrollbar>();
        sanityBar = GameObject.Find("Sanity Bar").GetComponent<Scrollbar>();
        iqBar = GameObject.Find("IQ Bar").GetComponent<Scrollbar>();
        eqBar = GameObject.Find("EQ Bar").GetComponent<Scrollbar>();
    }

    void Update()
    {

        //每隔updateInterval向Events请求新的随机事件，并生成新的事件卡片，将老卡片右移
        if (Time.time > nextUpdate)
        {
            nextUpdate = Time.time + updateInterval;

            if (Abilities.Health <= 0 || Abilities.Age >= Abilities.Lifespan)
            {  //将发生死亡事件
                //弹出文字提示死亡，点击进入结算界面
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);   //进入结算界面
            }
            else
            {
                if (Abilities.Age == 0)
                {    //出生事件
                    List<SinglePersonal> birthEvents = new List<SinglePersonal>();
                    birthEvents.Add(new SinglePersonal());
                    birthEvents[0].content = Abilities.Name + "出生了，是" + Abilities.Gender + "孩";
                    birthEvents[0].heightRatio = 0.5f;
                    birthEvents[0].widthRatio = 0.5f;
                    birthEvents[0].influence = new Influence();
                    birthEvents[0].influence.infAge = "+1";
                    return;
                }
                List<int> latestEventList = EventManager.personalPool.FetchEvents(1);  //从事件池随机抽取 一个 新事件，以后可以拓展多事件

                if (latestEventList.Count != 0)
                {
                    latestEvent = latestEventList[0];
                    EventManager.UpdateAbilities(latestEvent); //更新人物属性
                    Abilities.personalEventList.Add(latestEvent);
                    UpdateBar(); //更新下屏状态栏
                    ShowNewEvent(); //将latestEvent转换为卡片显示
                    eventCardList.Add(latestEventCard);
                }
            }

        }
    }

    private void UpdateBar()
    {
        ageText.text = "年龄：" + Abilities.Age;
        propBar.GetComponent<Scrollbar>().size = Abilities.Property / 100f;
        hpBar.GetComponent<Scrollbar>().size = Abilities.Health / 100f;
        sanityBar.GetComponent<Scrollbar>().size = Abilities.Sanity / 100f;
        iqBar.GetComponent<Scrollbar>().size = Abilities.Iq / 100f;
        eqBar.GetComponent<Scrollbar>().size = Abilities.Eq / 100f;
    }
    
    private void ShowNewEvent() //将latestEvent处理成卡片并显示，并右移所有eventCardList的位置
    {
        Debug.Log("年龄：" + Abilities.Age);

        //实例化新卡片，位置与卡组最后一张相同
        if (eventCardList.Count != 0)
        {
            latestEventCard = GameObject.Instantiate(cardPrefab, eventCardList[eventCardList.Count - 1].transform.position, eventCardList[eventCardList.Count - 1].transform.rotation, GameObject.Find("Up Canvas").transform);
        }
        //实例化新卡片，位置与预制件位置相同
        else latestEventCard = GameObject.Instantiate(cardPrefab, GameObject.Find("Up Canvas").transform);

        //调整新卡尺寸
        RectTransform rt = latestEventCard.GetComponent<RectTransform>();
        rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, UpScreenHeight * EventManager.personalPool.personalEventList[latestEvent].widthRatio);
        rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, UpScreenHeight * EventManager.personalPool.personalEventList[latestEvent].heightRatio);

        //调整新卡底色
        Image latestBg = latestEventCard.GetComponent<Image>();
        latestBg.color = BgColor();
        latestBg.enabled = true;

        float currentInsetToLeft = latestEventCard.GetComponent<RectTransform>().sizeDelta.x;
        for (int i = eventCardList.Count - 1; i >= 0; --i)
        {
            eventCardList[i].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, currentInsetToLeft, eventCardList[i].GetComponent<RectTransform>().sizeDelta.x);
            currentInsetToLeft += eventCardList[i].GetComponent<RectTransform>().sizeDelta.x;
        }

        //如果要显示文字，则去掉注释
        latestEventCard.GetComponentInChildren<Text>().text = EventManager.personalPool.personalEventList[latestEvent].content;

        //TTS阅读该段文本
        //... ...
    }

    private Color BgColor() //获取latestEvent的颜色
    {
        switch (EventManager.personalPool.personalEventList[latestEvent].possibility)
        {
            case 9: return Color.white;
            case 8: return Color.yellow;
            case 7: case 6: return Color.green;
            case 5: case 4: return Color.cyan;
            case 3: return Color.blue;
            case 2: return Color.magenta;
            default: return Color.red;
        };
    }
}
