using Spine.Common.Enums;

namespace Spine.Common.Extensions
{

    public static class PermissionExtensions
    {
        public static string GetStringValue(this Permissions permission)
        {
            // return the integer enum value as a string
            return ((int)permission).ToString();
        }


    }

}
