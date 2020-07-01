using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.WorldGeneratorStuff
{
    class CellularAutomata
    {
        private int width, height;
        private int[,] field;
        //private int[,] viewField;
        //private World world;
        private Random ran;

        //CA parameters
        private float rockCellsPercentage;    //percentage of rock cells
        private int n;      //number of neighbourhood cells
        private int M;      //CA generations
        private int T;      //neighbour threshold that defines a rock

        private float rockCellsPercentageAbove;     //percentage right under Overworld
        private int overworldThreshold;             //height where overworld is

        public CellularAutomata(GeneratorSettings settings)
        {
            width = settings.WorldWidth;
            height = settings.WorldHeight;
            field = new int[width, height];

            rockCellsPercentage = settings.RockPercentage;
            n = settings.NeighbourCells;
            M = settings.Generations;
            T = settings.UndergroundStoneThreshold;
            rockCellsPercentageAbove = settings.rockCellsPercentageAbove;
            overworldThreshold = settings.MinStoneHeight;
            ran = new Random(settings.Seed);
            InitField();
        }
    

        public int[,] Generate()
        {
            for (int iM = 0; iM < M; iM++)
            {
                int[,] newField = (int[,])field.Clone();
                for (int iy = 0; iy < height; iy++)
                {
                    for (int ix = 0; ix < width; ix++)
                    {
                        int neighbourRocks = 0;
                        for (int iny = -n; iny <= n; iny++)
                        {
                            for (int inx = -n; inx <= n; inx++)
                            {
                                if (inx == 0 && iny == 0) continue;
                                if (GetPoint(ix + inx, iy + iny) == 1) neighbourRocks++;
                            }
                        }

                        if (neighbourRocks >= T)
                        {
                            newField[ix, iy] = 1;
                        }
                        else
                        {
                            newField[ix, iy] = 0;
                        }
                    }
                }
                field = newField;
            }
            return field;
        }

        private int GetPoint(int x, int y)
        {
            if (x > width - 1 || y > height - 1 || x < 0 || y < 0)
            {
                return 1;
            }

            return field[x, y];
        }

        /// <summary>
        /// Initialisiert das Feld mit einer gegebenen Wahrscheinlichekit zufällig mit Stein
        /// </summary>
        public void InitField()
        {
            for (int iy = 0; iy < height; iy++)
            {
                for (int ix = 0; ix < width; ix++)
                {
                    if (iy > overworldThreshold)
                    {
                        if (ran.NextDouble() > rockCellsPercentageAbove)
                        {
                            field[ix, iy] = 0;
                        }
                        else
                        {
                            field[ix, iy] = 1;
                        }
                    }
                    else
                    {
                        if (ran.NextDouble() > rockCellsPercentage)
                        {
                            field[ix, iy] = 0;
                        }
                        else
                        {
                            field[ix, iy] = 1;
                        }
                    }
                }
            }
        }
    }
}
