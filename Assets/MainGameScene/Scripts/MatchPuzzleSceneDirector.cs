using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class MatchPuzzleSceneDirector : MonoBehaviour
{
    //変更可能なゲームルール
    [SerializeField] int filedWidth;
    [SerializeField] int filedHeight;
    [SerializeField] int matchColorCount;
    [SerializeField] int deleteScore;
    [SerializeField] float gameTimer;

    [SerializeField] SpriteRenderer field;
    [SerializeField] TileController prefabTile;
    //[SerializeField] TextMeshProUGUI textGameTimer;
    [SerializeField] TextMeshProUGUI textGameScore;
    [SerializeField] TextMeshProUGUI textCombo;
    [SerializeField] GameObject panelResult;
    [SerializeField] TextMeshProUGUI textResultScore;
    [SerializeField] AudioClip seDelete;
    [SerializeField] Vector2 fieldOffset;   //UI調整用オフセット


    TileController[,] fieldTiles;
    

    enum GameMode
    {
        WaitFall,
        Delete,
        Fall,
        Spawn,
        Touch,
        WaitSwap,
        WaitBackSwap,
    }
    GameMode gameMode;//モード判別用

    //タイル交換
    Vector2Int swapIndexA;
    Vector2Int swapIndexB;
    Vector2 touchDownPoint;//押した座標
    bool isTouchDown;
    
    int gameScore;
    int comboScore;
    bool isInitialBoard = true;

    private int chainCount;
    private List<TileController> removedTilesAll = new List<TileController>();
    // 全連鎖の消去ピース

    //サウンド
    AudioSource audioSource;

    /// <summary>
    /// 2次元に並べた配列（いわゆる“マス目”）として宣言している。
    /// フィールドの各マスに TileController を入れて管理するための箱。
    /// </summary>
    
    void Start()
    {
        //サウンド
        audioSource = GetComponent<AudioSource>();

        //フィールドデータ作成
        fieldTiles = new TileController[filedWidth, filedHeight];
        //ゲーム画面
        field.transform.localScale = new Vector2(filedWidth, filedHeight);
        ///field というオブジェクト（恐らく盤面画像や背景）を盤面の大きさに合わせて拡大・縮小している。
        /// localScaleは大きさ倍率
        
        gameScore = 0;
        comboScore = 0;

        UpdateTextCombo();

        //リザルト非表示
        panelResult.SetActive(false);

        //タイル生成
        SpawnTiles();

        gameMode = GameMode.WaitFall;
    }

    void Update()
    {
        // gameTimer -= Time.deltaTime;
        // if (gameTimer < 0)
        // {
        //     GameResult();
        //     gameTimer = -1;
        // }

        // textGameTimer.text = "" + (int)(gameTimer + 1);

        if (gameMode == GameMode.WaitFall)  
        WaitFallMode();
        else if (gameMode == GameMode.Delete)
        DeleteMode();
        else if (gameMode == GameMode.Fall)
        FallMode();
        else if(gameMode == GameMode.Spawn)
        SpawnMode();
        else if (gameMode == GameMode.Touch)
        TouchMode();
        else if (gameMode == GameMode.WaitSwap)
        WaitSwapMode();
        else if (gameMode == GameMode.WaitBackSwap)
        WaitBackSwapMode();
    }


    /// <summary>
    /// インデックス座標（配列の位置） → ワールド座標（Unityの3D/2D空間）
    /// 「グリッド（マス目）ゲーム」 を作る時に必ず使う考え方
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    /*-------------------------------------------------------
    /          フィールドデータ(IndexToWorldPosition)
    --------------------------------------------------------*/
    public Vector2 IndexToWorldPosition(int x, int y)//画面中央に限定
    {
        // フィールドの中心 = Field.position + Offset
        Vector2 basePos = (Vector2)field.transform.position + fieldOffset;

        // 中央を基準に 0.5f の補正
        float worldX = basePos.x + (x - filedWidth / 2f + 0.5f);
        float worldY = basePos.y + (y - filedHeight / 2f + 0.5f);

        return new Vector2(worldX, worldY);
    }

    //タイル生成
    TileController SpawnTile(int x, int y)
    {
        Vector2 position = IndexToWorldPosition(x, y);
        TileController tile = Instantiate(prefabTile, position, Quaternion.identity);

        return tile;
    }

    //配列外か？
    public bool IsOutOfRange(int x, int y)
    {
        if (x < 0 || filedWidth - 1 < x ||
            y < 0 || filedHeight - 1 < y)
        {
            return true;
        }
        return false;
    }

    //データ獲得
    public TileController GetFieldTile(int x, int y)
    {
        if(IsOutOfRange(x, y)) return null;
        return fieldTiles[x, y];
    }

    //データのdefault,呼び出すための処理なので、一旦まとめ設定
    public void SetFieldTile(int x, int y, TileController tile = null)
    {
        if(IsOutOfRange(x, y)) return;
        fieldTiles[x, y] = tile;
    }

    void SpawnTiles()//空マスにタイルを補充する典型的な処理
    {
        for (int x = 0; x < filedWidth; x++)
        {
            int emptyCount = 0;//「空だったマスの数」をカウントする数値。
            for (int y = 0; y < filedHeight; y++)
            {
                if(GetFieldTile(x, y)) continue;
                
                TileController tile = SpawnTile(x, filedHeight + emptyCount);
                tile.GravityFall(IndexToWorldPosition(x, y));
            
                SetFieldTile(x, y, tile);
                
                emptyCount++;
            }
        }
    }
    ///フィールドを下から上へスキャンして(2重ループで座標全体check)
    /// タイルが無いマスに新しいタイルをスポーン（生成）して
    /// 上の見えない位置から落下させる処理

    void WaitFallMode()
    {
        if(!IsEndMoveTiles()) return;
        gameMode = GameMode.Delete;
    }

    void DeleteMode()
    {
        //デフォルトタッチモードへ
        gameMode = GameMode.Touch;
        List<Vector2Int> deleteTiles = GetDeleteTiles();

        if (deleteTiles.Count > 0)
        {
            DeleteTiles(deleteTiles);
            gameMode = GameMode.Fall;
        }
        else 
        {
            List<Vector2Int> movableTiles = GetMovableTiles();
            if(movableTiles.Count < 1)
            {
                deleteTiles.Clear();

                foreach(var tile in fieldTiles)
                {
                    Vector2Int index = WorldToIndexPosition(tile.transform.position);
                    deleteTiles.Add(index);
                }

                DeleteTiles(deleteTiles);//全タイル削除

                gameMode = GameMode.Fall;
            }

            //連鎖が完全に終了したタイミングで結果をまとめる
            if (!isInitialBoard && removedTilesAll.Count > 0)
            {
                ProcessChainResult();
            }

        }
    }

    void ProcessChainResult()
    {
        int red = 0;
        int green = 0;
        int blue = 0;
        int yellow = 0;

        foreach(var tile in removedTilesAll)
        {
            switch (tile.ColorType)
            {
                case 0: red++; break;
                case 1: green++; break;
                case 2: blue++; break;
                case 3: yellow++; break;
            }
        }

        //BattleManager に渡す（後で実装）
        PuzzleResult result = new PuzzleResult(chainCount, red, green, blue, yellow);

        Debug.Log("連鎖結果 => " + result.ToString());
        // 戦闘管理に送る予定（あとで作る）
        // BattleManager.Instance.OnPuzzleChainResult(chainCount, red, green, blue, yellow);

        //次の連鎖反応まで備える
        chainCount = 0;
        removedTilesAll.Clear();
    }

    void FallMode()
    {
        FallTiles();
        gameMode = GameMode.Spawn;
    }

    void SpawnMode()
    {
        SpawnTiles();
        gameMode = GameMode.WaitFall;
    }

    void TouchMode()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //スクリーン座標からワールド座標に
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit2D = Physics2D.Raycast(worldPoint, Vector2.zero);

            //あたり判定あるかどうか
            if (hit2D)
            {
                swapIndexA = WorldToIndexPosition(hit2D.transform.position);
                touchDownPoint = worldPoint;
                isTouchDown = true;
            }

            if(isInitialBoard)
            isInitialBoard = false;
        }
        //タッチ離れたとき
        else if (Input.GetMouseButtonUp(0))
        {
            if(!isTouchDown) return;

            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 vec = (worldPoint - touchDownPoint);
            Vector2Int indexDirection = Vector2Int.zero;

            if(Mathf.Abs(vec.x) < Mathf.Abs(vec.y))//縦軸
            {
                indexDirection = Vector2Int.up;
                if(vec.y < 0)
                {
                    indexDirection = Vector2Int.down;
                }
            }
            else if(Mathf.Abs(vec.y) < Mathf.Abs(vec.x))//横軸
            {
                indexDirection = Vector2Int.right;
                if(vec.x < 0)
                {
                    indexDirection = Vector2Int.left;
                }
            }

            //交換するタイルのインデックス
            swapIndexB = swapIndexA + indexDirection;
            SwapTiles(swapIndexA, swapIndexB);
            isTouchDown = false;

            comboScore = 0;
            UpdateTextCombo();

            //次のモードへ
            gameMode =  GameMode.WaitSwap;
        }
    }
    

    void WaitSwapMode()
    {
        //移動未終了
        if (!IsEndMoveTiles()) return;

        //削除可能か
        List<Vector2Int> deleteTiles = GetDeleteTiles();

        if(deleteTiles.Count > 0)
        {
            gameMode = GameMode.Delete;
        }
        else
        {
            SwapTiles(swapIndexA, swapIndexB);
            gameMode = GameMode.WaitBackSwap;
        }
    }

    void WaitBackSwapMode()//タイルが元から戻るのを待つモード
    {
        //移動未終了
        if (!IsEndMoveTiles()) return;

        gameMode = GameMode.Touch;
    }

    /*-------------------------------------------------------
    /         フィールドデータ(WorldToIndexPosition)
    --------------------------------------------------------*/
    Vector2Int WorldToIndexPosition(Vector2 worldPos)
    {
         
        Vector2 basePos = (Vector2)field.transform.position + fieldOffset;

        float localX = worldPos.x - basePos.x;
        float localY = worldPos.y - basePos.y;

        int x = Mathf.FloorToInt(localX + filedWidth / 2f);
        int y = Mathf.FloorToInt(localY + filedHeight / 2f);

        return new Vector2Int(x, y);
    }

    //タイルデータ入れ替わる
    bool SwapTileDatas(Vector2Int indexA, Vector2Int indexB)//物理上
    {
        if (IsOutOfRange(indexA.x, indexA.y) || IsOutOfRange(indexB.x, indexB.y)) return false;
        if (indexA == indexB) return false;

        TileController tmpTile = fieldTiles[indexA.x, indexA.y];
        fieldTiles[indexA.x, indexA.y] = fieldTiles[indexB.x, indexB.y];
        fieldTiles[indexB.x, indexB.y] = tmpTile;
        return true;
    }

    void SwapTiles(Vector2Int indexA, Vector2Int indexB)//視覚上
    {
        bool isSwapTileDatas = SwapTileDatas(indexA, indexB);
        if (!isSwapTileDatas) return;

        fieldTiles[indexA.x, indexA.y].SwapMove(fieldTiles[indexB.x, indexB.y].transform.position);
        fieldTiles[indexB.x, indexB.y].SwapMove(fieldTiles[indexA.x, indexA.y].transform.position);
    }
