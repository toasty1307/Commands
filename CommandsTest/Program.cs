using System.Threading.Tasks;

namespace CommandsTest
{
    internal class Program
    {
        public static async Task Main()
        {
            var bot = new CommandsTestBot();
            await bot.Run();
            await Task.Delay(-1);
        }
    }
}