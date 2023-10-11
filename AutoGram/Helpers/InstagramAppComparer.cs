using System.Collections.Generic;
using Database;

namespace AutoGram.Helpers
{
    public class InstagramAppComparer : IEqualityComparer<AndroidDevice>
    {
        public bool Equals(AndroidDevice x, AndroidDevice y)
        {
            return y != null && (x != null && x.Uuid.Equals(y.Uuid));
        }

        public int GetHashCode(AndroidDevice obj)
        {
            return obj.GetHashCode();
        }
    }
}
