using System;
using System.Collections.Generic;
using System.Text;

namespace AUS2_S1.KD_Tree
{
    public class KDKey<T> where T : IComparable<T>
    {
        private T Value;

        public KDKey(T value)
        {
            Value = value;
        }

        public int CompareTo(KDKey<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), "Other KDKey cannot be null.");
            }
            return Value.CompareTo(other.Value);
        }
    }
}
