using Kugua;




BotHost.Instance.Start();



while (true)
{
    Thread.Sleep(10000);
}


System.Diagnostics.Debug.WriteLine("-- BOT EXIT --");
BotHost.Instance.Stop();