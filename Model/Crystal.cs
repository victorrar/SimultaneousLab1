using System.Text;

namespace Model;

public abstract class Crystal
{
    private const int TickRateMs = 0;
    private const int SnapshotRateMs = 1000;
    private readonly double _particleMoveProbability;
    private readonly int _particles;

    private readonly List<Particle> _particlesList = new();
    private readonly Barrier _startBarrier;
    private readonly Barrier _stopBarrier;

    internal readonly int Cells;
    protected readonly int[] CellsArray;
    private bool _isRunning;

    protected Crystal(int cells, int particles, double particleMoveProbability)
    {
        Cells = cells;
        _particleMoveProbability = particleMoveProbability;
        _particles = particles;
        CellsArray = new int[cells];

        _startBarrier = new Barrier(particles + 1); // +1 for main thread
        _stopBarrier = new Barrier(particles + 1); // +1 for main thread
    }

    public void Start(TimeSpan stopTime)
    {
        if (_isRunning)
            throw new Exception("Simulation is already running");

        _isRunning = true;

        for (var i = 0; i < _particles; i++)
        {
            var particle = new Particle(_particleMoveProbability, TickRateMs, _startBarrier, MoveParticle, i, this,
                _stopBarrier);
            _particlesList.Add(particle);
            particle.Run();
        }

        //clear cells array
        for (var i = 0; i < CellsArray.Length; i++) CellsArray[i] = 0;

        CellsArray[0] = _particles;

        PrintSnapshot();

        _startBarrier.SignalAndWait();

        var beginTime = DateTime.Now;
        while (DateTime.Now - beginTime < stopTime)
        {
            PrintSnapshot();
            Thread.Sleep(SnapshotRateMs);
        }

        foreach (var particle in _particlesList) particle.Stop();

        _stopBarrier.SignalAndWait();

        Console.Out.WriteLine($"Simulation done in {stopTime.TotalSeconds} seconds");
        PrintSnapshot();
        PrintIntegrityCheck();
        PrintTicksPerformed();

        //clear cells array

        for (var i = 0; i < CellsArray.Length; i++) CellsArray[i] = 0;

        //delete particles

        _particlesList.Clear();

        _isRunning = false;
    }

    private void PrintSnapshot()
    {
        //pretty print values of array in single line with cell indices on the new row
        var sb = new StringBuilder();

        sb.Append("Snapshot:\n");

        for (var i = 0; i < CellsArray.Length; i++) sb.Append($"{i.ToString().PadLeft(4)} ");

        sb.Append('\n');
        for (var i = 0; i < CellsArray.Length; i++)
            sb.Append($"{((int)CellsArray.GetValue(i)!).ToString().PadLeft(4)} ");

        sb.Append('\n');

        Console.Out.WriteLine(sb.ToString());
    }

    private void PrintTicksPerformed()
    {
        long totalTicks = 0;
        foreach (var particle in _particlesList) totalTicks += particle.TickCount;

        //split ticks integer by 3 digits and print with commas

        var sb = new StringBuilder();
        sb.Append("Ticks performed: ");
        sb.Append(totalTicks.ToString("N0"));
        sb.Append('\n');

        Console.Out.WriteLine(sb.ToString());
    }

    protected abstract void MoveParticle(int fromIndex, int toIndex);

    private void PrintIntegrityCheck()
    {
        //sum of all cells values should be equal to number of particles

        var sum = CellsArray.Sum();

        if (sum == _particles)
            Console.Out.WriteLine($"Integrity check passed ( {sum} == {_particles} )");
        else
            Console.Out.WriteLine($"Integrity check failed ( {sum} != {_particles} )");
    }
}

public class CrystalInvalid : Crystal
{
    public CrystalInvalid(int cells, int particles, double particleMoveProbability) : base(cells, particles,
        particleMoveProbability)
    {
    }

    protected override void MoveParticle(int fromIndex, int toIndex)
    {
        CellsArray[fromIndex]--;
        CellsArray[toIndex]++;
    }
}

public class CrystalGlobalMutex : Crystal
{
    private readonly Mutex _mutex = new();

    public CrystalGlobalMutex(int cells, int particles, double particleMoveProbability) : base(cells, particles,
        particleMoveProbability)
    {
    }

    protected override void MoveParticle(int fromIndex, int toIndex)
    {
        _mutex.WaitOne();

        CellsArray[fromIndex]--;
        CellsArray[toIndex]++;

        _mutex.ReleaseMutex();
    }
}

public class CrystalLocalMutex : Crystal
{
    private readonly Mutex[] _mutexes;

    public CrystalLocalMutex(int cells, int particles, double particleMoveProbability) : base(cells, particles,
        particleMoveProbability)
    {
        _mutexes = new Mutex[cells];

        for (var i = 0; i < cells; i++) _mutexes[i] = new Mutex();
    }

    protected override void MoveParticle(int fromIndex, int toIndex)
    {
        var lowerIndex = Math.Min(fromIndex, toIndex);
        var higherIndex = Math.Max(fromIndex, toIndex);
        
        if(lowerIndex == higherIndex)
            return;

        _mutexes[higherIndex].WaitOne();
        _mutexes[lowerIndex].WaitOne();

        CellsArray[fromIndex]--;
        CellsArray[toIndex]++;

        _mutexes[lowerIndex].ReleaseMutex();
        _mutexes[higherIndex].ReleaseMutex();
    }
}