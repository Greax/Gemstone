using UnityEngine;
using System.Collections;

public class Gemstone : MonoBehaviour {

    public int rowIndex;//行号
    public int columnIndex;//列号

    //屏幕的偏移
    public float xOffset = -5.5f;
    public float yOffset = -3.5f;

    public GameObject[] gemstones;//存放宝石预制的数组
    public int type;//宝石类型号
    public GameController gameController;//全局控制器
    public SpriteRenderer sr;//方便改变选中时的颜色

    private GameObject gem;
    public  void UpdatePos()
    {
        this.transform.position = new Vector3(columnIndex * 1.2f + xOffset, rowIndex * 1.2f + yOffset, 0);
    }

    public void TweenToPos()
    {
        iTween.MoveTo(this.gameObject, iTween.Hash(
            "x", columnIndex * 1.2f + xOffset,
            "y", rowIndex * 1.2f + yOffset,
            "time",.5f
            ));
    }
    public void RandGemstone()
    {
        if (gem != null)
            return;
        type = Random.Range(0, gemstones.Length);
        gem = Instantiate(gemstones[type]) as GameObject;
        sr = gem.GetComponent<SpriteRenderer>();
        gem.transform.parent = this.transform;
    }

    void OnMouseDown()
    {
        gameController.Select(this);
    }

    public void Dispose()
    {
        gameController = null;
        Destroy(gem.gameObject);
        Destroy(this.gameObject);
    }
}
