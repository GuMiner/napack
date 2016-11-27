using System;
using System.Collections.Generic;
using System.Linq;

namespace NapackClient
{
    /// <summary>
    /// Holds a validly defined napack version. This doesn't mean the napack exists, only that it's format is valid.
    /// </summary>
    public class DefinedNapackVersion
    {
        public DefinedNapackVersion(string napackDirectoryName)
        {
            List<string> components = napackDirectoryName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (components.Count != 4)
            {
                throw new ArgumentException(napackDirectoryName);
            }

            this.NapackName = components[0];
            components.RemoveAt(0);

            List<int> versionComponents = components.Select(item => int.Parse(item)).ToList();
            if (versionComponents.Any(item => item < 0))
            {
                throw new InvalidNapackVersionException(string.Join(".", versionComponents));
            }

            this.Major = versionComponents[0];
            this.Minor = versionComponents[1];
            this.Patch = versionComponents[2];
        }

        public DefinedNapackVersion(string napackName, string versionString)
        {
            this.NapackName = napackName;

            List<int> components = versionString.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Select(item => int.Parse(item)).ToList();
            if (components.Count != 3 || components.Any(item => item < 0))
            {
                throw new InvalidNapackVersionException(versionString);
            }

            this.Major = components[0];
            this.Minor = components[1];
            this.Patch = components[2];
        }

        public string NapackName { get; private set; }

        public int Major { get; private set; }

        public int Minor { get; private set; }

        public int Patch { get; private set; }

        public string GetDirectoryName()
            => this.NapackName + "." + this.Major + "." + this.Minor + "." + this.Patch;

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            DefinedNapackVersion other = obj as DefinedNapackVersion;
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
