using System;
using System.Collections.Generic;

namespace HonorAmongThieves
{
    public static class Utils
    {
        public static Random Rng = new Random();

        public static bool IsValidName(string userName, int minLength = 1, int maxLength = 10)
        {
            if (userName.Length < minLength
                || userName.Length > maxLength)
            {
                return false;
            }

            foreach (char c in userName.ToCharArray())
            {
                if (!char.IsLetterOrDigit(c))
                {
                    return false;
                }
            }

            return true;
        }

        public static string GenerateId<T>(int trimmedLength, Dictionary<string, T> existingEntries)
        {
            string id = null;
            int tries = 0;
            while (tries < 300 &&
                (id == null || existingEntries.ContainsKey(id)))
            {
                id = Guid.NewGuid().ToString().Substring(0, trimmedLength);
                tries++;
            }

            return id;
        }

        public static void Shuffle<T>(IList<T> list)
        {
            // Performs the fisher-yates shuffle
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
