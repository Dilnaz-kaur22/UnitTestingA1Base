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
    }
}