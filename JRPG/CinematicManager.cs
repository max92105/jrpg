using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JRPG
{
    public static class CinematicManager
    {
        public static async Task PlayOpeningScene()
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            //await mainWindow.Fade(1, 0.5); // Fade to black
            //await Task.Delay(1000);

            //// Set player position
            //mainWindow.game.PlayerX = 5;
            //mainWindow.game.PlayerY = 10;

            //await mainWindow.Fade(0, 0.5); // Fade back in
            //await Task.Delay(1000);

            // Dialogue 1
            mainWindow.ShowDialogue(new List<string>()
            {
                "Long ago, our ancestors built this sacred tree.",
                "You must protect it now, young one."
            }, "Assets/figtherPortrait.png", "Fighter", null);

            await Task.Delay(500);

            // Move player (fake move by changing coordinates or playing a "walking" animation)
            for (int i = 0; i < 5; i++)
            {
                mainWindow.game.playerX += 1;
                await Task.Delay(300); // fake walking delay
            }

            // Final dialogue
            mainWindow.ShowDialogue(new List<string>()
            {
                "Beware",
            }, "Assets/figtherPortrait.png", "Fighter", null);

            //mainWindow.Fade(1, 0.5);
            //Task.Delay(1000);

            //// Teleport to another map maybe
            //mainWindow.game.currentMapFile = "forest.map";
            //mainWindow.game.LoadFromFile(mainWindow.game.currentMapFile);
            //mainWindow.game.PlayerX = 2;
            //mainWindow.game.PlayerY = 3;

            //await mainWindow.Fade(0, 0.5);
        }
    }
}
