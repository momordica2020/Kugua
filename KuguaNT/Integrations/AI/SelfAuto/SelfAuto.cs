namespace Kugua
{
    /// <summary>
    ///  全局自动运动模块
    /// </summary>
    public class SelfAuto
    {
        System.Timers.Timer MainTimer;
        public double TimerInterval = 1000 * 1;    // 1s








        public SelfAuto()
        {

        }


        public void Start()
        {
            try
            {
                if (MainTimer != null)
                {

                    MainTimer.Stop();
                }
                MainTimer = new(TimerInterval);
                MainTimer.AutoReset = true;
                MainTimer.Start();
                MainTimer.Elapsed += MainLoop;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }


        private void MainLoop(object sender, System.Timers.ElapsedEventArgs e)
        {

        }

    }
}