/*-------------------------------------------------------
    /         処理Endであるかどうか
--------------------------------------------------------*/
    //全てのタイル移動完了したか
    bool IsEndMoveTiles()
    {
        foreach(var item in fieldTiles)
        {
            if (!item) continue;
            if (item.enabled) return false;
        }
        return true;
    }
/*-------------------------------------------------------
    /         タイル削除
--------------------------------------------------------*/
//マッチ判定の本体
List<Vector2Int> GetMatchTiles(Vector2Int index, List<Vector2Int> directions)
    {
        List<Vector2Int> matchTiles = new List<Vector2Int>();
        matchTiles.Add(index);//開始位置
        
        int mainColor = GetFieldTile(index.x, index.y).ColorType;//色保存

        

        //全方向調べる
        foreach(var dir in directions)
        {
            Vector2Int checkindex = index + dir;//開始位置

            //まだ追加されなければループする
            while (!matchTiles.Contains(checkindex))
            {
                TileController tile = GetFieldTile(checkindex.x, checkindex.y);
                if(!tile) break;
                if(mainColor != tile.ColorType) break;

                matchTiles.Add(checkindex);//同じ色なら追加し、さらに1マス進む
                checkindex += dir;
            }
        }
        return matchTiles;
    }

    //新しいアイテムのみ追加
    void AddNewItems(List<Vector2Int> targetList, List<Vector2Int> items)
    {
        foreach (var item in items)
        {
            if(targetList.Contains(item)) continue;
            targetList.Add(item);
        }
    }

    //全体から削除可能なタイルを返す
    List<Vector2Int> GetDeleteTiles()
    {
        List<Vector2Int> deleteTiles = new List<Vector2Int>();

        foreach(var tile in fieldTiles)
        {
            if(!tile) continue;
            Vector2Int index = WorldToIndexPosition(tile.transform.position);

            //左右方向
            List<Vector2Int> directions = new List<Vector2Int>()
            {
                Vector2Int.left,
                Vector2Int.right
            };

            List<Vector2Int> matchTiles = GetMatchTiles(index, directions);//同じ色のタイル取得

            if(matchColorCount <= matchTiles.Count)
            {
                AddNewItems(deleteTiles, matchTiles);
            }

             //上下方向 
            directions = new List<Vector2Int>()
            {
                Vector2Int.up,
                Vector2Int.down
            };

            matchTiles = GetMatchTiles(index, directions);

            if(matchColorCount <= matchTiles.Count)
            {
                AddNewItems(deleteTiles, matchTiles);
            }
        }
        return deleteTiles;
    }
   
    //タイル削除
    void DeleteTiles(List<Vector2Int> deleteTiles)
    {
        if (!isInitialBoard)//最初の盤面生成じゃなければ連鎖カウント
        {
            chainCount++;    
        }

        //先に removedTilesAll に入れる（Delete() より前）
        foreach (var item in deleteTiles)
        {
            TileController tile = GetFieldTile(item.x, item.y);
            if(tile != null)
            {
                removedTilesAll.Add(tile);
                //DeleteTiles の「内部データクリアより前（tile.Delete() より前）」で入れるのが安全
            }
        }

        foreach (var item in deleteTiles)
        {
            TileController tile = GetFieldTile(item.x, item.y);
            if(!tile) continue;

            tile.Delete();
            SetFieldTile(item.x, item.y);//内部データクリア
        }
        if (isInitialBoard)
        {
            comboScore = 0;
            UpdateTextCombo();
            return;
        }

        //スコアとコンボ計算

        comboScore++;
        UpdateTextCombo();

        int baseScore = deleteTiles.Count * deleteScore;
        int comboCount = comboScore * deleteScore;

        gameScore += baseScore + comboScore;
        textGameScore.text = "" + gameScore;

        //SE
        audioSource.PlayOneShot(seDelete);
    }

    //落下するとき空いているスペースあるかどうか
    int GetBottomY(int x, int y)
    {
        int bottomY = -1;//返却するｙ座標

        for (int checkY = y-1; 0 <= checkY; checkY--)//一番下のｙを探す
        {
            if(IsOutOfRange(x, checkY)) continue;
            if(!GetFieldTile(x, checkY))
            {
                bottomY = checkY;
            }
        }
        return bottomY;
    }

    void FallTiles()
    {
        foreach(var tile in fieldTiles)
        {
            if (!tile) continue;

            //タイルの現在位置を特定するために
            Vector2Int indexA = WorldToIndexPosition(tile.transform.position);
            
            int bottomY = GetBottomY(indexA.x, indexA.y);
            if (bottomY == -1) continue;

            Vector2Int indexB = new Vector2Int(indexA.x, bottomY);//場所

            SwapTileDatas(indexA, indexB);
            tile.GravityFall(IndexToWorldPosition(indexB.x, indexB.y));   
        }
    }

    //コンボ表示
    void UpdateTextCombo()
    {
        string text = "" + comboScore + " Combo!!";

        if(comboScore < 2)
        {
            text = "";
        }

        textCombo.text = text;
    }

    //このタイルはスワイプするとどれかの方向でマッチが作れる
    List<Vector2Int> GetMovableTiles()
    {
        List<Vector2Int> movableTiles = new List<Vector2Int>();

        foreach ( var tile in fieldTiles)
        {
            Vector2Int indexA = WorldToIndexPosition(tile.transform.position);//現在のタイル取得

            List<Vector2Int> directions = new List<Vector2Int>()
            {
                Vector2Int.left,
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.down
            };

            foreach(var dir in directions)
            {
                Vector2Int indexB = indexA + dir;
                SwapTileDatas(indexA, indexB);

                if(GetDeleteTiles().Count > 0)
                {
                    if (!movableTiles.Contains(indexA))//同じタイルを重複追加しないようにチェックしつつ登録。
                    {
                        movableTiles.Add(indexA);
                    }
                }

                SwapTileDatas(indexA, indexB);
            }
        }
        return movableTiles;
    }

    void GameResult()
    {
        textResultScore.text = "" + gameScore;
        panelResult.SetActive(true);

        enabled = false;
        CleaerAllTils();
    }

    //リトライボタン
    public void OnClickRetry()
    {
        SceneManager.LoadScene("MatchPuzzle");
    }

    void CleaerAllTils()
    {
        for (int x = 0; x < filedWidth; x++)
        {
            for(int y = 0; y < filedHeight; y++)
            {
                TileController tile = fieldTiles[x, y];
                if (tile)
                {
                    Destroy(tile.gameObject);
                    fieldTiles[x, y] = null;
                }
            }
        }
    }
}
