using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ITProject.WorldGeneratorStuff.WorldGenerator;

namespace ITProject.WorldGeneratorStuff
{
    public class GeneratorSettings
    {
        public enum ViewType { World, Noise }
        public enum UndergroundGeneratorType { Cellular_Automata, Noise }
        public ViewType ShowViewType;
        public GeneratorType WorldGeneratorType;
        public UndergroundGeneratorType WorldUndergroundGeneratorType;

        //World Settings
        public int WorldWidth;
        public int WorldHeight;

        public int DirtHeight;

        //Overworld Generator Settings
        public bool Overworld;
        public int MinStoneHeight;
        public int MaxStoneHeight;
        public int OverworldStoneThreshold;

        public int Seed;
        public float Frequency;
        public float NoiseOffset;

        public int OffsetX;
        public int OffsetY;

        public int FractalOctaves;
        public float FractalGain;
        public float FractalLacunarity;

        public FastNoise.NoiseType NoiseType;
        public FastNoise.FractalType FractalType;
        public FastNoise.Interp Interpolation;

        //Underground generator Settings (CA)
        public bool Underground;
        public float RockPercentage;
        public int NeighbourCells;
        public int Generations;
        public int UndergroundStoneThreshold;
        public float rockCellsPercentageAbove;

        //Underground Generator Settings (Noise)
        public int UndergroundNoiseThreshold;
    }
}
