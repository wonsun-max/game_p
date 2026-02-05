namespace PrismPulse.Utils
{
    public enum GameMode { Classic, Obstacle, Blind }
    public enum GameState { Start, Playing, GameOver }

    public static class Constants
    {
        public const string GAME_NAME = "PRISM PULSE";
        public const int BOARD_SIZE = 8;
        public const float CELL_SIZE = 0.85f;
        public const float BOARD_VISUAL_SCALE = 0.82f; // Creates the 'lines' between blocks
        
        public const int SCORE_PER_CELL = 1;
        public const int SCORE_PER_LINE = 100;
        public const int SCORE_MULTI_LINE_BONUS = 150; 
        
        public const float ANIM_SPEED = 15f;
        public const float PIECE_START_SCALE = 0.5f;
        public const string PREF_HIGHSCORE_PREFIX = "PrismPulse_HS_";
    }
}
