using System;
using System.Collections;

namespace Hydr10n
{
    namespace Collections
    {
        class SquareArray<T> : ICloneable, IEnumerable
        {
            private T[,] array;
            public int SideLength { get; private set; }
            public T this[int i, int j] { get => array[i, j]; set => array[i, j] = value; }

            public SquareArray(int sideLength)
            {
                array = new T[sideLength, sideLength];
                SideLength = sideLength;
            }

            public static SquareArray<T> FromArray(T[,] array)
            {
                if (array == null || array.GetLength(0) != array.GetLength(1))
                    return null;
                return new SquareArray<T>(array.GetLength(0)) { array = array.Clone() as T[,] };
            }

            public static SquareArray<T> FromArray(T[][] array)
            {
                if (array == null)
                    return null;
                SquareArray<T> squareArray = new SquareArray<T>(array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] == null || array[i].Length != array.Length)
                        return null;
                    for (int j = 0; j < array.Length; j++)
                        squareArray[i, j] = array[i][j];
                }
                return squareArray;
            }

            public T[,] ToArray() => array.Clone() as T[,];

            public object Clone() => new SquareArray<T>(SideLength) { array = ToArray() };

            public IEnumerator GetEnumerator()
            {
                foreach (T element in array)
                    yield return element;
            }
        }
    }
}