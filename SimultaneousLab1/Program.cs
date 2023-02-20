// See https://aka.ms/new-console-template for more information

using Model;

Console.WriteLine("Crystal Invalid");

const int particles = 100;
const int cells = 10;
const double particleMoveProbability = 0.5;

Crystal crystal = new CrystalInvalid(cells, particles, particleMoveProbability);
crystal.Start(TimeSpan.FromMilliseconds(5000));

Console.WriteLine("Crystal Global");

crystal = new CrystalGlobalMutex(cells, particles, particleMoveProbability);

crystal.Start(TimeSpan.FromMilliseconds(5000));

Console.WriteLine("Crystal Local");

crystal = new CrystalLocalMutex(cells, particles, particleMoveProbability);

crystal.Start(TimeSpan.FromMilliseconds(5000));

Console.WriteLine("Finished");