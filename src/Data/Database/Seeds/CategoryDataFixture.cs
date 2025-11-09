using System;
using System.Collections.Generic;
using Bogus;
using Domain.Entitites.Categories;

namespace Data.Database.Seeds;

internal static class CategoryDataFixture
{
    private const int CategoryCount = 150;
    private const int RandomSeed = 4_204_284;

    private static readonly Lazy<IReadOnlyCollection<Category>> CategoriesBuilder =
        new(GenerateCategories);

    public static IReadOnlyCollection<Category> Categories => CategoriesBuilder.Value;

    private static IReadOnlyCollection<Category> GenerateCategories()
    {
        Randomizer.Seed = new Random(RandomSeed);

        var faker = new Faker<Category>("en")
            .RuleFor(c => c.Name, (f, _) => $"{f.Commerce.Department()} {f.UniqueIndex}")
            .RuleFor(c => c.Description, (f, _) => f.Commerce.ProductDescription());

        return faker.Generate(CategoryCount).AsReadOnly();
    }
}

