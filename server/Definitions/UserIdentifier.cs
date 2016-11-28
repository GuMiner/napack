using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Napack.Server
{
    public class UserIdentifier
    {
        /// <summary>
        /// The email the user has provided.
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        /// The SHA512 hash of the user's access keys.
        /// </summary>
        public string UserHash { get; set; }

        /// <summary>
        /// Creates a new user identifier for the specified email, leaving the user's hash blank.
        /// </summary>
        /// <param name="userEmail">The email to associated with the hash. Not used in hash generation.</param>
        public UserIdentifier(string userEmail)
        {
            this.UserEmail = userEmail;
            this.UserHash = null;
        }

        /// <summary>
        /// Returns true if the provided user hashes match.
        /// </summary>
        public bool DoHashes(string otherHash)
        {
            return this.UserHash.Equals(otherHash, StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Assigns the user hash and returns the identifiers used in calculating that has.
        /// </summary>
        public List<Guid> AssignUserHash()
        {
            Guid first = Guid.NewGuid();
            Guid second = Guid.NewGuid();
            Guid third = Guid.NewGuid();
            this.UserHash = this.GetHashedIdentifiers(first, second, third);
            return new[] { first, second, third }.ToList();
        }

        /// <summary>
        /// Hashes the provided identifiers into a Base64 string using SHA512.
        /// </summary>
        public string GetHashedIdentifiers(Guid first, Guid second, Guid third)
        {
            byte[] firstGuid = first.ToByteArray();
            byte[] secondGuid = second.ToByteArray();
            byte[] thirdGuid = third.ToByteArray();
            byte[] input = new byte[firstGuid.Length + secondGuid.Length + thirdGuid.Length];
            firstGuid.CopyTo(input, 0);
            secondGuid.CopyTo(input, firstGuid.Length);
            thirdGuid.CopyTo(input, firstGuid.Length + secondGuid.Length);

            // As we're using this as the authentication validation for the end user, we want to avoid
            //  hash collisions, so we need a collision-resistent cryptographically-secure hash function, *not* MD5.
            using (SHA512 sha256 = SHA512.Create())
            {
                byte[] resultingHash = sha256.ComputeHash(input);
                return Convert.ToBase64String(resultingHash);
            }
        }
    }
}
