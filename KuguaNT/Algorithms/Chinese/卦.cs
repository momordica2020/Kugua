namespace Kugua.Algorithms.Chinese
{
    public class 蓍草卜筮
    {
        readonly int 大衍之数 = 50;
        class 蓍草
        {}

        class 草束
        {
            List<蓍草> 草;
            public 草束(List<蓍草> 草)
            {
                this.草 = 草;
            }
            public 草束()
            {
                草 = new List<蓍草>();

            }
            public 草束(int 数量)
            {
                草 = new List<蓍草>();
                while (数量-- > 0) 草.Add(new 蓍草());
            }

            public int 数量
            {
                get
                {
                    return 草 == null ? 0 : 草.Count;
                }
            }

            public (草束 左, 草束 右) 分而为二
            {
                get
                {
                    var 划分处 = MyRandom.Next(草.Count);
                    var 左 = new 草束(草.GetRange(0, 划分处));
                    var 右 = new 草束(草.GetRange(划分处, 草.Count - 划分处));
                    草.Clear();
                    return (左, 右);
                }

            }

            public void 归入(草束 去向, int 数量 = -1)
            {
                if (数量 > 草.Count || 数量 <= 0)
                {
                    数量 = 草.Count;
                }
                去向.草.AddRange(草.GetRange(0, 数量));
                草.RemoveRange(0, 数量);
            }

            //public void 挂出(int 数量)
            //{
            //    归入(挂出蓍草, 数量);
            //}

        }

        草束 全部蓍草;
        草束 挂出蓍草;
        草束 揲出蓍草;

        public 蓍草卜筮()
        {
            // 初始化
            全部蓍草 = new 草束(大衍之数);
            挂出蓍草 = new 草束();
            揲出蓍草 = new 草束();
        }

        public List<int> 卜卦
        {
            get
            {
                return [卜爻, 卜爻, 卜爻, 卜爻, 卜爻, 卜爻];
            }
        }

        int 卜爻
        {
            get
            {
                // 大衍之数五十，其用四十有九
                全部蓍草.归入(挂出蓍草, 数量:1);

                // 三易
                全部蓍草 = 四营;
                全部蓍草 = 四营;
                全部蓍草 = 四营;

                // 查数画爻
                int 数 = 全部蓍草.数量 / 4;
                // 回收
                挂出蓍草.归入(全部蓍草);

                return 数;
            }
          
        }

        草束 四营
        {
            get {
                (var 左, var 右) = 全部蓍草.分而为二;
                //挂一
                右.归入(挂出蓍草, 数量:1);
                //揲之以四
                while (左.数量 > 4) 左.归入(揲出蓍草, 数量: 4);
                while (右.数量 > 4) 右.归入(揲出蓍草, 数量: 4);
                //归奇于扐
                左.归入(挂出蓍草);
                右.归入(挂出蓍草);
                //挂
                揲出蓍草.归入(全部蓍草);
                return 全部蓍草;
             }
        }

        
    }
}
