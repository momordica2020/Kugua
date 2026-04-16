namespace Kugua.Mods.ModTransShits
{
    public class ShitTarget
    {
        public ShitTransGroupInfo groupinfo;
        public DateTime lastPublishDate = DateTime.Now;
        public List<Shit> shits = new List<Shit>();
        public double spanM {
            get {
                return (DateTime.Now - lastPublishDate).TotalMinutes;
            } 
        }

        public ShitTarget(ShitTransGroupInfo info)
        {
            groupinfo = info;
            lastPublishDate = DateTime.Now;
            shits = new List<Shit>();
        }


        /// <summary>
        /// 从shit中取出尚未发表的里面AI得分最高的一个
        /// </summary>
        /// <param name="exists"></param>
        /// <returns></returns>
        private Shit getMaxScoreUnPublicAIShit(List<Shit> exists)
        {
            long nowBestScore = 0;
            Shit nowBestShit = null;
            for (int i = 0; i < shits.Count; i++)
            {
                var s = shits[i];
                if (exists.Contains(s)
                    //|| s.published
                    //|| s.publishedAI
                    || s.score <= 3
                //|| DateTime.Now - s.createTime < new TimeSpan(0, config.deal_score_span_min, 0) 
                ) continue;

                if (s.score > nowBestScore)
                {
                    nowBestScore = s.score;
                    nowBestShit = s;
                }
            }
            return nowBestShit;
        }


        public List<Shit> getBestAIShits()
        {
            // find bests
            List<Shit> bestShits = new List<Shit>();
            int bestNumMax = 1;
            for (int i = 0; i < bestNumMax; i++)
            {
                var best = getMaxScoreUnPublicAIShit(bestShits);
                if (best == null) break;
                bestShits.Add(best);
            }
            bestShits.Sort((x, y) => (x.createTime > y.createTime ? 1 : -1));
            return bestShits;
        }

        public void addShit(Shit shit)
        {
            if (shit == null) return;
            shits.Add(shit);
        }
    }
}
