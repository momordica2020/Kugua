namespace MMDKMonitor
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.tbMmdk = new System.Windows.Forms.TextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.清空日志ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button1 = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lbUseNum = new System.Windows.Forms.Label();
            this.lbGroupNum = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.lbFriendNum = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.lbQQ = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lbTimeSpan = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lbBeginTime = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lbPort = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lbVersion = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lbState = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.pbCPU = new System.Windows.Forms.ProgressBar();
            this.lbCPU = new System.Windows.Forms.Label();
            this.lbMem = new System.Windows.Forms.Label();
            this.pbMem = new System.Windows.Forms.ProgressBar();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tbMirai = new System.Windows.Forms.TextBox();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.清空日志ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbMmdk
            // 
            this.tbMmdk.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbMmdk.ContextMenuStrip = this.contextMenuStrip1;
            this.tbMmdk.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbMmdk.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbMmdk.Location = new System.Drawing.Point(3, 3);
            this.tbMmdk.MaxLength = 32767000;
            this.tbMmdk.Multiline = true;
            this.tbMmdk.Name = "tbMmdk";
            this.tbMmdk.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbMmdk.Size = new System.Drawing.Size(579, 479);
            this.tbMmdk.TabIndex = 0;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.清空日志ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(125, 26);
            // 
            // 清空日志ToolStripMenuItem
            // 
            this.清空日志ToolStripMenuItem.Name = "清空日志ToolStripMenuItem";
            this.清空日志ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.清空日志ToolStripMenuItem.Text = "清空日志";
            this.清空日志ToolStripMenuItem.Click += new System.EventHandler(this.清空日志ToolStripMenuItem_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Red;
            this.button1.Font = new System.Drawing.Font("宋体", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button1.ForeColor = System.Drawing.Color.Yellow;
            this.button1.Location = new System.Drawing.Point(3, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(109, 32);
            this.button1.TabIndex = 1;
            this.button1.Text = "波特启动";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer1.Panel1.Controls.Add(this.button1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(801, 511);
            this.splitContainer1.SplitterDistance = 205;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Inset;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 39.58333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60.41667F));
            this.tableLayoutPanel1.Controls.Add(this.lbUseNum, 0, 11);
            this.tableLayoutPanel1.Controls.Add(this.lbGroupNum, 0, 10);
            this.tableLayoutPanel1.Controls.Add(this.label15, 0, 10);
            this.tableLayoutPanel1.Controls.Add(this.lbFriendNum, 0, 9);
            this.tableLayoutPanel1.Controls.Add(this.label13, 0, 9);
            this.tableLayoutPanel1.Controls.Add(this.label12, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.lbQQ, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.label10, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.lbTimeSpan, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.lbBeginTime, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.lbPort, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lbVersion, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lbState, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pbCPU, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.lbCPU, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.lbMem, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.pbMem, 1, 6);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 41);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 12;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(204, 426);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // lbUseNum
            // 
            this.lbUseNum.AutoSize = true;
            this.lbUseNum.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbUseNum.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbUseNum.Location = new System.Drawing.Point(85, 373);
            this.lbUseNum.Name = "lbUseNum";
            this.lbUseNum.Size = new System.Drawing.Size(114, 51);
            this.lbUseNum.TabIndex = 23;
            this.lbUseNum.Text = "0";
            this.lbUseNum.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbGroupNum
            // 
            this.lbGroupNum.AutoSize = true;
            this.lbGroupNum.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbGroupNum.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbGroupNum.Location = new System.Drawing.Point(85, 338);
            this.lbGroupNum.Name = "lbGroupNum";
            this.lbGroupNum.Size = new System.Drawing.Size(114, 33);
            this.lbGroupNum.TabIndex = 22;
            this.lbGroupNum.Text = "0";
            this.lbGroupNum.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label15.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label15.Location = new System.Drawing.Point(5, 373);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(72, 51);
            this.label15.TabIndex = 21;
            this.label15.Text = "调用次数";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbFriendNum
            // 
            this.lbFriendNum.AutoSize = true;
            this.lbFriendNum.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbFriendNum.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbFriendNum.Location = new System.Drawing.Point(85, 306);
            this.lbFriendNum.Name = "lbFriendNum";
            this.lbFriendNum.Size = new System.Drawing.Size(114, 30);
            this.lbFriendNum.TabIndex = 20;
            this.lbFriendNum.Text = "0";
            this.lbFriendNum.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label13.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label13.Location = new System.Drawing.Point(5, 338);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(72, 33);
            this.label13.TabIndex = 19;
            this.label13.Text = "群数";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label12.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label12.Location = new System.Drawing.Point(5, 275);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(72, 29);
            this.label12.TabIndex = 18;
            this.label12.Text = "QQ";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbQQ
            // 
            this.lbQQ.AutoSize = true;
            this.lbQQ.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbQQ.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbQQ.Location = new System.Drawing.Point(85, 275);
            this.lbQQ.Name = "lbQQ";
            this.lbQQ.Size = new System.Drawing.Size(114, 29);
            this.lbQQ.TabIndex = 17;
            this.lbQQ.Text = "0";
            this.lbQQ.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label10.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label10.Location = new System.Drawing.Point(5, 306);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(72, 30);
            this.label10.TabIndex = 16;
            this.label10.Text = "好友数";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbTimeSpan
            // 
            this.lbTimeSpan.AutoSize = true;
            this.lbTimeSpan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbTimeSpan.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbTimeSpan.Location = new System.Drawing.Point(85, 135);
            this.lbTimeSpan.Name = "lbTimeSpan";
            this.lbTimeSpan.Size = new System.Drawing.Size(114, 42);
            this.lbTimeSpan.TabIndex = 15;
            this.lbTimeSpan.Text = "222天\r\n10小时33分22秒";
            this.lbTimeSpan.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label8.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.Location = new System.Drawing.Point(5, 135);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(72, 42);
            this.label8.TabIndex = 14;
            this.label8.Text = "运行时长";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbBeginTime
            // 
            this.lbBeginTime.AutoSize = true;
            this.lbBeginTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbBeginTime.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbBeginTime.Location = new System.Drawing.Point(85, 83);
            this.lbBeginTime.Name = "lbBeginTime";
            this.lbBeginTime.Size = new System.Drawing.Size(114, 50);
            this.lbBeginTime.TabIndex = 13;
            this.lbBeginTime.Text = "2020-11-22 11:22:33";
            this.lbBeginTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(5, 83);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 50);
            this.label6.TabIndex = 12;
            this.label6.Text = "启动时间";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbPort
            // 
            this.lbPort.AutoSize = true;
            this.lbPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbPort.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbPort.Location = new System.Drawing.Point(85, 54);
            this.lbPort.Name = "lbPort";
            this.lbPort.Size = new System.Drawing.Size(114, 27);
            this.lbPort.TabIndex = 11;
            this.lbPort.Text = "9999";
            this.lbPort.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(5, 54);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 27);
            this.label4.TabIndex = 4;
            this.label4.Text = "本地端口";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbVersion
            // 
            this.lbVersion.AutoSize = true;
            this.lbVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbVersion.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbVersion.Location = new System.Drawing.Point(85, 31);
            this.lbVersion.Name = "lbVersion";
            this.lbVersion.Size = new System.Drawing.Size(114, 21);
            this.lbVersion.TabIndex = 10;
            this.lbVersion.Text = "v 0.1.0";
            this.lbVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(5, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 21);
            this.label2.TabIndex = 3;
            this.label2.Text = "版本";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbState
            // 
            this.lbState.AutoSize = true;
            this.lbState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbState.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbState.Location = new System.Drawing.Point(85, 2);
            this.lbState.Name = "lbState";
            this.lbState.Size = new System.Drawing.Size(114, 27);
            this.lbState.TabIndex = 3;
            this.lbState.Text = "初始";
            this.lbState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(5, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 27);
            this.label1.TabIndex = 0;
            this.label1.Text = "运行状态";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pbCPU
            // 
            this.pbCPU.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbCPU.Location = new System.Drawing.Point(85, 182);
            this.pbCPU.Name = "pbCPU";
            this.pbCPU.Size = new System.Drawing.Size(114, 36);
            this.pbCPU.TabIndex = 4;
            // 
            // lbCPU
            // 
            this.lbCPU.AutoSize = true;
            this.lbCPU.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbCPU.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbCPU.Location = new System.Drawing.Point(5, 179);
            this.lbCPU.Name = "lbCPU";
            this.lbCPU.Size = new System.Drawing.Size(72, 42);
            this.lbCPU.TabIndex = 5;
            this.lbCPU.Text = "CPU\r\n(100%)";
            this.lbCPU.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMem
            // 
            this.lbMem.AutoSize = true;
            this.lbMem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbMem.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbMem.Location = new System.Drawing.Point(5, 223);
            this.lbMem.Name = "lbMem";
            this.lbMem.Size = new System.Drawing.Size(72, 40);
            this.lbMem.TabIndex = 8;
            this.lbMem.Text = "内存\r\n(0%)";
            this.lbMem.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pbMem
            // 
            this.pbMem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbMem.Location = new System.Drawing.Point(85, 226);
            this.pbMem.Name = "pbMem";
            this.pbMem.Size = new System.Drawing.Size(114, 34);
            this.pbMem.TabIndex = 9;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(593, 511);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tbMmdk);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(585, 485);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "苦瓜日志";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tbMirai);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(585, 485);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Mirai日志";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tbMirai
            // 
            this.tbMirai.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbMirai.ContextMenuStrip = this.contextMenuStrip2;
            this.tbMirai.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbMirai.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbMirai.Location = new System.Drawing.Point(3, 3);
            this.tbMirai.MaxLength = 32767000;
            this.tbMirai.Multiline = true;
            this.tbMirai.Name = "tbMirai";
            this.tbMirai.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbMirai.Size = new System.Drawing.Size(579, 479);
            this.tbMirai.TabIndex = 1;
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.清空日志ToolStripMenuItem1});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(125, 26);
            // 
            // 清空日志ToolStripMenuItem1
            // 
            this.清空日志ToolStripMenuItem1.Name = "清空日志ToolStripMenuItem1";
            this.清空日志ToolStripMenuItem1.Size = new System.Drawing.Size(124, 22);
            this.清空日志ToolStripMenuItem1.Text = "清空日志";
            this.清空日志ToolStripMenuItem1.Click += new System.EventHandler(this.清空日志ToolStripMenuItem1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(801, 511);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(601, 500);
            this.Name = "Form1";
            this.Text = "MIraiKUgua";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.contextMenuStrip2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox tbMmdk;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label lbState;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar pbCPU;
        private System.Windows.Forms.Label lbCPU;
        private System.Windows.Forms.Label lbMem;
        private System.Windows.Forms.ProgressBar pbMem;
        private System.Windows.Forms.TextBox tbMirai;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem 清空日志ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 清空日志ToolStripMenuItem1;
        private System.Windows.Forms.Label lbUseNum;
        private System.Windows.Forms.Label lbGroupNum;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label lbFriendNum;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label lbQQ;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label lbTimeSpan;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lbBeginTime;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lbPort;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lbVersion;
        private System.Windows.Forms.Label label2;
    }
}

