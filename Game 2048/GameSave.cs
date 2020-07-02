using Windows.Foundation.Collections;
using Windows.Storage;

namespace Game_2048
{
    class GameSave
    {
        private const char RowDelimiter = ';', ColumnDelimiter = ',';

        private readonly IPropertySet localData = ApplicationData.Current.RoamingSettings.Values;

        public void SaveData(string key, int data) => localData[key] = data;

        public void SaveData(string key, int[,] data)
        {
            string str = "";
            if (data != null)
            {
                int length1D = data.GetLength(0), length2D = data.GetLength(1);
                for (int i = 0; i < length1D; i++)
                {
                    for (int j = 0; j < length2D; j++)
                        str += data[i, j] + (j == length2D - 1 ? "" : ColumnDelimiter.ToString());
                    str += (i == length1D - 1 ? "" : RowDelimiter.ToString());
                }
            }
            localData[key] = str;
        }

        public int LoadIntData(string key, out bool hasKey)
        {
            hasKey = false;
            var value = localData[key];
            if (value == null)
                return 0;
            hasKey = true;
            return (int)value;
        }

        public int[][] LoadInt2DData(string key)
        {
            string rawData = localData[key]?.ToString();
            if (rawData == null || rawData == "")
                return null;
            int[][] data;
            try
            {
                string[] strings = rawData.Split(RowDelimiter);
                data = new int[strings.Length][];
                for (int i = 0; i < strings.Length; i++)
                {
                    string[] temp = strings[i].Split(ColumnDelimiter);
                    data[i] = new int[temp.Length];
                    for (int j = 0; j < temp.Length; j++)
                        data[i][j] = int.Parse(temp[j]);
                }
            }
            catch { return null; }
            return data;
        }
    }
}