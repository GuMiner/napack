﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Napack.Common;
using Newtonsoft.Json;

namespace Napack.Server
{
    public class UserIdentifier
    {
        /// <summary>
        /// The email the user has provided; used as the user's ID.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Email { get; set; }
        
        /// <summary>
        /// THe computed hash used to authenticate the user.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string Hash { get; set; }

        /// <summary>
        /// Hashes the provided identifiers into a Base64 string using SHA512.
        /// </summary>
        public static string ComputeUserHash(List<Guid> secrets)
        {
            List<byte[]> secretsAsBytes = secrets.Select(secret => secret.ToByteArray()).ToList();
            byte[] input = new byte[secretsAsBytes.Sum(secret => secret.Length)];

            int currentLength = 0;
            foreach (byte[] secret in secretsAsBytes)
            {
                secret.CopyTo(input, currentLength);
                currentLength += secret.Length;
            }

            // As we're using this as the authentication validation for the end user, we want to avoid
            //  hash collisions, so we need a collision-resistent cryptographically-secure hash function, *not* MD5.
            using (SHA512 sha256 = SHA512.Create())
            {
                byte[] resultingHash = sha256.ComputeHash(input);
                return Convert.ToBase64String(resultingHash);
            }
        }

        public static void VerifyAuthorization(Dictionary<string, IEnumerable<string>> headers, INapackStorageManager storageManager, List<string> authorizedUserIds)
        {
            // Check that valid inputs were provided.
            if (!headers.ContainsKey(CommonHeaders.UserKeys) || !headers.ContainsKey(CommonHeaders.UserId))
            {
                throw new UnauthorizedUserException();
            }

            List<Guid> userSecrets;
            string userId;
            try
            {
                userSecrets = Serializer.Deserialize<List<Guid>>(
                    Encoding.UTF8.GetString(Convert.FromBase64String(string.Join(string.Empty, headers[CommonHeaders.UserKeys]))));
                userId = string.Join(string.Empty, headers[CommonHeaders.UserId]);
            }
            catch (Exception)
            {
                throw new UnauthorizedUserException();
            }

            // Check that the user is authorized to perform this operation.
            if (!authorizedUserIds.Contains(userId, StringComparer.InvariantCulture))
            {
                throw new UnauthorizedUserException();
            }

            // Check that the user is who they say they are.
            UserIdentifier user = storageManager.GetUser(userId);
            string hash = UserIdentifier.ComputeUserHash(userSecrets);
            if (!user.Hash.Equals(hash, StringComparison.InvariantCulture))
            {
                throw new UnauthorizedUserException();
            }
        }
    }
}
