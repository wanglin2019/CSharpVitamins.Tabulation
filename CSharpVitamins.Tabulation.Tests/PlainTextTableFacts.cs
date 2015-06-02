﻿using CSharpVitamins.Tabulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions;

//namespace CSharpVitamins.Tabulation.Tests
namespace Tests
{
	public class PlainTextTableFacts
	{
		ITestOutputHelper output;

		public PlainTextTableFacts(ITestOutputHelper output)
		{
			this.output = output;
		}

		IEnumerable<string[]> create_test_data()
		{
			return new[]
			{
				new [] { "Col A", "Col B", "Col C" },   // header
				new [] { "R1-A", "R1-B", "R1-C" },      // data
				new [] { "R2-A", "R2-B", "R2-C long" }, // data
				new [] { "R3-A", "R3-B", "R3-C" },      // data
			};
		}

		[Fact]
		void expectedColumns_should_inferMaxFromFirstRow()
		{
			var tab = new PlainTextTable();

			tab.ImportRows(create_test_data());

			Assert.Equal(3, tab.ColumnsExpected);
		}

		[Fact]
		void expectedColumns_should_throwExceptionIfRowElementLengthIsInvalid()
		{
			var tab = new PlainTextTable(4);

			Assert.Throws(typeof(UnexpectedColumnCountException), () => tab.ImportRows(create_test_data()));
		}

		[Fact]
		void columnState_should_throwExceptionIfExpectedColumnIsInvalid()
		{
			var tab = new PlainTextTable();

			Assert.Throws(typeof(InvalidOperationException), () => tab.GetColumnState());
		}

		[Fact]
		void columnState_should_inferColumnWidthsFromTestData()
		{
			var tab = new PlainTextTable(create_test_data());

			var columns = tab.GetColumnState();

			Assert.Equal(3, columns.Length);

			Assert.Equal(5, columns[0].Width);
			Assert.Equal(5, columns[1].Width);
			Assert.Equal(9, columns[2].Width);
		}

		[Fact]
		void fluid_should_setSeparateByDelimiter()
		{
			var tab = new PlainTextTable();

			tab.SeparateBy("TEST");

			Assert.Equal("TEST", tab.ColumnSeparator);
		}

		[Fact]
		void fluid_should_setTrimTrailingSpace()
		{
			var tab = new PlainTextTable();

			tab.TrimTrailingSpace(true);

			Assert.Equal(true, tab.TrimTrailingWhitespace);
		}

		[Fact]
		void fluid_should_setAlignmentByIndexer()
		{
			var tab = new PlainTextTable();

			tab.Align(2, 'r');

			Assert.Equal(Alignment.Right, tab.Alignments[2]);
		}

		[Fact]
		void fluid_should_setAlignments()
		{
			var tab = new PlainTextTable(create_test_data());

			tab.Align('l', 'm', 'r');

			Assert.Equal(3, tab.Alignments.Count);

			var columns = tab.GetColumnState();

			Assert.Equal(3, columns.Length);

			Assert.Equal(Alignment.Left, columns[0].Align);
			Assert.Equal(Alignment.Center, columns[1].Align);
			Assert.Equal(Alignment.Right, columns[2].Align);
		}

		[Theory]
		[InlineData("Left", 6, "Left  ", Alignment.Left, true)]
		[InlineData("Left", 6, "Left", Alignment.Left, false)]
		[InlineData("Right", 7, "  Right", Alignment.Right, true)]
		[InlineData("Right", 7, "  Right", Alignment.Right, false)]
		[InlineData("Mid", 7, "  Mid  ", Alignment.Center, true)]
		[InlineData("Mid", 7, "  Mid", Alignment.Center, false)]
		void stringPadding_should_padWith(string input, int length, string expected, Alignment align, bool padRight)
		{
			var actual = PlainTextTable.Pad(input, length, align, padRight);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData("Mid", 6, " Mid  ", Alignment.Center, true)]
		[InlineData("Mid", 6, " Mid", Alignment.Center, false)]
		[InlineData("Col A", 10, "  Col A   ", Alignment.Center, true)]
		void stringPadding_should_preferLeftOffsetWhenAligningCentre(string input, int length, string expected, Alignment align, bool padRight)
		{
			var actual = PlainTextTable.Pad(input, length, align, padRight);

			Assert.Equal(expected, actual);
		}

		[Fact]
		void render_should_writeSpaceDelimitedTable()
		{
			var tab = new PlainTextTable(create_test_data());

			var result = tab.ToString();
			output.WriteLine(result + "EOL");
			
			string expected = @"Col A Col B Col C    
R1-A  R1-B  R1-C     
R2-A  R2-B  R2-C long
R3-A  R3-B  R3-C     
";

			Assert.Equal(expected, result);
		}

		[Fact]
		void render_should_writeTrimmedRightColumn()
		{
			var tab = new PlainTextTable(create_test_data());

			tab.TrimTrailingSpace(true);

			var result = tab.ToString();
			output.WriteLine(result + "EOL");
			
			string expected = @"Col A Col B Col C
R1-A  R1-B  R1-C
R2-A  R2-B  R2-C long
R3-A  R3-B  R3-C
";

			Assert.Equal(expected, result);
		}

		[Fact]
		void render_should_writeSpacePipeSpaceDelimitedTable()
		{
			var tab = new PlainTextTable(create_test_data());

			tab.SeparateBy(" | ");

			var result = tab.ToString();
			output.WriteLine(result + "EOL");
			
			string expected = @"Col A | Col B | Col C    
R1-A  | R1-B  | R1-C     
R2-A  | R2-B  | R2-C long
R3-A  | R3-B  | R3-C     
";

			Assert.Equal(expected, result);
		}

		[Fact]
		void render_should_writeAlignedColumns()
		{
			var tab = new PlainTextTable(create_test_data());
			
			tab.SeparateBy("|");
			tab.Align('l', 'r', 'm');

			var result = tab.ToString();
			output.WriteLine(result + "EOL");
			
			string expected = @"Col A|Col B|  Col C  
R1-A | R1-B|  R1-C   
R2-A | R2-B|R2-C long
R3-A | R3-B|  R3-C   
";

			Assert.Equal(expected, result);
		}

		[Fact]
		void render_should_writeAlignedColumnsAndTrimTrailingWhite()
		{
			var tab = new PlainTextTable(create_test_data());
			
			tab.SeparateBy("|");
			tab.Align('l', 'r', 'm');
			tab.TrimTrailingSpace(true);

			var result = tab.ToString();
			output.WriteLine(result + "EOL");
			
			string expected = @"Col A|Col B|  Col C
R1-A | R1-B|  R1-C
R2-A | R2-B|R2-C long
R3-A | R3-B|  R3-C
";

			Assert.Equal(expected, result);
		}
	}
}