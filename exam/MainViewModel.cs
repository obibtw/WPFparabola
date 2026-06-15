// Файл: MainViewModel.cs
using exam;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ParabolaAnimation
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private double _x;
        private double _y;
        private int _launchCount;
        private bool _isAnimating;
        private const double CanvasWidth = 400;
        private const double CanvasHeight = 300;
        private const double AnimationDurationMs = 2000;
        private const int FrameIntervalMs = 20;

        public MainViewModel()
        {
            _dbService = new DatabaseService();
            StartCommand = new RelayCommand(async _ => await StartAnimation(), _ => !IsAnimating);
            RefreshCountCommand = new RelayCommand(async _ => await RefreshCount());

            // Начальная позиция: левый нижний угол
            X = 0;
            Y = CanvasHeight; // предполагаем, что Ellipse имеет Height/Width, нужно учесть смещение
        }

        public double X
        {
            get => _x;
            set { _x = value; OnPropertyChanged(); }
        }

        public double Y
        {
            get => _y;
            set { _y = value; OnPropertyChanged(); }
        }

        public int LaunchCount
        {
            get => _launchCount;
            set { _launchCount = value; OnPropertyChanged(); }
        }

        public bool IsAnimating
        {
            get => _isAnimating;
            set
            {
                _isAnimating = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand StartCommand { get; }
        public ICommand RefreshCountCommand { get; }

        public async Task InitializeAsync()
        {
            await _dbService.InitializeAsync();
            await RefreshCount(); // показать текущее количество
        }

        private async Task StartAnimation()
        {
            if (IsAnimating) return;
            IsAnimating = true;

            // Сохраняем запуск в БД
            await _dbService.SaveLaunchAsync(DateTime.Now);

            // Параметры параболы: y = 4 * H * x * (W - x) / W^2, но нам нужен Y от 0 до H
            // Вершина в центре сверху
            double totalSteps = AnimationDurationMs / FrameIntervalMs;
            double stepX = CanvasWidth / totalSteps;

            for (int i = 0; i <= totalSteps; i++)
            {
                double currentX = i * stepX;
                double parabolaY = 4 * CanvasHeight * currentX * (CanvasWidth - currentX) / (CanvasWidth * CanvasWidth);
                // Инвертируем Y, так как 0 — верх Canvas
                double canvasY = CanvasHeight - parabolaY;

                X = currentX;
                Y = canvasY - 15; // небольшое смещение, чтобы центр эллипса был на кривой (Ellipse Width/Height=30)
                await Task.Delay(FrameIntervalMs);
            }

            IsAnimating = false;
            // Обновим счетчик после завершения
            await RefreshCount();
        }

        private async Task RefreshCount()
        {
            LaunchCount = await _dbService.GetLaunchCountLastHourAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}