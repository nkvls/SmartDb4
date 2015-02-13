namespace SmartDb4.Helpers
{
    public static class RegXConstants
    {
        public const string RegExDecimal = @"^[0-9]+(\.[0-9][0-9]+)?$";
        public const string RegExPhoneNo = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$";
    }
}