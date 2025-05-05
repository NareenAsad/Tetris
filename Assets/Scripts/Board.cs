using UnityEngine;
using TMPro;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }
    public TetrominoData[] tetrominoes;
    public Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public UIManager uiManager;
    public bool isGameOver = false;
    public int score = 0;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public int totalLinesCleared = 0;
    public int level = 1;
    public int linesPerLevel = 3;

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }
    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();
        for (int i = 0; i < this.tetrominoes.Length; i++)
        {
            this.tetrominoes[i].Initialize();
        }
    }
    private void Start()
    {
        SpawnPiece();
        levelText.text = "Level: " + level;
    }
    public void SpawnPiece()
    {
        int random = Random.Range(0, this.tetrominoes.Length);
        TetrominoData data = this.tetrominoes[random];
        this.activePiece.Initialize(this, this.spawnPosition, data);

        if (IsValidPosition(activePiece, spawnPosition))
        {
            Set(activePiece);
        }
        else
        {
            GameOver();
        }
    }
    public void GameOver()
    {
        isGameOver = true;
        tilemap.ClearAllTiles();
        uiManager.ShowGameOver(score);

        GameObject bgMusic = GameObject.Find("BackgroundMusic");
        if (bgMusic != null)
        {
            AudioSource audio = bgMusic.GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.Stop();
            }
        }
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }
    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }
    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }
            if (this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }
        return true;
    }

    public void AddScore(int lines)
    {
        int points = 0;
        switch (lines)
        {
            case 1: points = 10; break;
            case 2: points = 20; break;
            case 3: points = 30; break;
            case 4: points = 40; break;
        }
        score += points;
        scoreText.text = "Score: " + score;

        // Add line tracking and level-up logic
        totalLinesCleared += lines;
        int newLevel = (totalLinesCleared / linesPerLevel) + 1;

        if (newLevel > level)
        {
            level = newLevel;
            UpdateLevel();
        }
    }

    public void UpdateLevel()
    {
        levelText.text = "Level: " + level;

        // Increase delay more significantly (e.g., +0.3s per level), capped at 3 seconds
        activePiece.stepDelay = Mathf.Min(2.0f, 1.0f + (level - 1) * 0.2f);
    }

    public void ClearLines()
    {
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;
        int linesCleared = 0;

        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
                linesCleared++;
            }
            else
            {
                row++;
            }
        }
        if (linesCleared > 0)
        {
            AddScore(linesCleared);
            totalLinesCleared += linesCleared;
            IncreaseDifficulty();
        }
    }

    private void IncreaseDifficulty()
    {
        // Reduce delay every 10 lines cleared, to a minimum of 0.1f
        float newDelay = Mathf.Min(2.0f, 1.0f - (totalLinesCleared / 5 ) * 0.2f);
        activePiece.stepDelay = newDelay;

        int level = totalLinesCleared / 5 + 1;
        if (levelText != null)
            levelText.text = "Level: " + level;
    }

    public bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            if (!this.tilemap.HasTile(position))
            {
                return false;
            }
        }
        return true;
    }

    public void LineClear(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(position, null);
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(position, above);
            }
            row++;
        }
    }

}
