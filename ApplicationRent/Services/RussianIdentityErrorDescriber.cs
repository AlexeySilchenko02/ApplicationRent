using Microsoft.AspNetCore.Identity;

namespace ApplicationRent.Services
{
    public class RussianIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError
            {
                Code = nameof(PasswordTooShort),
                Description = $"Пароль должен быть не менее {length} символов."
            };
        }

        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresNonAlphanumeric),
                Description = "Пароль должен содержать хотя бы один не буквенно-цифровой символ."
            };
        }

        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresDigit),
                Description = "Пароль должен содержать хотя бы одну цифру."
            };
        }

        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresLower),
                Description = "Пароль должен содержать хотя бы одну строчную букву (a-z)."
            };
        }

        public override IdentityError PasswordRequiresUpper()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresUpper),
                Description = "Пароль должен содержать хотя бы одну заглавную букву (A-Z)."
            };
        }
    }
}
