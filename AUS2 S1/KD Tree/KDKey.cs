using System;
using System.Collections.Generic;
using System.Text;

namespace AUS2_S1.KD_Tree
{
    // Vygenerovane / upravene pomocou AI (Model: Gemini 3.8 Flash) - Prompt 24
    public class KDKey<T>(T value) where T : IComparable<T>
    {
        public readonly T Value = value;

        // Vygenerovane / upravene pomocou AI (Model: Gemini 3.8 Flash) - Prompt 24
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
