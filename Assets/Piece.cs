using UnityEngine;
using UnityEngine.Tilemaps;

public class Piece : MonoBehaviour
{
    public Board board;
    public TetrominoData data;
    public Vector3Int position;
    public Vector3Int[] cells;
    public int rotationIndex = 0; // 0〜3の4段階で回転状態を管理
    public float stepDelay = 1.0f;  // 1秒ごとに落ちる
    public float lockDelay = 0.5f;  // 床に着いてから0.5秒の猶予（後で使います）

    private float stepTime; // 次に落ちる時刻を記憶する変数
    private float lockTime; // 固定される時刻を記憶する変数

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

    // ↓ ここから下を追加！
    
    // 毎フレーム（1秒に約60回）呼ばれる監視役
    private void Update()
    {
        this.board.Clear(this);
        
        // --- ここから下を追加・変更 ---

        // A. 時間による自動落下処理
        // 「固定時間（lockTime）」がセットされていない場合だけ実行
        this.lockTime += Time.deltaTime;

        // 「今の時間」が「次に落ちる予定の時間」を過ぎたら
        if (Time.time >= this.stepTime)
        {
            Step(); // 1段落とす関数（あとで作ります）
        }

        // B. キー操作（ここは今まで通りですが、少し機能を追加します）
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            Move(new Vector2Int(-1, 0));
        } else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            Move(new Vector2Int(1, 0));
        } else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            Step(); // 下キーを押したときも「Step」を呼ぶように変更！
        } else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
            Rotate(1);
        }

        this.board.Set(this);
    }

    // --- 新しい関数 Step() ---
    // 1段落とす処理を関数として独立させました
    private void Step()
    {
        // 次の予定時間をセット（今の時間 + 1秒後）
        this.stepTime = Time.time + this.stepDelay;

        // 下に動かしてみる
        Move(new Vector2Int(0, -1));

        // ※ここで「もし下に動けなかったら？」という処理は次回やります
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

    private void Rotate(int direction)
    {
        // I, O, T... などの各ブロックの座標を取り出して、計算用の変数に入れる
        int originalRotation = this.rotationIndex; // 今の向きを保存（失敗したら戻すため）
        this.rotationIndex = Wrap(this.rotationIndex + direction, 0, 4);

        // 回転の計算（行列計算）を適用する
        ApplyRotationMatrix(direction);

        // 壁チェック！「回転したら壁に埋まる？」
        if (!this.board.IsValidPosition(this, this.position))
        {
            // ダメなら、回転をなかったことにする（元に戻す）
            this.rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    // 行列計算の実体（難しいのでコピペでOK！）
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
                    // I型とO型は中心がずれているため、少し特殊な計算が必要
                    // （今は一旦、普通の回転だけ実装して、後で修正します）
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

    // 0〜3の範囲をループさせる便利関数（右回転し続けたら一周して戻るように）
    private int Wrap(int input, int min, int max)
    {
        if (input < min) {
            return max - (min - input) % (max - min);
        } else {
            return min + (input - min) % (max - min);
        }
    }
}
