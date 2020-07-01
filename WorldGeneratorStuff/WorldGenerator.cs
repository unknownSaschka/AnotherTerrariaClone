using ITProject.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.WorldGeneratorStuff
{
    public class WorldGenerator
    {
        /*
             * Varianten um Overworld zu generieren:
             * -Mehrere Sinus/Cosinus Kurven mit teils random Werten übereinander legen
             * -Flache Welt mit Perlin Noise übereinander legen und mit Threshold durchlaufen
             */
        public enum GeneratorType { NoiseWorldV1, BicubicWorld, FractalWorldV1, FractalWorldV2, TurbulenceWorld }

        //private int[,] world;
        private int width, height;

        private GeneratorSettings settings;

        private int turbulenceHeightMin = 150, turbuleneHightMax = 220;
        private int perlinHeight = 510;
        private int steps = 20;

        private int Threshold = 140;
        private int minStoneHeight;
        private int maxStoneHeight;

        Random ran = new Random();

        public WorldGenerator(GeneratorSettings settings)
        {
            width = settings.WorldWidth;
            height = settings.WorldHeight;
            Threshold = settings.OverworldStoneThreshold;
            minStoneHeight = settings.MinStoneHeight;
            maxStoneHeight = settings.MaxStoneHeight;
            this.settings = settings;
            //minStoneHeight = (int) (height / 3f);
            //maxStoneHeight = (int) (height * (2f / 3f));
        }

        public void Update(double deltaTime)
        {

        }

        public ushort[,] NewWorld()
        {
            ushort[,] world = new ushort[width, height];

            if (settings.Overworld)
            {
                int[] heightLine = new int[width];
                NoiseWorldV1(world, heightLine);
                OverworldDirt(world, heightLine);
            }

            if (settings.Underground)
            {
                int[,] underground = GenerateUndergroundMap(width, height);

                if (!settings.Overworld)
                {
                    for (int iy = 0; iy < height; iy++)
                    {
                        for (int ix = 0; ix < width; ix++)
                        {
                            if (underground[ix, iy] == 1)
                            {

                                world[ix, iy] = 1;
                            }
                            else
                            {
                                world[ix, iy] = 0;
                            }
                        }
                    }
                }
                else
                {
                    for (int iy = 0; iy < height; iy++)
                    {
                        for (int ix = 0; ix < width; ix++)
                        {
                            if (world[ix, iy] == 1)    //Falls in der Overworld Stein an der Stelle ist, schau ob in der Underground World Höhle oder Stein ist
                            {
                                if (underground[ix, iy] == 0)
                                {
                                    world[ix, iy] = 0;
                                }
                            }
                        }
                    }
                }
            }

            return world;
        }

        private FastNoise SetFastNoiseSettings()
        {
            FastNoise fastNoise = new FastNoise();
            fastNoise.SetSeed(settings.Seed);
            fastNoise.SetNoiseType(settings.NoiseType);
            fastNoise.SetFractalType(settings.FractalType);
            fastNoise.SetInterp(settings.Interpolation);
            fastNoise.SetFractalGain(settings.FractalGain);
            fastNoise.SetFrequency(settings.Frequency);
            fastNoise.SetFractalOctaves(settings.FractalOctaves);
            fastNoise.SetFractalLacunarity(settings.FractalLacunarity);
            return fastNoise;
        }

        private FastNoise SetFastNoiseSettings(GeneratorSettings settings)
        {
            FastNoise fastNoise = new FastNoise();
            fastNoise.SetSeed(settings.Seed);
            fastNoise.SetNoiseType(settings.NoiseType);
            fastNoise.SetFractalType(settings.FractalType);
            fastNoise.SetInterp(settings.Interpolation);
            fastNoise.SetFractalGain(settings.FractalGain);
            fastNoise.SetFrequency(settings.Frequency);
            fastNoise.SetFractalOctaves(settings.FractalOctaves);
            fastNoise.SetFractalLacunarity(settings.FractalLacunarity);
            return fastNoise;
        }

        public void NoiseWorldV1(ushort[,] world, int[] heightLine)
        {
            FastNoise fastNoise = SetFastNoiseSettings();
            SmoothWorld(world, minStoneHeight, maxStoneHeight);

            for (int iy = 0; iy < minStoneHeight; iy++)
            {
                for (int ix = 0; ix < width; ix++)
                {
                    world[ix, iy] = 1;
                }
            }

            for (int ix = 0; ix < width; ix++)
            {
                int stoneHeight = 0;

                for (int iy = minStoneHeight; iy < height; iy++)
                {
                    float fractalValue = fastNoise.GetNoise(ix, iy);
                    int worldValue = (ushort)GameExtentions.Remap(fractalValue, -1f, 1f, 0f, 255f);

                    world[ix, iy] = (ushort)GameExtentions.Remap(world[ix, iy] + worldValue, 0f, 510f, 0f, 255f);


                    if (world[ix, iy] > Threshold)
                    {
                        world[ix, iy] = 1;
                        stoneHeight = iy;
                    }
                    else
                    {
                        world[ix, iy] = 0;
                    }
                }

                heightLine[ix] = stoneHeight;
            }
        }

        public void FractalWorldV1(ushort[,] world)
        {
            for (int ix = 0; ix < width; ix++)
            {
                int pHeight = (int)GameExtentions.Remap((float)Perlin.OctavePerlin(ix, perlinHeight, 1, 4, 0.1d, 0.05d, 1d), 0f, 2f, turbulenceHeightMin, turbuleneHightMax);
                for (int iy = 0; iy <= pHeight; iy++)
                {
                    world[ix, iy] = 1;
                }
            }
        }

        public void FractalWorldV2(ushort[,] world)
        {
            TurbulenceWorld(world);

            for (int iy = 0; iy < height; iy++)
            {
                for (int ix = 0; ix < width; ix++)
                {
                    ushort pointValue = (ushort)GameExtentions.Remap((float)Perlin.OctavePerlin(ix, perlinHeight, 1, 4, 0.1d, 0.05d, 1d), 0f, 2f, 0f, 255f);
                    if (world[ix, iy] == 255)
                    {
                        world[ix, iy] -= pointValue;
                    }
                    else
                    {
                        world[ix, iy] += pointValue;
                    }

                    if (world[ix, iy] > 100)
                    {
                        world[ix, iy] = 1;
                    }
                    else
                    {
                        world[ix, iy] = 0;
                    }
                }
            }
        }

        public void BicubicWorld(ushort[,] world)
        {
            List<int> randomHeights = new List<int>();
            int newY;
            for (int ix = 0; ix <= width; ix += steps)
            {
                randomHeights.Add(ran.Next(turbulenceHeightMin, turbuleneHightMax));
            }

            for (int ix = 0; ix < width; ix++)
            {
                if (ix % steps == 0)
                {
                    world[ix, randomHeights[ix / steps]] = 1;
                    FillDownwards(world, ix, randomHeights[ix / steps]);
                }
                else
                {
                    double percentageToNext = ((double)ix % (double)steps) / (double)steps;
                    double sx = (-2 * Math.Pow(percentageToNext, 3)) + (3 * Math.Pow(percentageToNext, 2));
                    newY = (int)Math.Round((1 - sx) * randomHeights[ix / steps] + sx * randomHeights[(ix + steps) / steps], MidpointRounding.ToEven);
                    world[ix, newY] = 1;
                    FillDownwards(world, ix, newY);
                }
            }
        }

        public void SmoothWorld(ushort[,] world, int stoneMin, int stoneMax)
        {
            for (int iy = 0; iy < height; iy++)
            {
                for (int ix = 0; ix < width; ix++)
                {
                    if (iy > stoneMax)
                    {
                        world[ix, iy] = 0;
                    }
                    else if (iy < stoneMin)
                    {
                        world[ix, iy] = 255;
                    }
                    else
                    {
                        world[ix, iy] = (ushort)GameExtentions.Remap(iy, stoneMax, stoneMin, 0f, 255f);
                    }
                }
            }
        }

        public void TurbulenceWorld(ushort[,] world)
        {
            FlatWorld(world);
            for (int iy = turbulenceHeightMin; iy < turbuleneHightMax; iy++)
            {
                for (int ix = 0; ix < width; ix++)
                {
                    if (ran.NextDouble() < GameExtentions.Remap(iy, turbulenceHeightMin, turbuleneHightMax, 0f, 1f))
                    {
                        world[ix, iy] = 0;
                    }
                    else
                    {
                        world[ix, iy] = 255;
                    }
                }
            }
        }

        public void FlatWorld(ushort[,] world)
        {
            for (int iy = 0; iy < height; iy++)
            {
                for (int ix = 0; ix < width; ix++)
                {
                    if (iy > 150)
                    {
                        world[ix, iy] = 0;
                    }
                    else
                    {
                        world[ix, iy] = 1;
                    }
                }
            }
        }

        public void FillDownwards(ushort[,] world, int x, int upToY)
        {
            for (int iy = 0; iy < upToY; iy++)
            {
                world[x, iy] = 1;
            }
        }

        private void NoiseWorld(ushort[,] world)
        {
            world = new ushort[width, height];
            FastNoise fastNoise = SetFastNoiseSettings();

            for (int iy = 0; iy < height; iy++)
            {
                for (int ix = 0; ix < width; ix++)
                {
                    world[ix, iy] = (ushort)GameExtentions.Remap(fastNoise.GetNoise(ix, iy), -1f, 1f, 0f, 255f);
                }
            }
        }

        //Muss evtl nochmal bearbeitet werden!!
        public int GetSquareTypeView(ushort[,] world, int x, int y)
        {
            if (x > width - 1 || y > height - 1 || x < 0 || y < 0)
            {
                return 1;
            }

            return world[x, y];
        }

        private int[,] GenerateUndergroundMap(int width, int height)
        {
            switch (settings.WorldUndergroundGeneratorType)
            {
                case GeneratorSettings.UndergroundGeneratorType.Cellular_Automata:
                    CellularAutomata ca = new CellularAutomata(settings);
                    return ca.Generate();
                case GeneratorSettings.UndergroundGeneratorType.Noise:
                    return GenerateNoiseUnderground(width, height);
                default:
                    return null;
            }
        }

        private int[,] GenerateNoiseUnderground(int width, int height)
        {
            FastNoise fs = new FastNoise(settings.Seed);
            fs.SetNoiseType(FastNoise.NoiseType.PerlinFractal);
            fs.SetFrequency(0.015f);
            fs.SetFractalOctaves(1);
            fs.SetFractalGain(0.5f);
            fs.SetFractalLacunarity(1);
            fs.SetFractalType(FastNoise.FractalType.Billow);
            fs.SetInterp(FastNoise.Interp.Hermite);

            int[,] underground = new int[width, height];
            int t = 0, f = 0;
            for (int iy = 0; iy < height; iy++)
            {
                for (int ix = 0; ix < width; ix++)
                {
                    int temp = (int)GameExtentions.Remap(fs.GetNoise(ix, iy), -1f, 1f, 255f, 0f);
                    if (temp > settings.UndergroundNoiseThreshold)
                    {
                        underground[ix, iy] = 0;
                        t++;
                    }
                    else
                    {
                        underground[ix, iy] = 1;
                        f++;
                    }
                }
            }
            return underground;
        }

        private void OverworldDirt(ushort[,] world, int[] heightLine)
        {
            for(int ix = 0; ix < width; ix++)
            {
                int minDirtHeight = heightLine[ix] - settings.DirtHeight;

                for(int iy = 0; iy < height; iy++)
                {
                    if (iy < minDirtHeight || iy > heightLine[ix]) continue;
                    if(iy >= minDirtHeight && iy < heightLine[ix])
                    {
                        world[ix, iy] = 2;
                    }
                    else if(iy == heightLine[ix])
                    {
                        world[ix, iy] = 3;
                    }
                }
            }
        }

        private void CreateBiomes()
        {

        }
    }
}
