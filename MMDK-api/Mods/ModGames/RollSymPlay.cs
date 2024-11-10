using ImageMagick;
using ImageMagick.Drawing;
using MMDK.Util;


namespace MMDK.Mods
{
    public class RollSymPlay
    {


        //static string[] emojis;// = "D:\\Projects\\momordica2020\\MIraiKUgua\\output\\Debug\\net8.0\\RunningData\\game\\emojis";


        public class SymbolRoller
        {
            int startx;
            int starty;
            int height;
            int width;
            List<string> syms = new List<string>();

            bool readyStop = false;

            public double nowP = 1;
            public List<int[]> choices=new List<int[]>();

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
                var emojilist = ModRaceHorse.Instance.emojis.Keys.ToArray();
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

                Drawables drawing = new Drawables();

                int xP = startx;

                for(int i = 0; i < 5; i++)
                {
                    int yP = (int)(starty + height * (-1 + i - (nowP % 1)));
                    string p = syms[(syms.Count + realPIndex - 1 + i) % syms.Count];

                    var pngImage1 = new MagickImage(new MemoryStream(ModRaceHorse.Instance.emojis[p]));
                    pngImage1.Resize((uint)width, (uint)height);
                    bkg.Composite(pngImage1, xP, yP, CompositeOperator.Over);

                }

                //int yP1 = (int)(starty + height * (-1 - (nowP % 1)));
                //int yP2 = (int)(starty + height * (0 - (nowP % 1)));
                //int yP3 = (int)(starty + height * (1 - (nowP % 1)));
                //int yP4 = (int)(starty + height * (2 - (nowP % 1)));
                //int yP5 = (int)(starty + height * (3 - (nowP % 1)));
                //string p1 = syms[(syms.Count + realPIndex - 1) % syms.Count];
                //string p2 = syms[realPIndex % syms.Count];
                //string p3 = syms[(realPIndex + 1) % syms.Count];
                //string p4 = syms[(realPIndex + 2) % syms.Count];
                //string p5 = syms[(realPIndex + 3) % syms.Count];

                //var pngImage = new MagickImage(p1);
                //pngImage.Resize((uint)width, (uint)height);
                //bkg.Composite(pngImage, xP, yP1, CompositeOperator.Over);
                //pngImage = new MagickImage(p2);
                //pngImage.Resize((uint)width, (uint)height);
                //bkg.Composite(pngImage, xP, yP2, CompositeOperator.Over);
                //pngImage = new MagickImage(p3);
                //pngImage.Resize((uint)width, (uint)height);
                //bkg.Composite(pngImage, xP, yP3, CompositeOperator.Over);
                //pngImage = new MagickImage(p4);
                //pngImage.Resize((uint)width, (uint)height);
                //bkg.Composite(pngImage, xP, yP4, CompositeOperator.Over);
                //pngImage = new MagickImage(p5);
                //pngImage.Resize((uint)width, (uint)height);
                //bkg.Composite(pngImage, xP, yP5, CompositeOperator.Over);


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


        // 生成 GIF 动画并返回图片流
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
            int fullTimt = 5;   // gif总共几秒
            var frameCount = 100;
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                var frame = new MagickImage(
                    MagickColor.FromRgb(255, (byte)(frameIndex * (255.0 / frameCount)), (byte)(frameIndex * (255.0 / frameCount))), 
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
