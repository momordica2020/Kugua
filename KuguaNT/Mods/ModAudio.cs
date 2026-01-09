using Kugua.Core;
using Kugua.Integrations.AI;
using Kugua.Integrations.NTBot;
using Kugua.Mods.Base;
using System.Text.RegularExpressions;

namespace Kugua.Mods
{
    /// <summary>
    /// 音频/音乐处理
    /// </summary>
    public class ModAudio : Mod
    {
        MusicDownloader musicDownloader;


        public override bool Init(string[] args)
        {
            ModCommands.Add(new ModCommand(new Regex(@"^点歌[∶|:|：|\s](.+)"), getMusic));
            ModCommands.Add(new ModCommand(new Regex(@"^反向点歌[∶|:|：|\s](.+)"), getMusicReverse));
            ModCommands.Add(new ModCommand(new Regex(@"^说[∶|:|：](.+)", RegexOptions.Singleline), say));
            ModCommands.Add(new ModCommand(new Regex(@"^反着说[∶|:|：](.+)", RegexOptions.Singleline), sayReverse));
            //ModCommands.Add(new ModCommand(null, dealRecord,_needAsk:false, _useAudio:true));


            musicDownloader = new MusicDownloader();


            return true;
        }



        /// <summary>
        /// 点歌（爱来自QQ音乐），搜到多首歌曲会唱第一首
        /// 点歌 初音未来的消失
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string getMusic(MessageContext context, string[] param)
        {
            try
            {
                string mname = param[1].Trim();
                string localPath = "";// Config.Instance.ResourceFullPath($"music/{mname}.mp3");
                string infoDesc = "";
                string url = "";
                int index = 1;
                if (musicDownloader == null) return null;
                var mlist = musicDownloader.GetMusicList(mname);
                if (mlist != null)
                {
                    foreach (var vm in mlist)
                    {
                        if (vm.CanDownload)
                        {
                            if (string.IsNullOrWhiteSpace(localPath))
                            {
                                localPath = Config.Instance.FullPath($"music/{vm.Name}_{vm.Singer}.mp3");
                                if (System.IO.File.Exists(localPath)) System.IO.File.Delete(localPath);
                                url = musicDownloader.GetMusicDownloadURL(vm.DownloadInfo, enmMusicSource.QQ);
                                Network.Download(url, localPath);


                                infoDesc += $"[{index++}]-{vm.Name} {vm.Singer} ({vm.Class})\n";
                            }
                            else
                            {
                                infoDesc += $" {index++} -{vm.Name} {vm.Singer} ({vm.Class})\n";
                            }

                        }

                    }
                }

                if (!string.IsNullOrWhiteSpace(infoDesc))
                {
                    context.SendBackText(infoDesc, true);
                }
                if (!string.IsNullOrWhiteSpace(localPath) && System.IO.File.Exists(localPath))
                {
                    context.SendBack([
                        new Record($"file://{localPath}")
                            ]);
                    //var amrb64 = StaticUtil.Mp32AmrBase64(localPath);
                    //if (!string.IsNullOrWhiteSpace(amrb64))
                    //{
                    //    context.SendBack(new Message[] {
                    //            new Voice(null, null, null, amrb64)
                    //        });
                    //}
                }

                Task.Delay(2000).ContinueWith(t =>
                {
                    try
                    {
                        File.Delete(localPath);
                    }
                    catch { }
                });

                return null;

            }
            catch (Exception ex)
            {
                Logger.Log(ex);

            }
            return $"不给点";







            //var music = Directory.GetFiles(@"D:\Projects\musicapi\music", $"{mname}.mp3");
            //if (music.Length > 0)
            //{
            //    var amrb64 = StaticUtil.Mp32AmrBase64(music.First());
            //    if (!string.IsNullOrWhiteSpace(amrb64))
            //    {
            //        context.SendBack(new Message[] {
            //            new Voice(null, null, null, amrb64)
            //        });
            //    }

            //    return null;
            //}
            //return $"曲库没有{mname}";
            //if(!string.IsNullOrWhiteSpace(mname))
        }


