namespace Common.Constants
{
    public static class Validation
    {
        public static class Authorization
        {
            public const string Administrator = "el.admin";
        }

        public static class Common
        {
            public const int Division = 200;
            public const int NameLength = 50;
            public const int DescriptionLength = 60;
            public const int NotesLength = 500;
            public const int IdLength = 40;
            public const int PhoneNumberLength = 15;
            public const int EmailLength = 250;
            public const int SecondsInADay = 86400;
            public const string DateFormat = "^[0-9]{8}$";
            public const string WorkCenterFormat = "^[A-Z0-9]{1,8}$";
            public const string TimeWithHoursOnly = @"^(?<hours>\d{1,2})$";
            public const string TimeWithoutSecondsFormat = @"^(?<hours>\d{1,2})(?<minutes>\d{2})$";
            public const string TimeWithSecondsFormat = @"^(?<hours>\d{1,2})(?<minutes>\d{2})(?<seconds>\d{2})$";
            public const string SpecialCharactersFormat = "^[^A-Za-z0-9]+$";
            public const string NameFormat = @"^(?<first>\p{L}+(-\p{L}+)*)(?<middle>.*)(?<last>\s\p{L}+(-\p{L}+)*)$";
            public const string PhoneFormat = "^(\\+|\\d)*[\\d|\\s\\-]+$";
            public const string EmailPattern =
                @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
            public const string RentalAgreementNumberPattern = @"^[A]\d{6}$";
        }

        public static class User
        {
            public static bool IsValidUserId(string? userId)
            {
                if (string.IsNullOrEmpty(userId))
                    return false;

                return userId.Length <= Common.IdLength;
            }
        }
    }

    public record AllowedIdRange(int First, int Last)
    {
        public int First { get; } = First;
        public int Last { get; } = Last;

        public override string ToString()
        {
            return $"[{First}-{Last}]";
        }
    }
}