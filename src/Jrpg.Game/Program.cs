using System.Diagnostics;
using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Game;
using Jrpg.Game.Flow;

ContentCatalog catalog;
try
{
    catalog = ContentCatalogLoader.Load(ContentRootLocator.Find(args, typeof(JrpgGame).Assembly));
}
catch (Exception exception) when (exception is ContentValidationException or IOException or UnauthorizedAccessException or ArgumentException)
{
    // No text can be drawn yet (slice 1c), so the reason goes where a developer sees it: the console and the debug output.
    var message = $"The game can't start: {exception.Message}";
    Console.Error.WriteLine(message);
    Debug.WriteLine(message);
    return 1;
}

using var game = new JrpgGame(new GameFlow(catalog));
game.Run();
return 0;