        /// <summary>
        /// 反向点歌（爱来自QQ音乐），搜到多首歌曲会唱第一首
        /// 反向点歌 初音未来的消失
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string getMusicReverse(MessageContext context, string[] param)
        {
            try
            {
                string mname = param[1].Trim();
                string localPath = "";// Config.Instance.ResourceFullPath($"music/{mname}.mp3");
                string infoDesc = "";
                string url = "";
                int index = 1;
                if (musicDownloader == null) return null;
                var mlist = musicDownloader.GetMusicList(mname);
                if (mlist != null)
                {
                    foreach (var vm in mlist)
                    {
                        if (vm.CanDownload)
                        {
                            if (string.IsNullOrWhiteSpace(localPath))
                            {
                                localPath = Config.Instance.FullPath($"music/{vm.Name}_{vm.Singer}.mp3");
                                if (System.IO.File.Exists(localPath)) System.IO.File.Delete(localPath);
                                url = musicDownloader.GetMusicDownloadURL(vm.DownloadInfo, enmMusicSource.QQ);
                                Network.Download(url, localPath);


                                infoDesc += $"[{index++}]-{vm.Name} {vm.Singer} ({vm.Class})\n";
                            }
                            else
                            {
                                infoDesc += $" {index++} -{vm.Name} {vm.Singer} ({vm.Class})\n";
                            }

                        }

                    }
                }

                if (!string.IsNullOrWhiteSpace(infoDesc))
                {
                    context.SendBackText(infoDesc, true);
                }
                if (!string.IsNullOrWhiteSpace(localPath) && System.IO.File.Exists(localPath))
                {
                    string localPath2 = AudioUtil.WavReverse(localPath);
                    context.SendBack([
                        new Record($"file://{localPath2}")
                            ]);
                    //var amrb64 = StaticUtil.Mp32AmrBase64(localPath);
                    //if (!string.IsNullOrWhiteSpace(amrb64))
                    //{
                    //    context.SendBack(new Message[] {
                    //            new Voice(null, null, null, amrb64)
                    //        });
                    //}
                    Task.Delay(2000).ContinueWith(t =>
                    {
                        try
                        {
                            File.Delete(localPath);
                            File.Delete(localPath2);
                        }
                        catch { }
                    });
                }



                return null;

            }
            catch (Exception ex)
            {
                Logger.Log(ex);

            }
            return $"不给点";







            //var music = Directory.GetFiles(@"D:\Projects\musicapi\music", $"{mname}.mp3");
            //if (music.Length > 0)
            //{
            //    var amrb64 = StaticUtil.Mp32AmrBase64(music.First());
            //    if (!string.IsNullOrWhiteSpace(amrb64))
            //    {
            //        context.SendBack(new Message[] {
            //            new Voice(null, null, null, amrb64)
            //        });
            //    }

            //    return null;
            //}
            //return $"曲库没有{mname}";
            //if(!string.IsNullOrWhiteSpace(mname))
        }


        ///// <summary>
        ///// 语音识别
        ///// 
        ///// </summary>
        ///// <param name="context"></param>
        ///// <param name="param"></param>
        ///// <returns></returns>
        //private string dealRecord(MessageContext context, string[] param)
        //{
        //    if (!context.IsAdminUser) return "";
        //    var r = context.Audios;
        //    string content = "";
        //    if (r.Count > 0)
        //    {
        //        Thread.Sleep(1000);
        //        string file = AudioUtil.SilkV32Mp3(r.First().path, false);
        //        content = $"{file}";
        //        if (!string.IsNullOrWhiteSpace(content))
        //        {
        //            content = LLM.HSRecognizeAudio(file);
        //        }

        //    }


        //    return content;
        //}



        /// <summary>
        /// 在线棒读
        /// 说：你好
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string say(MessageContext context, string[] param)
        {
            string speakSentence = param[1];
            if (string.IsNullOrWhiteSpace(speakSentence)) return "";

            LLM.Instance.Speech(context, $"{speakSentence}");


            return null;
        }

        /// <summary>
        /// 在线反向音频
        /// 反着说：你好
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string sayReverse(MessageContext context, string[] param)
        {
            string speakSentence = param[1];
            if (string.IsNullOrWhiteSpace(speakSentence)) return "";

            LLM.Instance.Speech(context, $"{speakSentence}", true);


            return null;
        }
    }
}
