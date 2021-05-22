using STCM2LPacker;
using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using static STCM2LPacker.STCM2L;

namespace STCM2LPacker
{
	public partial class STCM2LPackerGUI : Form
	{
		STCM2L stcm2l;

		public STCM2LPackerGUI()
		{
			InitializeComponent();
		}

		private void disableControls()
		{
			openFileButton.Enabled = false;
			openOpcodeFileButton.Enabled = false;
		}
		private void enableControls()
		{
			openFileButton.Enabled = true;
			openOpcodeFileButton.Enabled = true;
		}

		private void openFile_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Title = "Open STCM2L file";
			dlg.RestoreDirectory = true;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				var filePath = dlg.FileName;
				openFileTextBox.Text = String.Format(filePath);
				
				readFileToDataSource(filePath);
			}
		}

		private void openOpcodeFile_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Title = "Open opcode map file";
			dlg.RestoreDirectory = true;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				var opcodeFilePath = dlg.FileName;
				opcodeFileTextBox.Text = String.Format(opcodeFilePath);
				_opcodeTable = STCM2LPacker.ReadOpCodeTable(opcodeFilePath);
				readFileToDataSource(openFileTextBox.Text);
			}
		}

		private void clearOpcode_Click(object sender, EventArgs e)
		{
			_opcodeTable = null;
			opcodeFileTextBox.Text = "";
			readFileToDataSource(openFileTextBox.Text);
		}

		private void openCharTable_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Title = "Open character table file";
			dlg.RestoreDirectory = true;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				var tableFilePath = dlg.FileName;
				openCharTableTextBox.Text = String.Format(tableFilePath);
			}
		}

		private void readFileToDataSource(string filePath)
		{
			if (filePath == null || filePath == "") return;

			disableControls();

			// Populate list view
			dataGridView1.DataSource = null;
			stcm2l = STCM2LPacker.ReadFile(filePath);
			var bindingList = new BindingList<DataLine>();
			bindingList.RaiseListChangedEvents = false;

			var lineCount = 1;

			for (var i = 0; i < stcm2l.instructions.Count; i++)
			{
				var instruction = stcm2l.instructions[i];
				if (_opcodeTable != null && (instruction.lines == null || instruction.lines.Count == 0)) continue;
				
				if (instruction.lines != null && instruction.lines.Count > 0)
				{
					foreach (var line in instruction.lines)
					{

						var dataLine = new DataLine(instruction, line, lineCount);
						bindingList.Add(dataLine);
						lineCount++;
					}
				}
				else
				{
					var dataLine = new DataLine(instruction, lineCount);
					bindingList.Add(dataLine);
					lineCount++;
				}
				
			}
			bindingList.ResetBindings();
			dataGridView1.AutoGenerateColumns = true;
			dataGridView1.DataSource = bindingList;
			dataGridView1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

			// Get possible text opcodes
			var opcodes = STCM2LPacker.GetPossibleTextOpcodes(stcm2l);
			var text = "";
			foreach (var opcode in opcodes)
			{
				text += opcode.ToString("x").ToUpper() + " ";
			}
			couldBeOpcodeTextBox.Text = text;

			enableControls();
		}

		private class DataLine {
			public DataLine(Instruction instruction, int no)
			{
				No = no;
				Opcode = instruction.opcode.ToString("x").ToUpper();
				Text = "";
				if (instruction.extraData != null)
				{
					Text = Encoding.GetEncoding(_encoding).GetString(instruction.extraData);
				}
				string labelValue = "";
				if (_opcodeTable != null)
				{
					_opcodeTable.TryGetValue(Opcode, out labelValue);
				}
				Label = labelValue;
			}

			public DataLine(Instruction instruction, Line line, int no)
			{
				No = no;
				Opcode = instruction.opcode.ToString("x").ToUpper();
				Text = "";
				if (line != null)
				{
					Text = line.text;
				}
				else if (instruction.extraData != null)
				{
					Text = Encoding.GetEncoding(_encoding).GetString(instruction.extraData);
				}
				string labelValue = "";
				if (_opcodeTable != null)
				{
					_opcodeTable.TryGetValue(Opcode, out labelValue);
				}
				Label = labelValue;
			}
			public int No { get; set; }
			public String Opcode { get; set; }
			public String Label { get; set; }
			public String Text { get; set; }
		}

		private void exportButton_Click(object sender, EventArgs e)
		{
			if (stcm2l == null || _opcodeTable == null)
			{
				MessageBox.Show("Open a STCM2L file & opcode table first.");
				return;
			}

			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Title = "Save CSV file";
			dlg.RestoreDirectory = true;
			dlg.FileName = openFileTextBox.Text + ".csv";
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				var outputFile = dlg.FileName;
				STCM2LPacker.Export(stcm2l, outputFile);
				MessageBox.Show("Done.");
			}
		}

		private void importButton_Click(object sender, EventArgs e)
		{
			if (stcm2l == null || _opcodeTable == null)
			{
				MessageBox.Show("Open a STCM2L file & opcode table first.");
				return;
			}

			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Title = "Open translated CSV file";
			dlg.RestoreDirectory = true;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				SaveFileDialog dlg2 = new SaveFileDialog();
				dlg2.RestoreDirectory = true;
				dlg2.Title = "Save patched STCM2L file";
				dlg2.FileName = dlg.FileName.Replace(".csv", "");
				if (dlg2.ShowDialog() == DialogResult.OK)
				{
					if (dlg2.FileName == openFileTextBox.Text)
					{
						MessageBox.Show("Output file can't be the same as input file");
						return;
					}
					var csvFile = dlg.FileName;
					var stcm2lFile = openFileTextBox.Text;
					var outputFile = dlg2.FileName;
					var tableFile = openCharTableTextBox.Text;
					
					STCM2LPacker.Import(stcm2l, csvFile, stcm2lFile, outputFile, tableFile);
					MessageBox.Show("Done.");
				}
			}
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MessageBox.Show("Made by aes@otomevn");
		}
	}
}
