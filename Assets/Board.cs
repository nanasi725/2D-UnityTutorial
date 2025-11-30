using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap;
    public Piece activePiece;   
    public TetrominoData[] tetrominoes;

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
        // ランダムな形を選ぶ
        int random = Random.Range(0, this.tetrominoes.Length);
        TetrominoData data = this.tetrominoes[random];

        // ピースを初期化（画面の上の方に出現させる）
        this.activePiece.Initialize(this, new Vector3Int(-1, 8, 0), data);
        
        // 画面に描画する
        Set(this.activePiece);
    }

    // タイルマップにブロックを描き込む関数
    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            // ブロックの現在地 + 形のオフセット = 塗る場所
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            // nullをセットすると、その場所のタイルが消えます
            this.tilemap.SetTile(tilePosition, null);
        }
    }
}
