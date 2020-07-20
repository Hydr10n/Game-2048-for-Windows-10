using System;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Hydr10n
{
    namespace Utils
    {
        static class AppData<T>
        {
            private static readonly IPropertySet localData = ApplicationData.Current.RoamingSettings.Values;

            public static void Save(string key, T data) => localData[key] = data;

            public static T Load(string key, out bool hasKey)
            {
                object data = localData[key];
                return (hasKey = data != null) ? (T)data : default;
            }
        }

        static class AppData2D<T> where T : struct, IConvertible
        {
            private const char RowDelimiter = ';', ColumnDelimiter = ',';

            public static void Save(string key, T[,] data)
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
                AppData<string>.Save(key, str);
            }

            public static T[][] Load(string key, out bool hasKey)
            {
                string rawData = AppData<string>.Load(key, out hasKey)?.ToString();
                if (!hasKey || rawData.Trim() == "")
                    return null;
                T[][] data = null;
                try
                {
                    string[] strs = rawData.Split(RowDelimiter);
                    data = new T[strs.Length][];
                    for (int i = 0; i < strs.Length; i++)
                    {
                        string[] temp = strs[i].Split(ColumnDelimiter);
                        data[i] = new T[temp.Length];
                        for (int j = 0; j < temp.Length; j++)
                            data[i][j] = (T)Convert.ChangeType(temp[j], typeof(T));
                    }
                }
                catch { }
                return data;
            }
        }
    }
}