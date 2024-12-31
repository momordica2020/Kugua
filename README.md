# Kugua 苦音酱


一款基于 ~~Mirai~~ NTBot 框架的~~QQ~~多功能bot。

> 还是继续用.net开发，包装成了web api，以便作为组件使用 2024.11.6
>
> ~~暂停开发，开始向基于[Ariadne](https://github.com/GraiaProject/Ariadne)的新框架转移！移步：(https://github.com/momordica2020/PMiraiKugua) *2024.11.5*~~
> 
> *超绝重构中！ 2024.10.26*

![我苦](https://s3.bmp.ovh/imgs/2024/10/26/7c122c98a1627ffb.png)

------

## 依赖

- 本程序使用`.Net`开发，你的操作系统需要是`Windows 10+`并安装`.Net 8.0`运行环境。
- 本bot的运行依赖[Mirai机器人框架](https://github.com/mamoe/mirai)，启动我之前请先启动该框架（我用的是[mirai-console-loader](https://github.com/iTXTech/mirai-console-loader)），并按其要求配置端口等字段。目前采用mirai-api-http的websocket模式连接。
- 代码中与Mirai通讯的框架依赖于C#的库[MeowMiraiLib](https://github.com/DavidSciMeow/MeowMiraiLib)
- bot功能实现基于[OpenMomordica](https://github.com/hontsev/OpenMomordica) 并做了大幅改动。

## 使用方法

1. 在程序路径下的`config.json`文件中修改配置，将您准备挂载本bot的QQ账号信息写入，并设置呼唤词等。（我有空写个文档。？）
2. 启动 MiraiConsole 
3. 启动本bot的主程序`Kugua.exe`

## 技术支持

- QQ讨论群：`833246207`
- EMail: `287859992@qq.com`
