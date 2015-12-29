// --------------------------------------
//  Domain	: Realtime.co
//  Author	: Nicholas Ventimiglia
//  Product	: Realtime Messaging
//  Copyright (c) 2014 IBT  All rights reserved.
//  -------------------------------------

using System;
using System.Text;

namespace Foundation.Server.Infrastructure.Helpers
{
    /// <summary>
    /// Class used for operations with strings.
    /// </summary>
    public class Strings
    {
        public static Random random = new Random();

        /// <summary>
        /// Randoms the number.
        /// </summary>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns></returns>
        public static int RandomNumber(int min, int max)
        {
            return random.Next(min, max);
        } 
        
        /// <summary>
        /// Randoms the number.
        /// </summary>
        /// <returns></returns>
        public static double RandomDecimal()
        {
            return random.NextDouble();
        }

        /// <summary>
        /// Randoms the string.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        public static string RandomString(int size)
        {
            var builder = new StringBuilder();
            char ch;

            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Generates an id.
        /// </summary>
        /// <returns></returns>
        public static string GenerateId(int size)
        {
            return RandomString(size);
        }

        /// <summary>
        /// Generates an id.
        /// </summary>
        /// <returns></returns>
        public static DateTime RandomDate(int daysback)
        {
            return DateTime.UtcNow.AddDays(-(random.Next(0, daysback)));
        }

        public static string RandomEmail()
        {
            return string.Format("{0}@{1}.com", RandomString(5), RandomString(5));
        }

        public static string RandomPhone()
        {
            return string.Format("{0}-{1}-{2}", RandomString(3), RandomString(3), RandomString(4));
        }

        public static string RandomUrl()
        {
            return string.Format("http://{0}.com", RandomString(6));
        }
    }
}
