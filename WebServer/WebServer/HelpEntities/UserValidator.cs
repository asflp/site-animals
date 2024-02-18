using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using WebServer.Entities;

namespace WebServer;

public class UserValidator : AbstractValidator<UserLogin>
{
    public UserValidator()
    {
        RuleFor(user => user.Name).NotEmpty()
            .WithMessage("Имя не может быть пустым")
            .Must(x => !x.Contains(" "))
            .WithMessage("Имя не может содержать пробелы")
            .Length(2, 15)
            .WithMessage("Длина имени должна быть от 2 до 15 символов")
            .Must(x => x.All(char.IsLetter))
            .WithMessage("Имя должно содержать только буквы");

        RuleFor(user => user.Nickname).NotEmpty()
            .WithMessage("Ник не может быть пустым")
            .Must(x => !x.Contains(" "))
            .WithMessage("Ник не может содержать пробелы")
            .Length(5, 30)
            .WithMessage("Длина никнейма должна быть от 5 до 30 символов")
            .Matches("^[a-zA-Z_]+$")
            .WithMessage("Ник может содержать английские буквы и нижнее подчеркивание")
            .Must(x => x.Any(char.IsLetter))
            .WithMessage("Ник должен содержать хотя бы одну букву")
            .Must(x => x[0] != '_')
            .WithMessage("Ник не должен начинаться с нижнего подчеркивания");

        RuleFor(user => user.Email).NotEmpty()
            .WithMessage("Почта не может быть пустой")
            .EmailAddress()
            .WithMessage("Введите адрес электронной почты");

            RuleFor(user => user.Password).NotEmpty()
                .WithMessage("Пароль не может быть пустым")
                .Length(8, 30)
                .WithMessage("Длина пароля должна быть от 8 до 30 символов")
                .Must(x => x.Any(char.IsLetter))
                .WithMessage("Пароль должен содержать хотя бы одну букву")
                .Must(x => x.Any(char.IsDigit))
                .WithMessage("Пароль должен содержать хотя бы одну цифру");
    }
    
    public string? GetErrors(ValidationResult validationResult)
    {
        if (!validationResult.IsValid)
        {
            var dict = new Dictionary<string, string>();
            
            foreach (var failure in validationResult.Errors)
            {
                dict.TryAdd(failure.PropertyName, failure.ErrorMessage);
            }

            return JsonSerializer.Serialize(dict);
        }

        return null;
    }
}