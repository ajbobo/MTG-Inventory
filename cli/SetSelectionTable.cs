using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Scryfall;
using Spectre.Console.Rendering;

namespace MTG_CLI
{
    public class SetSelectionTable
    {
        private static List<Scryfall.Set> SetList = new List<Scryfall.Set>();
        private static int HighlightIndex = -1;
        private static int MinIndex = 0;
        private static int CurHeight = 0;

        public SetSelectionTable(List<Scryfall.Set> list)
        {
            SetList = list;
        }

        public void Reset()
        {
            HighlightIndex = -1;
            MinIndex = 0;
            CurHeight = 0;
        }

        public Scryfall.Set GetSelectedSet()
        {
            return SetList[HighlightIndex];
        }

        public Table GetTable(Set selectedSet, int height)
        {
            Table table = new Table();
            table.AddColumns(
                new TableColumn("Current").Width(2).RightAligned(),
                new TableColumn("Name"));
            table.HideHeaders();
            table.Border(TableBorder.None);

            CurHeight = height;
            if (HighlightIndex < 0)
                HighlightIndex = SetList.IndexOf(selectedSet);
            else if (HighlightIndex >= SetList.Count)
                HighlightIndex = SetList.Count - 1;

            if (HighlightIndex < MinIndex)
                MinIndex = HighlightIndex;
            else if (HighlightIndex > MinIndex + height - 1)
                MinIndex = HighlightIndex - height + 1;
            else if (HighlightIndex > SetList.Count - height + 1)
                MinIndex = SetList.Count - height;

            for (int x = MinIndex; x < MinIndex + height; x++)
            {
                if (x == HighlightIndex)
                {
                    table.AddRow("[blue]>[/]", string.Format("[blue]{0}[/]", SetList[x].Name ?? ""));
                }
                else
                {
                    table.AddRow("", SetList[x].Name ?? "");
                }
            }

            return table;
        }

        public void OnUpArrow()
        {
            HighlightIndex--;
            if (HighlightIndex < 0)
                HighlightIndex = 0;
        }

        public void OnDownArrow()
        {
            HighlightIndex++;
            if (HighlightIndex >= SetList.Count)
                HighlightIndex = SetList.Count - 1;
        }

        public void OnPageUp()
        {
            int pageAmt = Math.Min(CurHeight, HighlightIndex);
            HighlightIndex -= pageAmt;
            MinIndex -= pageAmt;
            if (MinIndex < 0)
                MinIndex = 0;
        }

        public void OnPageDown()
        {
            int pageAmt = Math.Min(CurHeight, SetList.Count - HighlightIndex);
            HighlightIndex += pageAmt;
            MinIndex += pageAmt;
        }
    }
}