using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleReflection.Implementation
{
    public class TableBuilder
    {
        /// <summary>
        /// Формирует форматированную таблицу в виде строки с заданными ширинами колонок.
        /// </summary>
        /// <param name="table">Двумерный массив с содержимым таблицы</param>
        /// <param name="cellWidth">Массив ширин для каждой колонки</param>
        /// <returns>Строка с форматированной таблицей</returns>
        public string ToString(string[][] table, params int[] cellWidth)
        {
            var tableWidth = cellWidth.Aggregate((x, y) => x + y);
            var text = string.Empty;

            text = string.Concat(text, "\n", GetLine(tableWidth));
            text = string.Concat(text, "\n", GetRow(cellWidth, table[0]));
            text = string.Concat(text, "\n", GetLine(tableWidth));

            for (int i = 1; i < table.Length; i++)
            {
                text = string.Concat(text, "\n", GetRow(cellWidth, table[i]));
            }
            text = string.Concat(text, "\n", GetLine(tableWidth));
            return text;
        }

        /// <summary>
        /// Создает горизонтальную линию для границы таблицы.
        /// </summary>
        /// <param name="tableWidth">Ширина таблицы</param>
        /// <returns>Строка с дефисами длины tableWidth</returns>
        private static string GetLine(int tableWidth)
        {
            return new string('-', tableWidth);
        }

        /// <summary>
        /// Формирует строку таблицы с содержимым и разделителями.
        /// </summary>
        /// <param name="cellWidth">Массив ширин для каждой колонки</param>
        /// <param name="columns">Содержимое колонок</param>
        /// <returns>Строка с форматированным содержимым</returns>
        private static string GetRow(int[] cellWidth, params string[] columns)
        {
            string row = "|";

            for (int i = 0; i < columns.Length; i++)
            {
                if (cellWidth.Length <= i)
                    break;

                row += AlignCentre(columns[i], cellWidth[i]) + "|";
            }

            return row;
        }

        /// <summary>
        /// Выравнивает текст по центру в пределах заданной ширины.
        /// </summary>
        /// <param name="text">Текст для выравнивания</param>
        /// <param name="width">Желаемая ширина</param>
        /// <returns>Выравненный текст с пробелами</returns>
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