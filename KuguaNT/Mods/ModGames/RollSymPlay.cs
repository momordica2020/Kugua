using ImageMagick;
using ImageMagick.Drawing;


namespace Kugua.Mods
{
    /// <summary>
    /// 老虎机运行逻辑
    /// </summary>
    public class RollSymPlay
    {


        //static string[] emojis;// = "D:\\Projects\\momordica2020\\MIraiKUgua\\output\\Debug\\net8.0\\RunningData\\game\\emojis";

        /// <summary>
        /// 单个滚筒
        /// </summary>
        public class SymbolRoller
        {
            int startx;
            int starty;
            int height;
            int width;
            List<string> syms = new List<string>();

            bool readyStop = false;

            public double nowP = 1;
            public List<int[]> choices = new List<int[]>();

            double speed = 0;
            double speedA = 0;

            public SymbolRoller(int startx, int starty, int height, int width)
            {
                this.startx = startx;
                this.starty = starty;
                this.height = height;
                this.width = width;
            }

            public void Start(double _speed = 4)
            {
                choices = new List<int[]>();
                readyStop = false;
                syms.Clear();
                var emojilist = ModSlotMachine.Instance.emojis.Keys.ToArray();
                StaticUtil.FisherYates(emojilist);
                for (int i = 0; i < emojilist.Length; i++)
                {
                    syms.Add(emojilist[i]);
                }

                nowP = 1;
                speed = _speed;
                speedA = -0.7;
            }

            public void handleFrame(MagickImage bkg)
            {
                nowP = (nowP + speed) % syms.Count;
                int realPIndex = (int)(nowP);



                // 绘制图像
             if (bkg != null)
            {
                    Drawables drawing = new Drawables();

                    int xP = startx;

                    for (int i = 0; i < 5; i++)
                    {
                        int yP = (int)(starty + height * (-1 + i - (nowP % 1)));
                        string p = syms[(syms.Count + realPIndex - 1 + i) % syms.Count];

                        var pngImage1 = new MagickImage(new MemoryStream(ModSlotMachine.Instance.emojis[p]));
                        pngImage1.Resize((uint)width, (uint)height);
                        bkg.Composite(pngImage1, xP, yP, CompositeOperator.Over);

                    }
                }


                if (!readyStop)
                {
                    speed = speed + speedA;
                    if (speed <= 1)
                    {
                        //speed = 1;
                        readyStop = true;
                    }
                }
                else
                {
                    speed = Math.Max(0.1, speed * 0.8);
                    if (speed <= 0.1 && (Math.Abs(nowP - Math.Round(nowP)) < 0.1))
                    {
                        // stop;
                        nowP = Math.Round(nowP);
                        speed = 0;
                        speedA = 0;

                        if(choices.Count <= 0)
                        {
                            var rnowP = (int)((syms.Count + nowP) % syms.Count);
                            for (int i = 0; i < 3; i++)
                            {
                                var tmp = Path.GetFileNameWithoutExtension(syms[(rnowP - 1 + i) % syms.Count]).Split('-', StringSplitOptions.TrimEntries);
                                int[] rr = new int[tmp.Length];
                                for (int j = 0; j < rr.Length; j++) rr[j] = int.Parse(tmp[j]);
                                choices.Add(rr);
                            }
                        }

                    }
                }

            }

        }

        /// <summary>
        /// 只生成emoji不生成gif
        /// 
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public static string GenerateEmoji(out List<int[]> results)
        {
            List<SymbolRoller> symbols = new List<SymbolRoller>();
            string[] emojis = ["⭐️","🏆️","💎","🍖","😅","☘️","🍎","🍌"];
            int number = 3;
            for (int i = 0; i < number; i++)
            {
                SymbolRoller sb = new SymbolRoller(0, 0, 0, 0);
                symbols.Add(sb);
                sb.Start(11 + i * 2);
            }

            // 模拟抽奖过程并添加每一帧
            int fullTimt = 4;   // 总共几秒
            var frameCount = 80;
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                foreach(var symbol in symbols)
                {
                    symbol.handleFrame(null);
                }
            }
            var resultEmojis = "";
            List<int[]> res = new List<int[]>();
            for (int i = 0; i < number; i++)
            {
                foreach (var cc in symbols[i].choices)
                {
                    res.Add(cc);
                }
                
            }
            results = res;

            resultEmojis += emojis[res[1][0] - 1];
            resultEmojis += emojis[res[4][0] - 1];
            resultEmojis += emojis[res[7][0] - 1];
            resultEmojis += "\n"; 
            return resultEmojis;
        }


        /// <summary>
        /// 生成 GIF 动画并返回图片流
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public static string GenerateGif(out List<int[]> results)
        {
            
            // 创建一个 MagickImageCollection 用于存储每一帧
            var images = new MagickImageCollection();

            int sw = 64;
            int sh = 64;
            int number = 3;
            int bkw = sw * number;
            int bkh = sh * 2;

            List<SymbolRoller> symbols = new List<SymbolRoller>();

            for (int i = 0; i < number; i++)
            {
                SymbolRoller sb = new SymbolRoller(sw * i, (int)(bkh/2 - sh / 2), sh, sw);
                symbols.Add(sb);
                sb.Start(11 + i * 2);
            }

            // 模拟抽奖过程并添加每一帧
            int fullTimt = 4;   // gif总共几秒
            var frameCount = 80;
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                var frame = new MagickImage(
                    MagickColor.FromRgb(255, 255, 255), 
                    (uint)bkw, 
                    (uint)bkh
                ); // 设置图像尺寸
                frame.AnimationDelay = (uint)(fullTimt * 100 / frameCount);
                for (int i = 0; i < number; i++)
                {
                    symbols[i].handleFrame(frame);
                }

                images.Add(frame);
            }

            // 将图像集合导出为 GIF 格式的字节数组
            byte[] gifBytes;
            using (var memoryStream = new System.IO.MemoryStream())
            {
                images.Write(memoryStream, MagickFormat.Gif);
                gifBytes = memoryStream.ToArray();
            }
            //File.WriteAllBytes(@"D:\Projects\momordica2020\MIraiKUgua\output\Debug\net8.0\RunningData\game\out.gif", gifBytes);
            List<int[]> res = new List<int[]>();
            for(int i = 0; i < number; i++)
            {
                foreach(var cc in symbols[i].choices)
                {
                    res.Add(cc);
                }
            }
            results = res;
            return Convert.ToBase64String(gifBytes);
        }


    }

}
