namespace Model;

internal class Particle
{
    private readonly Crystal _crystal;

    //void function "move"
    private readonly Action<int, int> _move;
    private readonly double _moveProbability;

    private readonly Random _random = new();

    private readonly Barrier _startBarrier;
    private readonly Barrier _stopBarrier;
    private readonly Thread _thread;
    private readonly int _tickRateMcs;
    private int _arrayIndex;
    private int _particleIndex;
    private bool _shutDown;

    internal long TickCount;

    public Particle(double moveProbability, int tickRateMcs, Barrier startBarrier,
        Action<int, int> move, int particleIndex, Crystal crystal, Barrier stopBarrier)
    {
        _moveProbability = moveProbability;
        _tickRateMcs = tickRateMcs;
        _startBarrier = startBarrier;
        _move = move;
        _particleIndex = particleIndex;
        _crystal = crystal;
        _stopBarrier = stopBarrier;
        _thread = new Thread(Loop);
    }

    public void Run()
    {
        _thread.Start();
    }

    public void Stop()
    {
        _shutDown = true;
    }

    private void Loop()
    {
        // Console.Out.WriteLine($"Particle #{_particleIndex} thread started");
        _startBarrier.SignalAndWait();

        while (true)
        {
            Tick();
            Thread.Sleep(TimeSpan.FromMicroseconds(_tickRateMcs));
            Thread.Yield();
            if (_shutDown)
                break;
        }

        _stopBarrier.SignalAndWait();
    }

    private void Tick()
    {
        TickCount++;

        if (_random.NextDouble() > _moveProbability)
            return;

        var direction = _random.Next() % 2 == 0 ? -1 : 1;
        var targetCell = _arrayIndex + direction;

        if (targetCell < 0)
            targetCell = 0;

        if (targetCell >= _crystal.Cells)
            targetCell = _crystal.Cells - 1;

        _move(_arrayIndex, targetCell);
        _arrayIndex = targetCell;
    }
}