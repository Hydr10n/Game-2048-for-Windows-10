using System.ComponentModel;
using Windows.UI.Xaml;

namespace Game_2048
{
    class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
            get => bestScore;
            set
            {
                bestScore = value;
                OnPropertyChanged(nameof(BestScore));
            }
        }

        public virtual GameState GameState { get; set; } = GameState.NotStarted;
    }

    sealed class ViewModelEx : ViewModel
    {
        private bool isGamepadActive;
        public bool IsGamepadActive
        {
            get => isGamepadActive;
            set
            {
                isGamepadActive = value;
                OnPropertyChanged(nameof(IsGamepadActive));
            }
        }

        public bool IsVictoryWhenGameOver { get; private set; }
        public double GameStateTextOpacity { get; private set; }
        public Visibility GameStateTextVisibility { get; private set; } = Visibility.Collapsed;
        public override GameState GameState
        {
            get => base.GameState;
            set
            {
                base.GameState = value;
                switch (value)
                {
                    case GameState.Won:
                        IsVictoryWhenGameOver = true;
                        GameStateTextVisibility = Visibility.Visible;
                        GameStateTextOpacity = 0.9;
                        break;
                    case GameState.Over:
                        IsVictoryWhenGameOver = false;
                        GameStateTextVisibility = Visibility.Visible;
                        GameStateTextOpacity = 0.9;
                        break;
                    default:
                        GameStateTextVisibility = Visibility.Collapsed;
                        GameStateTextOpacity = 0;
                        break;
                }
                OnPropertyChanged(nameof(IsVictoryWhenGameOver));
                OnPropertyChanged(nameof(GameStateTextVisibility));
                OnPropertyChanged(nameof(GameStateTextOpacity));
            }
        }
    }
}