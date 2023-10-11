using System;
using System.Collections.Generic;
using System.Linq;
using UnitTestingA1Base.Models;

namespace UnitTestingA1Base.Data
{
    public class BusinessLogicLayer
    {
        private AppStorage _appStorage;

        public BusinessLogicLayer(AppStorage appStorage)
        {
            _appStorage = appStorage;
        }

        public HashSet<Recipe> GetRecipesByIngredient(int? id, string? name)
        {
            Ingredient ingredient;
            HashSet<Recipe> recipes = new HashSet<Recipe>();

            if (id != null)
            {
                ingredient = _appStorage.Ingredients.First(i => i.Id == id);

                HashSet<RecipeIngredient> recipeIngredients = _appStorage.RecipeIngredients
                    .Where(rI => rI.IngredientId == ingredient.Id)
                    .ToHashSet();

                recipes = _appStorage.Recipes
                    .Where(r => recipeIngredients.Any(ri => ri.RecipeId == r.Id))
                    .ToHashSet();
            }

            return recipes;
        }

        public HashSet<Recipe> GetRecipesByDiet(int dietaryRestrictionId, object value)
        {
            // Implement the GetRecipesByDiet method.
            var recipes = new HashSet<Recipe>();

            // Get the dietary restriction based on ID.
            var dietaryRestriction = _appStorage.DietaryRestrictions.FirstOrDefault(dr => dr.Id == dietaryRestrictionId);

            if (dietaryRestriction != null)
            {
                // Get the restricted ingredients for the dietary restriction.
                var restrictedIngredients = _appStorage.IngredientRestrictions
                    .Where(ir => ir.DietaryRestrictionId == dietaryRestriction.Id)
                    .Select(ir => _appStorage.Ingredients.First(i => i.Id == ir.IngredientId))
                    .ToHashSet();

                // Filter recipes that do not contain restricted ingredients.
                recipes = _appStorage.Recipes
                    .Where(r => !r.RecipeIngredients
                        .Any(ri => restrictedIngredients
                            .Contains(_appStorage.Ingredients.First(i => i.Id == ri.IngredientId))))
                    .ToHashSet();
            }

            return recipes;
        }
        public HashSet<Recipe> GetRecipes(int? id, string? name)
        {
            // Implement the GetRecipes method to filter recipes by ID and/or name.
            var recipes = new HashSet<Recipe>();

            if (id != null)
            {
                recipes = _appStorage.Recipes
                    .Where(r => r.Id == id)
                    .ToHashSet();
            }

            if (!string.IsNullOrEmpty(name))
            {
                recipes = _appStorage.Recipes
                    .Where(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    .ToHashSet();
            }

            return recipes;
        }

        public APIResponse AddRecipe(RecipeInput recipeInput)
        {
            try
            {
                // Check if a recipe with the same name already exists
                if (_appStorage.Recipes.Any(r => r.Name.Equals(recipeInput.Recipe.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    return new APIResponse(false, "A recipe with the same name already exists.");
                }

                // Create a new recipe with a generated primary key
                var newRecipeId = _appStorage.GeneratePrimaryKey();
                var newRecipe = new Recipe
                {
                    Id = newRecipeId,
                    Name = recipeInput.Recipe.Name,
                    Description = recipeInput.Recipe.Description,
                    Servings = recipeInput.Recipe.Servings
                };

                // Add the new recipe to the Recipes collection
                _appStorage.Recipes.Add(newRecipe);

                // Add new ingredients to the Ingredients collection and create RecipeIngredient objects
                foreach (var ingredient in recipeInput.Ingredients)
                {
                    // Check if the ingredient with the same name already exists
                    var existingIngredient = _appStorage.Ingredients.FirstOrDefault(i => i.Name.Equals(ingredient.Name, StringComparison.OrdinalIgnoreCase));

                    if (existingIngredient == null)
                    {
                        // Create a new ingredient with a generated primary key
                        var newIngredientId = _appStorage.GeneratePrimaryKey();
                        existingIngredient = new Ingredient
                        {
                            Id = newIngredientId,
                            Name = ingredient.Name
                        };

                        // Add the new ingredient to the Ingredients collection
                        _appStorage.Ingredients.Add(existingIngredient);
                    }

                    // Create a RecipeIngredient object and add it to the RecipeIngredients collection
                    var recipeIngredient = new RecipeIngredient
                    {
                        IngredientId = existingIngredient.Id,
                        RecipeId = newRecipeId,
                        Amount = ingredient.Amount,
                        MeasurementUnit = ingredient.MeasurementUnit
                    };

                    _appStorage.RecipeIngredients.Add(recipeIngredient);
                }

                return new APIResponse(true, $"Recipe '{newRecipe.Name}' added successfully.");
            }
            catch (Exception ex)
            {
                return new APIResponse(false, "An error occurred while adding the recipe.");
            }
        }

        public APIResponse DeleteIngredient(int id, int? recipeId)
        {
            try
            {
                // Check if the ingredient with the specified ID exists
                var ingredient = _appStorage.Ingredients.FirstOrDefault(i => i.Id == id);

                if (ingredient == null)
                {
                    return new APIResponse(false, "Ingredient not found.");
                }

                // Check if the ingredient is used by multiple recipes
                var recipesUsingIngredient = _appStorage.RecipeIngredients
                    .Where(ri => ri.IngredientId == id)
                    .Select(ri => ri.RecipeId)
                    .Distinct()
                    .ToList();

                if (recipesUsingIngredient.Count > 1)
                {
                    return new APIResponse(false, "Ingredient is used by multiple recipes. Cannot delete.");
                }

                // If there's only one recipe using the ingredient, delete the recipe and associated RecipeIngredients
                if (recipesUsingIngredient.Count == 1)
                {
                    var recipeIdToDelete = recipesUsingIngredient.First();

                    // Delete associated RecipeIngredients
                    var recipeIngredientsToDelete = _appStorage.RecipeIngredients.Where(ri => ri.RecipeId == recipeIdToDelete).ToList();

                    foreach (var recipeIngredient in recipeIngredientsToDelete)
                    {
                        _appStorage.RecipeIngredients.Remove(recipeIngredient);
                    }

                    // Delete the recipe
                    var recipeToDelete = _appStorage.Recipes.FirstOrDefault(r => r.Id == recipeIdToDelete);
                    _appStorage.Recipes.Remove(recipeToDelete);
                }

                // Delete the ingredient
                _appStorage.Ingredients.Remove(ingredient);

                return new APIResponse(true, "Ingredient deleted successfully.");
            }
            catch (Exception ex)
            {
                return new APIResponse(false, "An error occurred while deleting the ingredient.");
            }
        }

        public APIResponse DeleteRecipe(int id, int? ingredientId)
        {
            try
            {
                // Check if the recipe with the specified ID exists
                var recipe = _appStorage.Recipes.FirstOrDefault(r => r.Id == id);

                if (recipe == null)
                {
                    return new APIResponse(false, "Recipe not found.");
                }

                // Check if the recipe is using multiple ingredients
                var ingredientsUsedInRecipe = _appStorage.RecipeIngredients
                    .Where(ri => ri.RecipeId == id)
                    .Select(ri => ri.IngredientId)
                    .Distinct()
                    .ToList();

                if (ingredientsUsedInRecipe.Count > 1)
                {
                    return new APIResponse(false, "Recipe uses multiple ingredients. Cannot delete.");
                }

                // If the recipe is using only one ingredient, delete the ingredient and associated RecipeIngredients
                if (ingredientsUsedInRecipe.Count == 1 && ingredientId.HasValue)
                {
                    var ingredientIdToDelete = ingredientsUsedInRecipe.First();

                    // Delete the associated RecipeIngredients
                    var recipeIngredientsToDelete = _appStorage.RecipeIngredients.Where(ri => ri.RecipeId == id).ToList();

                    foreach (var recipeIngredient in recipeIngredientsToDelete)
                    {
                        _appStorage.RecipeIngredients.Remove(recipeIngredient);
                    }

                    // Delete the ingredient
                    var ingredientToDelete = _appStorage.Ingredients.FirstOrDefault(i => i.Id == ingredientIdToDelete);
                    _appStorage.Ingredients.Remove(ingredientToDelete);
                }

                // Delete the recipe
                _appStorage.Recipes.Remove(recipe);

                return new APIResponse(true, "Recipe deleted successfully.");
            }
            catch (Exception ex)
            {
                return new APIResponse(false, "An error occurred while deleting the recipe.");
            }
        }
    }
}
