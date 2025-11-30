using UnityEngine;
using UnityEngine.Tilemaps;

public class Piece : MonoBehaviour
{
    public Board board;
    public TetrominoData data;
    public Vector3Int position;
    public Vector3Int[] cells;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        
        if (this.cells == null) {
            this.cells = new Vector3Int[data.cells.Length];
        } else if (this.cells.Length != data.cells.Length) {
            this.cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < data.cells.Length; i++) {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }

    // ↓ ここから下を追加！
    
    // 毎フレーム（1秒に約60回）呼ばれる監視役
    private void Update()
    {
        // 1. 古い場所を消す
        this.board.Clear(this);

        // 2. キー入力に応じて座標を変える
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            Move(new Vector2Int(-1, 0)); // 左へ
        } else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            Move(new Vector2Int(1, 0));  // 右へ
        } else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            Move(new Vector2Int(0, -1)); // 下へ
        }

        // 3. 新しい場所に描く
        this.board.Set(this);
    }

    // 座標を足し算するだけの関数
// 変更前は「無条件で移動」していましたが...
    // ↓
    private void Move(Vector2Int translation)
    {
        // 1. まず「移動後の未来の座標」を計算してみる
        Vector3Int newPosition = this.position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        // 2. Boardに「そこ行っていい？」と聞く
        bool valid = this.board.IsValidPosition(this, newPosition);

        // 3. OKと言われたら、初めて実際に座標を更新する
        if (valid)
        {
            this.position = newPosition;
        }
    }
}