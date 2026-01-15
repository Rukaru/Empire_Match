using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    [SerializeField] List<Sprite> matchColors;
    public int ColorType;
    const float MoveSpeed = 3.5f;
    Vector2 targetPosition;
    float targetDistance;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        GetComponent<SpriteRenderer>().sortingOrder = -10;
        GetComponent<BoxCollider2D>().isTrigger = false;

        Vector2 force = new Vector2(Random.Range(-500.0f, 500.0f), Random.Range(500.0f, 1000.0f));
        rb.AddForce(force);

        rb.gravityScale = MoveSpeed;
        rb.mass = 0.6f;
        Destroy(gameObject, 2);
    }
}
