

namespace AIEnv
{
    using AIEnv.Clients.StableDiffusion;
    using AIEnv.Clients;

    public class Program
    {
        public static ManualResetEvent quitEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            Console.CancelKeyPress += (sender, eArgs) =>
            {
                quitEvent.Set();
                eArgs.Cancel = true;
            };
            VEnv.Instance.Activate(StableDiffusionClient.Instance);

            quitEvent.WaitOne();
        }
    }
}