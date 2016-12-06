using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Napack.Common;

namespace Napack.Server
{
    public class UserIdentifier
    {
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
        /// The email the user has provided.
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        /// The SHA512 hash of the user's access keys.
        /// </summary>
        public string UserHash { get; set; }

        /// <summary>
        /// Assigns the user hash and returns the identifiers used in calculating that has.
        /// </summary>
        public List<Guid> AssignUserHash()
        {
            Guid first = Guid.NewGuid();
            Guid second = Guid.NewGuid();
            Guid third = Guid.NewGuid();
            this.UserHash = UserIdentifier.GetHashedIdentifiers(first, second, third);
            return new[] { first, second, third }.ToList();
        }

        /// <summary>
        /// Hashes the provided identifiers into a Base64 string using SHA512.
        /// </summary>
        public static string GetHashedIdentifiers(Guid first, Guid second, Guid third)
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

        internal static void Validate(Dictionary<string, IEnumerable<string>> dictionary, List<string> authorizedUserHashes)
        {
            if (!dictionary.ContainsKey(CommonHeaders.UserKeys))
            {
                throw new UnauthorizedUserException();
            }

            List<Guid> keys;
            try
            {
                keys = dictionary[CommonHeaders.UserKeys].Select(item => Guid.Parse(item)).ToList();
            }
            catch (Exception)
            {
                throw new UnauthorizedUserException();
            }

            if (keys.Count != 3)
            {
                throw new UnauthorizedUserException();
            }

            string hash = UserIdentifier.GetHashedIdentifiers(keys[0], keys[1], keys[2]);
            if (!authorizedUserHashes.Contains(hash, StringComparer.InvariantCulture))
            {
                throw new UnauthorizedUserException();
            }
        }
    }
}
