using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace STCM2LPacker
{
	public class STCM2L
	{
		public class Header
		{
			public String STCM2L; //len 32
			public UInt32 exportOffset;
			public UInt32 exportCount;
			public UInt32 field28;
			public UInt32 collectionLinkOffset;
			public byte[] zeros; //len 32

			public void read(BinaryReader b)
			{
				STCM2L = Encoding.GetEncoding(_encoding).GetString(b.ReadBytes(32));
				exportOffset = b.ReadUInt32();
				exportCount = b.ReadUInt32();
				field28 = b.ReadUInt32();
				collectionLinkOffset = b.ReadUInt32();
				zeros = b.ReadBytes(32);
			}
		};

		// Params for Instruction
		public class Parameter
		{
			public List<UInt32> listParam;
			public List<long> _listParamOffset;

			public void read(BinaryReader b)
			{
				listParam = new List<UInt32>();
				_listParamOffset = new List<long>();
				for (var i = 0; i < 3; i++)
				{
					var _paramOffset = b.BaseStream.Position;
					_listParamOffset.Add(_paramOffset);

					var param = b.ReadUInt32();
					listParam.Add(param);
				}
			}
		};

		// Text Data for Instruction
		public class TextData
		{
			public long _address;
			public UInt32 zero;
			public UInt32 textOpCode1; // op1: textLen/4
			public UInt32 textOpCode2; // op2: 1
			public UInt32 textLen;
			public String text; // textLen

			public void read(BinaryReader b)
			{
				_address = b.BaseStream.Position;
				zero = b.ReadUInt32();
				textOpCode1 = b.ReadUInt32();
				textOpCode2 = b.ReadUInt32();
				textLen = b.ReadUInt32();
				text = Encoding.GetEncoding(_encoding).GetString(b.ReadBytes((int)textLen));
			}
		};

		// Instruction
		public class Instruction
		{
			public long _address;
			public bool _isExported;
			public long _textDataAddressOffset;

			public UInt32 isCall; // 0 or 1
			public UInt32 opcode; // OpCode
			public UInt32 paramCount; // Number of params (<16)
			public UInt32 size; // Total size of the instruction
			public List<Parameter> parameters;
			public TextData textData;
			public byte[] extraData;

			public int _extraDataLen;

			public int read(BinaryReader b)
			{
				_address = b.BaseStream.Position;
				_isExported = false;
				_textDataAddressOffset = 0;

				isCall = b.ReadUInt32();
				opcode = b.ReadUInt32();
				paramCount = b.ReadUInt32();
				size = b.ReadUInt32();

				parameters = new List<Parameter>();
				for (var i = 0; i < paramCount; i++)
				{
					var parameter = new Parameter();
					parameter.read(b);
					parameters.Add(parameter);
				}

				// Extra data len = size of the instruction - (size of isCall + opcode + paramCount + size field) - (paramCount * paramSize)
				_extraDataLen = (int)size - 16 - (int)paramCount * 12;
				if (_extraDataLen > 0)
				{
					markParam(b.BaseStream.Position);
					if (isText())
					{
						textData = new TextData();
						textData.read(b);
						return 1; // Is text instruction
					}
					else
					{
						extraData = b.ReadBytes(_extraDataLen);
						return 0; // Is not text instruction
					}
				}
				
				return 0;
			}

			public bool isText()
			{
				var opcodeHex = opcode.ToString("x").ToUpper();
				return _extraDataLen > 0 && _opcodeTable != null && _opcodeTable.ContainsKey(opcodeHex);
			}

			// If the instruction is text, there will be one parameter pointing to the text (extra data)
			public bool markParam(long extraDataAddress)
			{
				if (_extraDataLen > 0)
				{
					foreach (var parameter in parameters)
					{
						for (var i = 0; i < parameter.listParam.Count; i++)
						{
							var param = parameter.listParam[i];
							if (param == extraDataAddress)
							{
								_textDataAddressOffset = parameter._listParamOffset[i];
								return true;
							}
						}
					}
				}
				return false;
			}
		}

		// ExportEntry for ExportData
		public class ExportEntry
		{
			public UInt32 type; // 0 = CODE, 1 = DATA
			public String name; // len 32
			public UInt32 offset;

			public void read(BinaryReader b)
			{
				type = b.ReadUInt32();
				name = Encoding.GetEncoding(_encoding).GetString(b.ReadBytes(32));
				offset = b.ReadUInt32();
			}
		};

		// Export Data
		public class ExportData
		{
			public List<ExportEntry> exportEntries;

			public void read(BinaryReader b)
			{
				exportEntries = new List<ExportEntry>();
				while (b.BaseStream.Position < b.BaseStream.Length)
				{
					var entry = new ExportEntry();
					entry.read(b);
					exportEntries.Add(entry);
				}
			}
		};

		// "utf-8", "shift_js"
		public static String _encoding;
		// User-defined
		public static Dictionary<String, String> _opcodeTable;

		// Data
		public Header header;
		public String GLOBAL_DATA; // len 11
		public String CODE_START_; // len 12
		public List<Instruction> instructions;
		public ExportData exportData;

		public int _textLineCount;

		public void read(BinaryReader b, String encoding)
		{
			_encoding = encoding;
			// STCM2L file structure
			// Header
			header = new Header();
			header.read(b);

			// GLOBAL_DATA text
			GLOBAL_DATA = Encoding.GetEncoding(encoding).GetString(b.ReadBytes(11));

			// CODE_START_ text at an address divisible by 16
			while (b.BaseStream.Position % 16 != 0 || b.PeekChar() != 'C')
			{
				b.ReadByte();
			}
			CODE_START_ = Encoding.GetEncoding(encoding).GetString(b.ReadBytes(12));

			// List of instructions
			_textLineCount = 0;
			instructions = new List<Instruction>();
			while (b.BaseStream.Position < header.exportOffset - 12)
			{
				var instruction = new Instruction();
				try
				{
					_textLineCount += instruction.read(b); // Read returns 1 if this is a text line
				}
				catch (Exception e) {
					Console.WriteLine(e.ToString());
				}
				
				instructions.Add(instruction);
				b.BaseStream.Seek(instruction._address + instruction.size, SeekOrigin.Begin);
			}

			// EXPORT_DATA at the end of the file
			b.BaseStream.Seek(header.exportOffset, SeekOrigin.Begin);
			exportData = new ExportData();
			exportData.read(b);

			// Marks which instructions are exported
			
			for (int i = 0; i < instructions.Count; i++)
			{
				for (int j = 0; j < header.exportCount; j++)
				{
					if (instructions[i]._address == exportData.exportEntries[j].offset)
					{
						instructions[i]._isExported = true;
					}
				}
			}
		}
	}
};
