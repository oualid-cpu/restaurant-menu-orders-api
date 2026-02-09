using FluentValidation;
using RestaurantApi.DTOs;

namespace RestaurantApi.Validation;

public class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemDtoValidator()
    {
        RuleFor(x => x.MenuItemId)
            .GreaterThan(0).WithMessage("MenuItemId must be greater than 0.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
            .LessThanOrEqualTo(1000).WithMessage("Quantity must be 1000 or less.");
    }
}

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.Items)
            .NotNull().WithMessage("Items is required.")
            .Must(items => items != null && items.Count > 0)
            .WithMessage("At least one order item is required.");

        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemDtoValidator());

        RuleFor(x => x.Items)
            .Must(items =>
            {
                if (items == null) return true;
                return items
                    .Where(i => i.MenuItemId > 0)
                    .GroupBy(i => i.MenuItemId)
                    .All(g => g.Count() == 1);
            })
            .WithMessage("Duplicate MenuItemId(s) found. Combine quantities into one line.");
    }
}
