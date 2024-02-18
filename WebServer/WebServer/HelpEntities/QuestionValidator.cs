using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using WebServer.Entities;

namespace WebServer;

public class QuestionValidator : AbstractValidator<Question>
{
    private List<string> _categories = new() { "Кошки", "Собаки", "Грызуны" };
    
    public QuestionValidator()
    {
        RuleFor(question => question.Title).NotEmpty()
            .WithMessage("Заголовок вопроса не может быть пустым")
            .Length(5, 200)
            .WithMessage("Длина заголовка вопроса должна быть от 5 до 200 символов");
        
        RuleFor(question => question.Category).NotEmpty()
            .WithMessage("Категория вопроса не может быть пустой")
            .Must(c => _categories.Contains(c))
            .WithMessage("Несуществующая категория");

        RuleFor(question => question.Text).NotEmpty()
            .WithMessage("Текст вопроса не может быть пустым")
            .Length(10, 100)
            .WithMessage("Длина текста вопроса должна быть от 10 до 1000 символов");
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