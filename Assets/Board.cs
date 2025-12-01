using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap;
    public Piece activePiece;
    public TetrominoData[] tetrominoes;

    // ↓ 1. 盤面のサイズを定義（横10マス、縦20マス）
    // 左下が(-5, -10)、サイズが(10, 20) という意味です
    public RectInt Bounds = new RectInt(new Vector2Int(-5, -10), new Vector2Int(10, 20));

    private void Awake()
    {
        for (int i = 0; i < this.tetrominoes.Length; i++) {
            this.tetrominoes[i].Initialize();
        }
    }

    private void Start()
    {
        SpawnPiece();
    }

    public void SpawnPiece()
    {
        int random = Random.Range(0, this.tetrominoes.Length);
        TetrominoData data = this.tetrominoes[random];

        // スポーン位置（上の方）
        Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);

        this.activePiece.Initialize(this, spawnPosition, data);
        
        // ↓ ここを変更！
        // 「もしスポーン位置に置けるなら（空いているなら）」
        if (IsValidPosition(this.activePiece, spawnPosition))
        {
            Set(this.activePiece);
        }
        else
        {
            // 「もう置けない（積み上がっちゃった）なら」
            GameOver();
        }
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    public void GameOver()
    {
        this.tilemap.ClearAllTiles();
    }

    // 行が揃っているかチェックして消す（完成版）
    public void ClearLines()
    {
        RectInt bounds = this.Bounds;
        int row = bounds.yMin; // 一番下の行からスタート

        // 一番上の行までチェック
        while (row < bounds.yMax)
        {
            // 1. この行は満杯か？
            if (IsLineFull(row))
            {
                // 2. 満杯なら消す！
                LineClear(row);
                
                // 3. 消した後、上のブロックが落ちてくるので、
                // row（行番号）を増やさずに、もう一度同じ行をチェックさせる
            }
            else
            {
                // 満杯じゃなければ、次の行へ
                row++;
            }
        }
    }

    // 指定された行が「全部埋まってるか」調べる関数
    private bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;

        // 左端(xMin)から右端(xMax)まで全部見る
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            // もし1つでも「空っぽ」があったら false
            if (!this.tilemap.HasTile(position)) {
                return false;
            }
        }
        return true; // 全部埋まってた！
    }

    // 指定された行を消して、上をずらす関数
    private void LineClear(int row)
    {
        RectInt bounds = this.Bounds;

        // 1. 指定された行(row)のタイルを全部消す
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(position, null);
        }

        // 2. その行より「上」にある全ブロックを、1段ずつ下にコピーする
        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int sourcePos = new Vector3Int(col, row + 1, 0); // 上の段
                Vector3Int targetPos = new Vector3Int(col, row, 0);     // 下の段

                TileBase above = this.tilemap.GetTile(sourcePos); // 上の段のタイルを取得
                this.tilemap.SetTile(targetPos, above);           // 下の段に置く
            }
            row++;
        }
    }
    // ↓ 2. ここが今回の主役！「その場所に行ってもいい？」を判定する関数
    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            // ブロック1つ1つの予定地を計算
            Vector3Int tilePosition = piece.cells[i] + position;

            // A. 盤面の枠からはみ出していないか？
            if (!bounds.Contains((Vector2Int)tilePosition)) {
                return false; // ダメ！枠外です
            }

            // B. すでに他のブロック（タイル）がないか？
            if (this.tilemap.HasTile(tilePosition)) {
                return false; // ダメ！すでに埋まってます
            }
        }

        return true; // 全部OK！
    }
}