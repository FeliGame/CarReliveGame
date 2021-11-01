using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UIGoodGame : MonoBehaviour
{
    public GameObject inheritedBarPrefab;
    private Text newGiftIntroText;
    private Text conclusionText;
    private List<GameObject> inheritedGiftObjects;  //继承天赋
    public static List<SingleGift> inheritedGifts;
    private List<GameObject> newGiftObjectsPresented;    //显示的天赋列表（皮）
    private List<SingleGift> newGiftsPresented;    //显示的天赋本体（肉）

    private Color unselectedGiftColor = Color.white; //未选中时的天赋颜色
    private Color selectedGiftColor = Color.green; //选中的天赋颜色
    private int giftNumOfPrevLife;
    private int newGiftPresentedNum = 3;   //显示的天赋个数
    private int newGiftAvailableNum;  //最大可选的天赋数
    private int newGiftChosenNum = 0;    //选中的天赋数

    void Awake()
    {
        giftNumOfPrevLife = Abilities.gifts == null ? 0 : Abilities.gifts.Count;
        newGiftAvailableNum = Math.Max(1, giftNumOfPrevLife);   //至少每轮要可选一个天赋
        InitConclusion();   //玩家本局结算
        InitInheritedGifts();  //随机继承一半的天赋
        ShowInheritedGifts();   //在Scroll View中展示继承的天赋
        InitNewGiftObjects();  //随机从天赋池抽取新的天赋，可选 最少为0，最多为开局设定的天赋数
        RefreshGiftIntro();
        Reroll();
    }

    private void InitConclusion()
    {
        conclusionText = GameObject.Find("Conclusion Text").GetComponent<Text>();
        string conclusion = Abilities.myName + "享年" + Abilities.Age + "岁，已继承以下天赋：";
        conclusionText.text = conclusion;
    }

    private void InitInheritedGifts()
    {
        if(Abilities.gifts == null) //非正常游戏流程
        {
            Debug.LogError("Inherited gifts undefined!");
            return;
        }
        if(giftNumOfPrevLife == 0) return;
        inheritedGifts = new List<SingleGift>();
        for (int i = 0; i < Math.Max(1, giftNumOfPrevLife / 2); ++i) //随机继承一半的天赋
        {
            int index = UnityEngine.Random.Range(0, Abilities.gifts.Count);
            inheritedGifts.Add(Abilities.gifts[index]);
            Abilities.gifts.RemoveAt(index);    //防止抽到重复的
        }
        Abilities.gifts.Clear();    //还剩一半天赋也要清除掉
    }

    private void ShowInheritedGifts()
    {
        foreach (var item in inheritedGifts)
        {
            GameObject newObj = GameObject.Instantiate(inheritedBarPrefab,inheritedBarPrefab.transform.position,inheritedBarPrefab.transform.rotation,inheritedBarPrefab.transform.parent);
            newObj.GetComponent<Text>().text = item.name;
        }
    }

    private void InitNewGiftObjects()
    {
        newGiftIntroText = GameObject.Find("NewGift Text").GetComponent<Text>();
        //增加监听器
        newGiftObjectsPresented = new List<GameObject>();
        for (int i = 0; i < newGiftPresentedNum; ++i)
        {
            newGiftObjectsPresented.Add(GameObject.Find("NGift (" + i + ")"));
        }
        int index = 0;
        foreach (var item in newGiftObjectsPresented)
        {
            int bindingIndex = index;   //监听事件绑定的是变量引用
            item.GetComponent<Button>().onClick.AddListener(() => SelectGift(item.GetComponent<Button>(), bindingIndex)); //直接使用index会导致监听出错
            ++index;
        }
    }
    private void RefreshGiftIntro()
    {
        newGiftIntroText.text = "请选择天赋（" + newGiftChosenNum + " / " + newGiftAvailableNum + "）";   //更新UI
    }
    public void SelectGift(Button button, int pos)    //选中或取消选中该天赋项，在Abilities中直接进行操作，pos用于定位
    {
        if (newGiftsPresented.Count < pos)
        {
            Debug.Log("No gift selected!");
            return;
        }
        //判断该按钮是否高光，有则将其去掉高光，并减少chosenNum，排除Abilities.gifts中该项
        else if (button.GetComponent<Text>().color == selectedGiftColor)
        {
            button.GetComponent<Text>().color = unselectedGiftColor;
            newGiftChosenNum -= 1;
            if (Abilities.gifts.Count > pos)
            {
                Abilities.gifts.RemoveAt(pos);
            }
        }
        else if (newGiftChosenNum >= newGiftAvailableNum)
        {
            Debug.Log("Too much gifts you selected!");
            //弹出小文字
            return;
        }
        //若无且该位置有天赋栏，则将其高光，并增加chosenNum，在Abilities.gifts中增添该项
        else if (pos < newGiftsPresented.Count)
        {
            button.GetComponent<Text>().color = selectedGiftColor;
            Debug.Log("button i tapped: " + button.GetComponent<Text>().text);
            newGiftChosenNum += 1;
            Debug.Log("picked: " + pos + newGiftsPresented[pos].name);
            Abilities.gifts.Add(newGiftsPresented[pos]);
        }
        RefreshGiftIntro();
    }

    public void HighlightChosenGift()  //高光已经选中的天赋（Reroll时会用到），未选中的高光取消
    {   //对每个presented的天赋在chosen中查找，若有则高光
        for (int i = 0; i < newGiftsPresented.Count; ++i)
        {
            if (Abilities.gifts.Contains(newGiftsPresented[i]))
            {
                Debug.Log(newGiftsPresented[i].name + " cause it turning green!");
                newGiftObjectsPresented[i].GetComponent<Text>().color = selectedGiftColor;
            }
            else newGiftObjectsPresented[i].GetComponent<Text>().color = unselectedGiftColor;
        }
    }
    public void Reroll()    //reroll时避开已经继承的天赋
    {
        newGiftsPresented = new List<SingleGift>(GiftManager.giftpool.FetchGifts(newGiftPresentedNum, true));
        //int index = -1;
        
        foreach(var item in newGiftObjectsPresented)    //重置表单
        {
            item.GetComponent<Text>().text = "";
        }
        for (int i = 0; i < Math.Min(newGiftsPresented.Count, GiftManager.giftpool.poolCapacity); ++i)
        {
            // if (inheritedGifts.Contains(newGiftsPresented[i]))
            // {
            //     newGiftsPresented.RemoveAt(i);
            //     continue; //该天赋已经继承，不显示}
            // }
            newGiftObjectsPresented[i].GetComponent<Text>().text = newGiftsPresented[i].name;   
        }
        HighlightChosenGift();
    }
    public void Restart()
    {
        if (newGiftChosenNum < newGiftAvailableNum)
        {
            Debug.Log("You can choose more!");
            //可以不选择天赋开局，但会弹出提示窗口
        }
        Abilities.gifts.AddRange(inheritedGifts);   //gifts中原本只有你新选的天赋，此处将其与前世继承天赋合并
        Abilities.Bear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
    public void Quit()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 2);
    }
}
