using System;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Hydr10n
{
    namespace Utils
    {
        static class AppData
        {
            private static readonly IPropertySet localData = ApplicationData.Current.RoamingSettings.Values;

            public static void Save<T>(string key, T data) => localData[key] = data;

            public static void Load<T>(string key, out T data, out bool hasKey)
            {
                object dat = localData[key];
                data = (hasKey = dat != null) ? (T)dat : default;
            }
        }

        static class AppData2D
        {
            private const char RowDelimiter = ';', ColumnDelimiter = ',';

            public static void Save<T>(string key, T[,] data) where T : struct, IConvertible
            {
                string str = "";
                if (data != null)
                {
                    int length1D = data.GetLength(0), length2D = data.GetLength(1);
                    for (int i = 0; i < length1D; i++)
                    {
                        for (int j = 0; j < length2D; j++)
                            str += data[i, j] + (j == length2D - 1 ? "" : ColumnDelimiter.ToString());
                        str += i == length1D - 1 ? "" : RowDelimiter.ToString();
                    }
                }
                AppData.Save(key, str);
            }

            public static void Load<T>(string key, out T[][] data, out bool hasKey) where T : struct, IConvertible
            {
                data = null;
                AppData.Load(key, out string rawData, out hasKey);
                if (!hasKey || rawData.Trim() == "")
                    return;
                try
                {
                    string[] strs = rawData.Split(RowDelimiter);
                    T[][] dat = new T[strs.Length][];
                    for (int i = 0; i < strs.Length; i++)
                    {
                        string[] temp = strs[i].Split(ColumnDelimiter);
                        dat[i] = new T[temp.Length];
                        for (int j = 0; j < temp.Length; j++)
                            dat[i][j] = (T)Convert.ChangeType(temp[j], typeof(T));
                    }
                    data = dat;
                }
                catch { }
            }
        }
    }
}