using Microsoft.AspNetCore.Identity;

public class ApplicationDbContextSeed
{
    public static async Task SeedEssentialsAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Seed Roles
        await roleManager.CreateAsync(new IdentityRole(Authorization.Roles.Administrator.ToString()));
        await roleManager.CreateAsync(new IdentityRole(Authorization.Roles.Moderator.ToString()));
        await roleManager.CreateAsync(new IdentityRole(Authorization.Roles.User.ToString()));

        // Seed Default Admin User
        var adminUser = new User
        {
            UserName = "PhiLia093",
            Email = "admin@secureapi.com",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            FirstName = "Admin",
            LastName = "User"
        };

        // Check if Admin User already exists
        var existingAdmin = await userManager.FindByEmailAsync(adminUser.Email);
        if (existingAdmin == null)
        {
            var result = await userManager.CreateAsync(adminUser, Authorization.default_password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Authorization.Roles.Administrator.ToString());
            }
        }

        // Seed Default Moderator User
        var moderatorUser = new User
        {
            UserName = "HubRis504",
            Email = Authorization.default_mod_email,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            FirstName = "Moderator",
            LastName = "User"
        };

        // Check if Moderator User already exists
        var existingModerator = await userManager.FindByEmailAsync(moderatorUser.Email);
        if (existingModerator == null)
        {
            var result = await userManager.CreateAsync(moderatorUser, Authorization.default_password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(moderatorUser, Authorization.Roles.Moderator.ToString());
            }
        }

        // Seed Default User
        var defaultUser = new User
        {
            UserName = "NeiKos496",
            Email = Authorization.default_user_email,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            FirstName = "Default",
            LastName = "User"
        };

        var existingUser = await userManager.FindByEmailAsync(defaultUser.Email);
        if (existingUser == null)
        {
            var result = await userManager.CreateAsync(defaultUser, Authorization.default_password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(defaultUser, Authorization.default_role.ToString());
            }
        }
    }
}