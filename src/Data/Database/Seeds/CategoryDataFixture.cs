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

        var faker = new Faker();
        var categories = new List<Category>(CategoryCount);

        for (var index = 0; index < CategoryCount; index++)
        {
            string name = $"{faker.Commerce.Department()} {index}";
            string description = faker.Commerce.ProductDescription();
            categories.Add(new Category(name, description));
        }

        return categories.AsReadOnly();
    }
}

