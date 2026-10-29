using BCryptNet = BCrypt.Net.BCrypt;

namespace TTDaoTaoLaiXe.Helpers
{
    public static class PasswordHelper
    {
        public static string Hash(string password)
        {
            return BCryptNet.HashPassword(password);
        }

        public static bool Verify(string password, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                return false;

            try
            {
                return BCryptNet.Verify(password, passwordHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}