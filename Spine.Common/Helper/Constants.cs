using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using Spine.Common.Enums;

namespace Spine.Common.Helpers
{
    public static class Constants
    {
        public const int DaysToDisableAccount = 7;
        public const int DaysToStartVerificationReminder = 14;
        public const int DaysToStartSubscriptionReminder = 7;
        public const string NigerianCurrencyCode = "NGN";

        public static DateTime GetCurrentDateTime(TimeZoneInfo timeZone = null)
        {
            if (timeZone == null || timeZone == TimeZoneInfo.Local)
                return DateTime.Now;

            return DateTime.UtcNow;
        }

        public const string OtpProvider = "LoginOTP";
        public const string EmailConfirmationProvider = "EmailConfirmation";

        public const string PermissionClaim = "PermissionClaim";
        public const string PermissionCache = "PermissionCache";
        public const string AllBannerCache = "BannerCache";
        public const string BannerCache = "BannerCache_";
        public const string SignatureCache = "SignatureCache_";
        public const string LogoCache = "LogoCache_";

        public static List<Permissions> BusinessOwnerPermission()
        {
            var allPermissions = Enum.GetValues(typeof(Permissions)).Cast<Permissions>().ToList();
            return allPermissions;
        }

        public static string GetConfirmAccountLink(string webUrl, string code)
        {
            return $"{webUrl}confirm?code={UrlEncoder.Default.Encode(code)}";
        }

        public static string GetAcceptInvitetLink(string webUrl, string code)
        {
            return $"{webUrl}accept-invite?code={UrlEncoder.Default.Encode(code)}";
        }

        public static string GetResetPasswordLink(string webUrl, string code)
        {
            return $"{webUrl}reset-password?code={UrlEncoder.Default.Encode(code)}";
        }

        public static long GenerateId(int length = 6)
        {
            string numbers = "1234567890";

            string characters = numbers;
            string id = string.Empty;
            for (int i = 0; i < length; i++)
            {
                string character = string.Empty;
                do
                {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                } while (id.IndexOf(character) != -1);
                id += character;
            }

            return long.Parse(id);
        }

        public static string GenerateTransactionReference(DateTime date, int serial)
        {
            return $"TR{date:ddMMyyyy}_{serial:D5}";
        }

        public enum SerialNoType
        {
            PO,
            GR,
            Period,
            Journal,
            GeneralLedger
        }
        public static string GenerateSerialNo(SerialNoType type, int  serial)
        {
            switch (type)
            {
                case SerialNoType.PO:
                    return $"PO-{serial:D5}";
                case SerialNoType.GR:
                    return $"GR-{serial:D5}";
                case SerialNoType.Journal:
                    return $"JO-{serial:D5}";
                case SerialNoType.Period:
                    return $"PED-{serial:D5}";
                case SerialNoType.GeneralLedger:
                    return $"REF-{serial:D6}";
                default:
                    throw new NotImplementedException("Invalid serial no type");
            }
        }
        public static string GenerateAlphaNumericId(int length = 8, bool isNumericOnly = false)
        {
            string alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string small_alphabets = "abcdefghijklmnopqrstuvwxyz";
            string numbers = "1234567890";

            string characters = numbers;
            if (!isNumericOnly)
            {
                characters += alphabets + small_alphabets + numbers;
            }
            string id = string.Empty;
            for (int i = 0; i < length; i++)
            {
                string character = string.Empty;
                do
                {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                } while (id.IndexOf(character) != -1);
                id += character;
            }

            return id;
        }

    }

}
