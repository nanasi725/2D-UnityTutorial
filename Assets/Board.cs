using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap;                 // 背景のグリッド
    public TetrominoData[] tetrominoes;     // 全7種類のブロックデータ

    private void Awake()
    {
        // データの初期化（辞書から座標データを読み込む）
        for (int i = 0; i < this.tetrominoes.Length; i++)
        {
            this.tetrominoes[i].Initialize();
        }
    }
}