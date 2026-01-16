using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp.Models
{
    public class TestInfo
    {
        public int ElementCount { get; private set; }
        public List<TestMethodInfo> MethodInfos { get; private set; }
        public TestInfo(int elementCount)
        {
            ElementCount = elementCount;
            MethodInfos = new List<TestMethodInfo>();
        }

        /// <summary>
        /// Формирование текста по тестированию
        /// </summary>
        /// <param name="cellWidth"></param>
        /// <returns></returns>
        public string ToString(params int[] cellWidth)
        {
            var tableWidth = cellWidth.Aggregate((x,y) => x + y);
            var text = string.Concat($"Element count: {ElementCount.ToString("0,0")}", "\n");

            text = string.Concat(text, "\n", GetLine(tableWidth));
            text = string.Concat(text, "\n", GetRow(cellWidth, "Method", "Load time", "Result"));
            text = string.Concat(text, "\n", GetLine(tableWidth));

            foreach (var methodInfo in MethodInfos)
            {
                text = string.Concat(text, "\n", GetRow(cellWidth, methodInfo.TestType, methodInfo.LoadTime.ToString() + " ms", methodInfo.Result.ToString()));
            }
            text = string.Concat(text, "\n", GetLine(tableWidth));
            return text;
        }


        private static string GetLine(int tableWidth)
        {
            return new string('-', tableWidth);
        }

        private static string GetRow(int[] cellWidth, params string[] columns)
        {
            string row = "|";

            for (int i = 0; i < columns.Length; i++)
            {
                if(cellWidth.Length <= i)
                    break;

                row += AlignCentre(columns[i], cellWidth[i]) + "|";
            }

            return row;
        }

        private static string AlignCentre(string text, int width)
        {
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;

            if (string.IsNullOrEmpty(text))
            {
                return new string(' ', width);
            }
            else
            {
                return text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
            }
        }
    }
}