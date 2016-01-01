using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
    public Gemstone gemstone;
    public int rowNum = 7  ;
    public int columnNum = 10;

    public AudioClip errorClip;
    public AudioClip explosionClip;
    public AudioClip matchClip;
    public AudioClip swapClip;

    public GameObject explosion;

    private ArrayList GemArr = new ArrayList();

	void Start () {
        for (int rowIndex = 0; rowIndex < rowNum; ++rowIndex)
        {
            ArrayList temp = new ArrayList();
            for (int colIndex = 0; colIndex < columnNum; ++colIndex)
            {
                Gemstone gem = CreateGemAtPoint(rowIndex, colIndex);
                temp.Add(gem);
            }
            GemArr.Add(temp);
        }
        if (CheckMaches())
            RemoveMaches();
	}
    //在某个row col坐标生成一个宝石
    Gemstone CreateGemAtPoint(int rowIndex, int colIndex)
    {
        Gemstone gem = Instantiate(gemstone) as Gemstone;
        gem.rowIndex = rowIndex;
        gem.columnIndex = colIndex;
        gem.RandGemstone();
        gem.UpdatePos();
        gem.gameController = this;
        gem.transform.parent = this.transform;
        return gem;
    }
    //从二位数组中得到rol col 坐标的宝石
    Gemstone GetGem(int row,int col)
    {
        ArrayList temp = GemArr[row] as ArrayList;
        Gemstone gem = temp[col] as Gemstone;
        return gem;
    }
    //把宝石存储在二位数组里面
    void SetGem(int row, int col, Gemstone gem)
    {
        ArrayList temp = GemArr[row] as ArrayList;
        temp[col] = gem;
    }
    private Gemstone firstGem;
    public void Select(Gemstone secondGem)
    {
        if (firstGem == null) //第一次鼠标点击宝石，设置第一次点击的宝石
        {
            firstGem = secondGem;
            firstGem.sr.color = new Color(1, 1, 1, .5f);
        }
        else //第二次，交换宝石
        {
            //只让数周围的宝石交换
            if (Mathf.Abs(firstGem.rowIndex - secondGem.rowIndex) +
                Mathf.Abs(firstGem.columnIndex - secondGem.columnIndex) == 1)
            {
                Exchange(firstGem, secondGem);
                if (CheckMaches()) //如果匹配...
                    RemoveMaches();
                else //若不匹配 ,重新交换回来
                {
                    audio.PlayOneShot(swapClip);
                    StartCoroutine(WaitToSwap(firstGem, secondGem));
                }
            }else
                audio.PlayOneShot(errorClip);
            firstGem.sr.color = Color.white;
            firstGem = null; //重新更新第一次点击的宝石
        }
    }

    IEnumerator WaitToSwap(Gemstone g1, Gemstone g2)
    {
        yield return new WaitForSeconds(.3f);
        Exchange(g1, g2);
    }
    bool CheckMaches()
    {
        return CheckHorizontalMaches() || CheckVerticalMaches();
    }
    bool CheckHorizontalMaches()
    {
        bool isMaches = false;
        for (int rowIndex = 0; rowIndex < rowNum; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < columnNum - 2; columnIndex++)
            {
                if (GetGem(rowIndex, columnIndex).type == GetGem(rowIndex, columnIndex + 1).type &&
                    GetGem(rowIndex, columnIndex + 2).type == GetGem(rowIndex, columnIndex + 1).type)
                {//如果所在的那行有有3个以上相同类型的宝石
                    AddMaches(GetGem(rowIndex, columnIndex));
                    AddMaches(GetGem(rowIndex, columnIndex + 1));
                    AddMaches(GetGem(rowIndex, columnIndex + 2));
                    audio.PlayOneShot(matchClip);
                    isMaches = true;
                }
            }
        }
        return isMaches;
    }
    bool CheckVerticalMaches()
    {
        bool isMaches = false;
        for (int columnIndex = 0; columnIndex < columnNum; columnIndex++)
        {
            for (int rowIndex = 0; rowIndex < rowNum - 2; rowIndex++)
            {
                if (GetGem(rowIndex, columnIndex).type == GetGem(rowIndex + 1, columnIndex).type &&
                    GetGem(rowIndex + 2, columnIndex).type == GetGem(rowIndex + 1, columnIndex).type)
                {//如果所在的那行有有3个以上相同类型的宝石
                    AddMaches(GetGem(rowIndex, columnIndex));
                    AddMaches(GetGem(rowIndex + 1, columnIndex));
                    AddMaches(GetGem(rowIndex + 2, columnIndex));
                    isMaches = true;
                }
            }
        }
        return isMaches;
    }
    private ArrayList tmp;
    void AddMaches(Gemstone c)
    {
        if (tmp == null) 
            tmp = new ArrayList();
        if (tmp.IndexOf(c) == -1)
        {
            tmp.Add(c);
        }
    }

    void RemoveMaches()
    {
        audio.PlayOneShot(explosionClip);
        CameraShake.shakeFor(.1f, .1f);
        for (int i = 0; i < tmp.Count; i++)
        {
            Gemstone g = tmp[i] as Gemstone;
            Instantiate(explosion, g.transform.position, g.transform.rotation);
            Remove(g.rowIndex, g.columnIndex);
        }
        tmp = new ArrayList();
        StartCoroutine(WaitForSecond());
    }

    IEnumerator WaitForSecond()
    {
        yield return new WaitForSeconds(.3f);
        if (CheckMaches())
            RemoveMaches();
    }
    private Gemstone gem;
    void Remove(int row, int col) //移除某个row col坐标的宝石
    {
        Gemstone g = GetGem(row, col);
        g.Dispose();

        //消除后上面的宝石往下移动填补
        for (int rowIndex = row + 1;  rowIndex< rowNum; rowIndex++)
        {
            gem = GetGem(rowIndex, col);
            SetGem(rowIndex - 1, col, gem);
            gem.rowIndex = rowIndex - 1;
            gem.TweenToPos();
        }
        //上面继续生成新的宝石
        Gemstone newGem = CreateGemAtPoint(rowNum,col);
        newGem.rowIndex = rowNum - 1;
        newGem.columnIndex = col;
        SetGem(rowNum - 1, col, newGem);
        newGem.TweenToPos();
    }
    //两个宝石的交换
    private void Exchange(Gemstone g1, Gemstone g2)
    {
        //改变交换后的rol col 并存入二维数组中
        SetGem(g1.rowIndex, g1.columnIndex, g2);
        SetGem(g2.rowIndex, g2.columnIndex, g1);
        //交换Gemstone中的row
        int rowIndex = g1.rowIndex;
        g1.rowIndex = g2.rowIndex;
        g2.rowIndex = rowIndex;
        //交换Gemstone中的col
        int columnIndex = g1.columnIndex;
        g1.columnIndex = g2.columnIndex;
        g2.columnIndex = columnIndex;
        //更新坐标
        g1.TweenToPos();
        g2.TweenToPos();
    }
}
