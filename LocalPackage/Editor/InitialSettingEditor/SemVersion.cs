using System;
using System.Text.RegularExpressions;

namespace NF.UnityTools.Essentials.InitialSettingEditor
{
    public struct UnityVersion : IComparable<UnityVersion>, IEquatable<UnityVersion>
    {
        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }
        public string Suffix { get; }

        public UnityVersion(int major, int minor, int patch, string suffix = "")
        {
            if (major < 0 || minor < 0 || patch < 0)
            {
                throw new ArgumentException("Version numbers cannot be negative.");
            }

            Major = major;
            Minor = minor;
            Patch = patch;
            Suffix = suffix;
        }

        public static bool TryParse(string version, out UnityVersion result)
        {
            result = default;
            Match match = Regex.Match(version, @"^(\d+)\.(\d+)\.(\d+)([a-z]\d+)?$");

            if (!match.Success)
            {
                return false;
            }

            int major, minor, patch;
            if (!int.TryParse(match.Groups[1].Value, out major) ||
                !int.TryParse(match.Groups[2].Value, out minor) ||
                !int.TryParse(match.Groups[3].Value, out patch))
            {
                return false;
            }

            string suffix = match.Groups[4].Value;

            result = new UnityVersion(major, minor, patch, suffix);
            return true;
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Patch}{Suffix}";
        }

        public int CompareTo(UnityVersion other)
        {
            int result = Major.CompareTo(other.Major);
            if (result != 0)
            {
                return result;
            }

            result = Minor.CompareTo(other.Minor);
            if (result != 0)
            {
                return result;
            }

            result = Patch.CompareTo(other.Patch);
            if (result != 0)
            {
                return result;
            }

            return string.Compare(Suffix, other.Suffix, StringComparison.Ordinal);
        }

        public bool Equals(UnityVersion other)
        {
            return Major == other.Major && Minor == other.Minor &&
                   Patch == other.Patch && Suffix == other.Suffix;
        }

        public override bool Equals(object obj)
        {
            if (obj is UnityVersion other)
            {
                return Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Major, Minor, Patch, Suffix);
        }

        public static bool operator ==(UnityVersion left, UnityVersion right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UnityVersion left, UnityVersion right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(UnityVersion left, UnityVersion right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(UnityVersion left, UnityVersion right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(UnityVersion left, UnityVersion right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(UnityVersion left, UnityVersion right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}