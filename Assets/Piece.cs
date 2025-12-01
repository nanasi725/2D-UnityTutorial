using UnityEngine;
using UnityEngine.Tilemaps;

public class Piece : MonoBehaviour
{
    public Board board;             // 盤面
    public TetrominoData data;      // ブロックの形データ
    public Vector3Int position;     // 今の座標
    public Vector3Int[] cells;      // 4つのブロックの相対位置
    public int rotationIndex = 0;   // 今の回転の向き(0~3)

    public float stepDelay = 1.0f;  // 自動落下のスピード(1秒)
    public float lockDelay = 0.5f;  // (今回はまだ使いませんが、将来のために残しておきます)

    private float stepTime;         // 次に落ちる予定時刻
    private float lockTime;         // (今回はまだ使いませんが、将来のために残しておきます)

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;
        this.stepTime = Time.time + this.stepDelay;
        this.lockTime = 0f;

        if (this.cells == null) {
            this.cells = new Vector3Int[data.cells.Length];
        } else if (this.cells.Length != data.cells.Length) {
            this.cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < data.cells.Length; i++) {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update()
    {
        // 1. 移動前の場所を一旦消す
        this.board.Clear(this);

        // 2. 時間を管理して自動で落とす
        this.lockTime += Time.deltaTime;

        if (Time.time >= this.stepTime)
        {
            HandleStep();
        }

        // 3. キー入力を受け付ける
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            Move(new Vector2Int(-1, 0));
        } else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            Move(new Vector2Int(1, 0));
        } else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            HandleStep(); // 下キーのときは「1段落とす」処理をする
        } else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
            Rotate(1);
        }

        // 4. 新しい場所に描く
        this.board.Set(this);
    }

    // 1段落とす処理（自動落下＆下キー共通）
    private void HandleStep()
    {
        this.stepTime = Time.time + this.stepDelay;

        // 下（Vector3Int.down）に行けるかチェックして移動
        Move(Vector2Int.down);

        // もし移動できていなかったら？（＝Moveしても座標が変わってない＝床に着いた）
        // ※Move関数の中では「移動できないなら座標を変えない」ようになっているため、
        // 「もし下が埋まっていたら」を確認します。
        if (!this.board.IsValidPosition(this, this.position + Vector3Int.down))
        {
            Lock();
        }
    }

    // 床に着いたときの処理
    private void Lock()
    {
        this.board.Set(this);
        this.board.ClearLines();
        this.board.SpawnPiece();
    }

    // 移動処理（チェック付き）
    private void Move(Vector2Int translation)
    {
        Vector3Int newPosition = this.position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        // Boardに「そこ行っていい？」と聞いて、OKなら移動
        if (this.board.IsValidPosition(this, newPosition))
        {
            this.position = newPosition;
        }
    }

    // 回転処理
    private void Rotate(int direction)
    {
        int originalRotation = this.rotationIndex;
        this.rotationIndex = Wrap(this.rotationIndex + direction, 0, 4);

        ApplyRotationMatrix(direction);

        // 回転した結果、壁にめり込んだら元に戻す（Kickは未実装）
        if (!this.board.IsValidPosition(this, this.position))
        {
            this.rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    // 行列計算（ややこしいので触らなくてOK）
    private void ApplyRotationMatrix(int direction)
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3 cell = this.cells[i];
            int x, y;

            switch (this.data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
            }

            this.cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min) {
            return max - (min - input) % (max - min);
        } else {
            return min + (input - min) % (max - min);
        }
    }
}