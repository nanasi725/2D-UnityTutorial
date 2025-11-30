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

        this.activePiece.Initialize(this, new Vector3Int(-1, 8, 0), data);
        
        // ↓ スポーンした瞬間も一応チェックして、ダメならGAMEOVER（今はセットするだけ）
        if (IsValidPosition(this.activePiece, new Vector3Int(-1, 8, 0))) {
            Set(this.activePiece);
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