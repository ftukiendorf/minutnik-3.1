using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Diagnostics;

namespace Minutnik;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;
    private readonly Stopwatch _stopwatch = new();
    private TimeSpan _remaining = TimeSpan.FromMinutes(25);
    private bool _isWorkPhase = true;
    private bool _isRunning;
    private bool _isPaused;
    private int _completedCycles;

    public MainWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _timer.Tick += OnTimerTick;

        WorkDurationInput.ValueChanged += (_, _) =>
        {
            if (!_isRunning && !_isPaused && _isWorkPhase)
            {
                _remaining = TimeSpan.FromMinutes(GetDurationMinutes(WorkDurationInput));
                UpdateDisplay();
            }
        };

        UpdateDisplay();
    }

    private void OnStartPauseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_isRunning)
        {
            _timer.Stop();
            _stopwatch.Stop();
            _isRunning = false;
            _isPaused = true;
            StartPauseButton.Content = "Wznów";
            StatusLabel.Text = "Wstrzymano";
            return;
        }

        if (!_isPaused && _isWorkPhase)
        {
            _remaining = TimeSpan.FromMinutes(GetDurationMinutes(WorkDurationInput));
        }

        _isRunning = true;
        _isPaused = false;
        _stopwatch.Restart();
        _timer.Start();
        StartPauseButton.Content = "Pauza";
        StatusLabel.Text = _isWorkPhase ? "Czas na pracę" : "Czas na przerwę";
        UpdateDisplay();
    }

    private void OnResetClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _timer.Stop();
        _stopwatch.Reset();
        _isRunning = false;
        _isPaused = false;
        _isWorkPhase = true;
        _completedCycles = 0;
        _remaining = TimeSpan.FromMinutes(GetDurationMinutes(WorkDurationInput));
        StartPauseButton.Content = "Start";
        StatusLabel.Text = "Gotowy do pracy";
        UpdateDisplay();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        var elapsed = _stopwatch.Elapsed;
        _stopwatch.Restart();

        while (_isRunning)
        {
            if (elapsed < _remaining)
            {
                _remaining -= elapsed;
                break;
            }

            elapsed -= _remaining;

            if (_isWorkPhase)
            {
                _isWorkPhase = false;
                _remaining = TimeSpan.FromMinutes(GetDurationMinutes(BreakDurationInput));
                StatusLabel.Text = "Praca skończona — zaczyna się przerwa";
            }
            else
            {
                _completedCycles++;
                _isWorkPhase = true;
                _remaining = TimeSpan.FromMinutes(GetDurationMinutes(WorkDurationInput));
                _timer.Stop();
                _stopwatch.Reset();
                _isRunning = false;
                _isPaused = false;
                StartPauseButton.Content = "Start";
                StatusLabel.Text = "Cykl ukończony — gotowy do pracy";
                break;
            }
        }

        UpdateDisplay();
    }

    private static int GetDurationMinutes(NumericUpDown input)
    {
        return Math.Max(1, (int)(input.Value ?? 1));
    }

    private void UpdateDisplay()
    {
        var totalSeconds = Math.Max(0, (int)Math.Ceiling(_remaining.TotalSeconds));
        TimeDisplay.Text = $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
        PhaseLabel.Text = _isWorkPhase ? "Praca" : "Przerwa";
        CycleCountLabel.Text = _completedCycles.ToString();
    }
}