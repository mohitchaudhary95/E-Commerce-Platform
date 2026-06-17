using ECommerce.Identity.Application.Features.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Identity.Application.Features.Validators
{
    public class IdentityValidators
    {
        public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
        {
            public RegisterUserValidator()
            {
                RuleFor(x => x.Dto.FirstName)
                    .NotEmpty().WithMessage("First name is required.")
                    .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

                RuleFor(x => x.Dto.LastName)
                    .NotEmpty().WithMessage("Last name is required.")
                    .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

                RuleFor(x => x.Dto.Email)
                    .NotEmpty().WithMessage("Email is required.")
                    .EmailAddress().WithMessage("A valid email address is required.");

                RuleFor(x => x.Dto.Password)
                    .NotEmpty().WithMessage("Password is required.")
                    .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                    .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                    .Matches("[0-9]").WithMessage("Password must contain at least one number.");

                // Cross-field validation — confirm password must match
                RuleFor(x => x.Dto.ConfirmPassword)
                    .Equal(x => x.Dto.Password).WithMessage("Passwords do not match.");
            }
        }

        public class LoginValidator : AbstractValidator<LoginCommand>
        {
            public LoginValidator()
            {
                RuleFor(x => x.Dto.Email)
                    .NotEmpty().WithMessage("Email is required.")
                    .EmailAddress().WithMessage("A valid email address is required.");

                RuleFor(x => x.Dto.Password)
                    .NotEmpty().WithMessage("Password is required.");
            }
        }

        public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
        {
            public ChangePasswordValidator()
            {
                RuleFor(x => x.Dto.CurrentPassword)
                    .NotEmpty().WithMessage("Current password is required.");

                RuleFor(x => x.Dto.NewPassword)
                    .NotEmpty().WithMessage("New password is required.")
                    .MinimumLength(8).WithMessage("New password must be at least 8 characters.")
                    .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                    .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                    .NotEqual(x => x.Dto.CurrentPassword).WithMessage("New password must differ from current password.");

                RuleFor(x => x.Dto.ConfirmNewPassword)
                    .Equal(x => x.Dto.NewPassword).WithMessage("Passwords do not match.");
            }
        }
    }
}
