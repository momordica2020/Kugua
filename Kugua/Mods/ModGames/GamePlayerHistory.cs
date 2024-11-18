namespace Kugua
{
    /// <summary>
    /// 玩家战绩
    /// </summary>
    public class GamePlayerHistory
    {
        public long id = 0;
        public long money = 0;
        public long playnum = 0;
        public long winnum = 0;

        public long losenum
        {
            get { return playnum - winnum; }
        }

        public double winP
        {
            get
            {
                return (playnum <= 0 ? 0 : Math.Round(100.0 * winnum / playnum, 2));
            }
        }

        public override string ToString()
        {
            return $"{id}\t{money}\t{playnum}\t{winnum}";
        }

        public void Init(string line)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var items = line.Split('\t', StringSplitOptions.TrimEntries);
                    if (items.Length >= 4)
                    {
                        id = long.Parse(items[0]);
                        money = long.Parse(items[1]);
                        playnum = long.Parse(items[2]);
                        winnum = long.Parse(items[3]);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}
