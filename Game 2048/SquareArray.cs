using System;
using System.Collections;

namespace Hydr10n
{
    namespace Collections
    {
        class SquareArray<T> : ICloneable, IEnumerable
        {
            public int SideLength { get; private set; }
            public T[,] Array { get; private set; }
            public ref T this[int i, int j] => ref Array[i, j];

            public SquareArray(int sideLength)
            {
                SideLength = sideLength;
                Array = new T[sideLength, sideLength];
            }

            public object Clone() => new SquareArray<T>(SideLength) { Array = Array.Clone() as T[,] };

            public IEnumerator GetEnumerator()
            {
                foreach (T item in Array)
                    yield return item;
            }
        }
    }
}