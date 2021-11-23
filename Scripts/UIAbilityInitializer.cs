using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Random = System.Random;

public class UIAbilityInitializer : MonoBehaviour   //初始化界面使用的脚本类
{
    private InputField nameInput;    //名字输入框
    private Dropdown genderOption; //性别选项
    private Text giftIntroText;    //天赋栏注释
    private Color unselectedGiftColor = Color.white; //未选中时的天赋颜色
    private Color selectedGiftColor = Color.green; //选中的天赋颜色
    private List<GameObject> giftObjectsPresented;    //显示的天赋列表（皮）
    private List<int> giftsPresented;    //显示的天赋本体（肉）
    private int giftPresentedNum = 5;   //显示的天赋个数
    private int giftAvailableNum = 3;  //最大可选的天赋数
    private int giftChosenNum = 0;    //选中的天赋数

    private void Awake()
    {
        JSONRegister.Launch();   //本地生成JSON文件
        //以下测试用代码
        EventManager.Launch();
        GiftManager.Launch();
        //。。。至此

        //NetRequests.QueryEvent();   //向网络发起数据库调用请求，加载事件池（实际上只有定期更新时调用）,暂时用TestJSONProvider本地生成代替！
        giftAvailableNum = Math.Min(giftAvailableNum, GiftManager.giftPool.giftList.Capacity);    //可选天赋数最大值不超过展示数量
        Abilities.gifts = new List<int>();

        nameInput = GameObject.Find("Name InputField").GetComponent<InputField>();
        nameInput.onValueChanged.AddListener(SetName);   //监听事件
        genderOption = GameObject.Find("Gender DropDown").GetComponent<Dropdown>();
        genderOption.onValueChanged.AddListener(SetGender);
        giftIntroText = GameObject.Find("Gift Text").GetComponent<Text>();
        giftObjectsPresented = new List<GameObject>();

        for (int i = 0; i < giftPresentedNum; ++i)
        {
            giftObjectsPresented.Add(GameObject.Find("Gift (" + i + ")"));
        }
        int index = 0;
        foreach (var item in giftObjectsPresented)
        {
            int bindingIndex = index;   //监听事件绑定的是变量引用
            item.GetComponent<Button>().onClick.AddListener(() => SelectGift(item.GetComponent<Button>(), bindingIndex)); //直接使用index会导致监听出错
            ++index;
        }
        RefreshGiftIntro();
        Reroll();   //开局刷新天赋显示列表
    }

    private void SetName(string name)
    {
        Abilities.Name = name;
    }

    private void SetGender(int option)
    {
        switch (option)
        {
            case 0:
                Random rd = new Random();
                option = rd.Next(1, 3);
                if (option == 1)
                {
                    Abilities.Gender = "男";
                }
                else if (option == 2)
                {
                    Abilities.Gender = "女";
                }
                else
                {
                    Debug.LogError("In SetGender: Random generation overflow!");
                }
                break;
            case 1:
                Abilities.Gender = "男";
                break;
            case 2:
                Abilities.Gender = "女";
                break;
        }
        Debug.Log(option);
    }

    private void RefreshGiftIntro()
    {
        giftIntroText.text = "请选择天赋（" + giftChosenNum + " / " + giftAvailableNum + "）";   //更新UI
    }

    public void Reroll()    //摇骰子，为了减少卡顿可能：每次roll实际上是加载已随机好的列表，并同时开始执行新一轮roll
    {
        giftsPresented = new List<int>(GiftManager.giftPool.FetchGifts(giftPresentedNum));
        foreach(var item in giftObjectsPresented)    //重置表单
        {
            item.GetComponent<Text>().text = "";
        }
        for (int i = 0; i < Math.Min(giftPresentedNum, GiftManager.giftPool.giftList.Capacity); ++i)
        {
            giftObjectsPresented[i].GetComponent<Text>().text = GiftManager.giftPool.giftList[giftsPresented[i]].name;
        }
        HighlightChosenGift();
    }

    private void SelectGift(Button button, int pos)    //选中或取消选中该天赋项，pos用于定位
    {
        if (giftsPresented.Count < pos)
        {
            Debug.Log("No gift selected!");
            return;
        }
        //判断该按钮是否高光，有则将其去掉高光，并减少chosenNum，排除Abilities.gifts中该项
        else if (button.GetComponent<Text>().color == selectedGiftColor)
        {
            button.GetComponent<Text>().color = unselectedGiftColor;
            giftChosenNum -= 1;
            if (Abilities.gifts.Count > pos)
            {
                Abilities.gifts.RemoveAt(pos);
            }
        }
        else if (giftChosenNum >= giftAvailableNum)
        {
            Debug.Log("Too much gifts you selected!");
            //弹出小文字
            return;
        }
        //若无且该位置有天赋栏，则将其高光，并增加chosenNum，在Abilities.gifts中增添该项
        else if (pos < giftsPresented.Count)
        {
            button.GetComponent<Text>().color = selectedGiftColor;
            giftChosenNum += 1;
            Debug.Log("pos" + pos);
            Abilities.gifts.Add(giftsPresented[pos]);
        }
        RefreshGiftIntro();
    }

    private void HighlightChosenGift()  //高光已经选中的天赋（Reroll时会用到），未选中的高光取消
    {   //对每个presented的天赋在chosen中查找，若有则高光
        for (int i = 0; i < giftsPresented.Count; ++i)
        {
            if (Abilities.gifts.Contains(giftsPresented[i]))
            {
                giftObjectsPresented[i].GetComponent<Text>().color = selectedGiftColor;
            }
            else giftObjectsPresented[i].GetComponent<Text>().color = unselectedGiftColor;
        }
    }

    private void StartGame()
    {
        if (giftChosenNum < giftAvailableNum)
        {
            Debug.Log("You can choose more!");
            //可以不选择天赋开局，但会弹出提示窗口
        }
        //若没有选择性别，则随机选择一个
        if (Abilities.Gender == "" || Abilities.Gender == null)
        {
            SetGender(0);
        }
        Abilities.Bear();
        //初始化完成后，跳转到主界面
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void QuitGame()
    {
        //弹出确认窗口

        //退出游戏
        Application.Quit();
    }
}
