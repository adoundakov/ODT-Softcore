   using SPTarkov.DI.Annotations;
   using SPTarkov.Server.Core.DI;
   using SPTarkov.Server.Core.Models.Utils;

   namespace Softcore;

   [Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
   public class Plugin(ISptLogger<Plugin> logger) : IOnLoad
   {
       public Task OnLoad()
       {
           logger.Success("[Softcore] Mod loaded successfully!");
           return Task.CompletedTask;
       }
   }