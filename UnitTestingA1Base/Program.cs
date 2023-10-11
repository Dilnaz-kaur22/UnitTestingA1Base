#region Setup
using System.Text.Json;
using UnitTestingA1Base.Data;
using UnitTestingA1Base.Models;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseHttpsRedirection();

// Application Storage persists for single session
AppStorage appStorage = new AppStorage();
BusinessLogicLayer bll = new BusinessLogicLayer(appStorage);    
#endregion


#region Endpoints

/*
 * All methods should return a NotFound response code if a search using a Primary Key does not find any results.
 * 
 * Otherwise, the method should return an empty collection of the resource indicated in the root directory of the request,hh.
 * 
 * All methods with a name and id parameter can use either. 
 * 
 * Searches by name should use the first queried element that contains the entire string (for example, "spa" returns "spaghetti" while "spaghettio" returns nothing).
 * 
 * Requests with a Name AND ID argument should query the database using the ID. If no match is found, the query should again be performed using the name.
 */

///<summary>
/// Returns a HashSet of all Recipes that contain the specified Ingredient by name or Primary Key
/// </summary>
app.MapGet("/recipes/byIngredient", (string? name, int? id) =>
{
    try
    {
        HashSet<Recipe> recipes;

        if (id.HasValue)
        {
            Ingredient ingredient = appStorage.Ingredients.First(i => i.Id == id);
            HashSet<RecipeIngredient> recipeIngredients = appStorage.RecipeIngredients
                .Where(ri => ri.IngredientId == ingredient.Id)
                .ToHashSet();
            recipes = appStorage.Recipes
                .Where(r => recipeIngredients.Any(ri => ri.RecipeId == r.Id))
                .ToHashSet();
        }
        else if (!string.IsNullOrWhiteSpace(name))
        {
            recipes = appStorage.Recipes
                .Where(r => r.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .ToHashSet();
        }
        else
        {
            recipes = new HashSet<Recipe>(); // No filter provided.
        }

        return Results.Ok(recipes);
    }
    catch (Exception ex)
    {
        return Results.NotFound();
    }
});

///<summary>
/// Returns a HashSet of all Recipes that only contain ingredients that belong to the Dietary Restriction provided by name or Primary Key
/// </summary>
app.MapGet("/recipes/byDiet", (string? name, int? id) =>
{
    try
    {
        HashSet<Recipe> recipes;

        if (id > 0)
        {
            DietaryRestriction restriction = appStorage.DietaryRestrictions.FirstOrDefault(dr => dr.Id == id);
            if (restriction != null)
            {
                HashSet<Ingredient> restrictedIngredients = appStorage.IngredientRestrictions
                    .Where(ir => ir.DietaryRestrictionId == restriction.Id)
                    .Select(ir => appStorage.Ingredients.First(i => i.Id == ir.IngredientId))
                    .ToHashSet();

                recipes = appStorage.Recipes
                    .Where(r => !r.RecipeIngredients
                        .Any(ri => restrictedIngredients
                            .Contains(appStorage.Ingredients.First(i => i.Id == ri.IngredientId))))
                    .ToHashSet();
            }
            else
            {
                return Results.NotFound();
            }
        }
        else if (!string.IsNullOrWhiteSpace(name))
        {
            recipes = appStorage.Recipes
                .Where(r => r.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .ToHashSet();
        }
        else
        {
            recipes = new HashSet<Recipe>(); // No filter provided.
        }

        return Results.Ok(recipes);
    }
    catch (Exception ex)
    {
        return Results.NotFound();
    }
});

///<summary>
///Returns a HashSet of all recipes by either Name or Primary Key. 
/// </summary>
app.MapGet("/recipes", (string? name, int? id) =>
{
    try
    {
        HashSet<Recipe> recipes;

        if (id.HasValue)
        {
            Recipe recipe = appStorage.Recipes.FirstOrDefault(r => r.Id == id);
            if (recipe != null)
            {
                recipes = new HashSet<Recipe> { recipe };
            }
            else
            {
                return Results.NotFound();
            }
        }
        else if (!string.IsNullOrWhiteSpace(name))
        {
            recipes = appStorage.Recipes
                .Where(r => r.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .ToHashSet();
        }
        else
        {
            recipes = appStorage.Recipes.ToHashSet();
        }

        return Results.Ok(recipes);
    }
    catch (Exception ex)
    {
        return Results.NotFound();
    }
});

///<summary>
/// Receives a JSON object which should contain a Recipe and Ingredients
/// 
/// A new Class should be created to serve as the Parameter of this method
/// 
/// If a Recipe with the same name as the new Recipe already exists, an InvalidOperation exception should be thrown.
/// 
/// If an Ingredient with the same name as an existing Ingredient is provided, it should not add that ingredient to storage.
/// 
/// The method should add all Recipes and (new) ingredients to the database. It should create RecipeIngredient objects and add them to the database to represent their relationship. Remember to use the IDs of any preexisting ingredients rather than new IDs.
/// 
/// All IDs should be created for these objects using the returned value of the AppStorage.GeneratePrimaryKey() method
/// </summary>
app.MapPost("/recipes", async (RecipeInput input) =>
{
    try
    {
        // Check if a recipe with the same name already exists
        if (appStorage.Recipes.Any(r => r.Name.Equals(input.Recipe.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return Results.BadRequest("A recipe with the same name already exists.");
        }

        // Create a new recipe with a generated primary key
        var newRecipeId = appStorage.GeneratePrimaryKey();
        var newRecipe = new Recipe
        {
            Id = newRecipeId,
            Name = input.Recipe.Name,
            Description = input.Recipe.Description,
            Servings = input.Recipe.Servings
        };

        // Add the new recipe to the Recipes collection
        appStorage.Recipes.Add(newRecipe);

        // Add new ingredients to the Ingredients collection and create RecipeIngredient objects
        foreach (var ingredient in input.Ingredients)
        {
            // Check if the ingredient with the same name already exists
            var existingIngredient = appStorage.Ingredients.FirstOrDefault(i => i.Name.Equals(ingredient.Name, StringComparison.OrdinalIgnoreCase));

            if (existingIngredient == null)
            {
                // Create a new ingredient with a generated primary key
                var newIngredientId = appStorage.GeneratePrimaryKey();
                existingIngredient = new Ingredient
                {
                    Id = newIngredientId,
                    Name = ingredient.Name
                };

                // Add the new ingredient to the Ingredients collection
                appStorage.Ingredients.Add(existingIngredient);
            }

            // Create a RecipeIngredient object and add it to the RecipeIngredients collection
            var recipeIngredient = new RecipeIngredient
            {
                IngredientId = existingIngredient.Id,
                RecipeId = newRecipeId,
                Amount = ingredient.Amount,
                MeasurementUnit = ingredient.MeasurementUnit
            };

            appStorage.RecipeIngredients.Add(recipeIngredient);
        }

        return Results.Created($"/recipes/{newRecipe.Id}", newRecipe);
    }
    catch (Exception ex)
    {
        return Results.BadRequest("An error occurred while adding the recipe.");
    }
});

///<summary>
/// Deletes an ingredient from the database. 
/// If there is only one Recipe using that Ingredient, then the Recipe is also deleted, as well as all associated RecipeIngredients
/// If there are multiple Recipes using that ingredient, a Forbidden response code should be provided with an appropriate message
///</summary>
app.MapDelete("/ingredients", async (int? id, string? name) =>
{
    // Check if the ingredient with the specified ID exists
    var ingredient = appStorage.Ingredients.FirstOrDefault(i => i.Id == id);

    if (ingredient == null)
    {
        return Results.NotFound("Ingredient not found.");
    }

    // Check if the ingredient is used by multiple recipes
    var recipesUsingIngredient = appStorage.RecipeIngredients
        .Where(ri => ri.IngredientId == id)
        .Select(ri => ri.RecipeId)
        .Distinct()
        .ToList();

    if (recipesUsingIngredient.Count > 1)
    {
        var response = new
        {
            Message = "Ingredient is used by multiple recipes. Cannot delete."
        };
        return Results.Json(response, options: null, statusCode: 403);
    }


    // If there's only one recipe using the ingredient, delete the recipe and associated RecipeIngredients
    if (recipesUsingIngredient.Count == 1)
    {
        var recipeId = recipesUsingIngredient.First();

        // Delete associated RecipeIngredients
        var recipeIngredientsToDelete = appStorage.RecipeIngredients.Where(ri => ri.RecipeId == recipeId).ToList();

        foreach (var recipeIngredient in recipeIngredientsToDelete)
        {
            appStorage.RecipeIngredients.Remove(recipeIngredient);
        }

        // Delete the recipe
        var recipeToDelete = appStorage.Recipes.FirstOrDefault(r => r.Id == recipeId);
        appStorage.Recipes.Remove(recipeToDelete);
    }

    // Delete the ingredient
    appStorage.Ingredients.Remove(ingredient);

    return Results.Ok("Ingredient deleted successfully.");

});


/// <summary>
/// Deletes the requested recipe from the database
/// This should also delete the associated IngredientRecipe objects from the database
/// </summary>
app.MapDelete("/recipes", async (int? id, string? name) =>
{
    // Check if the recipe with the specified ID exists
    var recipe = appStorage.Recipes.FirstOrDefault(r => r.Id == id);

    if (recipe == null)
    {
        return Results.NotFound("Recipe not found.");
    }

    // Delete associated RecipeIngredients
    var recipeIngredientsToDelete = appStorage.RecipeIngredients.Where(ri => ri.RecipeId == id).ToList();

    foreach (var recipeIngredient in recipeIngredientsToDelete)
    {
        appStorage.RecipeIngredients.Remove(recipeIngredient);
    }

    // Delete the recipe
    appStorage.Recipes.Remove(recipe);

    return Results.Ok("Recipe deleted successfully.");
});


#endregion


app.Run();
