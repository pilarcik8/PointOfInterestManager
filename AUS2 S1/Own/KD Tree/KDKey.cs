using System;
using System.Collections.Generic;
using System.Text;

namespace DataStructures.KD_Tree
{
    // Vygenerovane / upravene pomocou AI - Kapitola 24
    public class KDKey<T>(T value) where T : IComparable<T>
    {
        public readonly T Value = value;

        // Vygenerovane / upravene pomocou AI - Kapitola 19 (v KDNode), 24 (presunute do kluča)
        public int CompareTo(KDKey<T>? other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), "Other KDKey cannot be null.");
            }
            return Value.CompareTo(other.Value);
        }
    }
}
