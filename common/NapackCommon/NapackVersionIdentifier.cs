using System;
using System.Collections.Generic;
using System.Linq;

namespace Napack.Common
{
    /// <summary>
    /// Holds a validly defined napack version. This doesn't mean the napack exists, only that it's format is valid.
    /// </summary>
    public class NapackVersionIdentifier
    {
        public NapackVersionIdentifier(string fullNapackName)
        {
            List<string> components = fullNapackName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (components.Count != 4)
            {
                throw new InvalidNapackVersionException();
            }

            this.NapackName = components[0];
            components.RemoveAt(0);

            List<int> versionComponents = components.Select(item => int.Parse(item)).ToList();
            if (components.Count != 3 || versionComponents.Any(item => item < 0))
            {
                throw new InvalidNapackVersionException();
            }

            this.Major = versionComponents[0];
            this.Minor = versionComponents[1];
            this.Patch = versionComponents[2];
        }

        public NapackVersionIdentifier(string napackName, int major, int minor, int patch)
        {
            this.NapackName = napackName;
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
        }

        public string NapackName { get; private set; }

        public int Major { get; private set; }

        public int Minor { get; private set; }

        public int Patch { get; private set; }

        public string GetFullName()
            => this.NapackName + "." + this.Major + "." + this.Minor + "." + this.Patch;

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            NapackVersionIdentifier other = obj as NapackVersionIdentifier;
            if (other == null)
            {
                return false;
            }

            return this.NapackName.Equals(other.NapackName, StringComparison.InvariantCultureIgnoreCase) &&
                this.Major == other.Major && this.Minor == other.Minor && this.Patch == other.Patch;
        }

        public override int GetHashCode()
        {
            return this.NapackName.ToUpperInvariant().GetHashCode() + 13 * (this.Major + 13 * (this.Minor + 13 * this.Patch));
        }
    }
}
