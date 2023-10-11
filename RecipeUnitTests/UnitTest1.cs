using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using UnitTestingA1Base.Data;
using UnitTestingA1Base.Models;

namespace RecipeUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        private BusinessLogicLayer _initializeBusinessLogic()
        {
            return new BusinessLogicLayer(new AppStorage());
        }

        [TestMethod]
        public void GetRecipesByIngredient_ValidId_ReturnsRecipesWithIngredient()
        {
            // arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            int ingredientId = 6;
            int recipeCount = 2;

            // act
            HashSet<Recipe> recipes = bll.GetRecipesByIngredient(ingredientId, null);

            Assert.AreEqual(recipeCount, recipes.Count);
        }

        [TestMethod]
        public void GetRecipesByIngredient_ValidName_ReturnsRecipesWithIngredient()
        {
            // Arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            string ingredientName = "Salmon";
            int recipeCount = 2; // Salmon is used in two recipes.

            // Act
            HashSet<Recipe> recipes = bll.GetRecipesByIngredient(null, ingredientName);

            // Assert
            Assert.AreEqual(recipeCount, recipes.Count);
        }

        [TestMethod]
        public void GetRecipesByIngredient_InvalidId_ReturnsNull()
        {
            // Arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            int invalidIngredientId = 999; // An invalid ID.

            // Act
            HashSet<Recipe> recipes = bll.GetRecipesByIngredient(invalidIngredientId, null);

            // Assert
            Assert.AreEqual(0, recipes.Count);
        }

        [TestMethod]
        public void GetRecipes_ValidId_ReturnsRecipe()
        {
            // Arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            int recipeId = 1;
            string expectedRecipeName = "Spaghetti Carbonara";

            // Act
            HashSet<Recipe> recipes = bll.GetRecipes(recipeId, null);

            // Assert
            Assert.AreEqual(1, recipes.Count);
            Assert.AreEqual(expectedRecipeName, recipes.First().Name);
        }

        [TestMethod]
        public void GetRecipes_ValidName_ReturnsRecipe()
        {
            // Arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            string recipeName = "Chocolate Cake";
            string expectedRecipeName = "Chocolate Cake";

            // Act
            HashSet<Recipe> recipes = bll.GetRecipes(null, recipeName);

            // Assert
            Assert.AreEqual(1, recipes.Count);
            Assert.AreEqual(expectedRecipeName, recipes.First().Name);
        }

        [TestMethod]
        public void GetRecipes_InvalidId_ReturnsEmptySet()
        {
            // Arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            int invalidRecipeId = 999;

            // Act
            HashSet<Recipe> recipes = bll.GetRecipes(invalidRecipeId, null);

            // Assert
            Assert.AreEqual(0, recipes.Count);
        }

        [TestMethod]
        public void AddRecipe_ValidRecipeAndIngredients_CreatesRecipeAndIngredients()
        {
            // Arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            RecipeInput recipeInput = new RecipeInput
            {
                Name = "New Recipe",
                Recipe = new Recipe
                {
                    Description = "Test Recipe",
                    Servings = 2
                },
                Ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient
                    {
                        Name = "Test Ingredient",
                        Amount = 100,
                        MeasurementUnit = MeasurementUnit.Grams
                    }
                }
            };

            // Act
            var result = bll.AddRecipe(recipeInput);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(201, result.StatusCode); // Created
        }

        [TestMethod]
        public void DeleteIngredient_ValidId_DeletesIngredientAndRelatedRecipe()
        {
            // Arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            int ingredientId = 6; // Salmon
            int expectedRecipeCount = 1; // One recipe uses Salmon.

            // Act
            var result = bll.DeleteIngredient(ingredientId, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode); // OK

            // Verify that the related recipe is deleted
            HashSet<Recipe> recipes = bll.GetRecipesByIngredient(ingredientId, null);
            Assert.AreEqual(expectedRecipeCount, recipes.Count);
        }

        [TestMethod]
        public void DeleteRecipe_ValidId_DeletesRecipe()
        {
            // Arrange
            BusinessLogicLayer bll = _initializeBusinessLogic();
            int recipeId = 1; // Spaghetti Carbonara

            // Act
            var result = bll.DeleteRecipe(recipeId, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode); // OK
        }
    }
}
