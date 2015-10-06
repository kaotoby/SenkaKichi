using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.AspNet.Identity;
using SenkaKichi.DbModels;

namespace SenkaKichi.Models
{
    /// <summary>
    ///     Token provider that uses an IDataProtector to generate encrypted tokens based off of the security stamp
    /// </summary>
    public class ApplicationUserTokenProvider
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="protector"></param>
        public ApplicationUserTokenProvider(IDataProtector protector) {
            if (protector == null) {
                throw new ArgumentNullException("protector");
            }
            Protector = protector;
            TokenLifespan = TimeSpan.FromHours(12);
        }

        /// <summary>
        ///     IDataProtector for the token
        /// </summary>
        public IDataProtector Protector { get; private set; }

        /// <summary>
        ///     Lifespan after which the token is considered expired
        /// </summary>
        public TimeSpan TokenLifespan { get; set; }

        /// <summary>
        ///     Generate a protected string for a user
        /// </summary>
        /// <param name="purpose"></param>
        /// <param name="manager"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public string Generate(string purpose, ApplicationUserManager manager, AspNetUser user) {
            if (user == null) {
                throw new ArgumentNullException("user");
            }
            var ms = new MemoryStream();
            using (var writer = ms.CreateWriter()) {
                writer.Write(DateTimeOffset.UtcNow);
                writer.Write(user.Id.ToString());
                writer.Write(purpose ?? "");
                string stamp = null;
                writer.Write(stamp ?? "");
            }
            var protectedBytes = Protector.Protect(ms.ToArray());
            return Convert.ToBase64String(protectedBytes);
        }

        /// <summary>
        ///     Return false if the token is not valid
        /// </summary>
        /// <param name="purpose"></param>
        /// <param name="token"></param>
        /// <param name="manager"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<bool> ValidateAsync(string purpose, string token, ApplicationUserManager manager, AspNetUser user) {
            try {
                var unprotectedData = Protector.Unprotect(Convert.FromBase64String(token));
                var ms = new MemoryStream(unprotectedData);
                using (var reader = ms.CreateReader()) {
                    var creationTime = reader.ReadDateTimeOffset();
                    var expirationTime = creationTime + TokenLifespan;
                    if (expirationTime < DateTimeOffset.UtcNow) {
                        return Task.FromResult<bool>(false);
                    }

                    var userId = reader.ReadString();
                    if (!String.Equals(userId, Convert.ToString(user.Id, CultureInfo.InvariantCulture))) {
                        return Task.FromResult<bool>(false);
                    }
                    var purp = reader.ReadString();
                    if (!String.Equals(purp, purpose)) {
                        return Task.FromResult<bool>(false);
                    }
                    var stamp = reader.ReadString();
                    if (reader.PeekChar() != -1) {
                        return Task.FromResult<bool>(false);
                    }

                    return Task.FromResult<bool>(stamp == "");
                }
            }
                // ReSharper disable once EmptyGeneralCatchClause
            catch {
                // Do not leak exception
            }
            return Task.FromResult<bool>(false);
        }
    }

    // Based on Levi's authentication sample
    internal static class StreamExtensions
    {
        internal static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);

        public static BinaryReader CreateReader(this Stream stream) {
            return new BinaryReader(stream, DefaultEncoding, true);
        }

        public static BinaryWriter CreateWriter(this Stream stream) {
            return new BinaryWriter(stream, DefaultEncoding, true);
        }

        public static DateTimeOffset ReadDateTimeOffset(this BinaryReader reader) {
            return new DateTimeOffset(reader.ReadInt64(), TimeSpan.Zero);
        }

        public static void Write(this BinaryWriter writer, DateTimeOffset value) {
            writer.Write(value.UtcTicks);
        }
    }
}