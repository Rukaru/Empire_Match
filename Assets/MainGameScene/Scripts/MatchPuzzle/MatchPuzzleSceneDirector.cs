using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [SerializeField] SpriteRenderer field;
    [SerializeField] TileController prefabTile;
    [SerializeField] TextMeshProUGUI textGameScore;
    [SerializeField] TextMeshProUGUI comboLabel;
    [SerializeField] TextMeshProUGUI comboValue;
    [SerializeField] GameObject panelResult;
    [SerializeField] TextMeshProUGUI textResultScore;
    [SerializeField] AudioClip seDelete;
    [SerializeField, Range(0f, 1f)] float SEVol = 0.6f;
    [SerializeField] Vector2 fieldOffset;   //UI調整用オフセット
    [SerializeField] ScreenTransitionFX transitionFX;
    
    TileController[,] fieldTiles;
    TileController selectedTile;

    [Header("Combo Punch")]
    [SerializeField] float comboPunchScale = 1.9f;
    [SerializeField] float comboPunchTime = 0.05f;
    [SerializeField] float comboDownTime = 0.12f;
    
    Coroutine comboPunchCo;
    Vector3 comboBaseScale = Vector3.one;

    [Header("Start")]
    [SerializeField] GameObject startPanel;
    [SerializeField] TextMeshProUGUI startText;
    [SerializeField] float readyTime = 0.8f;
    [SerializeField] float startTime = 0.4f;
    bool isStarting = true;

    [Header("Start Cushion Sound")]
    //サウンド
    AudioSource audioSource;
    [SerializeField] AudioClip mainBGM;
    [SerializeField, Range(0f, 1f)] float BGMVol = 0.6f;
    [SerializeField] AudioClip startSE;
    [SerializeField, Range(0f, 1f)] float S_SEVol = 0.6f;


    enum GameMode
    {
        StartWait, 
        WaitFall,
        Delete,
        DeleteFlash,
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
    int maxComboScore;
    bool isInitialBoard = true;

    [SerializeField] BattleManager battleManager;
    
    

    private int chainCount;
    private List<TileController> removedTilesAll = new List<TileController>();
    // 全連鎖の消去ピース



    /// <summary>
    /// 2次元に並べた配列（いわゆる“マス目”）として宣言している。
    /// フィールドの各マスに TileController を入れて管理するための箱。
    /// </summary>
    
    void Start()
    {
        if (comboLabel != null)
        comboBaseScale = comboLabel.rectTransform.localScale;

        //サウンド
        audioSource = GetComponent<AudioSource>();
        battleManager = FindObjectOfType<BattleManager>();
        if (battleManager == null)
        {
            return;
        }
        battleManager.OnBattleFinished += OnBattleFinished;


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

        gameMode = GameMode.StartWait;
        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        isStarting = true;

        if(AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
        }

        if(startPanel != null) startPanel.SetActive(true);

        if(startText != null) startText.text = "READY?";
        yield return new WaitForSeconds(readyTime);

        if(startText != null) startText.text = "START!";
        yield return new WaitForSeconds(startTime);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(startSE, S_SEVol);
        }
        yield return new WaitForSeconds(startTime);

        if(AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(mainBGM, BGMVol);
        }

        if(startPanel != null) startPanel.SetActive(false);
        isStarting = false;

        gameMode = GameMode.WaitFall;
    }

    void OnBattleFinished(bool isWin)
    {
        Debug.Log($"[MatchPuzzle] OnBattleFinished received isWin={isWin}");
        GameResult(isWin);
    }

    IEnumerator WinSequenceToResult()
    {
        if(transitionFX != null)
        {
            yield return transitionFX.PlayBossDefeatToResult();
        }
        else
        {
            yield return new WaitForSeconds(3.5f);
        }

        SceneManager.LoadScene("ResultScene");
    }

    void Update()
    {
        if(gameMode == GameMode.StartWait)
        return;
        if(battleManager != null && battleManager.battleEnded)
        return; 
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

        else if (gameMode == GameMode.DeleteFlash)
        return;
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
        // 削除対象タイル取得
        List<Vector2Int> deleteTiles = GetDeleteTiles();

        // ─────────────────────
        // 削除がある場合（連鎖中）
        // ─────────────────────

        // 🔽 削除がある場合
        if (deleteTiles.Count > 0)
        {
            StartCoroutine(DeleteTilesWithFlash(deleteTiles));
            gameMode = GameMode.DeleteFlash;
            
            return;
        }

        // 🔽 削除が無い = 盤面が完全に安定した状態
        // ───────────────────────────

        // ★ 初期盤面処理の終了判定は「ここ」だけで行う
        if (isInitialBoard)
        {
            isInitialBoard = false;
            Debug.Log("初期盤面処理完了");
        }

        // ★ 連鎖が一度でも起きていれば結果を確定
        if (removedTilesAll.Count > 0)
        {
            ProcessChainResult();
        }

        // 入力可能状態へ戻す
        gameMode = GameMode.Touch;
    }

    int bestMaxCombo = 0;

    void ProcessChainResult()
    {
        PuzzleResult result = new PuzzleResult();
        result.chain = chainCount;

        foreach(var tile in removedTilesAll)
        {
            switch (tile.ColorType)
            {
                case 0: result.red++; break;
                case 1: result.green++; break;
                case 2: result.yellow++; break;
                case 3: result.blue++; break;
            }
        }
        result.maxComboThisTurn = maxComboScore;

        battleManager.OnPuzzleChainResult(result);
        
        //次の連鎖反応まで備える
        chainCount = 0;
        removedTilesAll.Clear();
        maxComboScore = 0;
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
        if (isStarting) return;

        if (!isTouchDown) // 「まだ触ってない状態」ならタイマー開始していい
        {
            battleManager.StartTurnTimeLimit();
        }

        if (battleManager != null && battleManager.battleEnded) return;

        if (Input.GetMouseButtonDown(0))
        {
            //スクリーン座標からワールド座標に
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit2D = Physics2D.Raycast(worldPoint, Vector2.zero);

            //あたり判定あるかどうか
            if (hit2D)
            {
                battleManager.StopTurnTimeLimit();

                // いま選択中のが残ってたら消す
                if (selectedTile != null) selectedTile.HighlightOff();

                // 今触ったタイルを取得して光らせる
                selectedTile = hit2D.collider.GetComponent<TileController>();
                if (selectedTile != null) selectedTile.HighlightOn();

                swapIndexA = WorldToIndexPosition(hit2D.transform.position);
                touchDownPoint = worldPoint;
                isTouchDown = true;
            }
        }
        //タッチ離れたとき
        else if (Input.GetMouseButtonUp(0))
        {
            if(!isTouchDown) return;
            
            if(selectedTile != null)
            {
                selectedTile.HighlightOff();
                selectedTile = null;
            }

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
        bool countScore = !isInitialBoard;

        if(countScore)
        {
            chainCount++;
        }

        if (countScore)
        {
            foreach (var item in deleteTiles)
            {
                TileController tile = GetFieldTile(item.x, item.y);
                if(tile != null)
                {
                    removedTilesAll.Add(tile);
                }
            }
        }

        foreach (var item in deleteTiles)
        {
            TileController tile = GetFieldTile(item.x, item.y);
            if(!tile) continue;

            tile.Delete();
            SetFieldTile(item.x, item.y);//内部データクリア
        }

        //スコアとコンボ計算
        if(!countScore) return;

        comboScore++;
        UpdateTextCombo();

        if (comboScore > maxComboScore)//最大値の更新
        {
            maxComboScore = comboScore;
        }

        int baseScore = deleteTiles.Count * deleteScore;
        int comboBonus = comboScore * deleteScore;

        gameScore += baseScore + comboBonus;
        textGameScore.text = gameScore.ToString();

        //SE
        audioSource.PlayOneShot(seDelete, SEVol);
    }

    IEnumerator DeleteTilesWithFlash(List<Vector2Int> deleteTiles)
    {
        // 先にハイライトON
        foreach (var item in deleteTiles)
        {
            TileController tile = GetFieldTile(item.x, item.y);
            if (tile != null)
            {
                tile.HighlightOn();
            }
        }

        // ほんの少し待つ（演出）
        yield return new WaitForSeconds(0.05f);

        // ② ここで必ず元に戻す（飛び散る時は元の色にしたい）
        foreach (var item in deleteTiles)
        {
            var tile = GetFieldTile(item.x, item.y);
            if (tile != null) tile.HighlightOff();
        }

        // 元の削除処理を呼ぶ
        DeleteTiles(deleteTiles);

        yield return new WaitForSeconds(0.10f);

        gameMode = GameMode.Fall;
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
        if(comboScore < 2)
        {
            comboValue.text = "";
            comboLabel.text = "";
            if (comboLabel != null)comboLabel.rectTransform.localScale = comboBaseScale;
            return;
        }

        comboLabel.text = comboScore.ToString();
        comboValue.text = " Combo!!";

        PunchComboNumber();
    }

    void PunchComboNumber()
    {
        if(comboLabel == null) return;

        if(comboPunchCo != null) StopCoroutine(comboPunchCo);
        comboPunchCo = StartCoroutine(ComboPunchCo());
    }

    IEnumerator ComboPunchCo()
    {
        var rt = comboLabel.rectTransform;

        float t = 0f;
        while(t < comboPunchTime)
        {
            t += Time.deltaTime;
            float k = t / comboPunchTime;
            float s = Mathf.Lerp(1f, comboPunchScale, k);
            rt.localScale = comboBaseScale * s;
            yield return rt;
        }

        t = 0f;
        while (t < comboPunchTime)
        {
            t += Time.deltaTime;
            float k = t / comboDownTime;
            float s = Mathf.Lerp(comboPunchScale, 1f, k);
            rt.localScale = comboBaseScale * s;
            yield return null;
        }

        rt.localScale = comboBaseScale;
        comboPunchCo = null;
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


    void GameResult(bool isWin)
    {
        Debug.Log($"[SAVE] score={gameScore}, maxCombo={battleManager.MaxCombo}");

        GameResultData.isWin = isWin;
        GameResultData.score = gameScore;

        GameResultData.killedEnemyCount = battleManager.KilledEnemy;
        GameResultData.killedBossCount = battleManager.KilledBoss;

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
