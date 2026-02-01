using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    [SerializeField] List<Sprite> matchColors;
    [SerializeField] SpriteRenderer sr;
    [SerializeField] SpriteRenderer highlightSr;
    public int ColorType;
    const float MoveSpeed = 3.5f;
    Vector2 targetPosition;
    float targetDistance;
    private Rigidbody2D rb;
 
    Color baseColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if(sr == null) sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;

        if(highlightSr != null)
        {
            highlightSr.enabled = false;

            // ★ここ重要：必ず本体より前に
            highlightSr.sortingLayerID = sr.sortingLayerID;
            highlightSr.sortingOrder   = sr.sortingOrder + 1;

            highlightSr.color = new Color(1f, 1f, 1f, 0.7f);
        }
    }

    void Start()
    {
        //カラーをランダムで決定
        ColorType = Random.Range(0, matchColors.Count);

        SpriteRenderer spr = GetComponent<SpriteRenderer>();
        spr.sprite = matchColors[ColorType];
    }

    
    void Update()
    {
        float moveDistance = rb.velocity.y * Time.deltaTime;

        //交換用
        if (rb.gravityScale < 1)
        {
            moveDistance = MoveSpeed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, targetPosition,moveDistance);
        }

        targetDistance -= Mathf.Abs(moveDistance);//今フレームで実際に動いた距離
        //Mathf.Abs（絶対値)
        if (targetDistance < 0)
        {
            SnapToTargetPosition();
        }
    }
    /*-------------------------------------------------------
    /                  タイル移動
    --------------------------------------------------------*/

    public void SwapMove(Vector2 position)//「どこへ移動させたいか」 という情報を渡すための箱。
    {
        targetPosition = position;//位置保存
        targetDistance = Vector2.Distance(transform.position, position);//現在の座標とターゲットとの距離を計算
        enabled = true;
    }

    public void GravityFall(Vector2 position)//自然落下
    {
        SwapMove(position);
        rb.gravityScale = MoveSpeed;
    }

    //物体を止めて、目標位置にしっかりとめるよう
    void SnapToTargetPosition()
    {
        rb.gravityScale =0;
        rb.velocity = Vector2.zero;
        transform.position = targetPosition;

        enabled = false;
    }

/*-------------------------------------------------------
    /         タイル削除
--------------------------------------------------------*/
    public void Delete()
    {
        HighlightOff();

        // ① 見た目：背面へ
        var sr = GetComponent<SpriteRenderer>();
        sr.sortingOrder = -10; // もっと奥へ（値は盤面より小さく）

        // // ② 入力：Raycastを完全に無視（超重要）
        // gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        // // Colliderを無効化（これも重要）
        // var col = GetComponent<BoxCollider2D>();
        // if (col != null) col.enabled = false;

        // ③ 物理で飛ばす（今まで通り）
        var rb = GetComponent<Rigidbody2D>();
        Vector2 force = new Vector2(Random.Range(-500.0f, 500.0f), Random.Range(500.0f, 1000.0f));
        rb.AddForce(force);

        rb.gravityScale = MoveSpeed;
        rb.mass = 0.6f;

        Destroy(gameObject, 2);
    }

    /*-------------------------------------------------------
    /         タイル発光
    --------------------------------------------------------*/
    public void HighlightOn()
    {
        //sr.color = Color.magenta;
        if (highlightSr != null) highlightSr.enabled = true;
    }

    public void HighlightOff()
    {
        //sr.color = baseColor;
        if (highlightSr != null) highlightSr.enabled = false;
    }
}
