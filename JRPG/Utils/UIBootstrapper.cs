using JRPG.Core.Utils;
using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows;
using System.IO;
using JRPG.UI.GameMenu;
using System.Windows.Controls;
using JRPG.Core.Systems;
using JRPG.Managers;

namespace JRPG.Utils
{
    internal class UIBootstrapper
    {
        public static void Initialize(MainWindow window)
        {
            window.WindowStyle = WindowStyle.None;
            window.WindowState = WindowState.Maximized;
            window.ResizeMode = ResizeMode.NoResize;

            window.MenuBackground.Source = ImageLoader.LoadBackground("StartMenu.png");

            // Decode the battle background now so the first battle transition doesn't stall
            ImageLoader.LoadBackground("Forest.png");

            if (window.GameMenuControl == null)
            {
                window.GameMenuControl = new GameMenuControl();
                Panel.SetZIndex(window.GameMenuControl, 900);
                window.RootGrid.Children.Add(window.GameMenuControl);
            }

            window.GameMenuControl.OnLoadSlotConfirmed = async (slotIndex) =>
            {
                var save = await GameSystems.System.LoadFromSlotAsync(slotIndex);
                if (save == null)
                {
                    MessageBox.Show("Failed to load save file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await SceneManager.LoadGameAsync(save);
            };

            // Hook return to menu event
            window.GameMenuControl.ReturnToMainMenuRequested += window.ReturnToMainMenuAsync;

            // Optional: if needed to hide at startup
            window.GameMenuControl.Visibility = Visibility.Collapsed;

            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                var preload = new SaveLoadView(
                    SaveLoadView.SaveLoadMode.Load,
                    _ => Task.CompletedTask,
                    () => { }
                );
                preload.Measure(new Size(1, 1));
                preload.Arrange(new Rect(0, 0, 1, 1));
            }, DispatcherPriority.Loaded);
        }
    }
}