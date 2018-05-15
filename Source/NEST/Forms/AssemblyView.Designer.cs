namespace NEST
{
    partial class AssemblyView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssemblyView));
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.stepButton = new System.Windows.Forms.Button();
            this.runButton = new System.Windows.Forms.Button();
            this.pauseButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.interruptGroupBox = new System.Windows.Forms.GroupBox();
            this.flagsGroupBox = new System.Windows.Forms.GroupBox();
            this.carryFlagLabel = new System.Windows.Forms.Label();
            this.cFlagValue = new System.Windows.Forms.Label();
            this.zFlagValue = new System.Windows.Forms.Label();
            this.hFlagValue = new System.Windows.Forms.Label();
            this.otherGroupBox = new System.Windows.Forms.GroupBox();
            this.pcValue = new System.Windows.Forms.Label();
            this.programCounterLabel = new System.Windows.Forms.Label();
            this.registersGroupBox = new System.Windows.Forms.GroupBox();
            this.accumulatorValueLabel = new System.Windows.Forms.Label();
            this.accumulatorLabel = new System.Windows.Forms.Label();
            this.stackGroupBox = new System.Windows.Forms.GroupBox();
            this.stackValue = new System.Windows.Forms.Label();
            this.spValue = new System.Windows.Forms.Label();
            this.spValuesLabel = new System.Windows.Forms.Label();
            this.spLabel = new System.Windows.Forms.Label();
            this.refreshButton = new System.Windows.Forms.Button();
            this.resetButton = new System.Windows.Forms.Button();
            this.xValueLabel = new System.Windows.Forms.Label();
            this.xRegisterLabel = new System.Windows.Forms.Label();
            this.yValueLabel = new System.Windows.Forms.Label();
            this.yRegisterLabel = new System.Windows.Forms.Label();
            this.statusRegisterValueLabel = new System.Windows.Forms.Label();
            this.statusRegisterLabel = new System.Windows.Forms.Label();
            this.zeroFlagLabel = new System.Windows.Forms.Label();
            this.intDisableFlagLabel = new System.Windows.Forms.Label();
            this.decimalFlagLabel = new System.Windows.Forms.Label();
            this.negativeFlagLabel = new System.Windows.Forms.Label();
            this.overflowFlagLabel = new System.Windows.Forms.Label();
            this.emptyFlagLabel = new System.Windows.Forms.Label();
            this.breakFlagLabel = new System.Windows.Forms.Label();
            this.iFlagValue = new System.Windows.Forms.Label();
            this.dFlagValue = new System.Windows.Forms.Label();
            this.bFlagValue = new System.Windows.Forms.Label();
            this.vFlagValue = new System.Windows.Forms.Label();
            this.sFlagValue = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.flagsGroupBox.SuspendLayout();
            this.otherGroupBox.SuspendLayout();
            this.registersGroupBox.SuspendLayout();
            this.stackGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.BackColor = System.Drawing.Color.Gray;
            this.listBox1.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 18;
            this.listBox1.Location = new System.Drawing.Point(272, 12);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(357, 292);
            this.listBox1.TabIndex = 2;
            this.listBox1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBox1_DrawItem);
            this.listBox1.DoubleClick += new System.EventHandler(this.listBox1_DoubleClick);
            // 
            // stepButton
            // 
            this.stepButton.Location = new System.Drawing.Point(272, 310);
            this.stepButton.Name = "stepButton";
            this.stepButton.Size = new System.Drawing.Size(69, 44);
            this.stepButton.TabIndex = 4;
            this.stepButton.Text = "Step";
            this.stepButton.UseVisualStyleBackColor = true;
            this.stepButton.Click += new System.EventHandler(this.stepButton_Click);
            // 
            // runButton
            // 
            this.runButton.Location = new System.Drawing.Point(419, 310);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(65, 44);
            this.runButton.TabIndex = 5;
            this.runButton.Text = "Run";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // pauseButton
            // 
            this.pauseButton.Location = new System.Drawing.Point(490, 310);
            this.pauseButton.Name = "pauseButton";
            this.pauseButton.Size = new System.Drawing.Size(64, 44);
            this.pauseButton.TabIndex = 6;
            this.pauseButton.Text = "Pause";
            this.pauseButton.UseVisualStyleBackColor = true;
            this.pauseButton.Click += new System.EventHandler(this.pauseButton_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.interruptGroupBox);
            this.panel1.Controls.Add(this.flagsGroupBox);
            this.panel1.Controls.Add(this.otherGroupBox);
            this.panel1.Controls.Add(this.registersGroupBox);
            this.panel1.Controls.Add(this.stackGroupBox);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(254, 342);
            this.panel1.TabIndex = 3;
            // 
            // interruptGroupBox
            // 
            this.interruptGroupBox.BackColor = System.Drawing.Color.Gray;
            this.interruptGroupBox.Location = new System.Drawing.Point(154, 103);
            this.interruptGroupBox.Name = "interruptGroupBox";
            this.interruptGroupBox.Size = new System.Drawing.Size(93, 68);
            this.interruptGroupBox.TabIndex = 10;
            this.interruptGroupBox.TabStop = false;
            this.interruptGroupBox.Text = "Interrupt Values";
            // 
            // flagsGroupBox
            // 
            this.flagsGroupBox.BackColor = System.Drawing.Color.Gray;
            this.flagsGroupBox.Controls.Add(this.sFlagValue);
            this.flagsGroupBox.Controls.Add(this.vFlagValue);
            this.flagsGroupBox.Controls.Add(this.bFlagValue);
            this.flagsGroupBox.Controls.Add(this.dFlagValue);
            this.flagsGroupBox.Controls.Add(this.iFlagValue);
            this.flagsGroupBox.Controls.Add(this.negativeFlagLabel);
            this.flagsGroupBox.Controls.Add(this.overflowFlagLabel);
            this.flagsGroupBox.Controls.Add(this.emptyFlagLabel);
            this.flagsGroupBox.Controls.Add(this.breakFlagLabel);
            this.flagsGroupBox.Controls.Add(this.decimalFlagLabel);
            this.flagsGroupBox.Controls.Add(this.intDisableFlagLabel);
            this.flagsGroupBox.Controls.Add(this.zeroFlagLabel);
            this.flagsGroupBox.Controls.Add(this.carryFlagLabel);
            this.flagsGroupBox.Controls.Add(this.cFlagValue);
            this.flagsGroupBox.Controls.Add(this.zFlagValue);
            this.flagsGroupBox.Controls.Add(this.hFlagValue);
            this.flagsGroupBox.Location = new System.Drawing.Point(154, 3);
            this.flagsGroupBox.Name = "flagsGroupBox";
            this.flagsGroupBox.Size = new System.Drawing.Size(93, 94);
            this.flagsGroupBox.TabIndex = 8;
            this.flagsGroupBox.TabStop = false;
            this.flagsGroupBox.Text = "Flags";
            // 
            // carryFlagLabel
            // 
            this.carryFlagLabel.AutoSize = true;
            this.carryFlagLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.carryFlagLabel.Location = new System.Drawing.Point(5, 16);
            this.carryFlagLabel.Name = "carryFlagLabel";
            this.carryFlagLabel.Size = new System.Drawing.Size(19, 13);
            this.carryFlagLabel.TabIndex = 8;
            this.carryFlagLabel.Text = "C:";
            // 
            // cFlagValue
            // 
            this.cFlagValue.AutoSize = true;
            this.cFlagValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cFlagValue.Location = new System.Drawing.Point(29, 16);
            this.cFlagValue.Name = "cFlagValue";
            this.cFlagValue.Size = new System.Drawing.Size(0, 13);
            this.cFlagValue.TabIndex = 7;
            // 
            // zFlagValue
            // 
            this.zFlagValue.AutoSize = true;
            this.zFlagValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.zFlagValue.Location = new System.Drawing.Point(29, 29);
            this.zFlagValue.Name = "zFlagValue";
            this.zFlagValue.Size = new System.Drawing.Size(0, 13);
            this.zFlagValue.TabIndex = 10;
            // 
            // hFlagValue
            // 
            this.hFlagValue.AutoSize = true;
            this.hFlagValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hFlagValue.Location = new System.Drawing.Point(41, 61);
            this.hFlagValue.Name = "hFlagValue";
            this.hFlagValue.Size = new System.Drawing.Size(0, 13);
            this.hFlagValue.TabIndex = 7;
            // 
            // otherGroupBox
            // 
            this.otherGroupBox.BackColor = System.Drawing.Color.Gray;
            this.otherGroupBox.Controls.Add(this.pcValue);
            this.otherGroupBox.Controls.Add(this.programCounterLabel);
            this.otherGroupBox.Location = new System.Drawing.Point(3, 177);
            this.otherGroupBox.Name = "otherGroupBox";
            this.otherGroupBox.Size = new System.Drawing.Size(244, 157);
            this.otherGroupBox.TabIndex = 7;
            this.otherGroupBox.TabStop = false;
            this.otherGroupBox.Text = "Other Values";
            // 
            // pcValue
            // 
            this.pcValue.AutoSize = true;
            this.pcValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pcValue.Location = new System.Drawing.Point(51, 16);
            this.pcValue.Name = "pcValue";
            this.pcValue.Size = new System.Drawing.Size(0, 16);
            this.pcValue.TabIndex = 5;
            // 
            // programCounterLabel
            // 
            this.programCounterLabel.AutoSize = true;
            this.programCounterLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.programCounterLabel.Location = new System.Drawing.Point(13, 16);
            this.programCounterLabel.Name = "programCounterLabel";
            this.programCounterLabel.Size = new System.Drawing.Size(32, 16);
            this.programCounterLabel.TabIndex = 4;
            this.programCounterLabel.Text = "PC:";
            // 
            // registersGroupBox
            // 
            this.registersGroupBox.BackColor = System.Drawing.Color.Gray;
            this.registersGroupBox.Controls.Add(this.statusRegisterValueLabel);
            this.registersGroupBox.Controls.Add(this.statusRegisterLabel);
            this.registersGroupBox.Controls.Add(this.yValueLabel);
            this.registersGroupBox.Controls.Add(this.yRegisterLabel);
            this.registersGroupBox.Controls.Add(this.xValueLabel);
            this.registersGroupBox.Controls.Add(this.xRegisterLabel);
            this.registersGroupBox.Controls.Add(this.accumulatorValueLabel);
            this.registersGroupBox.Controls.Add(this.accumulatorLabel);
            this.registersGroupBox.Location = new System.Drawing.Point(3, 1);
            this.registersGroupBox.Name = "registersGroupBox";
            this.registersGroupBox.Size = new System.Drawing.Size(145, 96);
            this.registersGroupBox.TabIndex = 7;
            this.registersGroupBox.TabStop = false;
            this.registersGroupBox.Text = "Registers";
            // 
            // accumulatorValueLabel
            // 
            this.accumulatorValueLabel.AutoSize = true;
            this.accumulatorValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.accumulatorValueLabel.Location = new System.Drawing.Point(119, 23);
            this.accumulatorValueLabel.Name = "accumulatorValueLabel";
            this.accumulatorValueLabel.Size = new System.Drawing.Size(0, 16);
            this.accumulatorValueLabel.TabIndex = 4;
            // 
            // accumulatorLabel
            // 
            this.accumulatorLabel.AutoSize = true;
            this.accumulatorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.accumulatorLabel.Location = new System.Drawing.Point(2, 20);
            this.accumulatorLabel.Name = "accumulatorLabel";
            this.accumulatorLabel.Size = new System.Drawing.Size(97, 16);
            this.accumulatorLabel.TabIndex = 0;
            this.accumulatorLabel.Text = "Accumulator:";
            // 
            // stackGroupBox
            // 
            this.stackGroupBox.BackColor = System.Drawing.Color.Gray;
            this.stackGroupBox.Controls.Add(this.stackValue);
            this.stackGroupBox.Controls.Add(this.spValue);
            this.stackGroupBox.Controls.Add(this.spValuesLabel);
            this.stackGroupBox.Controls.Add(this.spLabel);
            this.stackGroupBox.Location = new System.Drawing.Point(3, 103);
            this.stackGroupBox.Name = "stackGroupBox";
            this.stackGroupBox.Size = new System.Drawing.Size(145, 68);
            this.stackGroupBox.TabIndex = 7;
            this.stackGroupBox.TabStop = false;
            this.stackGroupBox.Text = "Stack Values";
            // 
            // stackValue
            // 
            this.stackValue.AutoSize = true;
            this.stackValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stackValue.Location = new System.Drawing.Point(71, 36);
            this.stackValue.Name = "stackValue";
            this.stackValue.Size = new System.Drawing.Size(0, 16);
            this.stackValue.TabIndex = 9;
            // 
            // spValue
            // 
            this.spValue.AutoSize = true;
            this.spValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spValue.Location = new System.Drawing.Point(71, 20);
            this.spValue.Name = "spValue";
            this.spValue.Size = new System.Drawing.Size(0, 16);
            this.spValue.TabIndex = 8;
            // 
            // spValuesLabel
            // 
            this.spValuesLabel.AutoSize = true;
            this.spValuesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spValuesLabel.Location = new System.Drawing.Point(6, 36);
            this.spValuesLabel.Name = "spValuesLabel";
            this.spValuesLabel.Size = new System.Drawing.Size(59, 16);
            this.spValuesLabel.TabIndex = 6;
            this.spValuesLabel.Text = "SP Val:";
            // 
            // spLabel
            // 
            this.spLabel.AutoSize = true;
            this.spLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spLabel.Location = new System.Drawing.Point(7, 20);
            this.spLabel.Name = "spLabel";
            this.spLabel.Size = new System.Drawing.Size(32, 16);
            this.spLabel.TabIndex = 5;
            this.spLabel.Text = "SP:";
            // 
            // refreshButton
            // 
            this.refreshButton.Location = new System.Drawing.Point(347, 310);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(66, 44);
            this.refreshButton.TabIndex = 7;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(560, 310);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(69, 44);
            this.resetButton.TabIndex = 8;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // xValueLabel
            // 
            this.xValueLabel.AutoSize = true;
            this.xValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xValueLabel.Location = new System.Drawing.Point(119, 42);
            this.xValueLabel.Name = "xValueLabel";
            this.xValueLabel.Size = new System.Drawing.Size(0, 16);
            this.xValueLabel.TabIndex = 6;
            // 
            // xRegisterLabel
            // 
            this.xRegisterLabel.AutoSize = true;
            this.xRegisterLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xRegisterLabel.Location = new System.Drawing.Point(2, 39);
            this.xRegisterLabel.Name = "xRegisterLabel";
            this.xRegisterLabel.Size = new System.Drawing.Size(84, 16);
            this.xRegisterLabel.TabIndex = 5;
            this.xRegisterLabel.Text = "X Register:";
            // 
            // yValueLabel
            // 
            this.yValueLabel.AutoSize = true;
            this.yValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.yValueLabel.Location = new System.Drawing.Point(119, 58);
            this.yValueLabel.Name = "yValueLabel";
            this.yValueLabel.Size = new System.Drawing.Size(0, 16);
            this.yValueLabel.TabIndex = 8;
            // 
            // yRegisterLabel
            // 
            this.yRegisterLabel.AutoSize = true;
            this.yRegisterLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.yRegisterLabel.Location = new System.Drawing.Point(2, 55);
            this.yRegisterLabel.Name = "yRegisterLabel";
            this.yRegisterLabel.Size = new System.Drawing.Size(85, 16);
            this.yRegisterLabel.TabIndex = 7;
            this.yRegisterLabel.Text = "Y Register:";
            // 
            // statusRegisterValueLabel
            // 
            this.statusRegisterValueLabel.AutoSize = true;
            this.statusRegisterValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusRegisterValueLabel.Location = new System.Drawing.Point(119, 74);
            this.statusRegisterValueLabel.Name = "statusRegisterValueLabel";
            this.statusRegisterValueLabel.Size = new System.Drawing.Size(0, 16);
            this.statusRegisterValueLabel.TabIndex = 10;
            // 
            // statusRegisterLabel
            // 
            this.statusRegisterLabel.AutoSize = true;
            this.statusRegisterLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusRegisterLabel.Location = new System.Drawing.Point(2, 71);
            this.statusRegisterLabel.Name = "statusRegisterLabel";
            this.statusRegisterLabel.Size = new System.Drawing.Size(118, 16);
            this.statusRegisterLabel.TabIndex = 9;
            this.statusRegisterLabel.Text = "Status Register:";
            // 
            // zeroFlagLabel
            // 
            this.zeroFlagLabel.AutoSize = true;
            this.zeroFlagLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.zeroFlagLabel.Location = new System.Drawing.Point(5, 29);
            this.zeroFlagLabel.Name = "zeroFlagLabel";
            this.zeroFlagLabel.Size = new System.Drawing.Size(19, 13);
            this.zeroFlagLabel.TabIndex = 11;
            this.zeroFlagLabel.Text = "Z:";
            // 
            // intDisableFlagLabel
            // 
            this.intDisableFlagLabel.AutoSize = true;
            this.intDisableFlagLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.intDisableFlagLabel.Location = new System.Drawing.Point(8, 43);
            this.intDisableFlagLabel.Name = "intDisableFlagLabel";
            this.intDisableFlagLabel.Size = new System.Drawing.Size(15, 13);
            this.intDisableFlagLabel.TabIndex = 12;
            this.intDisableFlagLabel.Text = "I:";
            // 
            // decimalFlagLabel
            // 
            this.decimalFlagLabel.AutoSize = true;
            this.decimalFlagLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.decimalFlagLabel.Location = new System.Drawing.Point(5, 58);
            this.decimalFlagLabel.Name = "decimalFlagLabel";
            this.decimalFlagLabel.Size = new System.Drawing.Size(20, 13);
            this.decimalFlagLabel.TabIndex = 13;
            this.decimalFlagLabel.Text = "D:";
            // 
            // negativeFlagLabel
            // 
            this.negativeFlagLabel.AutoSize = true;
            this.negativeFlagLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.negativeFlagLabel.Location = new System.Drawing.Point(52, 57);
            this.negativeFlagLabel.Name = "negativeFlagLabel";
            this.negativeFlagLabel.Size = new System.Drawing.Size(19, 13);
            this.negativeFlagLabel.TabIndex = 17;
            this.negativeFlagLabel.Text = "S:";
            // 
            // overflowFlagLabel
            // 
            this.overflowFlagLabel.AutoSize = true;
            this.overflowFlagLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.overflowFlagLabel.Location = new System.Drawing.Point(52, 42);
            this.overflowFlagLabel.Name = "overflowFlagLabel";
            this.overflowFlagLabel.Size = new System.Drawing.Size(19, 13);
            this.overflowFlagLabel.TabIndex = 16;
            this.overflowFlagLabel.Text = "V:";
            // 
            // emptyFlagLabel
            // 
            this.emptyFlagLabel.AutoSize = true;
            this.emptyFlagLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.emptyFlagLabel.Location = new System.Drawing.Point(54, 28);
            this.emptyFlagLabel.Name = "emptyFlagLabel";
            this.emptyFlagLabel.Size = new System.Drawing.Size(11, 13);
            this.emptyFlagLabel.TabIndex = 15;
            this.emptyFlagLabel.Text = "-";
            // 
            // breakFlagLabel
            // 
            this.breakFlagLabel.AutoSize = true;
            this.breakFlagLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.breakFlagLabel.Location = new System.Drawing.Point(52, 15);
            this.breakFlagLabel.Name = "breakFlagLabel";
            this.breakFlagLabel.Size = new System.Drawing.Size(19, 13);
            this.breakFlagLabel.TabIndex = 14;
            this.breakFlagLabel.Text = "B:";
            // 
            // iFlagValue
            // 
            this.iFlagValue.AutoSize = true;
            this.iFlagValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.iFlagValue.Location = new System.Drawing.Point(29, 41);
            this.iFlagValue.Name = "iFlagValue";
            this.iFlagValue.Size = new System.Drawing.Size(0, 13);
            this.iFlagValue.TabIndex = 18;
            // 
            // dFlagValue
            // 
            this.dFlagValue.AutoSize = true;
            this.dFlagValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dFlagValue.Location = new System.Drawing.Point(29, 59);
            this.dFlagValue.Name = "dFlagValue";
            this.dFlagValue.Size = new System.Drawing.Size(0, 13);
            this.dFlagValue.TabIndex = 19;
            // 
            // bFlagValue
            // 
            this.bFlagValue.AutoSize = true;
            this.bFlagValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bFlagValue.Location = new System.Drawing.Point(73, 14);
            this.bFlagValue.Name = "bFlagValue";
            this.bFlagValue.Size = new System.Drawing.Size(0, 13);
            this.bFlagValue.TabIndex = 20;
            // 
            // vFlagValue
            // 
            this.vFlagValue.AutoSize = true;
            this.vFlagValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.vFlagValue.Location = new System.Drawing.Point(73, 42);
            this.vFlagValue.Name = "vFlagValue";
            this.vFlagValue.Size = new System.Drawing.Size(0, 13);
            this.vFlagValue.TabIndex = 21;
            // 
            // sFlagValue
            // 
            this.sFlagValue.AutoSize = true;
            this.sFlagValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sFlagValue.Location = new System.Drawing.Point(73, 57);
            this.sFlagValue.Name = "sFlagValue";
            this.sFlagValue.Size = new System.Drawing.Size(0, 13);
            this.sFlagValue.TabIndex = 22;
            // 
            // AssemblyView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(641, 360);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.pauseButton);
            this.Controls.Add(this.runButton);
            this.Controls.Add(this.stepButton);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.listBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AssemblyView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NEST - Assembly View";
            this.Load += new System.EventHandler(this.AssemblyView_Load);
            this.panel1.ResumeLayout(false);
            this.flagsGroupBox.ResumeLayout(false);
            this.flagsGroupBox.PerformLayout();
            this.otherGroupBox.ResumeLayout(false);
            this.otherGroupBox.PerformLayout();
            this.registersGroupBox.ResumeLayout(false);
            this.registersGroupBox.PerformLayout();
            this.stackGroupBox.ResumeLayout(false);
            this.stackGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button stepButton;
        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.Button pauseButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label spValuesLabel;
        private System.Windows.Forms.Label spLabel;
        private System.Windows.Forms.Label programCounterLabel;
        private System.Windows.Forms.Label accumulatorLabel;
        private System.Windows.Forms.GroupBox registersGroupBox;
        private System.Windows.Forms.GroupBox stackGroupBox;
        private System.Windows.Forms.Label stackValue;
        private System.Windows.Forms.Label spValue;
        private System.Windows.Forms.Label accumulatorValueLabel;
        private System.Windows.Forms.GroupBox otherGroupBox;
        private System.Windows.Forms.Label pcValue;
        private System.Windows.Forms.GroupBox flagsGroupBox;
        private System.Windows.Forms.Label carryFlagLabel;
        private System.Windows.Forms.Label zFlagValue;
        private System.Windows.Forms.Label cFlagValue;
        private System.Windows.Forms.Label hFlagValue;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.GroupBox interruptGroupBox;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Label statusRegisterValueLabel;
        private System.Windows.Forms.Label statusRegisterLabel;
        private System.Windows.Forms.Label yValueLabel;
        private System.Windows.Forms.Label yRegisterLabel;
        private System.Windows.Forms.Label xValueLabel;
        private System.Windows.Forms.Label xRegisterLabel;
        private System.Windows.Forms.Label negativeFlagLabel;
        private System.Windows.Forms.Label overflowFlagLabel;
        private System.Windows.Forms.Label emptyFlagLabel;
        private System.Windows.Forms.Label breakFlagLabel;
        private System.Windows.Forms.Label decimalFlagLabel;
        private System.Windows.Forms.Label intDisableFlagLabel;
        private System.Windows.Forms.Label zeroFlagLabel;
        private System.Windows.Forms.Label sFlagValue;
        private System.Windows.Forms.Label vFlagValue;
        private System.Windows.Forms.Label bFlagValue;
        private System.Windows.Forms.Label dFlagValue;
        private System.Windows.Forms.Label iFlagValue;
    }
}