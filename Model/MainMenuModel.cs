using ITProject.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ITProject.Logic.GameExtentions;
using static ITProject.Model.World;

namespace ITProject.Model
{
    public class MainMenuModel
    {
        public enum Screen { MainMenuStart, WorldSelect, PlayerSelect, NewPlayer, NewWorld }
        public Screen ScreenState;

        public WorldSaveInfo[] AvailableWorldSaves;
        public PlayerSaveInfo[] AvailablePlayerSaves;

        public List<ViewButtonPositions> ButtonPositions;

        public MainMenuModel()
        {
            ScreenState = Screen.MainMenuStart;
            Init();
        }

        private void Init()
        {
            SaveManagement.LoadPlayerWorldJSON(out AvailablePlayerSaves, out AvailableWorldSaves);
        }

        public void LeftClick(ViewButtonPositions button)
        {
            
        }
    }
}
