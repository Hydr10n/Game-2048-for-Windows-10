using System.ComponentModel;
using Windows.UI.Xaml;

namespace Game_2048
{
    class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool layoutReady;
        public bool LayoutReady
        {
            get => layoutReady;
            set
            {
                layoutReady = value;
                OnPropertyChanged(nameof(LayoutReady));
            }
        }

        private int score;
        public int Score
        {
            get => score;
            set
            {
                score = value;
                OnPropertyChanged(nameof(Score));
            }
        }

        private int bestScore;
        public int BestScore
        {
            get => bestScore; set
            {
                bestScore = value;
                OnPropertyChanged(nameof(BestScore));
            }
        }

        public double GameStateTextOpacity { get; set; }
        public string GameStateText { get; set; }
        public Visibility GameStateTextVisibility { get; set; } = Visibility.Collapsed;
        private GameState gameState = GameState.NotStarted;
        public GameState GameState
        {
            get => gameState;
            set
            {
                gameState = value;
                switch (gameState)
                {
                    case GameState.Won:
                        GameStateText = "YOU WIN!";
                        GameStateTextVisibility = Visibility.Visible;
                        GameStateTextOpacity = 0.9;
                        break;
                    case GameState.Over:
                        GameStateText = "GAME OVER!";
                        GameStateTextVisibility = Visibility.Visible;
                        GameStateTextOpacity = 0.9;
                        break;
                    default:
                        GameStateText = null;
                        GameStateTextVisibility = Visibility.Collapsed;
                        GameStateTextOpacity = 0;
                        break;
                }
                OnPropertyChanged(nameof(GameStateText));
                OnPropertyChanged(nameof(GameStateTextVisibility));
                OnPropertyChanged(nameof(GameStateTextOpacity));
            }
        }
    }
}