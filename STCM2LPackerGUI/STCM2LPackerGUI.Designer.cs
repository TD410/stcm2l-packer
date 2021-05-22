namespace STCM2LPacker
{
	partial class STCM2LPackerGUI
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(STCM2LPackerGUI));
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.fileLoaded = new System.Windows.Forms.Label();
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.sTCM2LPackerBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.openOpcodeFileButton = new System.Windows.Forms.Button();
			this.opcodeFileTextBox = new System.Windows.Forms.TextBox();
			this.openFileButton = new System.Windows.Forms.Button();
			this.openFileTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.couldBeOpcodeTextBox = new System.Windows.Forms.TextBox();
			this.exportButton = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.openCharTable = new System.Windows.Forms.Button();
			this.openCharTableTextBox = new System.Windows.Forms.TextBox();
			this.menuStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sTCM2LPackerBindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(689, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
			this.aboutToolStripMenuItem.Text = "About";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
			// 
			// fileLoaded
			// 
			this.fileLoaded.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.fileLoaded.AutoSize = true;
			this.fileLoaded.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
			this.fileLoaded.Location = new System.Drawing.Point(12, 539);
			this.fileLoaded.Name = "fileLoaded";
			this.fileLoaded.Size = new System.Drawing.Size(0, 15);
			this.fileLoaded.TabIndex = 1;
			// 
			// dataGridView1
			// 
			this.dataGridView1.AllowUserToAddRows = false;
			this.dataGridView1.AllowUserToDeleteRows = false;
			this.dataGridView1.AllowUserToOrderColumns = true;
			this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridView1.AutoGenerateColumns = false;
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.DataSource = this.sTCM2LPackerBindingSource;
			this.dataGridView1.Location = new System.Drawing.Point(12, 188);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.RowHeadersVisible = false;
			this.dataGridView1.Size = new System.Drawing.Size(665, 322);
			this.dataGridView1.TabIndex = 2;
			// 
			// openOpcodeFileButton
			// 
			this.openOpcodeFileButton.Location = new System.Drawing.Point(13, 66);
			this.openOpcodeFileButton.Name = "openOpcodeFileButton";
			this.openOpcodeFileButton.Size = new System.Drawing.Size(124, 23);
			this.openOpcodeFileButton.TabIndex = 3;
			this.openOpcodeFileButton.Text = "Opcode table";
			this.openOpcodeFileButton.UseVisualStyleBackColor = true;
			this.openOpcodeFileButton.Click += new System.EventHandler(this.openOpcodeFile_Click);
			// 
			// opcodeFileTextBox
			// 
			this.opcodeFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.opcodeFileTextBox.Location = new System.Drawing.Point(153, 68);
			this.opcodeFileTextBox.Name = "opcodeFileTextBox";
			this.opcodeFileTextBox.ReadOnly = true;
			this.opcodeFileTextBox.Size = new System.Drawing.Size(438, 20);
			this.opcodeFileTextBox.TabIndex = 4;
			// 
			// openFileButton
			// 
			this.openFileButton.Location = new System.Drawing.Point(12, 28);
			this.openFileButton.Name = "openFileButton";
			this.openFileButton.Size = new System.Drawing.Size(125, 23);
			this.openFileButton.TabIndex = 6;
			this.openFileButton.Text = "Open file";
			this.openFileButton.UseVisualStyleBackColor = true;
			this.openFileButton.Click += new System.EventHandler(this.openFile_Click);
			// 
			// openFileTextBox
			// 
			this.openFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.openFileTextBox.Location = new System.Drawing.Point(153, 30);
			this.openFileTextBox.Name = "openFileTextBox";
			this.openFileTextBox.ReadOnly = true;
			this.openFileTextBox.Size = new System.Drawing.Size(438, 20);
			this.openFileTextBox.TabIndex = 7;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(27, 155);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(110, 13);
			this.label2.TabIndex = 8;
			this.label2.Text = "Possible text opcodes";
			// 
			// couldBeOpcodeTextBox
			// 
			this.couldBeOpcodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.couldBeOpcodeTextBox.Location = new System.Drawing.Point(153, 152);
			this.couldBeOpcodeTextBox.Name = "couldBeOpcodeTextBox";
			this.couldBeOpcodeTextBox.ReadOnly = true;
			this.couldBeOpcodeTextBox.Size = new System.Drawing.Size(438, 20);
			this.couldBeOpcodeTextBox.TabIndex = 9;
			// 
			// exportButton
			// 
			this.exportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.exportButton.Location = new System.Drawing.Point(592, 528);
			this.exportButton.Name = "exportButton";
			this.exportButton.Size = new System.Drawing.Size(84, 23);
			this.exportButton.TabIndex = 10;
			this.exportButton.Text = "Export";
			this.exportButton.UseVisualStyleBackColor = true;
			this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.button1.Location = new System.Drawing.Point(12, 528);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(106, 23);
			this.button1.TabIndex = 11;
			this.button1.Text = "Import and Patch";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.importButton_Click);
			// 
			// button2
			// 
			this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button2.Location = new System.Drawing.Point(597, 68);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(80, 23);
			this.button2.TabIndex = 12;
			this.button2.Text = "Clear";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.clearOpcode_Click);
			// 
			// openCharTable
			// 
			this.openCharTable.Location = new System.Drawing.Point(15, 107);
			this.openCharTable.Name = "openCharTable";
			this.openCharTable.Size = new System.Drawing.Size(122, 23);
			this.openCharTable.TabIndex = 13;
			this.openCharTable.Text = "Char table";
			this.openCharTable.UseVisualStyleBackColor = true;
			this.openCharTable.Click += new System.EventHandler(this.openCharTable_Click);
			// 
			// openCharTableTextBox
			// 
			this.openCharTableTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.openCharTableTextBox.Location = new System.Drawing.Point(153, 107);
			this.openCharTableTextBox.Name = "openCharTableTextBox";
			this.openCharTableTextBox.ReadOnly = true;
			this.openCharTableTextBox.Size = new System.Drawing.Size(438, 20);
			this.openCharTableTextBox.TabIndex = 14;
			// 
			// STCM2LPackerGUI
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(689, 563);
			this.Controls.Add(this.openCharTableTextBox);
			this.Controls.Add(this.openCharTable);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.exportButton);
			this.Controls.Add(this.couldBeOpcodeTextBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.openFileTextBox);
			this.Controls.Add(this.openFileButton);
			this.Controls.Add(this.opcodeFileTextBox);
			this.Controls.Add(this.openOpcodeFileButton);
			this.Controls.Add(this.dataGridView1);
			this.Controls.Add(this.fileLoaded);
			this.Controls.Add(this.menuStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "STCM2LPackerGUI";
			this.Text = "STCM2LPackerGUI";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sTCM2LPackerBindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.Label fileLoaded;
		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.BindingSource sTCM2LPackerBindingSource;
		private System.Windows.Forms.Button openOpcodeFileButton;
		private System.Windows.Forms.TextBox opcodeFileTextBox;
		private System.Windows.Forms.Button openFileButton;
		private System.Windows.Forms.TextBox openFileTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox couldBeOpcodeTextBox;
		private System.Windows.Forms.Button exportButton;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button openCharTable;
		private System.Windows.Forms.TextBox openCharTableTextBox;
	}
}

