public class Authorization
{
    public enum Roles
    {
        Administrator,
        Moderator,
        User
    }
    public const string default_user_email = "user@secureapi.com";
    public const string default_admin_email = "admin@secureapi.com";
    public const string default_mod_email = "mod@secureapi.com";
    public const string default_password = "Password1!";
    public const Roles default_role = Roles.User;
}